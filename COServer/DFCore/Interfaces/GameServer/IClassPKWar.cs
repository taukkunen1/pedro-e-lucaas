namespace Core.Interfaces.GameServer
{
    public interface IClassPKWar
    {
        public byte Type { get; set; }
        public byte Level { get; set; }
        public uint Winner { get; set; }
        public uint LastFlag { get; set; }
    }
}
