using Core;
using GameServer.Database;
using System;
using System.Collections.Generic;
using static GameServer.Game.MsgTournaments.MsgSchedules;
using static GameServer.Pool;

namespace GameServer.Threading
{
    /// <summary>
    /// Controller for the player thread.
    /// </summary>
    public static class WorldTournaments
    {
        public static void Handle(int time)
        {
            try
            {
                DateTime clock = DateTime.Now;
                if (clock > UpdateServerStatus)
                {
                    LastServerPulse = DateTime.Now;
                    if (ServerConfig.IsInterServer)
                        Console.Title = "[" + GroupServerList.MyServerInfo.Name + "] QueuePackets: " + ServerSockets.PacketRecycle.Count + " Online " + Pool.GamePoll.Count + " Time: " + DateTime.Now.Hour + "/" + DateTime.Now.Minute + "/" + DateTime.Now.Second + "";
                    else
                        Console.Title = ServerConfig.ServerName + " - Online: " + Pool.Online + " - Max " + MaxOnline;
                    
                    RestApiHelper.PostRequestSuccessful("SetOnlinePlayers", new { Name = ServerConfig.ServerName, OnlineCount = Pool.GamePoll.Count });
                    UpdateServerStatus = DateTime.Now.AddSeconds(1);
                }
                if (DateTime.Now > LastGuildPulse.AddHours(24))
                {
                    foreach (var guilds in Role.Instance.Guild.GuildPoll.Values)
                    {
                        guilds.CreateMembersRank();
                        guilds.UpdateGuildInfo();
                    }
                    LastGuildPulse = DateTime.Now;
                }

                //UmbralForm.CheckUp();

                //new Action<Role.GameMap>(p => p.GenerateSectorTraps(430, 370, 12)).Invoke(Constants.ServerMaps[1002]);

                CheckUp(clock);

                Program.GlobalItems.Work();

                foreach (var elitegroup in Game.MsgTournaments.MsgTeamPkTournament.EliteGroups)
                    elitegroup.timerCallback(clock);

                foreach (var elitegroup in Game.MsgTournaments.MsgSkillTeamPkTournament.EliteGroups)
                    elitegroup.timerCallback(clock);

                foreach (var elitegroup in Game.MsgTournaments.MsgEliteTournament.EliteGroups)
                    elitegroup.timerCallback(clock);

                Game.MsgTournaments.MsgBroadcast.Work(clock);
            }
            catch (Exception e) { Console.WriteException(e); }
        }
    }
}
