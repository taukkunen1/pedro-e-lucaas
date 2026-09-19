namespace Core.Interfaces.GameServer
{
    public interface IBanIP
    {
        public string IP { get; set; }
        public uint Hours { get; set; }
        public long StartBan { get; set; }
    }
}
