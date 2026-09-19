using Core;
using System.Collections.Generic;

namespace GameServer.Database
{
    public class TeamArenaTable
    {
        internal void Save()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Database.DBActions.Write writer = new DBActions.Write("TeamArena.ini"))
                {
                    foreach (var user in Game.MsgTournaments.MsgTeamArena.ArenaPoll.Values)
                    {
                        writer.Add(user.ToString());
                    }
                    writer.Execute(DBActions.Mode.Open);
                }
            }
            else
            {
                List<Core.Models.GameServer.ArenaUser> toUpdate = new List<Core.Models.GameServer.ArenaUser>();
                foreach (var user in Game.MsgTournaments.MsgTeamArena.ArenaPoll.Values)
                {
                    toUpdate.Add(new Core.Models.GameServer.ArenaUser() { 
                        UID = user.UID,
                        Name = user.Name,
                        Level = user.Level,
                        Class = user.Class,
                        Mesh = user.Mesh,
                        ArenaPoints = user.Info.ArenaPoints,
                        CurrentHonor = user.Info.CurrentHonor,
                        HistoryHonor = user.Info.HistoryHonor,
                        TodayBattles = user.Info.TodayBattles,
                        TotalWin = user.Info.TotalWin,
                        LastSeasonArenaPoints = user.LastSeasonArenaPoints,
                        LastSeasonWin = user.LastSeasonWin,
                        LastSeasonLose = user.LastSeasonLose,
                        LastSeasonRank = user.LastSeasonRank,
                    });
                }
                RestApiHelper.PutRequestRaw("TeamArenaUser/Update", toUpdate);
            }
        }
        internal void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Database.DBActions.Read reader = new DBActions.Read("TeamArena.ini"))
                {
                    if (reader.Reader())
                    {
                        for (int i = 0; i < reader.Count; i++)
                        {
                            Game.MsgTournaments.MsgTeamArena.User user = new Game.MsgTournaments.MsgTeamArena.User();
                            user.Load(reader.ReadString(""));
                            if (!BaseFunc.UserIsNormal(user.Name) || !BaseFunc.UserExists(user.UID))
                            {
                                continue;
                            }
                            Game.MsgTournaments.MsgTeamArena.ArenaPoll.TryAdd(user.UID, user);
                        }
                    }
                }
            } else
            {
                List<Core.Models.GameServer.ArenaUser> apiArenaUsers = RestApiHelper.GetRequest<List<Core.Models.GameServer.ArenaUser>>("TeamArenaUser/Get");
                foreach (var apiArenaUser in apiArenaUsers)
                {
                    Game.MsgTournaments.MsgTeamArena.User user = new Game.MsgTournaments.MsgTeamArena.User
                    {
                        Info = new Game.MsgServer.MsgTeamArenaInfo(),
                        UID = apiArenaUser.UID,
                        Name = apiArenaUser.Name,
                        Level = apiArenaUser.Level,
                        Class = apiArenaUser.Class,
                        Mesh = apiArenaUser.Mesh
                    };
                    user.Info.ArenaPoints = apiArenaUser.ArenaPoints;
                    user.Info.CurrentHonor = apiArenaUser.CurrentHonor;
                    user.Info.HistoryHonor = apiArenaUser.HistoryHonor;
                    user.Info.TodayBattles = apiArenaUser.TodayBattles;
                    user.Info.TotalWin = apiArenaUser.TotalWin;
                    user.LastSeasonArenaPoints = apiArenaUser.LastSeasonArenaPoints;
                    user.LastSeasonWin = apiArenaUser.LastSeasonWin;
                    user.LastSeasonLose = apiArenaUser.LastSeasonLose;
                    user.LastSeasonRank = apiArenaUser.LastSeasonRank;
                    if (!BaseFunc.UserIsNormal(user.Name) || !BaseFunc.UserExists(user.UID))
                    {
                        continue;
                    }
                    Game.MsgTournaments.MsgTeamArena.ArenaPoll.TryAdd(user.UID, user);
                }
            }

            Game.MsgTournaments.MsgSchedules.TeamArena.CreateRankTop10();
            Game.MsgTournaments.MsgSchedules.TeamArena.CreateRankTop1000();
            Game.MsgTournaments.MsgTeamArena.UpdateRank();
        }

        internal void ResetArena()
        {

            foreach (var user in Game.MsgTournaments.MsgTeamArena.ArenaPoll.Values)
            {
                user.LastSeasonArenaPoints = user.Info.ArenaPoints;
                user.LastSeasonWin = user.Info.TodayWin;
                user.LastSeasonLose = user.Info.TotalLose;
                user.LastSeasonRank = user.Info.TodayRank;


                //if (user.Info.TodayWin >= 10)
                {
                    if (user.Info.TodayRank != 0 && user.Info.ArenaPoints != 4000)
                    {
                        user.Info.CurrentHonor += user.Info.ArenaPoints * 5;
                    }
                    if (user.Info.HistoryHonor < user.Info.CurrentHonor)
                        user.Info.HistoryHonor = user.Info.CurrentHonor;
                }

                user.Info.TodayWin = 0;
                user.Info.TodayBattles = 0;
                user.Info.TotalLose = 0;

                user.Info.TodayRank = 0;
                user.Info.ArenaPoints = 4000;

            }

            Game.MsgTournaments.MsgSchedules.TeamArena.CreateRankTop10();
            Game.MsgTournaments.MsgSchedules.TeamArena.CreateRankTop1000();
            Game.MsgTournaments.MsgTeamArena.UpdateRank();
        }
    }
}
