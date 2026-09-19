namespace Core.Enums
{
    public class SharedEnums
    {
        public enum MapObjectType : byte
        {
            Player = 0,
            Monster = 1,
            SobNpc = 2,
            StaticRole = 3,
            Item = 4,
            Npc = 5,
            PokerTable = 6,
            Count = 7

        }
        public enum NpcType : ushort
        {
            Stun = 0,
            Shop = 1,
            Talker = 2,
            Beautician = 5,
            Upgrader = 6,
            Socketer = 7,
            Pole = 10,
            Booth = 14,
            Gambling = 19,
            Stake = 21,
            Scarecrow = 22,
            Furniture = 25,
            Gate = 26,
            ClanInfo = 31,
            DialogAndGui = 32
        }
        public enum StaticMesh : ushort
        {
            Vendor = 406,
            LeftGate = 241,
            OpenLeftGate = 251,
            RightGate = 277,
            OpenRightGate = 287,
            Pole = 1137,
            SuperGuildWarPole = 31220
        }
    }
}
