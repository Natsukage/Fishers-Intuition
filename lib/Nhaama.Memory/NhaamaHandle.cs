using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Nhaama.Memory.Native;
using Nhaama.Memory.Native.Enums;

namespace Nhaama.Memory
{
    public class NhaamaHandle
    {
        private IntPtr _handle;
        private NhaamaProcess _process;
        
        public NhaamaHandle(IntPtr handle, NhaamaProcess process)
        {
            _handle = handle;
            _process = process;
            
            IntPtr dupHandle = IntPtr.Zero;

            var status = NtDll.NtDuplicateObject(process.BaseProcess.Handle, handle, Process.GetCurrentProcess().Handle, out dupHandle, 0, false, DuplicateOptions.DUPLICATE_SAME_ACCESS);
            
            if(status != NtStatus.Success)
                throw new Exception($"Could not duplicate handle. (NtStatus:{status.ToString()})");

            var objectNameInformationPtr = NtDll.NtQueryObject(dupHandle, OBJECT_INFORMATION_CLASS.ObjectNameInformation);

            if (objectNameInformationPtr == IntPtr.Zero)
                return;
                
            var objInfo = Marshal.PtrToStructure<Native.Structs.OBJECT_NAME_INFORMATION>(objectNameInformationPtr);
            if(objInfo.Name.ToString() != null)
                Name = objInfo.Name.ToString();
            
            Marshal.FreeHGlobal(objectNameInformationPtr);
            NtDll.NtClose(dupHandle);
        }

        public bool IsAlive => true;
        
        public string TypeName { get; private set; }
        public string Name { get; private set; } = "Unknown";

        public void Close()
        {
            //IntPtr hProcess = OpenProcess(ProcessAccessFlags.DupHandle, false, pid);
            IntPtr dupHandle = IntPtr.Zero;
            
            var status = NtDll.NtDuplicateObject(_process.BaseProcess.Handle, _handle, IntPtr.Zero, out dupHandle, 0, false, DuplicateOptions.DUPLICATE_CLOSE_SOURCE);
            NtDll.NtClose(dupHandle);
            
            if(status != NtStatus.Success)
                throw new Exception($"Could not close handle. (NtStatus:{status.ToString()})");
        }

        public override string ToString()
        {
            return $"{Name}({_handle.ToString("X")})";
        }
    }
}