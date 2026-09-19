using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class BanUID : IBanUID
    {
        public uint PlayerUID { get; set; }
        public uint Hours { get; set; }
        public long StartBan { get; set; }
        public string Name { get; set; }
        public string Reason { get; set; }
    }
}
