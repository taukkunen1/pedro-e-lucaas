using API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace API.Controllers
{
    [Route("api/status")]
    [ApiController]
    public class ServerStatusController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, ServerRuntimeHeartbeat> Heartbeats =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly TimeSpan HeartbeatTimeout = TimeSpan.FromMinutes(3);

        [HttpPost("heartbeat")]
        public IActionResult Heartbeat([FromBody] ServerRuntimeHeartbeat heartbeat)
        {
            if (heartbeat == null || string.IsNullOrWhiteSpace(heartbeat.ServerName))
                return BadRequest();

            heartbeat.ServerName = heartbeat.ServerName.Trim();
            heartbeat.OnlinePlayers = Math.Max(0, heartbeat.OnlinePlayers);
            heartbeat.LastHeartbeatUtc = DateTime.UtcNow;

            Heartbeats[heartbeat.ServerName] = heartbeat;
            return NoContent();
        }

        [HttpGet]
        public ActionResult<ServerRuntimeStatus> GetStatus(string serverName = null)
        {
            ServerRuntimeHeartbeat heartbeat = null;

            if (!string.IsNullOrWhiteSpace(serverName))
                Heartbeats.TryGetValue(serverName.Trim(), out heartbeat);
            else if (!Heartbeats.IsEmpty)
                heartbeat = Heartbeats.Values.OrderByDescending(x => x.LastHeartbeatUtc).FirstOrDefault();

            var now = DateTime.UtcNow;
            var fresh = heartbeat != null && now - heartbeat.LastHeartbeatUtc <= HeartbeatTimeout;

            return new ServerRuntimeStatus
            {
                ApiOnline = true,
                GameServerOnline = fresh,
                ServerName = heartbeat?.ServerName ?? serverName ?? "Placebo",
                OnlinePlayers = fresh ? heartbeat.OnlinePlayers : 0,
                StartedAtUtc = fresh ? heartbeat.StartedAtUtc : null,
                LastHeartbeatUtc = heartbeat?.LastHeartbeatUtc,
                UptimeSeconds = fresh && heartbeat.StartedAtUtc > DateTime.MinValue
                    ? Math.Max(0, (long)(now - heartbeat.StartedAtUtc).TotalSeconds)
                    : 0
            };
        }
    }
}
