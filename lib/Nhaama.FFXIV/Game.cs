using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Nhaama.FFXIV.Actor;
using Nhaama.Memory;

namespace Nhaama.FFXIV
{
    public partial class Game
    {
        public enum GameType
        {
            Dx9,
            Dx11
        }

        public readonly GameType Type;
        public readonly string Version;
        public readonly NhaamaProcess Process;
        public readonly Definitions Definitions;

        public readonly ActorTableCollection ActorTable;

        public int TerritoryType { get; private set; }

        public Game(Process process, bool loadRemote = true)
        {
            Type = process.MainModule.ModuleName.Contains("ffxiv_dx11") ? GameType.Dx11 : GameType.Dx9;
            Process = process.GetNhaamaProcess();

            var gameDirectory = new DirectoryInfo(process.MainModule.FileName);
            Version = File.ReadAllText(Path.Combine(gameDirectory.Parent.FullName, "ffxivgame.ver"));
            
            Definitions = loadRemote ? Definitions.Get(Process, Version, Type) : new Definitions(Process);

            ActorTable = new ActorTableCollection(this);
        }

        /// <summary>
        /// Update game data.
        /// </summary>
        public void Update()
        {
            ActorTable.Update();

            Definitions.TerritoryType.Resolve(Process);
            TerritoryType = Process.ReadUInt16(Definitions.TerritoryType);
        }
    }
}