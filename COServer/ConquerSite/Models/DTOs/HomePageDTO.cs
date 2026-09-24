using System.Collections.Generic;

namespace ConquerSite.Models.DTOs
{
    public class HomePageDTO
    {
        public ServerStatusDTO Status { get; set; } = new();
        public List<UpdatePostDTO> LatestUpdates { get; set; } = new();
    }
}
