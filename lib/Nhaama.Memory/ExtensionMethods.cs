using System;
using System.Diagnostics;
using System.Linq;

namespace Nhaama.Memory
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Get this Process as a Nhaama-wrapped Process.
        /// </summary>
        /// <returns>Nhaama-wrapped Process.</returns>
        public static NhaamaProcess GetNhaamaProcess(this Process process) => new NhaamaProcess(process);
    }
}