namespace Core.Interfaces.GameServer
{
    public interface ICrime
    {
        public string OwnerName { get; set; }
        public uint OwnerUID { get; set; }
        public uint Money { get; set; }
    }
}
