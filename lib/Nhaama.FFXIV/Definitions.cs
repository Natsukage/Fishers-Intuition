using System;
using System.Net;
using System.Security.Policy;
using Newtonsoft.Json;
using Nhaama.Memory;

namespace Nhaama.FFXIV
{
    public class Definitions
    {
        public Definitions(NhaamaProcess process)
        {
            TerritoryType = new Pointer(process, 0x1AE5A88, 0x5A8);
            Time = new Pointer(process, 0x1A8D3C0);
            Weather = new Pointer(process, 0x1A62448);
        }
        
        [JsonConstructor]
        private Definitions() {}

        public Pointer TerritoryType;
        public Pointer Time;
        public Pointer Weather;

        public ulong ActorTable = 0x1AA97B8;

        public ulong ActorID = 0x74;
        public ulong Name = 0x30;
        public ulong BnpcBase = 0x80;
        public ulong OwnerID = 0x84;
        public ulong ModelChara = 0x16FC;
        public ulong Job = 0x1790;
        public ulong Level = 0x1792;
        public ulong World = 0x174C;
        public ulong HomeWorld = 0x174E;
        public ulong CompanyTag = 0x16AA;
        public ulong Customize = 0x1688;
        public ulong RenderMode = 0x104;
        public ulong ObjectKind = 0x8C;
        public ulong SubKind = 0x8D;

        public ulong Head = 0x15E8;
        public ulong Body = 0x15EC;
        public ulong Hands = 0x15F0;
        public ulong Legs = 0x15F4;
        public ulong Feet = 0x15F8;
        public ulong Ear = 0x15FC;
        public ulong Neck = 0x1600;
        public ulong Wrist = 0x1604;
        public ulong RRing = 0x1608;
        public ulong LRing = 0x160C;

        public ulong MainWep = 0x1342;
        public ulong OffWep = 0x13A8;
        
        private static readonly Uri DefinitionStoreUrl = new Uri("https://raw.githubusercontent.com/goaaats/Nhaama/master/definitions/FFXIV/");
        
        public static Definitions Get(NhaamaProcess p, string version, Game.GameType gameType)
        {
            using (WebClient client = new WebClient())
            {
                var uri = new Uri(DefinitionStoreUrl, $"{gameType.ToString().ToLower()}/{version}.json");

                try
                {
                    var definitionJson = client.DownloadString(uri);
                    var serializer = p.GetSerializer();
                    var deserializedDefinition = serializer.DeserializeObject<Definitions>(definitionJson);

                    return deserializedDefinition;
                }
                catch (WebException exc)
                {
                    throw new Exception("Could not get definitions for version: " + uri, exc);
                }
            }
        }

        public static string GetJson(NhaamaProcess process)
        {
            var serializer = process.GetSerializer();

            return serializer.SerializeObject(new Definitions(process), Newtonsoft.Json.Formatting.Indented);
        }
    }
}