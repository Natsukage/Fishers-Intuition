using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Memory;

namespace 渔人的直感.Models
{
    /// <summary>
    /// A SigScanner facilitates searching for memory signatures in a given ProcessModule.
    /// </summary>
    public sealed class SigScanner : IDisposable
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, IntPtr nSize, out long lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        /// <summary>
        /// Set up the SigScanner.
        /// </summary>
        /// <param name="module">The ProcessModule to be used for scanning</param>
        /// <param name="doCopy">Whether or not to copy the module upon initialization for search operations to use, as to not get disturbed by possible hooks.</param>
        public Mem MemLib;
        public SigScanner(Process process, ProcessModule module, bool doCopy = false) {
            ProcessPtr = OpenProcess(PROCESS_VM_READ, false, process.Id);
            Module = module;
            Is32BitProcess = !Environment.Is64BitProcess;
            IsCopy = doCopy;
            MemLib = new Mem();
            MemLib.OpenProcess(process.Id);
            // Limit the search space to .text section.
            SetupSearchSpace(module);

            if (IsCopy)
                SetupCopiedSegments();

            Console.WriteLine($"Module base: {TextSectionBase}");
            Console.WriteLine($"Module size: {TextSectionSize}");
        }
        const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        const int PROCESS_VM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        /// <summary>
        /// If the search on this module is performed on a copy.
        /// </summary>
        public bool IsCopy { get; private set; }

        /// <summary>
        /// If the ProcessModule is 32-bit.
        /// </summary>
        public bool Is32BitProcess { get; }

        public IntPtr ProcessPtr;

        /// <summary>
        /// The base address of the search area. When copied, this will be the address of the copy.
        /// </summary>
        public IntPtr SearchBase => IsCopy ? this.moduleCopyPtr : Module.BaseAddress;

        /// <summary>
        /// The base address of the .text section search area.
        /// </summary>
        public IntPtr TextSectionBase => new IntPtr(SearchBase.ToInt64() + TextSectionOffset);
        /// <summary>
        /// The offset of the .text section from the base of the module.
        /// </summary>
        public long TextSectionOffset { get; private set; }
        /// <summary>
        /// The size of the text section.
        /// </summary>
        public int TextSectionSize { get; private set; }

        /// <summary>
        /// The base address of the .data section search area.
        /// </summary>
        public IntPtr DataSectionBase => new IntPtr(SearchBase.ToInt64() + DataSectionOffset);
        /// <summary>
        /// The offset of the .data section from the base of the module.
        /// </summary>
        public long DataSectionOffset { get; private set; }
        /// <summary>
        /// The size of the .data section.
        /// </summary>
        public int DataSectionSize { get; private set; }

        /// <summary>
        /// The ProcessModule on which the search is performed.
        /// </summary>
        public ProcessModule Module { get; }

        private IntPtr TextSectionTop => TextSectionBase + TextSectionSize;

        public byte[] ReadBytes(IntPtr offset, uint length) {
            var bytes = new byte[length];
            ReadProcessMemory(ProcessPtr,
                offset, bytes, new UIntPtr(length), IntPtr.Zero);
            return bytes;
        }

        public Int64 ReadInt64(IntPtr address,int offset = 0) => BitConverter.ToInt64(ReadBytes(IntPtr.Add(address,offset), 8), 0);


        public IntPtr ReadIntPtr(IntPtr address,int offset = 0) => (IntPtr)BitConverter.ToInt64(ReadBytes(IntPtr.Add(address,offset), 8), 0);


        public Int32 ReadInt32(IntPtr address, int offset = 0) => BitConverter.ToInt32(ReadBytes(IntPtr.Add(address, offset), 4), 0);


        public Int16 ReadInt16(IntPtr address, int offset = 0) => BitConverter.ToInt16(ReadBytes(IntPtr.Add(address, offset), 2), 0);

        public Byte ReadByte(IntPtr address, int offset = 0) => ReadBytes(IntPtr.Add(address, offset), 1)[0];

        public float ReadFloat(IntPtr address, int offset = 0) => BitConverter.ToSingle(ReadBytes(IntPtr.Add(address, offset), 4), 0);

        private void SetupSearchSpace(ProcessModule module) {
            var baseAddress = module.BaseAddress;
            // We don't want to read all of IMAGE_DOS_HEADER or IMAGE_NT_HEADER stuff so we cheat here.
            var ntNewOffset = ReadInt32(baseAddress, 0x3C);
            //Console.WriteLine(ntNewOffset);
            var ntHeader = baseAddress + ntNewOffset;

            // IMAGE_NT_HEADER
            var fileHeader = ntHeader + 4;
            var numSections = ReadInt16(ntHeader, 6);

            // IMAGE_OPTIONAL_HEADER
            var optionalHeader = fileHeader + 20;

            IntPtr sectionHeader;
            if (Is32BitProcess) // IMAGE_OPTIONAL_HEADER32
                sectionHeader = optionalHeader + 224;
            else // IMAGE_OPTIONAL_HEADER64
                sectionHeader = optionalHeader + 240;

            // IMAGE_SECTION_HEADER
            var sectionCursor = sectionHeader;
            for (var i = 0; i < numSections; i++) {
                var sectionName = ReadInt64(sectionCursor);

                // .text
                switch (sectionName) {
                    case 0x747865742E: // .text
                        TextSectionOffset = ReadInt32(sectionCursor, 12);
                        TextSectionSize = ReadInt32(sectionCursor, 8);
                        break;
                    case 0x617461642E: // .data
                        DataSectionOffset = ReadInt32(sectionCursor, 12);
                        DataSectionSize = ReadInt32(sectionCursor, 8);
                        break;
                }

                sectionCursor += 40;
            }
        }

        private IntPtr moduleCopyPtr;
        private long moduleCopyOffset;

        private unsafe void SetupCopiedSegments() {
            Console.WriteLine("module copy START");
            // .text
            this.moduleCopyPtr = Marshal.AllocHGlobal(Module.ModuleMemorySize);
            Console.WriteLine($"Alloc: {this.moduleCopyPtr.ToInt64():x}");
            Buffer.MemoryCopy(Module.BaseAddress.ToPointer(), this.moduleCopyPtr.ToPointer(), Module.ModuleMemorySize,
                              Module.ModuleMemorySize);

            this.moduleCopyOffset = this.moduleCopyPtr.ToInt64() - Module.BaseAddress.ToInt64();

            Console.WriteLine("copy OK!");
        }

        /// <summary>
        /// Free the memory of the copied module search area on object disposal, if applicable.
        /// </summary>
        public void Dispose() {
            Marshal.FreeHGlobal(this.moduleCopyPtr);
        }

        /// <summary>
        /// Scan for a byte signature in the .text section.
        /// </summary>
        /// <param name="signature">The signature.</param>
        /// <returns>The real offset of the found signature.</returns>
        public IntPtr ScanText(string signature) {
            var mBase = IsCopy ? this.moduleCopyPtr : TextSectionBase;

            //var scanRet = Scan(mBase, TextSectionSize, signature);
            var scanRet = (IntPtr)MemLib.AoBScan(signature,false, true).GetAwaiter().GetResult().FirstOrDefault();
            //MemLib.AoBScan();
            if (IsCopy)
                scanRet = new IntPtr(scanRet.ToInt64() - this.moduleCopyOffset);

            if (ReadByte(scanRet) == 0xE8)
                return ReadCallSig(scanRet);

            return scanRet;
        }




        /// <summary>
        /// Helper for ScanText to get the correct address for 
        /// IDA sigs that mark the first CALL location.
        /// </summary>
        /// <param name="SigLocation">The address the CALL sig resolved to.</param>
        /// <returns>The real offset of the signature.</returns>
        private IntPtr ReadCallSig(IntPtr SigLocation) {
            int jumpOffset = ReadInt32(IntPtr.Add(SigLocation, 1));
            return IntPtr.Add(SigLocation, 5 + jumpOffset);
        }

        /// <summary>
        /// Scan for a .data address using a .text function.
        /// This is intended to be used with IDA sigs.
        /// Place your cursor on the line calling a static address, and create and IDA sig.
        /// </summary>
        /// <param name="signature">The signature of the function using the data.</param>
        /// <param name="offset">The offset from function start of the instruction using the data.</param>
        /// <returns>An IntPtr to the static memory location.</returns>
        public IntPtr GetStaticAddressFromSig(string signature, int offset = 0) {
            IntPtr instrAddr = ScanText(signature);
            instrAddr = IntPtr.Add(instrAddr, offset);
            long bAddr = (long)Module.BaseAddress;
            long num;
            do {
                instrAddr = IntPtr.Add(instrAddr, 1);
                num = ReadInt32(instrAddr) + (long)instrAddr + 4 - bAddr;
            }
            while (!(num >= DataSectionOffset && num <= DataSectionOffset + DataSectionSize));
            return IntPtr.Add(instrAddr, ReadInt32(instrAddr) + 4);
        }



        public IntPtr ResolveRelativeAddress(IntPtr nextInstAddr, int relOffset) {
            if (Is32BitProcess) throw new NotSupportedException("32 bit is not supported.");

            return nextInstAddr + relOffset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe bool IsMatch(IntPtr pCursor, byte?[] needle) {
            for (var i = 0; i < needle.Length; i++) {
                var expected = needle[i];
                if (expected == null) continue;
                var actual = ReadByte(IntPtr.Add(pCursor, i));
                //Console.WriteLine(actual);
                if (expected != actual) return false;
            }

            return true;
        }
    }
}
