namespace Nhaama.FFXIV.Actor.Model
{
    public struct ActorEntry
    {
        public ulong Offset { get; set; }
        public string Name { get; set; }
        public string CompanyTag { get; set; }
        public uint ActorID { get; set; }
        public uint OwnerID { get; set; }
        public ushort ModelChara { get; set; }
        public uint DataId { get; set; }
        public byte Job { get; set; }
        public byte Level { get; set; }
        public byte World { get; set; }
        public byte HomeWorld { get; set; }
        public ObjectKind ObjectKind { get; set; }
        public byte SubKind { get; set; }
        public ActorAppearance Appearance { get; set; }

        public override string ToString()
        {
            string stringRep;

            switch (ObjectKind)
            {
                case ObjectKind.Player:
                    stringRep = Name;

                    if (CompanyTag.Length >= 1)
                        stringRep += $" <{CompanyTag}>";

                    break;

                default:
                    stringRep = $"{ObjectKind.ToString()}#{DataId} - {SubKind}";

                    if (Name.Length >= 1)
                        stringRep += $" - {Name}";
                    break;
            }

            return stringRep + $"({ActorID:X})";
        }
    }
}