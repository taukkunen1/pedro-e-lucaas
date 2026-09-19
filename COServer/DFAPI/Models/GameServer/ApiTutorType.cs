using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;

namespace API.Models.GameServer
{
    public class ApiTutorType : TutorType
    {
        [Key]
        public uint Id { get; set; }
    }
}
