namespace ConquerSite.Models
{
    public class Character
    {
        public uint UID { get; set; }
        public ushort Body { get; set; }
        public ushort Face { get; set; }
        public string Name { get; set; }
        public ushort Class { get; set; }
        public ushort Level { get; set; }
        public ushort Reborn { get; set; }
        public uint Map { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public string Spouse { get; set; }
        public uint Money { get; set; }
        public uint ConquerPoints { get; set; }
        public uint PkPoints { get; set; }
        public uint VipLevel { get; set; }
        public uint GuildID { get; set; }
        public uint GuildRank { get; set; }
        public uint DonationNobility { get; set; }
        public uint OnlineMinutes { get; set; }
        public uint RacePoints { get; set; }
    }
}
