using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class KOBoardRank : IKOBoardRank
    {
        public uint PlayerUID { get; set; }
        public string PlayerName { get; set; }
        public uint Points { get; set; }

    }
}
