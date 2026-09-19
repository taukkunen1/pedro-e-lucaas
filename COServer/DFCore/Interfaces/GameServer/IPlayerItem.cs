namespace Core.Interfaces.GameServer
{
    public interface IPlayerItem
    {
        public uint Uid { get; set; }
        public uint EntityID { get; set; }
        public uint ItemId { get; set; }
        public ushort Durability { get; set; }
        public ushort MaxDurability { get; set; }
        public ushort Position { get; set; }
        public uint SocketProgress { get; set; }
        public ushort SocketOne { get; set; }
        public ushort SocketTwo { get; set; }
        public ushort Effect { get; set; }
        public byte Plus { get; set; }
        public byte Bless { get; set; }
        public byte Bound { get; set; }
        public byte Enchant { get; set; }
        public byte Suspicious { get; set; }
        public byte Locked { get; set; }
        public uint PlusProgress { get; set; }
        public uint Inscribed { get; set; }
        public uint Activate { get; set; }
        public uint TimeLeftInMinutes { get; set; }
        public ushort StackSize { get; set; }
        public uint WarehouseId { get; set; }
        public ushort Color { get; set; }
        public int IDEvent { get; set; }
        public bool Fake { get; set; }
        public int UnlockTimer { get; set; }
        public uint ItemPoints { get; set; }
        public uint RemainingTime { get; set; }
        public uint PurificationItemID { get; set; }
        public uint PurificationLevel { get; set; }
        public uint PurificationDuration { get; set; }
        public long PurificationAddedOn { get; set; }
        public uint EffectID { get; set; }
        public uint EffectLevel { get; set; }
        public uint EffectPercent { get; set; }
        public uint EffectPercent2 { get; set; }
        public uint EffectDuration { get; set; }
        public long EffectAddedOn { get; set; }
        public long Expiration { get; set; }
        public uint DepositeCount { get; set; }
    }
}
