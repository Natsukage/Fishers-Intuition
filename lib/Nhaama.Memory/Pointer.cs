using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Nhaama.Memory.Native;

namespace Nhaama.Memory
{
    public unsafe class Pointer
    {
        public ulong Address = 0;

        public readonly ulong[] PointerPath;
        public readonly ProcessModule Module;

        private Pointer()
        {
        }

        /// <summary>
        /// Resolve a Pointer starting from the Process Base Address.
        /// </summary>
        /// <param name="process">The Process to act on.</param>
        /// <param name="pointerPath">The offsets needed to resolve the pointer.</param>
        public Pointer(NhaamaProcess process, params ulong[] pointerPath) : this(process,
            process.BaseProcess.MainModule, pointerPath)
        {
        }

        public Pointer(NhaamaProcess process, string code)
        {
            var parts = code.Split('+');
            Module = process.BaseProcess.Modules.Cast<ProcessModule>().First(x => x.ModuleName == parts[0]);

            var path = parts[1].Split(',');
            PointerPath = new ulong[path.Length];
            for (int i = 0; i < path.Length; i++)
            {
                PointerPath[i] = ulong.Parse(path[i], NumberStyles.HexNumber);
            }
            
            Resolve(process);
        }

        /// <summary>
        /// Resolve a Pointer starting from the specified Module.
        /// </summary>
        /// <param name="process">The Process to act on.</param>
        /// /// <param name="module">The Module to start from.</param>
        /// <param name="pointerPath">The offsets needed to resolve the pointer.</param>
        /// <exception cref="ArgumentNullException">Gets thrown when the pointer path is null.</exception>
        /// <exception cref="ArgumentException">Gets thrown when the pointer path is empty.</exception>
        public Pointer(NhaamaProcess process, ProcessModule module, params ulong[] pointerPath)
        {
            if (pointerPath == null)
                throw new ArgumentNullException("Pointer path must not be null.");

            if (pointerPath.Length == 0)
                throw new ArgumentException("Pointer path must have at least one step.");

            PointerPath = pointerPath;
            Module = module;

            Resolve(process);
        }

        /// <summary>
        /// Resolve a pointer for the specified Process based on prior set values.
        /// </summary>
        /// <param name="process">The Process to act on.</param>
        public void Resolve(NhaamaProcess process)
        {
            var currentAddress =
                process.ReadUInt64((ulong) Module.BaseAddress + PointerPath[0]);

            Address = 0;

            for (int i = 1; i < PointerPath.Length; i++)
            {
                Address = currentAddress + PointerPath[i];

                currentAddress =
                    process.ReadUInt64(Address);
            }
        }
        
        public static implicit operator ulong(Pointer p)  {  return p.Address;  }
    }
}