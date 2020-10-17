using System;
using System.Collections.Generic;

namespace 渔人的直感.Models
{
    /// <summary>
    /// 数据内容相关
    /// </summary>
    public static class Data
    {
        
        public const int FishEyesBuffId = 762; //50; //       鱼眼的Buff ID

        public static List<SpecialWeather> SpecialWeathers = new List<SpecialWeather>();

        public static IntPtr StatusPtr;
        public static IntPtr ActorTable;
        //public static IntPtr BuffTablePtr;
        public static IntPtr WeatherPtr;

        public const int UiStatusEffects = 6488; //UIStatusEffects相对于ActorTable的偏移。此值随着版本更新随时可能发生改变。

        public static void Initialize(SigScanner scanner)
        {
            //Status用于获取EventPlay时玩家动作，判断抛竿、咬钩、脱钩动作。
            //技术力不足，没法自动查找抛竿动作的地址。只能在每次更新后手动查找。Todo：找到方法自动查找Status地址。
            //手动查找教程:https://github.com/Natsukage/Fishers-Intuition/blob/master/how-to-find-offset.md
            //StatusPtr = scanner.GetStaticAddressFromSig("48 8B 0D ? ? ? ? 48 89 44 24 ? E8 ? ? ? ? B0 ?") + 0x8;
            int.TryParse(Properties.Settings.Default.Offset, System.Globalization.NumberStyles.HexNumber, null,
                out var offset);
            StatusPtr = scanner.Module.BaseAddress + offset;

            //ActorTable用于获取UIStatusEffects地址，追踪玩家身上的buff
            ActorTable = scanner.GetStaticAddressFromSig("88 91 ?? ?? ?? ?? 48 8D 3D ?? ?? ?? ??");
            //玩家退回标题界面重新登录后，ActorTable地址不变，但是UIStatusEffects对应地址会变，因此不能在初始化时直接解析，只能在每次访问时解析。
            //当处于登录界面时，ActorTable第一位指向的Actor地址为0。可以据此判断当前处于登录界面。
            //Data.BuffTablePtr = scanner.ReadIntPtr(Data.ActorTable) + 6488;

            //Weather用于获取当前天气，判断幻海流、空岛特殊天气触发等。
            WeatherPtr = scanner.GetStaticAddressFromSig("48 8D 0D ? ? ? ? E8 ? ? ? ? 0F B6 C8 E8 ? ? ? ? 45 33 ED") +
                         0x64;

            SpecialWeathers.Add(new SpecialWeather {Id = 145, Name = "幻海流", Duration = 120f});
            SpecialWeathers.Add(new SpecialWeather {Id = 133, Name = "灵烈火", Duration = 600f});
            SpecialWeathers.Add(new SpecialWeather {Id = 134, Name = "灵飘尘", Duration = 600f});
            SpecialWeathers.Add(new SpecialWeather {Id = 135, Name = "灵飞电", Duration = 600f});
            SpecialWeathers.Add(new SpecialWeather {Id = 136, Name = "灵罡风", Duration = 600f});
            //SpecialWeathers.Add(new SpecialWeather { Id = 1, Name = "碧空（测试）", Duration = 60f });
            //SpecialWeathers.Add(new SpecialWeather { Id = 2, Name = "晴朗（测试）", Duration = 60f });
        }
    }

    public struct SpecialWeather
    {
        public byte Id;
        public string Name;
        public float Duration;
    }
}