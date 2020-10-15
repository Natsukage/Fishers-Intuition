using System;
using System.Runtime.InteropServices;

namespace Nhaama.Memory.Native.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SYSTEM_HANDLE_INFORMATION
    {
        public int ProcessID;
        public byte ObjectTypeNumber;
        public byte Flags; // 0x01 = PROTECT_FROM_CLOSE, 0x02 = INHERIT
        public ushort Handle;
        public int Object_Pointer;
        public UInt32 GrantedAccess;

        public override string ToString()
        {
            return $"{Handle.ToString("X")}({ObjectTypeNumber}) - ${Object_Pointer.ToString("X")}";
        }
    }
}