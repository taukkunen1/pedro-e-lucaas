using GameServer.Database;
using System;
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
            }
            catch (Exception e) { Console.WriteException(e); }
        }
	}
}
