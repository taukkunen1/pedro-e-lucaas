using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class PlayerProficiency : IPlayerProficiency
    {
        public uint TypeID { get; set; }
        public uint Level { get; set; }
        public byte PreviousLevel { get; set; }
        public uint Experience { get; set; }
        public uint PlayerUid { get; set; }
    }
}
