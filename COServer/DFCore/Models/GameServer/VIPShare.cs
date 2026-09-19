using Core.Interfaces.GameServer;
using System;

namespace Core.Models.GameServer
{
    public class VIPShare : IVIPShare
    {
        public uint PlayerUID { get; set; }
        public uint ShareUID { get; set; }
        public string ShareName { get; set; }
        public byte ShareLevel { get; set; }
        public DateTime ShareExpiration { get; set; }
    }
}
