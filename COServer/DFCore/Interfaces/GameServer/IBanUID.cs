namespace Core.Interfaces.GameServer
{
    public interface IBanUID
    {
        public uint PlayerUID { get; set; }
        public uint Hours { get; set; }
        public long StartBan { get; set; }
        public string Name { get; set; }
        public string Reason { get; set; }
    }
}
