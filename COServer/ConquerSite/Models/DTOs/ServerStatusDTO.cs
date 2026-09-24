using System;

namespace ConquerSite.Models.DTOs
{
    public class ServerStatusDTO
    {
        public bool ApiOnline { get; set; }
        public bool GameServerOnline { get; set; }
        public string ServerName { get; set; } = "Placebo";
        public int OnlinePlayers { get; set; }
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? LastHeartbeatUtc { get; set; }
        public long UptimeSeconds { get; set; }
        public string ClientPatch { get; set; } = "-";

        public string UptimeLabel
        {
            get
            {
                if (!GameServerOnline || UptimeSeconds <= 0)
                    return "Unavailable";

                var uptime = TimeSpan.FromSeconds(UptimeSeconds);
                if (uptime.TotalDays >= 1)
                    return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
                if (uptime.TotalHours >= 1)
                    return $"{uptime.Hours}h {uptime.Minutes}m";
                return $"{uptime.Minutes}m";
            }
        }
    }
}
