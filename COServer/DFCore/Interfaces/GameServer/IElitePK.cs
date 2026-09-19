namespace Core.Interfaces.GameServer
{
    public interface IElitePK
    {
        public byte Tournament { get; set; }
        public byte Rank { get; set; }
        public uint PlayerId { get; set; }
        public string PlayerName { get; set; }
        public uint PlayerMesh { get; set; }
        public byte ClaimReward { get; set; }
        public uint ServerID { get; set; }
    }
}
