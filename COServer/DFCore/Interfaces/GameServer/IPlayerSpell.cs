namespace Core.Interfaces.GameServer
{
    public interface IPlayerSpell
    {
        public ushort TypeID { get; set; }
        public ushort Level { get; set; }
        public int Experience { get; set; }
        public byte PreviousLevel { get; set; }
        public byte SoulLevel { get; set; } // Not used in that version 5695 but maybe you want to upgrade and use it ;)
        public byte UseJiangSpell { get; set; } // Not used in that version 5695 but maybe you want to upgrade and use it ;)
        public uint PlayerUid { get; set; }
    }
}
