using System;

namespace 渔人的直感.Models
{
	/// <summary>
	/// 这里面的东西基本上没用了
	/// </summary>
	public static class ExtensionMethods
	{
		public static string AsHex(this ulong s) => string.Format("0x{0:X}", s);
		public static ulong ToUint64(this IntPtr ptr) => (ulong)ptr.ToInt64();
		public static string VersionString(this Version v) => string.Format("v{0}.{1}.{2}", v.Major, v.Minor, v.Build);
	}
}
