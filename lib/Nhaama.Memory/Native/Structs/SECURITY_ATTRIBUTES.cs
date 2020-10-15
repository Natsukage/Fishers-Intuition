using System;

namespace Nhaama.Memory.Native.Structs
{
	public struct SECURITY_ATTRIBUTES
	{
		public ulong length;
		public IntPtr securityDescriptor;
		public bool inheritHandle;
	}
}
