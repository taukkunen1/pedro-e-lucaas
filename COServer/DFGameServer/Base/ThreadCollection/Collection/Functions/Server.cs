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
            if (GameServer.Program.GSConfig == null)
                return;

            var heartbeat = new
            {
                ServerName = string.IsNullOrWhiteSpace(GameServer.Program.GSConfig.ServerName) ? "Placebo" : GameServer.Program.GSConfig.ServerName,
                OnlinePlayers = GamePoll.Count,
                StartedAtUtc = Program.StartedAtUtc,
                LastHeartbeatUtc = DateTime.UtcNow,
                ArenaActive = Game.MsgTournaments.MsgSchedules.Arena != null
                    && Game.MsgTournaments.MsgSchedules.Arena.Proces != Game.MsgTournaments.ProcesType.Dead,
                GuildWarActive = Game.MsgTournaments.MsgSchedules.GuildWar != null
                    && Game.MsgTournaments.MsgSchedules.GuildWar.Proces != Game.MsgTournaments.ProcesType.Dead,
                WeeklyPkActive = Game.MsgTournaments.MsgSchedules.PkWar != null
                    && !Game.MsgTournaments.MsgSchedules.PkWar.IsFinished(),
                LavaBeastsPending = Game.MsgTournaments.MsgSchedules.LavaBeastsCount,
                NextLavaBeastUtc = Game.MsgTournaments.MsgSchedules.LavaBeastsCount > 0
                    ? Game.MsgTournaments.MsgSchedules.NextLavaBeast.ToUniversalTime()
                    : (DateTime?)null
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
