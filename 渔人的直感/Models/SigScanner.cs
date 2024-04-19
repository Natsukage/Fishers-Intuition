using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;


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
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, IntPtr nSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, IntPtr nSize, out long lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr dwSize, ref int lpNumberOfBytesRead);

        /// <summary>
        /// Set up the SigScanner.
        /// </summary>
        /// <param name="module">The ProcessModule to be used for scanning</param>
        public SigScanner(Process process, ProcessModule module)
        {
            ProcessPtr = OpenProcess(PROCESS_VM_READ, false, process.Id);
            Module = module;
            Is32BitProcess = !Environment.Is64BitProcess;

            // Limit the search space to .text section.
            SetupSearchSpace(module);

            SetupCopiedSegments();

            Debug.WriteLine($"Module base: {TextSectionBase:X}");
            Debug.WriteLine($"Module size: {TextSectionSize:X}");
        }

        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;

        /// <summary>
        /// If the ProcessModule is 32-bit.
        /// </summary>
        public bool Is32BitProcess { get; }

        public IntPtr ProcessPtr;

        /// <summary>
        /// The base address of the search area. When copied, this will be the address of the copy.
        /// </summary>
        public IntPtr SearchBase => Module.BaseAddress;

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

        public byte[] ReadBytes(IntPtr offset, uint length)
        {
            var bytes = new byte[length];
            ReadProcessMemory(ProcessPtr,
                              offset, bytes, new IntPtr(length), IntPtr.Zero);
            return bytes;
        }

        public long ReadInt64(IntPtr address, int offset = 0) => BitConverter.ToInt64(ReadBytes(IntPtr.Add(address, offset), 8), 0);

        public IntPtr ReadIntPtr(IntPtr address, int offset = 0) => (IntPtr)BitConverter.ToInt64(ReadBytes(IntPtr.Add(address, offset), 8), 0);

        public int ReadInt32(IntPtr address, int offset = 0) => BitConverter.ToInt32(ReadBytes(IntPtr.Add(address, offset), 4), 0);

        public short ReadInt16(IntPtr address, int offset = 0) => BitConverter.ToInt16(ReadBytes(IntPtr.Add(address, offset), 2), 0);

        public byte ReadByte(IntPtr address, int offset = 0) => ReadBytes(IntPtr.Add(address, offset), 1)[0];

        public float ReadFloat(IntPtr address, int offset = 0) => BitConverter.ToSingle(ReadBytes(IntPtr.Add(address, offset), 4), 0);

        private void SetupSearchSpace(ProcessModule module)
        {
            var baseAddress = module.BaseAddress;
            Debug.WriteLine($"baseAddress: {baseAddress:X}");
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
            for (var i = 0; i < numSections; i++)
            {
                var sectionName = ReadInt64(sectionCursor);

                // .text
                switch (sectionName)
                {
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

        private List<byte> textSectionBytes;

        private void SetupCopiedSegments()
        {
            Debug.WriteLine("module copy START");

            var buffers = new byte[TextSectionSize];

            // .text section bytes
            ReadProcessMemory(ProcessPtr, TextSectionBase, buffers, (IntPtr)TextSectionSize, out _);
            textSectionBytes = buffers.ToList();

            Debug.WriteLine("copy OK!");
        }

        /// <summary>
        /// Free the memory of the copied module search area on object disposal, if applicable.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Scan for a byte signature in the .text section.
        /// </summary>
        /// <param name="signature">The signature.</param>
        /// <returns>The real offset of the found signature.</returns>
        public IntPtr ScanText(string signature)
        {
            Debug.WriteLine($"Parsing signature {signature}...");
            var bytes = ParseSignature(signature);

            var firstByte = bytes[0];

            var scanRet = IntPtr.Zero;
            Debug.WriteLine("Scanning....");

            var scanSize = textSectionBytes.Count - bytes.Length;
            for (var i = 0; i < scanSize; i++)
            {
                if (firstByte != 0xFFFF)
                    i = textSectionBytes.IndexOf((byte)firstByte, i);

                var found = true;

                for (var j = 1; j < bytes.Length; j++)
                {
                    var isWildCard = bytes[j] == 0xFFFF;
                    var isEqual = bytes[j] == textSectionBytes[j + i];

                    if (isWildCard || isEqual) continue;
                    found = false;
                    break;
                }

                if (!found)
                    continue;
                scanRet = TextSectionBase + i;
                break;
            }

            Debug.WriteLine($@"Finished scanning, {signature} -> scanRet: {scanRet:X}");

            if (scanRet != IntPtr.Zero && bytes[0] == 0xE8)
                return ReadCallSig(scanRet);

            return scanRet;
        }

        private ushort[] ParseSignature(string signature)
        {
            var bytesStr = signature.Split(' ');
            var bytes = new ushort[bytesStr.Length];

            for (var i = 0; i < bytes.Length; i++)
            {
                var str = bytesStr[i];
                if (str.Contains('?'))
                {
                    bytes[i] = 0xFFFF;
                    continue;
                }

                bytes[i] = Convert.ToByte(str, 16);
            }

            return bytes;
        }

        /// <summary>
        /// Helper for ScanText to get the correct address for 
        /// IDA sigs that mark the first CALL location.
        /// </summary>
        /// <param name="SigLocation">The address the CALL sig resolved to.</param>
        /// <returns>The real offset of the signature.</returns>
        private IntPtr ReadCallSig(IntPtr SigLocation)
        {
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
        public IntPtr GetStaticAddressFromSig(string signature, int offset = 0)
        {
            var instrAddr = ScanText(signature);

            if (instrAddr == IntPtr.Zero)
                return instrAddr;

            var disp = ReadInt32(instrAddr + offset);

            var result = instrAddr + disp + sizeof(int) + offset;
            Debug.WriteLine($"GetStaticAddressFromSig. {signature} -> {result:X} (disp: {disp:X})");
            return result;
        }
    }
}