using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class BanIP : IBanIP
    {
        public string IP { get; set; }
        public uint Hours { get; set; }
        public long StartBan { get; set; }
    }
}
