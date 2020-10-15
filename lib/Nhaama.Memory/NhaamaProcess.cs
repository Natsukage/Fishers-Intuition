using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Nhaama.Memory.Native;
using Nhaama.Memory.Native.Enums;
using Nhaama.Memory.Native.Structs;

namespace Nhaama.Memory
{
    public class NhaamaProcess
    {
        public readonly Process BaseProcess;

        /// <summary>
        /// Creates a new NhaamaProcess from a process.
        /// </summary>
        /// <param name="process">The Process to wrap.</param>
        /// <exception cref="Exception">Gets thrown, when the Process is inaccessible.</exception>
        public NhaamaProcess(Process process)
        {
            this.BaseProcess = process;

            // Check if we can access the process
            if (!Environment.Is64BitProcess && Is64BitProcess())
                throw new Exception(
                    "Cannot access 64bit process from within 32bit process - please build Nhaama and your app as 64bit.");
        }

        #region Readers

        /// <summary>
        /// Read a byte from the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <returns></returns>
        public byte ReadByte(ulong offset)
        {
            return ReadBytes(offset, 1)[0];
        }

        /// <summary>
        /// Read a byte array from the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <param name="length">Length to read.</param>
        /// <returns>Read byte array.</returns>
        public byte[] ReadBytes(ulong offset, uint length)
        {
            var bytes = new byte[length];
            Kernel32.ReadProcessMemory(BaseProcess.Handle,
                new UIntPtr(offset), bytes, new UIntPtr(length), IntPtr.Zero);

            return bytes;
        }

        /// <summary>
        /// Read a UInt64 from the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <returns>Read UInt64.</returns>
        public ulong ReadUInt64(ulong offset) => BitConverter.ToUInt64(ReadBytes(offset, 8), 0);
        
        /// <summary>
        /// Read a UInt32 from the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <returns>Read UInt32.</returns>
        public uint ReadUInt32(ulong offset) => BitConverter.ToUInt32(ReadBytes(offset, 4), 0);
        
        /// <summary>
        /// Read a UInt16 from the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <returns>Read UInt16.</returns>
        public ushort ReadUInt16(ulong offset) => BitConverter.ToUInt16(ReadBytes(offset, 2), 0);

		/// <summary>
		/// Read a float from the specified offset.
		/// </summary>
		/// <param name="offset">Offset to read from.</param>
		/// <returns>Read Float.</returns>
		public float ReadFloat(ulong offset) => BitConverter.ToSingle(ReadBytes(offset, 4), 0);

        /// <summary>
        /// Read a string from the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <param name="encodingType">Encoding, default: UTF-8</param>
        /// <returns>Read string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Gets thrown when an unknown string encoding is provided.</exception>
        public string ReadString(ulong offset, StringEncodingType encodingType = StringEncodingType.Utf8)
        {
            var bytes = new List<byte>();

            do
            {
                bytes.Add(ReadByte(offset));
                offset++;
            } while (bytes[bytes.Count - 1] != 0x0);

            bytes = bytes.Take(bytes.Count - 1).ToList();
            
            switch (encodingType)
            {
                case StringEncodingType.ASCII:
                    return Encoding.UTF8.GetString(bytes.ToArray());
                case StringEncodingType.Unicode:
                    return Encoding.UTF8.GetString(bytes.ToArray());
                case StringEncodingType.Utf8:
                    return Encoding.UTF8.GetString(bytes.ToArray());
                default:
                    throw new ArgumentOutOfRangeException(nameof(encodingType), encodingType, null);
            }
            
        }

        #endregion

        #region Writers

        /// <summary>
        /// Write a value to the specified offset, determined by type.
        /// </summary>
        /// <param name="offset">Offset to write to.</param>
        /// <param name="data">Value to write.</param>
        /// <exception cref="ArgumentException">Gets thrown, when the type to write is unsupported.</exception>
        public void Write(ulong offset, object data)
        {
            var @writeMethods = new Dictionary<Type, Action>
            {
                {typeof(byte[]), () => WriteBytes(offset, (byte[]) data)},
                {typeof(byte), () => WriteBytes(offset, new byte[] {(byte) data})},
                
                {typeof(char), () => WriteBytes(offset, new byte[] {(byte) data})},
                {typeof(short), () => WriteBytes(offset, BitConverter.GetBytes((short) data))},
                {typeof(ushort), () => WriteBytes(offset, BitConverter.GetBytes((ushort) data))},
                {typeof(int), () => WriteBytes(offset, BitConverter.GetBytes((int) data))},
                {typeof(uint), () => WriteBytes(offset, BitConverter.GetBytes((uint) data))},
                {typeof(long), () => WriteBytes(offset, BitConverter.GetBytes((long) data))},
                {typeof(ulong), () => WriteBytes(offset, BitConverter.GetBytes((ulong) data))},
                {typeof(float), () => WriteBytes(offset, BitConverter.GetBytes((float) data))},
                {typeof(double), () => WriteBytes(offset, BitConverter.GetBytes((double) data))},
            };

            if (@writeMethods.ContainsKey(data.GetType()))
                @writeMethods[data.GetType()]();
            else
                throw new ArgumentException("Unsupported type.");
        }

        /// <summary>
        /// Write a string to the specified offset.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="data"></param>
        /// <param name="encodingType"></param>
        /// <param name="zeroTerminated"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void WriteString(ulong offset, string data, StringEncodingType encodingType = StringEncodingType.Utf8,
            bool zeroTerminated = true)
        {
            if (zeroTerminated)
                data += "\0";
            
            byte[] stringBytes;
            switch (encodingType)
            {
                case StringEncodingType.ASCII:
                    stringBytes = Encoding.ASCII.GetBytes(data);
                    break;
                case StringEncodingType.Unicode:
                    stringBytes = Encoding.Unicode.GetBytes(data);
                    break;
                case StringEncodingType.Utf8:
                    stringBytes = Encoding.UTF8.GetBytes(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(encodingType), encodingType, null);
            }
            
            WriteBytes(offset, stringBytes);
        }

        /// <summary>
        /// Write a byte array to the specified offset.
        /// </summary>
        /// <param name="offset">Offset to write to.</param>
        /// <param name="data">Value to write.</param>
        public void WriteBytes(ulong offset, byte[] data)
        {
            Kernel32.WriteProcessMemory(BaseProcess.Handle, new UIntPtr(offset), data, new UIntPtr((uint) data.Length), IntPtr.Zero);
        }

		#endregion

		#region Assembler

		// array of memory allocations
		private List<MemoryAlloc> memoryAllocs = new List<MemoryAlloc>();

		/// <summary>
		/// Memory allocated
		/// </summary>
		struct MemoryAlloc
		{
			public ulong allocateNearThisAddress;
			public ulong address;
			public ulong pointer;
			public ulong size;
			public ulong SizeLeft => size - (pointer - address);
			public uint lastProtection;
		}

		/// <summary>
		/// Allocates memory within the current process.
		/// </summary>
		/// <param name="size">Size of memory to allocate.</param>
		/// <returns>Address of memory allocated.</returns>
		public ulong Alloc(uint size)
		{
			return Alloc(size, 0);
		}

		/// <summary>
		/// Allocates memory within the current process.
		/// </summary>
		/// <param name="size">Size of memory to allocate.</param>
		/// <param name="allocateNearThisAddress">Position in memory to allocate near.</param>
		/// <returns>Address of memory allocated.</returns>
		public ulong Alloc(uint size, ulong allocateNearThisAddress)
		{
			// retrieve system info
			var systemInfo = Kernel32.GetSystemInfo();

			// check alloc array for existing addresses near the address supplied
			try
			{
				// check for existing alloc near this address
				var i = memoryAllocs.Select((alloc, index) => new { alloc, index })
									.Where(pair => pair.alloc.allocateNearThisAddress == allocateNearThisAddress)
									.Select(pair => pair.index).First();

				// get the alloc from the array
				var found = memoryAllocs[i];
				// is there enough room
				if (found.SizeLeft >= size)
				{
					var ret = found.pointer;
					found.pointer += size;
					memoryAllocs[i] = found;
					return ret;
				}
			}
			catch (InvalidOperationException) {}
			
			// find a free block for memory
			var addr = FindFreeBlockForRegion(allocateNearThisAddress, size);

			// get information about this address
			Kernel32.VirtualQueryEx(BaseProcess.Handle, new IntPtr((long)addr), out MEMORY_BASIC_INFORMATION mbi, true);

			// create new alloc in array
			memoryAllocs.Add(new MemoryAlloc
			{
				address = addr,
				allocateNearThisAddress = allocateNearThisAddress,
				pointer = addr + size,
				size = systemInfo.pageSize,
				lastProtection = mbi.Protect
			});

			// allocate the memory
			if (Kernel32.VirtualAllocEx(BaseProcess.Handle, new IntPtr((long)addr), size, Constants.MEM_RESERVE | Constants.MEM_COMMIT, Constants.PAGE_EXECUTE_READWRITE) == null)
				throw new Exception("Couldn't allocate memory at " + addr);

			// return found address
			return addr;
		}

		/// <summary>
		/// Deallocate memory at a specific address.
		/// </summary>
		/// <param name="address">Starting address.</param>
		public void Dealloc(ulong address)
		{
			// deallocates an entire block of memory not exactly what we want, using try catch to avoid issues with deallocating areas we already have deallocated
			try
			{
				if (!Kernel32.VirtualFreeEx(BaseProcess.Handle, new IntPtr((long)address), UIntPtr.Zero, Constants.MEM_RELEASE))
					throw new Exception("Memory could not be freed at " + address);
			}
			catch (Exception)
			{
				// ignored
			}
		}

		/// <summary>
		/// Finds a free block of memory
		/// </summary>
		/// <param name="base">Base address to look to allocate memory from</param>
		/// <param name="size">How much memory needs to be allocated</param>
		/// <returns>Address to the beginning of block of memory to allocate from.</returns>
		private ulong FindFreeBlockForRegion(ulong @base, uint size)
		{
			// initialize minimum and maximum address space relative to the base address
			// maximum JMP instruction for 64-bit is a relative JMP using the RIP register
			// jump to offset of 32-bit value, max being 7FFFFFFF
			// cheat engine slices off the Fs to give just 70000000 for unknown reasons
			var minAddress = @base - 0x70000000; // 0x10000 (32-bit)
			var maxAddress = @base + 0x70000000; // 0xfffffffff (32-bit)

			// retrieve system info
			var systemInfo = Kernel32.GetSystemInfo();

			// keep min and max values within the system range for a given application
			if (minAddress < (ulong)systemInfo.minimumApplicationAddress.ToInt64())
				minAddress = (ulong)systemInfo.minimumApplicationAddress.ToInt64();
			if (maxAddress > (ulong)systemInfo.maximumApplicationAddress.ToInt64())
				maxAddress = (ulong)systemInfo.maximumApplicationAddress.ToInt64();

			// address for the current loop
			ulong addr = minAddress;
			// address from the last loop
			ulong oldAddr = 0;
			// current result to be passed back from function
			ulong result = 0;

			// query information about pages in virtual address space into mbi
			while (Kernel32.VirtualQueryEx(BaseProcess.Handle, new IntPtr((long)addr), out MEMORY_BASIC_INFORMATION mbi, true) != 0)
			{
				// the base address is past the max address
				if ((ulong)mbi.BaseAddress.ToInt64() > maxAddress)
					return 0; // throw new Exception("Base address is greater than max address.");

				// check if the state is free to allocate and the region size allocated is enough to fit our requested size
				if (mbi.State == Constants.MEM_FREE && mbi.RegionSize > size)
				{
					// set address to the current base address
					ulong nAddr = (ulong)mbi.BaseAddress.ToInt64();
					// get potential offset from granuarltiy alignment
					var offset = systemInfo.allocationGranularity - (nAddr % systemInfo.allocationGranularity);

					// checks base address if it's on the edge of the allocation granularity (page)
					if (mbi.BaseAddress.ToInt64() % systemInfo.allocationGranularity > 0)
					{
						if ((ulong)mbi.RegionSize - offset >= size)
						{
							// increase by potential offset
							nAddr += offset;

							// address is under base address
							if (nAddr < @base)
							{
								// move into the region
								nAddr += (ulong)mbi.RegionSize - offset - size;
								// prevent overflow past base address
								if (nAddr > @base)
									nAddr = @base;
								// align to page
								nAddr -= nAddr % systemInfo.allocationGranularity;
							}

							// new address is less than the one found last loop
							if (Math.Abs((long)(nAddr - @base)) < Math.Abs((long)(result - @base)))
								result = nAddr;
						}
					}
					else
					{
						// address is under base address
						if (nAddr < @base)
						{
							// move into the region
							nAddr += (ulong)mbi.RegionSize - size;
							// prevent overflow past base address
							if (nAddr > @base)
								nAddr = @base;
							// align to page
							nAddr -= nAddr % systemInfo.allocationGranularity;
						}

						// new address is less than the one found last loop
						if (Math.Abs((long)(nAddr - @base)) < Math.Abs((long)(result - @base)))
							result = nAddr;
					}
				}

				// region size isn't aligned with allocation granularity increase by difference 
				if (mbi.RegionSize % systemInfo.allocationGranularity > 0)
					mbi.RegionSize += systemInfo.allocationGranularity - (mbi.RegionSize % systemInfo.allocationGranularity);

				// set old address
				oldAddr = addr;
				// increase address to the next region from our base address
				addr = (ulong)mbi.BaseAddress + (ulong)mbi.RegionSize;

				// address goes over max size or overflow
				if (addr > maxAddress || oldAddr > addr)
					return result;
			}

			return result; // maybe not a good idea not sure
		}

		#endregion
		
		#region Handles

		public NhaamaHandle[] GetHandles()
		{
			var handles = new List<NhaamaHandle>();
			
			var nHandleInfoSize = 0x10000;
			var ipHandlePointer = Marshal.AllocHGlobal(nHandleInfoSize);
			var nLength = 0;
			IntPtr ipHandle;

			while ((NtDll.NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemHandleInformation, ipHandlePointer, nHandleInfoSize, ref nLength)) == NtStatus.InfoLengthMismatch)
			{
				nHandleInfoSize = nLength;
				Marshal.FreeHGlobal(ipHandlePointer);
				ipHandlePointer = Marshal.AllocHGlobal(nLength);
			}

			byte[] baTemp = new byte[nLength];
			Marshal.Copy(ipHandlePointer, baTemp, 0, nLength);

			long lHandleCount;
			if (Is64BitProcess())
			{
				lHandleCount = Marshal.ReadInt64(ipHandlePointer);
				ipHandle = new IntPtr(ipHandlePointer.ToInt64() + 8);
			}
			else
			{
				lHandleCount = Marshal.ReadInt32(ipHandlePointer);
				ipHandle = new IntPtr(ipHandlePointer.ToInt32() + 4);
			}

			SYSTEM_HANDLE_INFORMATION shHandle;

			List<SYSTEM_HANDLE_INFORMATION> test = new List<SYSTEM_HANDLE_INFORMATION>();
			
			for (long lIndex = 0; lIndex < lHandleCount; lIndex++)
			{
				shHandle = new SYSTEM_HANDLE_INFORMATION();
				if (Is64BitProcess())
				{
					shHandle = (SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(ipHandle, shHandle.GetType());
					ipHandle = new IntPtr(ipHandle.ToInt64() + Marshal.SizeOf(shHandle) + 8);
				}
				else
				{
					ipHandle = new IntPtr(ipHandle.ToInt64() + Marshal.SizeOf(shHandle));
					shHandle = (SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(ipHandle, shHandle.GetType());
				}
				if (shHandle.ProcessID != BaseProcess.Id) 
					continue;

				try
				{
					test.Add(shHandle);
				}
				catch
				{
					// ignored
				}
			}

			foreach (var systemHandleInformation in test)
			{
				try
				{
					handles.Add(new NhaamaHandle(new IntPtr(systemHandleInformation.Handle), this));
				}
				catch
				{
					//ignored
				}

			}
			
			return handles.ToArray();
		}
		
		#endregion

		#region Miscellaneous

		public IntPtr CreateRemoteThread(IntPtr address, out IntPtr threadId)
		{
			var ret = Kernel32.CreateRemoteThread(BaseProcess.Handle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out IntPtr thread);
            threadId = thread;
            return ret;
		}

		/// <summary>
		/// Method for checking if the Process is running as 64bit.
		/// </summary>
		/// <returns>Returns true, if the Process is running as 64bit.</returns>
		public bool Is64BitProcess()
        {
            return Environment.Is64BitOperatingSystem && Kernel32.IsWow64Process(BaseProcess.Handle, out var ret) &&
                   !ret;
        }


        public ulong GetModuleBasedOffset(string moduleName, ulong offset) =>
            (ulong)BaseProcess.Modules.Cast<ProcessModule>().First(x => x.ModuleName == moduleName).BaseAddress.ToInt64() + offset;

        #endregion
    }
}