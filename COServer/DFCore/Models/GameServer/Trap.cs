using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class Trap : ITrap
    {
        public uint TrapID { get; set; }
        public uint Map { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public bool AllowDinamic{ get; set; }
    }
}
