using System.Runtime.InteropServices;

namespace Nhaama.Memory.Native.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct OBJECT_NAME_INFORMATION
    {
        public UNICODE_STRING Name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string NameBuffer;
    }
}