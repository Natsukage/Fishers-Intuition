using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace 渔人的直感.Models
{
    /// <summary>
    /// 数据内容相关
    /// </summary>
    public static class Data
    {
        public const int FishEyesBuffId = 762; // 50; //      鱼眼的Buff ID

        public static List<SpecialWeather> SpecialWeathers = new List<SpecialWeather>();

        private static SigScanner _scanner;
        public static IntPtr StatusPtr;
        public static IntPtr ActorTable;

        //public static IntPtr BuffTablePtr;
        private static IntPtr weatherPtr;
        private static IntPtr territoryTypePtr;
        private static IntPtr conditionPtr;

        private static IntPtr eventFrameworkPtrAddress;
        private static IntPtr eventFrameworkPtr => _scanner.ReadIntPtr(eventFrameworkPtrAddress);
        private static short contentDirectorOffset;
        private static short oceanFishingTimeOffsetOffset;
        private static short oceanFishingCurrentZoneOffset;
        private static short contentTimeLeftOffset;
        private static short contentDirectorTypeOffset;

        public static IntPtr WeatherPtr => _scanner.ReadIntPtr(weatherPtr) + 0x20;

        public static int TerritoryType => (_scanner.ReadInt32(territoryTypePtr));

        public static bool IsGathering => _scanner.ReadByte(conditionPtr, 6) == 1;

        public static float OceanFishingRemainingTime
        {
            get
            {
                var director = GetInstanceContentDirector();
                if (director == IntPtr.Zero)
                    throw new InvalidOperationException("在获取海钓剩余时间时出现错误: 无法获取InstanceContent或当前不在海钓任务里");

                // https://github.com/aers/FFXIVClientStructs/blob/main/FFXIVClientStructs/FFXIV/Client/Game/InstanceContent/InstanceContentOceanFishing.cs#L17C4-L19
                var contentTime = _scanner.ReadFloat(director, contentTimeLeftOffset);
                var oceanFishingTimeOffset = _scanner.ReadInt32(director, oceanFishingTimeOffsetOffset);
                return contentTime - oceanFishingTimeOffset;
            }
        }

        public static byte OceanFishingCurrentZone
        {
            get
            {
                var director = GetInstanceContentDirector();
                if (director == IntPtr.Zero)
                    throw new InvalidOperationException("在获取海域时出现错误: 无法获取InstanceContent或当前不在海钓任务里");
                return _scanner.ReadByte(director, oceanFishingCurrentZoneOffset);
            }
        }

        public static bool IsInOceanFishing => GetInstanceContentDirector() != IntPtr.Zero;

        public static int UiStatusEffects; //UIStatusEffects相对于ActorTable的偏移。此值随着版本更新随时可能发生改变。

        public static void Initialize(SigScanner scanner)
        {
            _scanner = scanner;
            //Status用于获取EventPlay时玩家动作，判断抛竿、咬钩、脱钩动作。
            StatusPtr = scanner.GetStaticAddressFromSig("4C 8D 0D ?? ?? ?? ?? 4C 8B 13", 3);

            //ActorTable用于获取UIStatusEffects地址，追踪玩家身上的buff
            ActorTable = scanner.GetStaticAddressFromSig("48 8D 0D ? ? ? ? E8 ? ? ? ? 44 0F B6 83 ? ? ? ? C6 83", 3);
            //玩家退回标题界面重新登录后，ActorTable地址不变，但是UIStatusEffects对应地址会变，因此不能在初始化时直接解析，只能在每次访问时解析。
            //当处于登录界面时，ActorTable第一位指向的Actor地址为0。可以据此判断当前处于登录界面。
            //Data.BuffTablePtr = scanner.ReadIntPtr(Data.ActorTable) + 6488;

            //Weather用于获取当前天气，判断幻海流、空岛特殊天气触发等。
            weatherPtr = scanner.GetStaticAddressFromSig("48 8D 0D ? ? ? ? 0F 28 DE", 3) + 8;

            territoryTypePtr = scanner.GetStaticAddressFromSig("8B 0D ? ? ? ? E8 ? ? ? ? 48 85 C0 74 ? 0F B7 40", 2);

            conditionPtr = scanner.GetStaticAddressFromSig("48 8D 0D ? ? ? ? 45 33 C0 4C 8B F0", 3);

            //获取EventFrameworkPtr
            eventFrameworkPtrAddress = scanner.GetStaticAddressFromSig("48 83 3D ? ? ? ? ? 8B 9B", 3);
            // eventFrameworkPtrAddress = scanner.GetStaticAddressFromSig("48 83 3D ?? ?? ?? ?? ?? 44 0F B6 F0", 3);
            //获取Offset相关
            
            var contentDirectoryAddress = scanner.ScanText("48 83 B9 ? ? ? ? ? 74 ? B0 ? C3 48 8B 81"); // 国服6.57
            if (contentDirectoryAddress == IntPtr.Zero)
                contentDirectoryAddress = scanner.ScanText("48 83 B9 ?? ?? ?? ?? ?? 75 ?? 48 8B 81"); // 国际服 7.0

            contentDirectorOffset = scanner.ReadInt16(contentDirectoryAddress, 3);

            oceanFishingTimeOffsetOffset = scanner.ReadInt16(scanner.ScanText("48 89 83 ? ? ? ? 88 83 ? ? ? ? 89 83 ? ? ? ? 88 83 ? ? ? ? 48 8B C3"), 3);
            oceanFishingCurrentZoneOffset = scanner.ReadInt16(scanner.ScanText("48 89 83 ? ? ? ? 48 89 83 ? ? ? ? 88 83 ? ? ? ? 89 83 ? ? ? ? 88 83"), 3);

            var contentTimeLeftOffsetAddress = scanner.ScanText("40 53 48 83 EC ? 0F B6 81 ? ? ? ? 48 8B D9 A8 ? 0F 84 ? ? ? ? A8 ? 0F 84") + 0x2a;
            contentTimeLeftOffset = scanner.ReadInt16(contentTimeLeftOffsetAddress, 4);
            Debug.WriteLine($"{contentTimeLeftOffset:X}");
            
            contentDirectorTypeOffset = scanner.ReadInt16(scanner.ScanText("80 B8 ?? ?? ?? ?? ?? 75 ?? 83 FB ?? 73 ?? 0F B7 84 58"), 2);

            var uiStatusEffectstAddress = scanner.ScanText("48 8D 81 ? ? ? ? C3 CC CC CC CC CC CC CC CC 48 89 5C 24 ? 48 89 74 24 ? 57 48 83 EC ? 33 F6 48 8B D9"); // 国服6.57
            if (uiStatusEffectstAddress == IntPtr.Zero)
                uiStatusEffectstAddress = scanner.ScanText("48 8D 81 ? ? ? ? C3 CC CC CC CC CC CC CC CC 48 8B 41"); // 国际服7.0

            UiStatusEffects = scanner.ReadInt32(uiStatusEffectstAddress, 3);

            SpecialWeathers.Add(new SpecialWeather { Id = 145, Name = "幻海流", Duration = 120f });
            if (Properties.Settings.Default.CheckDiademWeather)
            {
                SpecialWeathers.Add(new SpecialWeather { Id = 133, Name = "灵烈火", Duration = 600f });
                SpecialWeathers.Add(new SpecialWeather { Id = 134, Name = "灵飘尘", Duration = 600f });
                SpecialWeathers.Add(new SpecialWeather { Id = 135, Name = "灵飞电", Duration = 600f });
                SpecialWeathers.Add(new SpecialWeather { Id = 136, Name = "灵罡风", Duration = 600f });
            }
            // SpecialWeathers.Add(new SpecialWeather { Id = 1, Name = "碧空（测试）", Duration = 60f });
            // SpecialWeathers.Add(new SpecialWeather { Id = 2, Name = "晴朗（测试）", Duration = 60f });
        }

        //基本上是rebuild了这个函数 E8 ?? ?? ?? ?? 0F B6 98
        private static IntPtr GetInstanceContentDirector()
        {
            //找不到EventFrameworkPtr
            if (eventFrameworkPtr == IntPtr.Zero)
            {
                Debug.WriteLine("Invalid eventFrameworkPtr");
                return IntPtr.Zero;
            }

            var directorPtr = _scanner.ReadIntPtr(eventFrameworkPtr + contentDirectorOffset);
            //找不到ContentDirector
            if (directorPtr == IntPtr.Zero)
            {
                Debug.WriteLine("Invalid directorPtr");
                return IntPtr.Zero;
            }

            //检查Director类型是否为InstanceContent. 下面这个也可以
            // var directorType = _scanner.ReadInt16(_scanner.ReadIntPtr(directorPtr + eventInfoOffset), 2);
            //这个是从FFXivClientStructs里看到的,应该不会变的吧
            var directorType = BitConverter.ToUInt16(_scanner.ReadBytes(directorPtr + 0x22, 2), 0);
            if (directorType != 0x8003)
            {
                Debug.WriteLine("Invalid director type");
                return IntPtr.Zero;
            }

            //检查InstanceContent的类型是否为OceanFishing
            if (_scanner.ReadByte(directorPtr, contentDirectorTypeOffset) != 16)
            {
                Debug.WriteLine("Invalid InstanceContent type");
                return IntPtr.Zero;
            }

            return directorPtr;
        }
    }

    public struct SpecialWeather
    {
        public byte Id;
        public string Name;
        public float Duration;
    }
}
