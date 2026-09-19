using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;

namespace API.Models.GameServer
{
    public class ApiVendorShop: VendorShop
    {
        [Key]
        public uint Id { get; set; }
    }
}
