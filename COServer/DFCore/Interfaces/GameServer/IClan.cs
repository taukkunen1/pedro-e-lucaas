namespace Core.Interfaces.GameServer
{
    public interface IClan
    {
        public string ClanID { get; set; }
        public string Name { get; set; }
        public string LeaderName { get; set; }
        public byte Level { get; set; }
        public uint Donation { get; set; }
        public string ClanBulletin { get; set; }
        public byte BP { get; set; }
        public string Allies { get; set; }
        public string Enemies { get; set; }
    }
}
