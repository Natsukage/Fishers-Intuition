namespace Nhaama.Memory.Native
{
    public static class Constants
    {
		// Privileges
		public const int PROCESS_CREATE_THREAD = 0x0002;
		public const int PROCESS_QUERY_INFORMATION = 0x0400;
		public const int PROCESS_VM_OPERATION = 0x0008;
		public const int PROCESS_VM_WRITE = 0x0020;
		public const int PROCESS_VM_READ = 0x0010;

        // Memory
        public const uint MEM_FREE = 0x10000;
		public const uint MEM_COMMIT = 0x00001000;
		public const uint MEM_RESERVE = 0x00002000;
		public const uint MEM_DECOMMIT = 0x00004000;
		public const uint MEM_RELEASE = 0x00008000;

		public const uint PAGE_READWRITE = 0x04;
		public const uint PAGE_WRITECOPY = 0x08;
		public const uint PAGE_EXECUTE_READWRITE = 0x40;
		public const uint PAGE_EXECUTE_WRITECOPY = 0x80;
		public const uint PAGE_EXECUTE = 0x10;
		public const uint PAGE_EXECUTE_READ = 0x20;

		public const uint PAGE_GUARD = 0x100;
		public const uint PAGE_NOACCESS = 0x01;

		public const uint MEM_PRIVATE = 0x20000;
		public const uint MEM_IMAGE = 0x1000000;
        
        internal enum MINIDUMP_TYPE
        {
            MiniDumpNormal = 0x00000000,
            MiniDumpWithDataSegs = 0x00000001,
            MiniDumpWithFullMemory = 0x00000002,
            MiniDumpWithHandleData = 0x00000004,
            MiniDumpFilterMemory = 0x00000008,
            MiniDumpScanMemory = 0x00000010,
            MiniDumpWithUnloadedModules = 0x00000020,
            MiniDumpWithIndirectlyReferencedMemory = 0x00000040,
            MiniDumpFilterModulePaths = 0x00000080,
            MiniDumpWithProcessThreadData = 0x00000100,
            MiniDumpWithPrivateReadWriteMemory = 0x00000200,
            MiniDumpWithoutOptionalData = 0x00000400,
            MiniDumpWithFullMemoryInfo = 0x00000800,
            MiniDumpWithThreadInfo = 0x00001000,
            MiniDumpWithCodeSegs = 0x00002000
        }
    }
}