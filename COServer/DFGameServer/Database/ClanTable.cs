using Core;
using Core.Models.GameServer;
using GameServer.MadeByDaRkFox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameServer.Database
{
    public class ClanTable
    {
        private static SqlMigrator sqlMigrator = new SqlMigrator();
        internal static void Save()
        {
            List<Core.Models.GameServer.Clan> clansToSaveInDB = new List<Core.Models.GameServer.Clan>();
            foreach (var obj in Role.Instance.Clan.Clans)
            {

                var clan = obj.Value;
                if (ServerConfig.DbFromFiles)
                {
                    using (DBActions.Write writer = new DBActions.Write(Path.Combine("Clans", obj.Key + ".txt")))
                    {
                        writer.Add(clan.ToString())
                            .Add(clan.SaveAlly())
                            .Add(clan.SaveEnemy()).Execute(DBActions.Mode.Open);
                    }
                } else
                {
                    var clanToSave = sqlMigrator.Mapper.Map<Core.Models.GameServer.Clan>(clan);
                    clanToSave.ClanBulletin = clan.ClanBuletin;
                    clansToSaveInDB.Add(clanToSave);
                }
            }
            if (!ServerConfig.DbFromFiles)
            {
                RestApiHelper.PostRequestSuccessful("Clans/Update", clansToSaveInDB);
            }
        }

        internal static void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                foreach (string fname in System.IO.Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Clans")))
                {
                    using (DBActions.Read reader = new DBActions.Read(fname, true))
                    {
                        if (reader.Reader())
                        {
                            Role.Instance.Clan clan = new Role.Instance.Clan();
                            clan.Load(reader.ReadString(""));
                            if (clan.ID > Role.Instance.Clan.CounterClansID.Count)
                                Role.Instance.Clan.CounterClansID.Set(clan.ID + 1);
                            LoadclanAlly(clan.ID, reader.ReadString(""));
                            LoadclanEnemy(clan.ID, reader.ReadString(""));

                            if (!Role.Instance.Clan.Clans.ContainsKey(clan.ID))
                                Role.Instance.Clan.Clans.TryAdd(clan.ID, clan);
                        }
                    }
                }
                LoadMembers();
                ClanExecuteAllyAndEnemy();
                GC.Collect();
            } else
            {
                List<Core.Models.GameServer.Clan> apiClans = RestApiHelper.GetRequest<List<Core.Models.GameServer.Clan>>("Clans/Get");
                List<Role.Instance.Clan> clans = sqlMigrator.Mapper.Map<List<Role.Instance.Clan>>(apiClans);
                foreach(var clan in clans)
                {
                    Core.Models.GameServer.Clan apiClan = apiClans.FirstOrDefault(x => x.ClanID == clan.ID.ToString());
                    clan.ClanBuletin = apiClan.ClanBulletin;
                    LoadclanAlly(clan.ID, apiClan.Allies);
                    LoadclanEnemy(clan.ID, apiClan.Enemies);
                    if (!Role.Instance.Clan.Clans.ContainsKey(clan.ID))
                        Role.Instance.Clan.Clans.TryAdd(clan.ID, clan);
                }
                LoadMembers();
                ClanExecuteAllyAndEnemy();
                GC.Collect();
            }
        }

        internal static void MigrateToSql()
        {
            List<Core.Models.GameServer.Clan> clansToMigrate = new List<Core.Models.GameServer.Clan>();
            foreach (string fname in System.IO.Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Clans")))
            {
                using (DBActions.Read reader = new DBActions.Read(fname, true))
                {
                    if (reader.Reader())
                    {
                        Role.Instance.Clan clan = new Role.Instance.Clan();
                        clan.Load(reader.ReadString(""));
                        if (clan.ID > Role.Instance.Clan.CounterClansID.Count)
                            Role.Instance.Clan.CounterClansID.Set(clan.ID + 1);
                        string Allies = reader.ReadString("");
                        string Enemies = reader.ReadString("");
                        var clanToMigrate = sqlMigrator.Mapper.Map<Core.Models.GameServer.Clan>(clan);
                        clanToMigrate.Allies = Allies;
                        clanToMigrate.Enemies = Enemies;
                        clanToMigrate.ClanBulletin = clan.ClanBuletin;
                        clansToMigrate.Add(clanToMigrate);
                    }
                }
            }
            GC.Collect();
            RestApiHelper.PostRequestSuccessful("Clans/Set", clansToMigrate);
            sqlMigrator.MigratedMessage(clansToMigrate.Count, "Clans");
        }
        public static void ClanExecuteAllyAndEnemy()
        {
            foreach (var obj in clanally)
            {
                foreach (var clan in obj.Value)
                {
                    Role.Instance.Clan alyclan;
                    if (Role.Instance.Clan.Clans.TryGetValue(clan, out alyclan))
                    {
                        if (Role.Instance.Clan.Clans.ContainsKey(obj.Key))
                        {
                            Role.Instance.Clan.Clans[obj.Key].Ally.TryAdd(alyclan.ID, alyclan);
                        }
                    }
                }
            }
            foreach (var obj in clanenemy)
            {
                foreach (var clan in obj.Value)
                {
                    Role.Instance.Clan enemyclan;
                    if (Role.Instance.Clan.Clans.TryGetValue(clan, out enemyclan))
                    {
                        if (Role.Instance.Clan.Clans.ContainsKey(obj.Key))
                        {
                            Role.Instance.Clan.Clans[obj.Key].Enemy.TryAdd(enemyclan.ID, enemyclan);
                        }
                    }
                }
            }
        }
        private static void LoadMembers()
        {
            if (ServerConfig.DbFromFiles)
            {
                IniFileHelper ini = new();
                foreach (string fname in Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Users")))
                {
                    ini.LoadFile(fname);

                    uint UID = ini.ReadUInt32("Character", "UID", 0);
                    string Name = ini.ReadString("Character", "Name", "None");
                    uint ClanID = ini.ReadUInt32("Character", "ClanID", 0);
                    if (ClanID != 0)
                    {
                        Role.Instance.Clan Clan;
                        if (Role.Instance.Clan.Clans.TryGetValue(ClanID, out Clan))
                        {
                            Role.Instance.Clan.Member member = new Role.Instance.Clan.Member();
                            member.UID = UID;
                            member.Name = Name;
                            member.Rank = (Role.Instance.Clan.Ranks)ini.ReadUInt16("Character", "ClanRank", 200);
                            member.Class = ini.ReadByte("Character", "Class", 0);
                            member.Level = (byte)ini.ReadUInt16("Character", "Level", 0);
                            member.Donation = ini.ReadUInt32("Character", "ClanDonation", 0);

                            if (!Clan.Members.ContainsKey(member.UID))
                                Clan.Members.TryAdd(member.UID, member);
                        }
                    }
                }
            } else
            {

                List<Player> players = RestApiHelper.GetPlayers();
                foreach (Player player in players)
                {
                    uint UID = player.UID;
                    string Name = player.Name;
                    uint ClanID = player.ClanUID;
                    if (ClanID != 0)
                    {
                        Role.Instance.Clan Clan;
                        if (Role.Instance.Clan.Clans.TryGetValue(ClanID, out Clan))
                        {
                            Role.Instance.Clan.Member member = new Role.Instance.Clan.Member();
                            member.UID = UID;
                            member.Name = Name;
                            member.Rank = (Role.Instance.Clan.Ranks)player.ClanRank;
                            member.Class = player.Class;
                            member.Level = player.Level;
                            member.Donation = player.ClanDonation;
                            if (!Clan.Members.ContainsKey(member.UID))
                            {
                                Clan.Members.TryAdd(member.UID, member);
                            }
                        }
                    }
                }
            }
        }
        public static Dictionary<uint, List<uint>> clanally = new Dictionary<uint, List<uint>>();
        public static Dictionary<uint, List<uint>> clanenemy = new Dictionary<uint, List<uint>>();
        private static void LoadclanAlly(uint id, string line)
        {
            Database.DBActions.ReadLine reader = new DBActions.ReadLine(line, '/');
            int count = reader.Read((int)0);
            for (int x = 0; x < count; x++)
            {
                uint obj = reader.Read((uint)0);
                if (clanally.ContainsKey(id))
                    clanally[id].Add(obj);
                else
                    clanally.Add(id, new List<uint>() { obj });
            }
        }
        private static void LoadclanEnemy(uint id, string line)
        {
            Database.DBActions.ReadLine reader = new DBActions.ReadLine(line, '/');
            int count = reader.Read((int)0);
            for (int x = 0; x < count; x++)
            {
                uint obj = reader.Read((uint)0);
                if (clanenemy.ContainsKey(id))
                    clanenemy[id].Add(obj);
                else
                    clanenemy.Add(id, new List<uint>() { obj });
            }
        }
    }
}
