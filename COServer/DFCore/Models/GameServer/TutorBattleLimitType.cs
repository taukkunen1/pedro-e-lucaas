using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class TutorBattleLimitType : ITutorBattleLimitType
    {
        public uint BP { get; set; }
        public uint SharedBPLimit { get; set; }
    }
}
