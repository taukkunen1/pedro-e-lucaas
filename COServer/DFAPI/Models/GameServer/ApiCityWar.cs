using Core.Models.GameServer;
using System.ComponentModel.DataAnnotations;

namespace API.Models.GameServer
{
    public class ApiCityWar : CityWar
    {
        [Key]
        public uint Id { get; set; }
    }
}
