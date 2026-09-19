using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class ClassPKWar : IClassPKWar
    {
        public byte Type { get; set; }
        public byte Level { get; set; }
        public uint Winner { get; set; }
        public uint LastFlag { get; set; }
    }
}
