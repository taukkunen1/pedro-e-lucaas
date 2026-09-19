using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("npcs")]
    public class ApiNPC : NPC
    {
        [Key]
        public uint Id {  get; set; }
    }
}
