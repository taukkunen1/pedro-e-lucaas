namespace Core.Interfaces.GameServer
{
    public interface IKOBoardRank
    {
        public uint PlayerUID { get; set; }
        public string PlayerName { get; set; }
        public uint Points { get; set; }
    }
}
