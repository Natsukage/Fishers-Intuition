using System;

namespace Nhaama.Memory.Native.Enums
{
    [Flags]
    public enum DuplicateOptions : uint
    {
        DUPLICATE_CLOSE_SOURCE = (0x00000001),
        DUPLICATE_SAME_ACCESS = (0x00000002),
    }
}