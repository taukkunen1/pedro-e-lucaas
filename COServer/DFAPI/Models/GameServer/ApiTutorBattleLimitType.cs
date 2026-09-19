using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("tutorbattlelimittypes")]
    public class ApiTutorBattleLimitType : TutorBattleLimitType
    {
        [Key]
        public uint Id { get; set; }
    }
}
