using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.GameServer
{
    [Table("furnitures")]
    public class ApiFurniture : Furniture
    {
        [Key]
        public uint Id { get; set; }
    }
}
