using Core;
using System;
using static GameServer.Game.MsgTournaments.MsgSchedules;

namespace GameServer.Threading
{
    /// <summary>
    /// Controller for the player thread.
    /// </summary>
    public static class Qualifier
    {
		/// <summary>
		/// Handles the thread.
		/// </summary>
		public static void ArenaQualifier(int time)
		{
            try
            {
                Arena.CheckGroups(DateTime.Now);
                Arena.CreateMatches(DateTime.Now);
                Arena.VerifyMatches(DateTime.Now);
            }
            catch (Exception e) { Console.WriteException(e); }
        }
        public static void TeamArenaQualifier(int time)
        {
            try
            {
                TeamArena.CheckGroups(DateTime.Now);
                TeamArena.CreateMatches(DateTime.Now);
                TeamArena.VerifyMatches(DateTime.Now);
            }
            catch (Exception e) { Console.WriteException(e); }
        }
    }
}
