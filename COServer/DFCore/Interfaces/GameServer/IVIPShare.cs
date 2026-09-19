using System;

namespace Core.Interfaces.GameServer
{
    public interface IVIPShare
    {
        public uint PlayerUID { get; set; }
        public uint ShareUID { get; set; }
        public string ShareName { get; set; }
        public byte ShareLevel { get; set; }
        public DateTime ShareExpiration { get; set; }

    }
}
