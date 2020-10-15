using GearTuple = System.Tuple<int, int, int>;

namespace Nhaama.FFXIV.Actor.Model
{
    public struct ActorAppearance
    {
        public GearTuple HeadGear { get; set; }
        public GearTuple BodyGear { get; set; }
        public GearTuple HandsGear { get; set; }
        public GearTuple LegsGear { get; set; }
        public GearTuple FeetGear { get; set; }
        public GearTuple EarGear { get; set; }
        public GearTuple NeckGear { get; set; }
        public GearTuple WristGear { get; set; }
        public GearTuple RRingGear { get; set; }
        public GearTuple LRingGear { get; set; }

        public GearTuple MainWep { get; set; }
        public GearTuple OffWep { get; set; }

        public byte[] Customize { get; set; }
    }
}