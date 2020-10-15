using System;
using System.Runtime.InteropServices;
using Nhaama.Memory.Native.Enums;

namespace Nhaama.Memory.Native
{
    public static class NtDll
    {
        [DllImport("ntdll.dll")]
        public static extern NtStatus NtQueryObject(IntPtr objectHandle, OBJECT_INFORMATION_CLASS informationClass, IntPtr informationPtr, uint informationLength, ref uint returnLength);

        public static IntPtr NtQueryObject(IntPtr handle, OBJECT_INFORMATION_CLASS infoClass, uint infoLength = 0)
        {
            if (infoLength == 0)
                infoLength = (uint)Marshal.SizeOf(typeof(uint));

            IntPtr infoPtr = Marshal.AllocHGlobal((int)infoLength);
            int tries = 0;
            NtStatus result;

            while (true)
            {
                result = NtQueryObject(handle, infoClass, infoPtr, infoLength, ref infoLength);

                if (result == NtStatus.InfoLengthMismatch || result == NtStatus.BufferOverflow || result == NtStatus.BufferTooSmall)
                {
                    Marshal.FreeHGlobal(infoPtr);
                    infoPtr = Marshal.AllocHGlobal((int)infoLength);
                    tries++;
                    continue;
                }
                else if (result == NtStatus.Success || tries > 5)
                    break;
                else
                {
                    //throw new Exception("Unhandled NtStatus " + result);
                    break;
                }
            }

            if (result == NtStatus.Success)
                return infoPtr; // Don't forget to free the pointer with Marshal.FreeHGlobal after you're done with it
            else
                Marshal.FreeHGlobal(infoPtr); // Free pointer when not Successful

            return IntPtr.Zero;
        }
        
        [DllImport("ntdll.dll")]
        public static extern NtStatus NtQuerySystemInformation(
            SYSTEM_INFORMATION_CLASS systemInformationClass,
            IntPtr systemInformation,
            int systemInformationLength,
            ref int returnLength);
        
        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern NtStatus NtDuplicateObject(IntPtr hSourceProcessHandle,
            IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle,
            uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, DuplicateOptions dwOptions);
        
        [DllImport("ntdll.dll", ExactSpelling=true, SetLastError=false)]
        public static extern NtStatus NtClose(IntPtr hObject);
    }
}