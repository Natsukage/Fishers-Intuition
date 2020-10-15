using System;

namespace Nhaama.Memory.Native.Structs
{
    public struct SYSTEM_INFO
    {
        public ushort processorArchitecture;
        private ushort reserved;
        public uint pageSize;
        public IntPtr minimumApplicationAddress;
        public IntPtr maximumApplicationAddress;
        public IntPtr activeProcessorMask;
        public uint numberOfProcessors;
        public uint processorType;
        public uint allocationGranularity;
        public ushort processorLevel;
        public ushort processorRevision;
    }
}