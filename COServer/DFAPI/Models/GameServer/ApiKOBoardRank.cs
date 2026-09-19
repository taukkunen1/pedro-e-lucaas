using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;

namespace API.Models.GameServer
{
    public class ApiKOBoardRank : KOBoardRank
    {
        [Key]
        public uint Id { get; set; }
    }
}
