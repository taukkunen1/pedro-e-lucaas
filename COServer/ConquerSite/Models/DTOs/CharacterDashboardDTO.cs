using ConquerSite.Models;

namespace ConquerSite.Models.DTOs
{
    public class CharacterDashboardDTO
    {
        public bool Available { get; set; }
        public Character Character { get; set; }
        public string ClassName { get; set; } = "Unknown";
        public string AvatarUrl { get; set; } = "/images/PlayerFace/296.png";
        public string MaskedUid { get; set; } = "****";
        public string PkStatus { get; set; } = "Normal";
        public string OnlineTimeLabel { get; set; } = "0h";
        public bool AutoJumpUnlocked { get; set; }
        public bool AutoPickupUnlocked { get; set; }
    }
}
