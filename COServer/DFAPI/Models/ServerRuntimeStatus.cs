namespace API.Models
{
    public class ServerRuntimeHeartbeat
    {
        public string ServerName { get; set; } = "Placebo";
        public int OnlinePlayers { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public DateTime LastHeartbeatUtc { get; set; }
    }

    public class ServerRuntimeStatus
    {
        public bool ApiOnline { get; set; }
        public bool GameServerOnline { get; set; }
        public string ServerName { get; set; } = "Placebo";
        public int OnlinePlayers { get; set; }
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? LastHeartbeatUtc { get; set; }
        public long UptimeSeconds { get; set; }
    }
}
