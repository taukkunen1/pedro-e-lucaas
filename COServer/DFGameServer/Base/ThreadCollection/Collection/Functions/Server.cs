using Core;
using GameServer.Database;
using System;
using System.Threading.Tasks;
using static GameServer.Pool;

namespace GameServer.Threading
{
    /// <summary>
    /// Controller for the player thread.
    /// </summary>
    public static class Server
	{
		/// <summary>
		/// Handles the thread.
		/// </summary>
		public static void Handle(int time)
		{
            try
            {
                DateTime clock = DateTime.Now;
                Database.Server.Reset(clock);
                if (clock > SaveServerDatabase)
                {
                    ServerDatabase.SaveDBPayers(clock);
                    SaveServerDatabase = DateTime.Now.AddMinutes(3);
                }
                if (clock.Hour == 20 && clock.Minute == 0)
                {
                    Database.Server.ResetSquamas();
                }
                LastSavePulse = DateTime.Now;

                if (clock > ResetRandom)
                {
                    ResetRandom = DateTime.Now.AddMinutes(30);
                }

                SendStatusHeartbeat();
            }
            catch (Exception e) { Console.WriteException(e); }
        }

        public static void SendStatusHeartbeat()
        {
            if (Program.ServerConfig == null || Program.ServerConfig.IsInterServer)
                return;

            var heartbeat = new
            {
                ServerName = string.IsNullOrWhiteSpace(Program.ServerConfig.ServerName) ? "Placebo" : Program.ServerConfig.ServerName,
                OnlinePlayers = GamePoll.Count,
                StartedAtUtc = Program.StartedAtUtc,
                LastHeartbeatUtc = DateTime.UtcNow
            };

            _ = Task.Run(() =>
            {
                try
                {
                    RestApiHelper.PostRequestSuccessful("status/heartbeat", heartbeat);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[STATUS] Heartbeat failed: {ex.Message}");
                }
            });
        }
	}
}
