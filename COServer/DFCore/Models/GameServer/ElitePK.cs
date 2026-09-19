using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class ElitePK : IElitePK
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
