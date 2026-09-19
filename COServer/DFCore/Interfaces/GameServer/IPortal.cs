namespace Core.Interfaces.GameServer
{
    public interface IPortal
    {
        public ushort MapID { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public ushort DestinationMapID { get; set; }
        public ushort DestinationX { get; set; }
        public ushort DestinationY { get; set; }
    }
}
