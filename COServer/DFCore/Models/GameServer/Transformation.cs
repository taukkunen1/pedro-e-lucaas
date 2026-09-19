using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class Transformation : ITransformation
    {
        public ushort SpellID { get; set; }
        public byte Level { get; set; }
        public string Name { get; set; }
        public ushort TransformID { get; set; }
        public ushort HitPoints { get; set; }
    }
}
