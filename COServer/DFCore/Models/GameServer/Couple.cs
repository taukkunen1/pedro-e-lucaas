using Core.Interfaces.GameServer;

namespace Core.Models.GameServer
{
    public class Couple : ICouple
    {
        public string Winner1 { get; set; } = string.Empty;
        public string Winner2 { get; set; } = string.Empty;
    }
}
