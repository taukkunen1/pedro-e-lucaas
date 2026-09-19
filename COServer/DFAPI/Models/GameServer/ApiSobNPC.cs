using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("sobnpcs")]
    public class ApiSobNPC : SobNPC
    {
        [Key]
        public uint Id {  get; set; }
    }
}
