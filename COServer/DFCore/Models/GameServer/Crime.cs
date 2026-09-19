using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class Crime : ICrime
    {
        public string OwnerName { get; set; }
        public uint OwnerUID { get; set; }
        public uint Money { get; set; }
    }
}
