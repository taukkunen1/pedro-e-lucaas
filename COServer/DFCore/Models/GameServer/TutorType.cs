using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class TutorType : ITutorType
    {
        public uint Index { get; set; }
        public uint MinLevel { get; set; }
        public uint MaxLevel { get; set; }
        public uint StudentNum { get; set; }
        public uint BattleLevShare { get; set; }
    }
}
