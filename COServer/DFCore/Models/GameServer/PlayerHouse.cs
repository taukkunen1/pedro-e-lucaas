using Core.Interfaces.GameServer;
using System.Collections.Generic;

namespace Core.Models.GameServer
{
    public class PlayerHouse : IPlayerHouse
    {
        public uint PlayerUID { get; set; }
        public uint Level { get; set; }
        public List<PlayerHouseFurniture> Furnitures { get; set; }
    }
}
