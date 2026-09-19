using Core;
using GameServer.Client;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Database
{
    public class ArenaTable
    {

        internal void Save()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Database.DBActions.Write writer = new DBActions.Write("Arena.ini"))
                {
                    foreach (var user in Game.MsgTournaments.MsgArena.ArenaPoll.Values)
                    {
                        writer.Add(user.ToString());
                    }
                    writer.Execute(DBActions.Mode.Open);
                }
            } else
            {
                List<Core.Models.GameServer.ArenaUser> toUpdate = new List<Core.Models.GameServer.ArenaUser>();
                foreach (var user in Game.MsgTournaments.MsgArena.ArenaPoll.Values)
                {
                    toUpdate.Add(new Core.Models.GameServer.ArenaUser()
                    {
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
                RestApiHelper.PutRequestRaw("ArenaUser/Update", toUpdate);
            }
        }
        internal void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Database.DBActions.Read reader = new DBActions.Read("Arena.ini"))
                {
                    if (reader.Reader())
                    {
                        for (int i = 0; i < reader.Count; i++)
                        {
                            Game.MsgTournaments.MsgArena.User user = new Game.MsgTournaments.MsgArena.User();
                            user.Load(reader.ReadString(""));
                            if (!BaseFunc.UserIsNormal(user.Name) || !BaseFunc.UserExists(user.UID))
                            {
                                continue;
                            }
                            Game.MsgTournaments.MsgArena.ArenaPoll.TryAdd(user.UID, user);
                        }
                    }
                }
            } else
            {
                List<Core.Models.GameServer.ArenaUser> apiArenaUsers = RestApiHelper.GetRequest<List<Core.Models.GameServer.ArenaUser>>("ArenaUser/Get");
                foreach(var apiArenaUser in  apiArenaUsers)
                {
                    Game.MsgTournaments.MsgArena.User user = new Game.MsgTournaments.MsgArena.User();
                    user.Info = new Game.MsgServer.MsgArenaInfo();
                    user.UID = apiArenaUser.UID;
                    user.Name = apiArenaUser.Name;
                    user.Level = apiArenaUser.Level;
                    user.Class = apiArenaUser.Class;
                    user.Mesh = apiArenaUser.Mesh;
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
                    Game.MsgTournaments.MsgArena.ArenaPoll.TryAdd(user.UID, user);
                }
            }

            Game.MsgTournaments.MsgSchedules.Arena.CreateRankTop10();
            Game.MsgTournaments.MsgSchedules.Arena.CreateRankTop1000();
            Game.MsgTournaments.MsgArena.UpdateRank();
        }
        void GiveCpsOffline(uint UID, uint val)
        {
            if (ServerConfig.DbFromFiles)
            {
                IniFileHelper ini = new(Path.Combine(ServerConfig.DbLocation, "Users", $"{UID}.ini"));
                uint CPS = ini.ReadUInt32("Character", "ArenaCPS", 0);
                int DragonPills = ini.ReadInt32("Character", "DragonPills", 0);
                ini.Write<uint>("Character", "ArenaCPS", CPS + val);
            } else
            {
                var player = RestApiHelper.GetPlayer(UID);
                uint CPS = player.ArenaCPS;
                int DragonPills = player.DragonPills;
                player.ArenaCPS = CPS + val;
            }
        }
        uint CPsForRank(uint Rank)
        {
            switch (Rank)
            {
                case 1: return 1000;
                case 2: return 700;
                case 3: return 500;
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                    return 215;
                default: return 0;
            }
        }
        internal void ResetArena()
        {

            foreach (var user in Game.MsgTournaments.MsgArena.ArenaPoll.Values)
            {
                user.LastSeasonArenaPoints = user.Info.ArenaPoints;
                user.LastSeasonWin = user.Info.TodayWin;
                user.LastSeasonLose = user.Info.TotalLose;
                user.LastSeasonRank = user.Info.TodayRank;
                //user.Info.CurrentHonor = user.Info.HistoryHonor = 0;

                if (user.Info.TodayRank >= 1 && user.Info.TodayRank <= 10 && user.Info.ArenaPoints != 4000)
                {
                    GameClient client;
                    if (Pool.GamePoll.TryGetValue(user.UID, out client))
                    {
                        client.Player.Money += CPsForRank(user.Info.TodayRank);
                        //  client.Player.DragonPills++;
                        client.Player.MessageBox($"You got {CPsForRank(user.Info.TodayRank)} Money for ranking in top10 arena.", null, null);
                    }
                    else
                    {
                        GiveCpsOffline(user.UID, CPsForRank(user.Info.TodayRank));
                    }
                }


                ////if (user.Info.TodayWin >= 10)
                //{
                if (user.Info.TodayRank >= 1 && user.Info.TodayRank <= 15)
                    if (user.Info.ArenaPoints != 4000)
                    {
                        int honner = 16000 - ((int)user.Info.TodayRank * 1000);
                        if (honner < 0)
                            honner = 0;
                        user.Info.CurrentHonor += (uint)honner;
                    }
                if (user.Info.HistoryHonor < user.Info.CurrentHonor)
                    user.Info.HistoryHonor = user.Info.CurrentHonor;
                //}

                user.Info.TodayWin = 0;
                user.Info.TodayBattles = 0;
                user.Info.TotalLose = 0;

                user.Info.TodayRank = 0;
                user.Info.ArenaPoints = 4000;

            }

            Game.MsgTournaments.MsgSchedules.Arena.CreateRankTop10();
            Game.MsgTournaments.MsgSchedules.Arena.CreateRankTop1000();
            Game.MsgTournaments.MsgArena.UpdateRank();
        }
    }
}
