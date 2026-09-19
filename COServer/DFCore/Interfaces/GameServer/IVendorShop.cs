namespace Core.Interfaces.GameServer
{
    public interface IVendorShop
    {
        public uint UID { get; set; }
        public ushort Mesh { get; set; }
        public string Name { get; set; }
        public ushort Map { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public string ItemsJson { get; set; }
        public VendorActionMode CostType { get; set; }
    }
    public enum VendorActionMode : ushort
    {
        Gold = 1,
        CPs = 3,
        ViewEquip = 4
    }
}
