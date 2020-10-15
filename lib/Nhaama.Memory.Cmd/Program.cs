using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Nhaama.FFXIV;
using Nhaama.FFXIV.Actor.Model;
using Nhaama.Memory;
using Nhaama.Memory.Serialization;
using Nhaama.Memory.Serialization.Converters;

namespace Nhaama.Memory.Cmd
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var proc = Process.GetProcessesByName("ffxiv_dx11")[0].GetNhaamaProcess();

            /*

            Pointer p = new Pointer(proc, 0x19D55E8, 0x4c);
            Console.WriteLine(p.Address.ToString("X"));
            Console.WriteLine(proc.ReadByte(p));
            
            var serializer = proc.GetSerializer();
            
            var pointerJson = serializer.SerializeObject(p, Formatting.Indented);
            Console.WriteLine(pointerJson);
            
            var p2 = serializer.DeserializeObject<Memory.Pointer>(pointerJson);
            
            Console.WriteLine(p2.Address.ToString("X"));
            
            var p3 = new Pointer(proc, "ffxiv_dx11.exe+19D55E8,4C");
            Console.WriteLine(proc.ReadByte(p3));
            
            
            
            
            var p4 = new Pointer(proc, 0x199DA38 + 8, 0);
            
            Console.WriteLine(p4.Address.ToString("X"));
            Console.WriteLine(proc.ReadString(p4 + 48));
            proc.WriteString(p4.Address + 48, "Test McTest", StringEncodingType.Utf8, true);



            var tp = new Pointer(proc, 0x19815F0, 0x10, 0x8, 0x28, 0x80);
            Console.WriteLine(tp.Address.ToString("X"));
            

            /*
            int time = 0;
            while (true)
            {
                proc.Write(p4, (UInt32) time);
                time++;
                Thread.Sleep(30);
            }
            */
            
            //Console.WriteLine(Definitions.GetJson(proc));

            var handles = proc.GetHandles();
            foreach (var nhaamaHandle in handles)
            {
                Console.WriteLine(nhaamaHandle);
                if(nhaamaHandle.Name.Contains("ffxiv_game0"))
                    nhaamaHandle.Close();
            }

            /*
            FFXIV.Game xivgame = new Game(proc.BaseProcess);
            xivgame.Update();
            
            foreach (var actorEntry in xivgame.ActorTable.Cast<ActorEntry>().Where(x=> x.ObjectKind == ObjectKind.BattleNpc && x.SubKind == 5))
            {
                Console.WriteLine(actorEntry);
            }
            */
        }
    }
}