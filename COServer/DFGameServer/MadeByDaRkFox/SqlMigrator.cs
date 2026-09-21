using AutoMapper;
using Core;
using Core.Enums;
using Core.Interfaces.GameServer;
using Core.Models.GameServer;
using GameServer.Database;
using GameServer.Database.DBActions;
using GameServer.Game.MsgServer;
using GameServer.Game.MsgTournaments;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Core.Enums.SharedEnums;
using static GameServer.Database.ClientSpells;
using static GameServer.Database.Tranformation;
using static GameServer.Role.Instance.AssociateGS;
using static GameServer.Role.KOBoard;

namespace GameServer.MadeByDaRkFox
{
    public class SqlMigrator
    {
        private IMapper _mapper;
        public IMapper Mapper { get => _mapper; set => _mapper = value; }
        public List<string> migrationSystems = new List<string>() { "npcs", "maps", "portals", "guilds", "clans", "tutors", "tutorbattle", "transformations", "crimes", "arena", "staticstatues", "players", "playeritems", "playerspells", "playerprofs", "vendorshops", "teamelitepks", "classpkwars", "elitepks", "couples", "citywars", "guildwars", "vipshares", "koboardranks", "associates" };
        public SqlMigrator()
        {
            _mapper = new MapperConfiguration(cfg => {
                cfg.CreateMap<Role.Instance.Guild, Guild>().ForMember(
                    dest => dest.GuildID,
                    opt => opt.MapFrom(src => src.Info.GuildID)
                ).ForMember(
                    dest => dest.LeaderName,
                    opt => opt.MapFrom(src => src.Info.LeaderName)
                ).ForMember(
                    dest => dest.CreateTime,
                    opt => opt.MapFrom(src => src.Info.CreateTime)
                ).ForMember(
                    dest => dest.SilverFund,
                    opt => opt.MapFrom(src => src.Info.SilverFund)
                ).ForMember(
                    dest => dest.ConquerPointFund,
                    opt => opt.MapFrom(src => src.Info.ConquerPointFund)
                ).ForMember(
                    dest => dest.Level,
                    opt => opt.MapFrom(src => src.Info.Level)
                ).ForMember(
                    dest => dest.MembersCount,
                    opt => opt.MapFrom(src => src.Info.MembersCount)
                ).ForMember(
                    dest => dest.CTFExploits,
                    opt => opt.MapFrom(src => src.CTF_Exploits)
                ).ForMember(
                    dest => dest.CTFNextMoney,
                    opt => opt.MapFrom(src => src.CTF_Next_Money)
                ).ForMember(
                    dest => dest.CTFNextConquerPoints,
                    opt => opt.MapFrom(src => src.CTF_Next_ConquerPoints)
                ).ForMember(
                    dest => dest.CTFRank,
                    opt => opt.MapFrom(src => src.CTF_Rank)
                ).ForMember(
                    dest => dest.Enemies,
                    opt => opt.MapFrom(src => src.Enemy)
                ).ForMember(
                    dest => dest.Allies,
                    opt => opt.MapFrom(src => src.Ally)
                ).ForMember(
                    dest => dest.Arsenal,
                    opt => opt.MapFrom(src => src.MyArsenal.ToString())
                );
                cfg.CreateMap<Role.Instance.Clan, Clan>().ForMember(
                    dest => dest.ClanID,
                    opt => opt.MapFrom(src => src.ID)
                ).ReverseMap();
            }, null).CreateMapper();
        }
        public List<Player> GeneratePlayerObjFromFiles(uint PlayerUID = 0)
        {
            List<Player> players = new List<Player>();
            string[] userFiles = Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Users"));
            if (PlayerUID > 0)
            {
                userFiles = Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Users", $"{PlayerUID}.ini"));
            }
            foreach (string UID in userFiles)
            {
                IniFileHelper reader = (new IniFileHelper(UID));
                Player player = new();
                player.UID = reader.ReadUInt32("Character", "UID", 1000000);
                player.Body = reader.ReadUInt16("Character", "Body", 1002);
                player.Face = reader.ReadUInt16("Character", "Face", 0);
                player.Name = reader.ReadString("Character", "Name", "None");
                player.Spouse = reader.ReadString("Character", "Spouse", "None");
                player.Class = reader.ReadByte("Character", "Class", 0);
                player.FirstClass = reader.ReadByte("Character", "FirstClass", 0);
                player.SecondClass = reader.ReadByte("Character", "SecoundeClass", 0);
                player.Avatar = reader.ReadUInt16("Character", "Avatar", 0);
                player.Map = reader.ReadUInt32("Character", "Map", 1002);
                player.X = reader.ReadUInt16("Character", "X", 429);
                player.Y = reader.ReadUInt16("Character", "Y", 378);
                player.MiningAttempts = reader.ReadUInt16("Character", "MiningAttempts", 200);
                player.PMap = reader.ReadUInt32("Character", "PMap", 1002);
                player.PMapX = reader.ReadUInt16("Character", "PMapX", 300);
                player.PMapY = reader.ReadUInt16("Character", "PMapY", 300);
                player.Agility = reader.ReadUInt16("Character", "Agility", 0);
                player.Strength = reader.ReadUInt16("Character", "Strength", 0);
                player.Spirit = reader.ReadUInt16("Character", "Spirit", 0);
                player.Vitality = reader.ReadUInt16("Character", "Vitaliti", 0);
                player.Attributes = reader.ReadUInt16("Character", "Atributes", 0);
                player.Reborn = reader.ReadByte("Character", "Reborn", 0);
                player.Level = (byte)reader.ReadUInt16("Character", "Level", 0);
                player.Hair = reader.ReadUInt16("Character", "Haire", 0);
                player.Experience = (ulong)reader.ReadInt64("Character", "Experience", 0);
                player.HitPoints = reader.ReadInt32("Character", "MinHitPoints", 0);
                player.Mana = reader.ReadUInt16("Character", "MinMana", 0);
                player.ConquerPoints = reader.ReadUInt32("Character", "ConquerPoints", 0);
                player.ArenaCPS = reader.ReadUInt32("Character", "ArenaCPS", 0);
                player.BoundConquerPoints = reader.ReadInt32("Character", "BoundConquerPoints", 0);
                player.Money = reader.ReadUInt32("Character", "Money", 0);
                player.VirtuePoints = reader.ReadUInt32("Character", "VirtutePoints", 0);
                player.VirtueEntries = reader.ReadByte("Character", "VirtuteEntries", 0);
                player.PKPoints = reader.ReadUInt16("Character", "PkPoints", 0);
                player.PVPPoints = reader.ReadUInt16("Character", "PVPPoints", 0);
                player.NobilityLastDonation = reader.ReadUInt64("PaidNobility", "LastNobilityDonation", 0);
                player.NobilityPaidRank = reader.ReadByte("PaidNobility", "PaidRank", 0);
                player.NobilityPeriodTime = reader.ReadUInt64("PaidNobility", "PeriodTime", 0);
                player.NobilityIsActive = reader.ReadBool("PaidNobility", "IsActive", false);
                player.JailerUID = reader.ReadUInt32("Character", "JailerUID", 0);
                player.QuizPoints = reader.ReadUInt32("Character", "QuizPoints", 0);
                player.Enlighten = reader.ReadUInt16("Character", "Enilghten", 0);
                player.EnlightenReceive = reader.ReadUInt16("Character", "EnlightenReceive", 0);
                player.DailySignUpDays = reader.ReadUInt64("Character", "DailySignUpDays", 0);
                player.DailyMonth = reader.ReadByte("Character", "DailyMonth", 0);
                player.DailySignUpRewards = reader.ReadByte("Character", "DailySignUpRewards", 0);
                player.VipLevel = reader.ReadByte("Character", "VipLevel", 0);
                player.ExpireVip = DateTime.FromBinary(reader.ReadInt64("Character", "VipTime", 0));
                player.LastDragonPill = DateTime.FromBinary(reader.ReadInt64("Character", "LastDragonPill", 0));
                player.Archivement = reader.ReadString("Character", "Achivement", "");
                player.WHMoney = reader.ReadInt64("Character", "WHMoney", 0);
                player.BlessTime = reader.ReadUInt32("Character", "BlessTime", 0);
                player.TrinityPoints = reader.ReadUInt32("Character", "TrinityPoints", 0);
                player.SpouseUID = reader.ReadUInt32("Character", "SpouseUID", 0);
                player.HeavenBlessing = reader.ReadInt32("Character", "HeavenBlessing", 0);
                player.HeavenBlessTime = new DateTime(reader.ReadInt64("Character", "LostTimeBlessing", 0));
                player.HuntingBlessing = reader.ReadUInt32("Character", "HuntingBlessing", 0);
                player.OnlineTrainingPoints = reader.ReadUInt32("Character", "OnlineTrainingPoints", 0);
                player.JoinOfflineTG = DateTime.FromBinary(reader.ReadInt64("Character", "JoinOnflineTG", 0));
                player.RateExp = reader.ReadUInt32("Character", "RateExp", 0);
                player.DExpTime = reader.ReadUInt32("Character", "DExpTime", 0);
                player.Day = reader.ReadInt32("Character", "Day", 0);
                player.BDExp = reader.ReadByte("Character", "BDExp", 0);
                player.ExpBallUsed = reader.ReadByte("Character", "ExpBallUsed", 0);
                player.MysteryFruit = reader.ReadByte("Character", "MysteryFruit", 0);
                player.FreeVIP = DateTime.FromBinary(reader.ReadInt64("Character", "FreeVIP", 0));
                player.GuildID = reader.ReadUInt32("Character", "GuildID", 0);
                player.GuildRank = (Core.Interfaces.GameServer.GuildMemberRank)reader.ReadUInt32("Character", "GuildRank", 200);
                player.EnabledTitles = reader.ReadString("Character", "EnabledTitles", "");
                player.SubProfInfo = reader.ReadString("Character", "SubProfInfo", "");
                player.Flowers = reader.ReadString("Character", "Flowers", "");
                player.NobilityDonation = reader.ReadUInt64("Character", "DonationNobility", 0);
                player.ClanUID = reader.ReadUInt32("Character", "ClanID", 0);
                player.ClanRank = reader.ReadUInt16("Character", "ClanRank", 0);
                player.ClanDonation = reader.ReadUInt32("Character", "ClanDonation", 0);
                player.FirstRebornLevel = reader.ReadByte("Character", "FRL", 0);
                player.SecondRebornLevel = reader.ReadByte("Character", "SRL", 0);
                player.Reincarnation = reader.ReadBool("Character", "Reincanation", false);
                player.LotteryEntries = reader.ReadByte("Character", "LotteryEntries", 0);
                player.QuestEntries = reader.ReadByte("Character", "QuestEntries", 0);
                player.Quest2Entries = reader.ReadByte("Character", "Quest2Entries", 0);
                player.Quest3Entries = reader.ReadByte("Character", "Quest3Entries", 0);
                player.MonsterEntries = reader.ReadByte("Character", "MonsterEntries", 0);
                player.MonsterEntries2 = reader.ReadByte("Character", "MonsterEntries2", 0);
                player.RemoveWeapon = reader.ReadByte("Character", "RemoveWeapon", 0);
                player.DbTry = reader.ReadBool("Character", "DbTry", false);
                player.DemonExterminator = reader.ReadString("Character", "DemonEx", "0/0/");
                if (uint.TryParse(reader.ReadString("Character", "PkName", "0"), out uint canParseMyKillerUID))
                {
                    player.MyKillerUID = uint.Parse(reader.ReadString("Character", "PkName", "0"));
                }
                player.MyKillerName = reader.ReadString("Character", "PkName", "None");
                player.CursedTimer = reader.ReadInt32("Character", "Cursed", 0);
                player.AtiveQuestApe = reader.ReadUInt32("Character", "enervant", 0);
                player.AparenceType = reader.ReadUInt32("Character", "AparenceType", 0);
                player.TournamentKills = reader.ReadUInt32("Character", "TKills", 0);
                player.OnlineMinutes = reader.ReadUInt32("Character", "OnlineMinutes", 0);
                player.HistoryChampionPoints = reader.ReadUInt32("Character", "HistoryChampionPoints", 0);
                player.ChampionPoints = reader.ReadUInt32("Character", "ChampionPoints", 0);
                player.TodayChampionPoints = reader.ReadUInt32("Character", "TodayChampionPoints", 0);
                player.DailySpiritBeadItem = reader.ReadUInt32("Character", "DailySpiritBeadItem", 0);
                player.SecurityPass = reader.ReadString("Character", "SecurityPass", "0,0,0");
                player.TCCaptainTimes = reader.ReadByte("Character", "TCT", 0);
                player.LastMan = reader.ReadUInt32("Character", "LastMan", 0);
                player.PTB = reader.ReadUInt32("Character", "PTB", 0);
                player.Get5Out = reader.ReadUInt32("Character", "Get5Out", 0);
                player.FreezeWar = reader.ReadUInt32("Character", "FreezeWar", 0);
                player.Infection = reader.ReadUInt32("Character", "Infection", 0);
                player.TheCaptain = reader.ReadUInt32("Character", "TheCaptain", 0);
                player.Kungfu = reader.ReadUInt32("Character", "Kungfu", 0);
                player.VampireWar = reader.ReadUInt32("Character", "VampireWar", 0);
                player.WhackTheThief = reader.ReadUInt32("Character", "WhackTheThief", 0);
                player.SSFB = reader.ReadUInt32("Character", "SSFB", 0);
                player.DonationPoints = reader.ReadUInt32("Character", "RacePoints", 0);
                player.NameEditCount = reader.ReadUInt16("Character", "NameEditCount", 0);
                player.MainFlag = (Core.Interfaces.GameServer.MainFlagType)reader.ReadUInt32("Character", "ClaimStateGift", 0);
                player.CountryID = reader.ReadUInt16("Character", "CountryID", 0);
                player.InventorySashCount = reader.ReadUInt16("Character", "InventorySashCount", 0);
                player.MyFootBallPoints = reader.ReadUInt32("Character", "MyFootBallPoints", 0);
                player.ExpProtection = reader.ReadUInt32("Character", "ExpProtection", 0);
                player.BanCount = reader.ReadByte("Character", "BanCount", 0);
                player.ExtraAttributes = reader.ReadUInt16("Character", "ExtraAtributes", 0);
                player.OpenHousePack = reader.ReadByte("Character", "OpenHousePack", 0);
                player.JoinPowerArenaStamp = DateTime.FromBinary(reader.ReadInt64("Character", "JPAStamp", 0));
                player.GiveFlowersToPerformer = reader.ReadInt32("Character", "GiveFlowersToPerformer", 0);
                player.UseChiToken = reader.ReadByte("Character", "UseChiToken", 0);
                player.OnlinePoints = reader.ReadUInt32("Character", "OnlinePoints", 0);
                player.VotePoints = reader.ReadUInt32("Character", "VotePoints", 0);
                player.TotalMobsKilled = reader.ReadUInt32("Character", "TotalMobsKilled", 0);
                player.TotalMobsKilled2 = reader.ReadUInt32("Character", "TotalMobsKilled2", 0);
                player.TotalSouls = reader.ReadUInt32("Character", "TotalSouls", 0);
                player.DragonPills = reader.ReadInt32("Character", "DragonPills", 0);
                player.BossPoints = reader.ReadInt32("Character", "BossPoints", 0);
                player.PVEPoints = reader.ReadUInt32("Character", "PVEPoints", 0);
                player.Agates = reader.ReadString("Character", "Agates", "");
                players.Add(player);
            }
            return players;
        }
        public int MigrateEliteTournaments(MsgTeamEliteGroup[] EliteGroups, EliteTournamentType Type)
        {
            List<TeamElitePK> teamElitePKs = new List<TeamElitePK>();
            for (int x = 0; x < EliteGroups.Length; x++)
            {
                var Tournament = EliteGroups[x];

                for (int i = 0; i < Tournament.Top8.Length; i++)
                {
                    var element = Tournament.Top8[i];
                    teamElitePKs.Add(new TeamElitePK() { Type = Type, Tournament = (byte)x, Rank = (byte)i, PlayerId = element.UID, PlayerName = element.Name, PlayerMesh = element.Mesh, ClaimReward = element.ClaimReward, LeaderUID = element.LeaderUID });
                }
            }
            return RestApiHelper.AddTeamElitePK(teamElitePKs);
        }
        public void MigrateNPCs()
        {
            List<SobNPC> sobnpcList = new List<SobNPC>();
            List<NPC> npcsList = new List<NPC>();
            List<Furniture> furnituresList = new List<Furniture>();
            #region SobNPC
            string[] baseText = File.ReadAllLines(Path.Combine(ServerConfig.DbLocation, "SobNpcs.txt"));
            foreach (var bas_line in baseText)
            {
                Database.DBActions.ReadLine line = new Database.DBActions.ReadLine(bas_line, ',');
                SobNPC sobnpc = new();
                sobnpc.ObjType = SharedEnums.MapObjectType.SobNpc;
                sobnpc.UID = line.Read((uint)0);
                sobnpc.Name = line.Read("");
                sobnpc.Type = (SharedEnums.NpcType)line.Read((ushort)0);
                sobnpc.Mesh = (SharedEnums.StaticMesh)line.Read((ushort)0);
                sobnpc.Map = line.Read((ushort)0);
                sobnpc.ShowName = true;
                if (sobnpc.Map != 5001)
                {
                    sobnpc.X = line.Read((ushort)0);
                    sobnpc.Y = line.Read((ushort)0);
                    sobnpc.HitPoints = (uint)line.Read((int)0);
                    sobnpc.MaxHitPoints = (uint)line.Read((int)0);
                    sobnpc.Sort = line.Read((ushort)0);
                    if (line.Read((byte)0) == 0)
                    {
                        sobnpc.ShowName = false;
                    }
                }
                if (sobnpc != null)
                {
                    sobnpcList.Add(sobnpc);
                }
            }
            RestApiHelper.PostRequestSuccessful("SobNPCs/Set", sobnpcList);
            MigratedMessage(sobnpcList.Count(), "SobNPCs");
            #endregion
            #region NPC
            string PathNPCS = Path.Combine(ServerConfig.DbLocation, "Npcs.txt");
            if (File.Exists(PathNPCS))
            {
                using (StreamReader read = File.OpenText(PathNPCS))
                {
                    while (true)
                    {
                        string aline = read.ReadLine();
                        if (aline != null && aline != "")
                        {
                            string[] line = aline.Split(',');
                            NPC npc = new();
                            npc.UID = uint.Parse(line[0]);
                            npc.Type = (SharedEnums.NpcType)byte.Parse(line[1]);
                            npc.Mesh = ushort.Parse(line[2]);
                            npc.Map = ushort.Parse(line[3]);
                            if (npc.Map != 5000)
                            {
                                npc.X = ushort.Parse(line[4]);
                                npc.Y = ushort.Parse(line[5]);
                                if (line.Length > 6)
                                {
                                    npc.Name = line[6];
                                }
                                npcsList.Add(npc);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            RestApiHelper.PostRequestSuccessful("NPCs/Set", npcsList);
            MigratedMessage(npcsList.Count(), "NPCs");
            #endregion
            #region Furnitures
            using (StreamReader read = File.OpenText(Path.Combine(ServerConfig.DbLocation, "furnitures.txt")))
            {
                while (true)
                {
                    string aline = read.ReadLine();
                    if (aline != null)
                    {

                        string[] line = aline.Split(',');
                        NPC npc = new();
                        npc.UID = uint.Parse(line[0]);
                        npc.Type = (SharedEnums.NpcType)byte.Parse(line[1]);
                        npc.Mesh = ushort.Parse(line[2]);
                        npc.Map = ushort.Parse(line[3]);
                        npc.X = ushort.Parse(line[4]);
                        npc.Y = ushort.Parse(line[5]);
                        Furniture furnit = new Furniture();
                        furnit.Name = line[6];
                        furnit.ItemID = uint.Parse(line[7]);
                        furnit.UID = npc.UID;
                        furnit.Mesh = npc.Mesh;
                        furnit.MoneyCost = uint.Parse(line[8]);
                        furnit.Map = npc.Map;
                        furnit.X = npc.X;
                        furnit.Y = npc.Y;
                        furnit.Type = npc.Type;
                        furnituresList.Add(furnit);
                    }
                    else
                        break;
                }
            }
            RestApiHelper.PostRequestSuccessful("Furnitures/Set", furnituresList);
            MigratedMessage(furnituresList.Count(), "Furnitures");
            #endregion
        }
        public void MigratePlayers()
        {
            List<Player> players = GeneratePlayerObjFromFiles();
            RestApiHelper.PostRequestSuccessful("Players/Set", players);
            MigratedMessage(players.Count(), "Players");
        }
        public void MigratePlayerItems()
        {
            int counterMigratedItems = 0;
            foreach (string pathUserFile in Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Users")))
            {
                IniFileHelper reader = (new IniFileHelper(pathUserFile));
                uint UID = reader.ReadUInt32("Character", "UID", 1000000);
                string p = Path.Combine(ServerConfig.DbLocation, "PlayersItems", UID + ".bin");
                if (File.Exists(p))
                {
                    List<PlayerItem> PlayerItems = new List<PlayerItem>();
                    BinaryFileHelper binHelper = new(p);
                    int ItemCount = binHelper.ReadInt();
                    for (int x = 0; x < ItemCount; x++)
                    {
                        ClientItems.DBItem Item = binHelper.Read<ClientItems.DBItem>();
                        if (!Item.Fake && Item.ITEM_ID != 0)
                        {
                            MsgGameItem ClientItem = Item.GetDataItem();
                            PlayerItem pItem = new(ClientItem, UID);
                            pItem.Uid = Pool.ItemUIDCounter.Next; // Force generate Uid unique in Database on API
                            PlayerItems.Add(pItem);
                        }
                    }
                    binHelper.Close();
                    int playerItemsUpdated = RestApiHelper.UpdatePlayerItems(new UpdatePlayerItem() { EntityUid = UID, PlayerItems = PlayerItems });
                    counterMigratedItems += playerItemsUpdated;
                }
            }
            MigratedMessage(counterMigratedItems, "PlayerItems");
        }
        public void MigratePlayerSpells()
        {
            List<Player> players = GeneratePlayerObjFromFiles();
            int affectedRows = 0;
            foreach (Player player in players) {
                List<PlayerSpell> PlayerSpells = new List<PlayerSpell>();
                BinaryFileHelper binary = new();
                if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, "PlayersSpells", player.UID + ".bin"), FileMode.Open))
                {
                    ClientSpells.DBSpell DBSpell;
                    int CountSpell = binary.ReadInt();
                    for (int x = 0; x < CountSpell; x++)
                    {
                        DBSpell = binary.Read<ClientSpells.DBSpell>();
                        if (Pool.Magic.ContainsKey(DBSpell.ID)) // Check if is valid magic
                        {
                            PlayerSpells.Add(new PlayerSpell() {TypeID = DBSpell.ID, Level = DBSpell.Level, Experience = DBSpell.Experience, PreviousLevel = DBSpell.PreviousLevel, SoulLevel = DBSpell.SoulLevel, UseJiangSpell = DBSpell.UseJiangSpell, PlayerUid = player.UID });
                        }
                    }
                    binary.Close();
                }
                int affectedRowsQuery = RestApiHelper.AddPlayerSpells(player.UID, PlayerSpells);
                affectedRows += affectedRowsQuery;
            }
            MigratedMessage(affectedRows, "PlayerSpells");
        }
        public void MigratePlayerProfs()
        {
            List<Player> players = GeneratePlayerObjFromFiles();
            int affectedRows = 0;
            foreach (Player player in players)
            {
                List<PlayerProficiency> PlayerProfs = new List<PlayerProficiency>();
                BinaryFileHelper binary = new();
                if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, "PlayersProfs", player.UID + ".bin"), FileMode.Open))
                {
                    ClientProficiency.DBProf DBProf;
                    int CountProf = binary.ReadInt();
                    for (int x = 0; x < CountProf; x++)
                    {
                        DBProf = binary.Read<ClientProficiency.DBProf>();
                        PlayerProfs.Add(new PlayerProficiency() { TypeID = DBProf.ID, Level = DBProf.Level, Experience = DBProf.Experience, PreviousLevel = DBProf.PreviousLevel, PlayerUid = player.UID });
                    }
                    binary.Close();
                }
                int affectedRowsQuery = RestApiHelper.AddPlayerProfs(player.UID, PlayerProfs);
                affectedRows += affectedRowsQuery;
            }
            MigratedMessage(affectedRows, "PlayerProfs");
        }
        public void MigrateVendorShops()
        {
            List<Core.Models.GameServer.VendorShop> vendorShops = new List<Core.Models.GameServer.VendorShop>();
            foreach(var vendorShopGS in Booth.Booths.Values)
            {
                vendorShops.Add(new Core.Models.GameServer.VendorShop() { UID = vendorShopGS.UID, Mesh = vendorShopGS.Mesh, Name = vendorShopGS.Name, Map = vendorShopGS.Map, X = vendorShopGS.X, Y = vendorShopGS.Y, Items = vendorShopGS.Items, CostType = (Core.Interfaces.GameServer.VendorActionMode)vendorShopGS.CostType });
            }
            int affectedRows = RestApiHelper.AddVendorShop(vendorShops);
            MigratedMessage(affectedRows, "VendorShops");
        }
        public void MigrateTeamElitePKS() {
            int totalMigrated = 0;
            totalMigrated += MigrateEliteTournaments(MsgTeamPkTournament.EliteGroups, Core.Interfaces.GameServer.EliteTournamentType.TeamElitePK);
            totalMigrated += MigrateEliteTournaments(MsgSkillTeamPkTournament.EliteGroups, Core.Interfaces.GameServer.EliteTournamentType.SkillTeamElitePK);
            MigratedMessage(totalMigrated, "TeamElitePKS");
        }
        public void MigrateElitePKS()
        {
            int totalMigrated = 0;
            List<ElitePK> elitePKs = new List<ElitePK>();
            for (int x = 0; x < MsgEliteTournament.EliteGroups.Length; x++)
            {
                var Tournament = MsgEliteTournament.EliteGroups[x];

                for (int i = 0; i < Tournament.Top8.Length; i++)
                {
                    var element = Tournament.Top8[i];
                    elitePKs.Add(new ElitePK() { Tournament = (byte)x, Rank = (byte)i, PlayerId = element.UID, PlayerName = element.Name, PlayerMesh = element.Mesh, ClaimReward = element.ClaimReward });
                }
            }
            totalMigrated = RestApiHelper.AddElitePK(elitePKs);
            MigratedMessage(totalMigrated, "ElitePKS");
        }
        public void MigrateClassPKWars()
        {
            List<ClassPKWar> toAdd = new List<ClassPKWar>();
            var pkwars = MsgSchedules.ClassPkWar.PkWars;
            for (ushort type = 0; type < pkwars.Length; type++)
            {
                for (ushort level = 0; level < pkwars[type].Length; level++)
                {
                    var pkwar = pkwars[type][level];
                    toAdd.Add(new ClassPKWar() { Type = (byte)type, Level = (byte)level, Winner = pkwar.Winner, LastFlag = (uint)pkwar.LastFlag });
                }
            }
            int totalMigrated = RestApiHelper.AddClassPKWars(toAdd);
            MigratedMessage(totalMigrated, "ClassPKWars");
        }
        public void MigrateCouples()
        {
            int totalMigrated = 0;
            List<Couple> couples = new List<Couple>();
            Read reader = new Read(MsgCouples.FileName);
            if (reader.Reader())
            {
                for (int x = 0; x < reader.Count; x++)
                {
                    ReadLine line = new ReadLine(reader.ReadString(""), '/');
                    string Winner1 = line.Read("NONE");
                    string Winner2 = line.Read("NONE");
                    couples.Add(new Couple { Winner1 = Winner1, Winner2 = Winner2 });

                }
            }
            totalMigrated = RestApiHelper.AddUpdateCouples(couples);
            MigratedMessage(totalMigrated, "Couples");
        }
        public void MigrateCityWars()
        {
            int totalMigrated = 0;
            // Load from file for migrate data
            IniFileHelper reader = new(Path.Combine(ServerConfig.DbLocation, "CityWar.ini"));
            MsgSchedules.CityWar.WinnerTC.GuildID = reader.ReadUInt32("Info", "TCID", 0);
            MsgSchedules.CityWar.WinnerTC.Name = reader.ReadString("Info", "TCName", "None");
            MsgSchedules.CityWar.Furnitures[821].Name = reader.ReadString("TCPole", "Name", "None");
            MsgSchedules.CityWar.Furnitures[821].HitPoints = reader.ReadInt32("TCPole", "HitPoints", 0);

            MsgSchedules.CityWar.WinnerPC.GuildID = reader.ReadUInt32("Info", "PCID", 0);
            MsgSchedules.CityWar.WinnerPC.Name = reader.ReadString("Info", "PCName", "None");
            MsgSchedules.CityWar.Furnitures[822].Name = reader.ReadString("PCPole", "Name", "None");
            MsgSchedules.CityWar.Furnitures[822].HitPoints = reader.ReadInt32("PCPole", "HitPoints", 0);

            MsgSchedules.CityWar.WinnerAC.GuildID = reader.ReadUInt32("Info", "ACID", 0);
            MsgSchedules.CityWar.WinnerAC.Name = reader.ReadString("Info", "ACName", "None");
            MsgSchedules.CityWar.Furnitures[823].Name = reader.ReadString("ACPole", "Name", "None");
            MsgSchedules.CityWar.Furnitures[823].HitPoints = reader.ReadInt32("ACPole", "HitPoints", 0);

            MsgSchedules.CityWar.WinnerDC.GuildID = reader.ReadUInt32("Info", "DCID", 0);
            MsgSchedules.CityWar.WinnerDC.Name = reader.ReadString("Info", "DCName", "None");
            MsgSchedules.CityWar.Furnitures[824].Name = reader.ReadString("DCPole", "Name", "None");
            MsgSchedules.CityWar.Furnitures[824].HitPoints = reader.ReadInt32("DCPole", "HitPoints", 0);

            MsgSchedules.CityWar.WinnerBI.GuildID = reader.ReadUInt32("Info", "BIID", 0);
            MsgSchedules.CityWar.WinnerBI.Name = reader.ReadString("Info", "BIName", "None");
            MsgSchedules.CityWar.Furnitures[825].Name = reader.ReadString("BIPole", "Name", "None");
            MsgSchedules.CityWar.Furnitures[825].HitPoints = reader.ReadInt32("BIPole", "HitPoints", 0);
            // Save in new obj for call to api
            List<CityWar> CityWars =
            [
                new CityWar() { CityWarType = Core.Interfaces.GameServer.CityWarType.TC, GuildId =  MsgSchedules.CityWar.WinnerTC.GuildID, GuildName = MsgSchedules.CityWar.WinnerTC.Name, PoleHitPoints = (uint)MsgSchedules.CityWar.Furnitures[821].HitPoints},
                new CityWar() { CityWarType = Core.Interfaces.GameServer.CityWarType.PC, GuildId =  MsgSchedules.CityWar.WinnerPC.GuildID, GuildName = MsgSchedules.CityWar.WinnerPC.Name, PoleHitPoints = (uint)MsgSchedules.CityWar.Furnitures[822].HitPoints},
                new CityWar() { CityWarType = Core.Interfaces.GameServer.CityWarType.AC, GuildId =  MsgSchedules.CityWar.WinnerAC.GuildID, GuildName = MsgSchedules.CityWar.WinnerAC.Name, PoleHitPoints = (uint)MsgSchedules.CityWar.Furnitures[823].HitPoints},
                new CityWar() { CityWarType = Core.Interfaces.GameServer.CityWarType.DC, GuildId =  MsgSchedules.CityWar.WinnerDC.GuildID, GuildName = MsgSchedules.CityWar.WinnerDC.Name, PoleHitPoints = (uint)MsgSchedules.CityWar.Furnitures[824].HitPoints},
                new CityWar() { CityWarType = Core.Interfaces.GameServer.CityWarType.BI, GuildId =  MsgSchedules.CityWar.WinnerBI.GuildID, GuildName = MsgSchedules.CityWar.WinnerBI.Name, PoleHitPoints = (uint)MsgSchedules.CityWar.Furnitures[825].HitPoints},
            ];
            totalMigrated = RestApiHelper.AddUpdateCityWars(CityWars);
            MigratedMessage(totalMigrated, "CityWars");
        }
        public void MigrateGuildWars()
        {
            int totalMigrated = 0;
            List<GuildWar> guildWars = new List<GuildWar>();
            //EliteGuildWar Migrate
            IniFileHelper reader = new(Path.Combine(ServerConfig.DbLocation, "Elite.ini"));
            var eliteGW = new GuildWar()
            {
                Type = GuildWarType.EliteGuildWar,
                WinnerGuildID = reader.ReadUInt32("Info", "ID", 0),
                WinnerName = reader.ReadString("Info", "Name", "None"),
                LeaderReward = reader.ReadInt32("Info", "LeaderReward", 0),
                DeputiLeaderReward = reader.ReadInt32("Info", "DeputiLeaderReward", 0),
                RewardLeaders = new List<uint>(),
                RewardDeputies = new List<uint>(),
                PoleHitPoints = reader.ReadInt32("Pole", "HitPoints", 0),
            };
            eliteGW.RewardLeaders.Add(reader.ReadUInt32("Info", "LeaderTop0", 0));
            for (int x = 0; x < 8; x++)
            {
                eliteGW.RewardDeputies.Add(reader.ReadUInt32("Info", "DeputiTop" + x.ToString() + "", 0));
            }
            guildWars.Add(eliteGW);
            //GuildWar Migrate
            IniFileHelper readerGW = new(Path.Combine(ServerConfig.DbLocation, "GuildWarInfo.ini"));
            var guildWar = new GuildWar
            {
                Type = GuildWarType.GuildWar,
                WinnerGuildID = readerGW.ReadUInt32("Info", "ID", 0),
                WinnerName = readerGW.ReadString("Info", "Name", "None"),
                LeaderReward = readerGW.ReadInt32("Info", "LeaderReward", 0),
                DeputiLeaderReward = readerGW.ReadInt32("Info", "DeputiLeaderReward", 0),
                PoleHitPoints = readerGW.ReadInt32("Pole", "HitPoints", 0),
                RewardLeaders = new List<uint>(),
                RewardDeputies = new List<uint>()
            };
            guildWar.RewardLeaders.Add(readerGW.ReadUInt32("Info", "LeaderTop0", 0));
            for (int x = 0; x < 8; x++)
            {
                guildWar.RewardDeputies.Add(readerGW.ReadUInt32("Info", $"DeputiTop{x}", 0));
            }
            ServerConfig.DbFromFiles = true; // Force to load from files for load guildwar data
            MsgSchedules.GuildWar.GuildConductors.Clear(); // Clean data before load
            MsgSchedules.GuildWar.Load();
            guildWar.GuildConductor1 = MsgSchedules.GuildWar.GuildConductors[Game.MsgNpc.NpcID.TeleGuild1].ToString();
            guildWar.GuildConductor2 = MsgSchedules.GuildWar.GuildConductors[Game.MsgNpc.NpcID.TeleGuild2].ToString();
            guildWar.GuildConductor3 = MsgSchedules.GuildWar.GuildConductors[Game.MsgNpc.NpcID.TeleGuild3].ToString();
            guildWar.GuildConductor4 = MsgSchedules.GuildWar.GuildConductors[Game.MsgNpc.NpcID.TeleGuild4].ToString();
            guildWars.Add(guildWar);
            // Save api call
            totalMigrated = RestApiHelper.AddUpdateGuildWars(guildWars);
            MigratedMessage(totalMigrated, "GuildWars");
        }
        public void MigrateVIPShares()
        {
            int totalMigrated = 0;
            List<VIPShare> VIPShares = new List<VIPShare>();
            using (Read Reader = new Read("Share.txt"))
            {
                if (Reader.Reader())
                {
                    uint count = (uint)Reader.Count;
                    for (uint i = 0; i < count; i++)
                    {
                        ReadLine readline = new ReadLine(Reader.ReadString(""), '/');
                        ShareVIP.Client x = new();
                        x.UID = readline.Read((uint)0);
                        x.ShareUID = readline.Read((uint)0);
                        x.ShareName = readline.Read("");
                        x.ShareLevel = readline.Read((byte)0);
                        x.ShareEnds = DateTime.FromBinary(readline.Read((long)0));
                        VIPShares.Add(new VIPShare()
                        {
                            PlayerUID = x.UID,
                            ShareUID = x.ShareUID,
                            ShareLevel = x.ShareLevel,
                            ShareName = x.ShareName,
                            ShareExpiration = x.ShareEnds
                        });
                    }
                }
            }
            totalMigrated = RestApiHelper.AddUpdateVIPShares(VIPShares);
            MigratedMessage(totalMigrated, "VIPShares");
        }
        public void MigrateKOBoardRanks()
        {
            int totalMigrated = 0;
            List<KOBoardRank> KOBoardRanksToMigrate = new List<KOBoardRank>();
            using (Read reader = new Read("KOBoardRanks.txt"))
            {
                if (reader.Reader())
                {
                    int count = reader.Count;
                    for (int x = 0; x < count; x++)
                    {
                        ReadLine line = new ReadLine(reader.ReadString("/"), '/');
                        Entry item = new Entry();
                        item.UID = line.Read((uint)0);
                        item.Name = line.Read("");
                        item.Points = line.Read((uint)0);
                        if (!BaseFunc.UserIsNormal(item.Name) || !BaseFunc.UserExists(item.UID))
                        {
                            continue;
                        }
                        KOBoardRanksToMigrate.Add(new KOBoardRank()
                        {
                            PlayerUID = item.UID,
                            PlayerName = item.Name,
                            Points = item.Points
                        });
                    }
                }
            }
            totalMigrated = RestApiHelper.AddUpdateKOBoardRanks(KOBoardRanksToMigrate);
            MigratedMessage(totalMigrated, "KOBoardRanks");
        }
        public void MigrateAssociates()
        {
            int totalMigrated = 0;
            try
            {
                using (Read r = new Read("Associate.txt"))
                {
                    if (r.Reader())
                    {
                        int count = r.Count;
                        for (uint x = 0; x < count; x++)
                        {
                            string[] data = r.ReadString("").Split('/');
                            uint UID = uint.Parse(data[0]);
                            byte Mod = byte.Parse(data[1]);
                            uint MentorExpBalls = uint.Parse(data[2]);
                            uint MentorBless = uint.Parse(data[3]);
                            uint MentorStone = uint.Parse(data[4]);
                            Member membru = new()
                            {
                                UID = uint.Parse(data[8]),
                                Timer = ulong.Parse(data[9]),
                                ExpBalls = uint.Parse(data[10]),
                                Stone = uint.Parse(data[11]),
                                Blessing = uint.Parse(data[12]),
                                Name = data[13],
                                KillsCount = ushort.Parse(data[14]),
                                BattlePower = ushort.Parse(data[15]),
                                Map = data[16]
                            };
                            if (Associates.ContainsKey(UID))
                            {
                                if (Associates[UID].Associat.ContainsKey(Mod))
                                    Associates[UID].Associat[Mod].TryAdd(membru.UID, membru);
                                else
                                {
                                    Associates[UID].Associat.TryAdd(Mod, new ConcurrentDictionary<uint, Member>());
                                    Associates[UID].Associat[Mod].TryAdd(membru.UID, membru);
                                }
                            }
                            else
                            {
                                MyAsociats assoc = new MyAsociats(UID);
                                assoc.MyUID = UID;
                                assoc.Mentor_ExpBalls = MentorExpBalls;
                                assoc.Mentor_Blessing = MentorBless;
                                assoc.Mentor_Stones = MentorStone;
                                assoc.Associat.TryAdd(Mod, new ConcurrentDictionary<uint, Member>());
                                assoc.Associat[Mod].TryAdd(membru.UID, membru);
                            }
                        }
                    }
                }
                GC.Collect();
                List<Associate> AssociatesToMigrate = new List<Associate>();
                foreach (var assoc in Associates)
                {
                    Associate associateDb = new Associate()
                    {
                        PlayerUID = assoc.Value.MyUID,
                        MentorExpballs = assoc.Value.Mentor_ExpBalls,
                        MentorBlessing = assoc.Value.Mentor_Blessing,
                        MentorStones = assoc.Value.Mentor_Stones,
                        Members = new List<AssociateMember>()
                    };
                    foreach (var mod in assoc.Value.Associat)
                    {
                        foreach (var member in mod.Value)
                        {
                            associateDb.Members.Add(new AssociateMember()
                            {
                                UID = member.Value.UID,
                                Type = (AssociateMemberType)mod.Key,
                                ExpBalls = member.Value.ExpBalls,
                                Stone = member.Value.Stone,
                                Blessing = member.Value.Blessing,
                                Name = member.Value.Name,
                                KillsCount = member.Value.KillsCount,
                                BattlePower = member.Value.BattlePower,
                                MapName = member.Value.Map,
                                Timer = member.Value.Timer
                            });
                        }
                    }
                    AssociatesToMigrate.Add(associateDb);
                }
                totalMigrated = RestApiHelper.AddUpdateAssociates(AssociatesToMigrate);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            MigratedMessage(totalMigrated, "Associates");
        }
        public void MigrateMaps()
        {
            List<GameMap> gameMaps = new List<GameMap>();
            using (var gamemap = new BinaryReader(new FileStream(Path.Combine(ServerConfig.CO2Folder, "ini", "GameMap.dat"), FileMode.Open)))
            {
                var amount = gamemap.ReadInt32();
                for (var i = 0; i < amount; i++)
                {

                    int id = gamemap.ReadInt32();
                    string fileName = Encoding.UTF8.GetString(gamemap.ReadBytes(gamemap.ReadInt32()));
                    int _puzzleSize = gamemap.ReadInt32();
                    if (File.Exists(Path.Combine(ServerConfig.DbLocation, "maps", id + ".ini")))
                    {
                        IniFileHelper reader = new(Path.Combine(ServerConfig.DbLocation, "maps", id + ".ini"));
                        uint mapId = reader.ReadUInt32("info", "id", 0);
                        string mapName = reader.ReadString("info", "name", "");
                        uint mapDoc = reader.ReadUInt32("info", "mapdoc", 0);
                        ulong typeStatus = (uint)reader.ReadUInt64("info", "type", 0);
                        ushort rebornX = reader.ReadUInt16("info", "portal0_x", 0);
                        ushort rebornY = reader.ReadUInt16("info", "portal0_y", 0);
                        ushort rebornMap = reader.ReadUInt16("info", "reborn_map", 0);
                        ushort raceRecord = reader.ReadUInt16("info", "race_record", 0);
                        uint mapColor = reader.ReadUInt32("info", "color", 0);
                        gameMaps.Add(new GameMap() { Name = mapName, MapColor = mapColor, MapDoc = mapDoc, RebornMap = rebornMap, RebornX = rebornX, RebornY = rebornY, RecordSteedRace = raceRecord, TypeStatus = typeStatus, Uid = mapId });
                    }
                }
                GC.Collect();
            }
            RestApiHelper.PostRequestSuccessful("GameMaps/Set", gameMaps);
            MigratedMessage(gameMaps.Count(), "GameMaps");
        }
        public void MigratePortals()
        {
            List<Portal> portals = new List<Portal>();
            if (File.Exists(Path.Combine(ServerConfig.DbLocation, "portals.ini")))
            {
                using (StreamReader read = File.OpenText(Path.Combine(ServerConfig.DbLocation, "portals.ini")))
                {
                    while (true)
                    {
                        string lines = read.ReadLine();
                        if (lines == null)
                            break;
                        //ushort Map = ushort.Parse(lines.Split('[')[1].ToString().Split(']')[0]);
                        ushort Count = ushort.Parse(read.ReadLine().Split('=')[1]);
                        for (ushort x = 0; x < Count; x++)
                        {
                            Role.Portal portal = new Role.Portal();
                            string[] line = read.ReadLine().Split('=')[1].Split(' ');
                            portal.MapID = ushort.Parse(line[0]);
                            portal.X = ushort.Parse(line[1]);
                            portal.Y = ushort.Parse(line[2]);
                            string[] dline = read.ReadLine().Split('=')[1].Split(' ');
                            portal.Destiantion_MapID = ushort.Parse(dline[0]);
                            portal.Destiantion_X = ushort.Parse(dline[1]);
                            portal.Destiantion_Y = ushort.Parse(dline[2]);
                            portals.Add(new Portal() { DestinationMapID = portal.Destiantion_MapID, DestinationX = portal.Destiantion_X, DestinationY = portal.Destiantion_Y, MapID = portal.MapID, X = portal.X, Y = portal.Y });
                        }
                    }
                }
            }
            GC.Collect();
            RestApiHelper.PostRequestSuccessful("Portals/Set", portals);
            MigratedMessage(portals.Count(), "Portals");
        }
        public void MigrateGuilds()
        {
            List<Guild> guilds = new List<Guild>();
            foreach (string fname in System.IO.Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Guilds")))
            {
                using (Database.DBActions.Read reader = new Database.DBActions.Read(fname, true))
                {
                    if (reader.Reader())
                    {
                        //--------- guild info ------------------
                        Database.DBActions.ReadLine GuildReader = new Database.DBActions.ReadLine(reader.ReadString("0/"), '/');
                        uint ID = GuildReader.Read((uint)0);
                        if (ID > 100000)
                            continue;
                        if (ID > Role.Instance.Guild.Counter.Count)
                            Role.Instance.Guild.Counter.Set(ID);
                        string Name = GuildReader.Read("None");
                        Role.Instance.Guild guild = new Role.Instance.Guild(null, Name, null);
                        guild.Info.GuildID = ID;
                        guild.Info.LeaderName = GuildReader.Read("None");
                        guild.Info.SilverFund = GuildReader.Read((long)0);
                        guild.Info.ConquerPointFund = GuildReader.Read((uint)0);
                        guild.Info.CreateTime = GuildReader.Read((uint)0);
                        guild.Bulletin = GuildReader.Read("None");
                        guild.UseAdvertise = GuildReader.Read((byte)0) == 1;
                        guild.BuletinEnrole = GuildReader.Read((int)0);
                        guild.Recruit.Load(reader.ReadString("0/"));
                        guild.AdvertiseRecruit.Load(reader.ReadString("0/"));
                        string Allies = reader.ReadString("0/");
                        string Enemies = reader.ReadString("0/");
                        Database.GuildTable.LoadGuildAlly(ID, Allies);
                        Database.GuildTable.LoadGuildEnemy(ID, Enemies);
                        string myArsenalStr = reader.ReadString("0/");
                        //guild.MyArsenal.Load(myArsenalStr);
                        try
                        {
                            guild.CTF_Exploits = reader.ReadUInt32(0);
                        }
                        catch
                        {
                            guild.CTF_Exploits = 0;
                        }
                        try
                        {
                            guild.CTF_Next_ConquerPoints = reader.ReadUInt32(0);
                            guild.CTF_Next_Money = reader.ReadUInt32(0);
                            guild.CTF_Rank = reader.ReadUInt32(0);
                            guild.ClaimCtfReward = reader.ReadUInt32(0);
                        }
                        catch
                        {
                        }
                        Guild g = _mapper.Map<Guild>(guild);
                        g.GuildName = Name;
                        //g.Allies = Allies;
                        //g.Enemies = Enemies;
                        //g.Arsenal = myArsenalStr;
                        guilds.Add(g);
                    }
                }
            }
            RestApiHelper.PostRequestSuccessful("Guilds/Set", guilds);
            MigratedMessage(guilds.Count(), "Guilds");
        }
        public void MigrateClans()
        {
            ClanTable.MigrateToSql();
        }
        public void MigrateTutorType()
        {
            List<TutorType> toMigrate = new List<TutorType>();
            string[] baseText = File.ReadAllLines(Path.Combine(ServerConfig.DbLocation, "cq_tutor_type.txt"));
            foreach (var bas_line in baseText)
            {
                string[] line = bas_line.Split(',');
                TutorType obj = new TutorType();
                obj.Index = uint.Parse(line[0]);
                obj.MinLevel = uint.Parse(line[1]);
                obj.MaxLevel = uint.Parse(line[2]);
                obj.StudentNum = uint.Parse(line[3]);
                obj.BattleLevShare = uint.Parse(line[4]);
                toMigrate.Add(obj);
            }
            RestApiHelper.PostRequestSuccessful("TutorType/Set", toMigrate);
            MigratedMessage(toMigrate.Count, "TutorTypes");
        }
        public void MigrateTutorBattleLimitType()
        {
            List<TutorBattleLimitType> toMigrate = new List<TutorBattleLimitType>();
            string[] baseText = File.ReadAllLines(Path.Combine(ServerConfig.DbLocation, "cq_tutor_battle_limit_type.txt"));
            foreach (var bas_line in baseText)
            {
                string[] line = bas_line.Split(',');
                toMigrate.Add(new TutorBattleLimitType() { BP = uint.Parse(line[0]) , SharedBPLimit = uint.Parse(line[1]) });
            }
            RestApiHelper.PostRequestSuccessful("TutorBattleLimitType/Set", toMigrate);
            MigratedMessage(toMigrate.Count, "TutorBattleLimitTypes");
        }
        public void MigrateTransformations()
        {
            List<Transformation> toMigrate = new List<Transformation>();
            TransformInfo = new Dictionary<ushort, Dictionary<byte, DBTranform>>();
            string[] baseText = File.ReadAllLines(Path.Combine(ServerConfig.DbLocation, "TransformInfo.txt"));
            foreach (string aline in baseText)
            {
                string[] line = aline.Split(' ');
                DBTranform info = new DBTranform()
                {
                    SpellID = ushort.Parse(line[0]),
                    Level = byte.Parse(line[1]),
                    Name = line[2],
                    ID = ushort.Parse(line[3]),
                    HitPoints = ushort.Parse(line[4])
                };
                if (TransformInfo.ContainsKey(info.SpellID))
                {
                    TransformInfo[info.SpellID].Add(info.Level, info);
                    toMigrate.Add(new Transformation() { SpellID = info.SpellID, HitPoints = info.HitPoints, TransformID = info.ID, Level = info.Level, Name = info.Name  });
                }
                else
                {
                    TransformInfo.Add(info.SpellID, new Dictionary<byte, DBTranform>());
                    TransformInfo[info.SpellID].Add(info.Level, info);
                    toMigrate.Add(new Transformation() { SpellID = info.SpellID, HitPoints = info.HitPoints, TransformID = info.ID, Level = info.Level, Name = info.Name });
                }
            }
            RestApiHelper.PostRequestSuccessful("Transformation/Set", toMigrate);
            MigratedMessage(toMigrate.Count, "Transforms");
        }
        public void MigrateCrimes()
        {
            List<Crime> toMigrate = new List<Crime>();
            using (Database.DBActions.Read Reader = new Database.DBActions.Read("Crime.ini"))
            {
                if (Reader.Reader())
                {
                    for (int x = 0; x < Reader.Count; x++)
                    {
                        string Line = Reader.ReadString("^");
                        Database.DBActions.ReadLine Readline = new Database.DBActions.ReadLine(Line.Split('^')[1], '/');
                        string OwnerName = Readline.Read("");
                        uint OwnerUID = Readline.Read((uint)0);
                        uint Money = (uint)Readline.Read(0);
                        toMigrate.Add(new Crime() { OwnerName = OwnerName, Money = Money, OwnerUID = OwnerUID });
                    }
                }
            }
            RestApiHelper.PostRequestSuccessful("Crime/Set", toMigrate);
            MigratedMessage(toMigrate.Count, "Crimes");
        }
        public void MigrateArenaAndTeamArena()
        {
            List<ArenaUser> toMigrate = new List<ArenaUser>();
            List<ArenaUser> toMigrateTeam = new List<ArenaUser>();
            using (Database.DBActions.Read reader = new Database.DBActions.Read("Arena.ini"))
            {
                if (reader.Reader())
                {
                    for (int i = 0; i < reader.Count; i++)
                    {
                        Game.MsgTournaments.MsgArena.User user = new Game.MsgTournaments.MsgArena.User();
                        user.Load(reader.ReadString(""));
                        toMigrate.Add(new ArenaUser() { ArenaPoints = user.Info.ArenaPoints, Class = user.Class, CurrentHonor = user.Info.CurrentHonor, HistoryHonor = user.Info.HistoryHonor, LastSeasonArenaPoints = user.LastSeasonArenaPoints, LastSeasonLose = user.LastSeasonLose, LastSeasonRank = user.LastSeasonRank, LastSeasonWin = user.LastSeasonWin, Level = user.Level, Mesh = user.Mesh, Name = user.Name, TodayBattles = user.Info.TodayBattles, TodayWin = user.Info.TodayWin, TotalLose = user.Info.TotalLose, TotalWin = user.Info.TotalWin, UID = user.UID });
                    }
                }
            }
            RestApiHelper.PostRequestSuccessful("ArenaUser/Set", toMigrate);
            using (Database.DBActions.Read reader = new Database.DBActions.Read("TeamArena.ini"))
            {
                if (reader.Reader())
                {
                    for (int i = 0; i < reader.Count; i++)
                    {
                        Game.MsgTournaments.MsgTeamArena.User user = new Game.MsgTournaments.MsgTeamArena.User();
                        user.Load(reader.ReadString(""));
                        toMigrateTeam.Add(new ArenaUser() { ArenaPoints = user.Info.ArenaPoints, Class = user.Class, CurrentHonor = user.Info.CurrentHonor, HistoryHonor = user.Info.HistoryHonor, LastSeasonArenaPoints = user.LastSeasonArenaPoints, LastSeasonLose = user.LastSeasonLose, LastSeasonRank = user.LastSeasonRank, LastSeasonWin = user.LastSeasonWin, Level = user.Level, Mesh = user.Mesh, Name = user.Name, TodayBattles = user.Info.TodayBattles, TodayWin = user.Info.TodayWin, TotalLose = user.Info.TotalLose, TotalWin = user.Info.TotalWin, UID = user.UID });
                    }
                }
            }
            MigratedMessage(toMigrate.Count, "Arenas");
            RestApiHelper.PostRequestSuccessful("TeamArenaUser/Set", toMigrateTeam);
            MigratedMessage(toMigrate.Count, "TeamArenas");
        }
        public void MigrateStaticStatues()
        {
            List<StaticStatue> toMigrate = new List<StaticStatue>();
            using (Database.DBActions.Read r = new Database.DBActions.Read("StaticStatue.txt"))
            {
                if (r.Reader())
                {
                    int count = r.Count;
                    for (uint x = 0; x < count; x++)
                    {
                        Database.DBActions.ReadLine readerline = new Database.DBActions.ReadLine(r.ReadString(""), '/');
                        int Size = readerline.Read((int)0);
                        if (Size != 0)
                        {
                            StaticStatue staticStatue = new StaticStatue() { StatueSize = (uint)Size };
                            for (int i = 0; i < staticStatue.StatueSize; i++)
                                staticStatue.StatuePackets += readerline.Read((byte)0) + "/";
                            staticStatue.StatuePackets = staticStatue.StatuePackets.TrimEnd('/');
                            staticStatue.ObjType = MapObjectType.SobNpc;
                            staticStatue.UID = readerline.Read((uint)0);
                            staticStatue.X = readerline.Read((ushort)0);
                            staticStatue.Y = readerline.Read((ushort)0);
                            staticStatue.Map = readerline.Read((ushort)0);
                            staticStatue.MaxHitPoints = (uint)readerline.Read(0);
                            staticStatue.HitPoints = (uint)readerline.Read(0);
                            toMigrate.Add(staticStatue);
                        }
                    }
                }
            }
            RestApiHelper.PostRequestSuccessful("StaticStatue/Set", toMigrate);
            MigratedMessage(toMigrate.Count, "StaticStatues");
        }
        public bool InitMigration()
        {
            Console.WriteLine("With migration lose data from that systems: [Quests, BanUID, BanIP, ClanWars, Houses]", ConsoleColor.DarkYellow);
            Console.WriteLine($"System to migrate: [all/{string.Join("/", migrationSystems)}]: ", ConsoleColor.DarkBlue);
            bool canConnectToDb = Utils.CanConnectGS();
            string specificSystem = Console.ReadLine();
            if (specificSystem.Trim().ToLower() == "all")
            {
                specificSystem = "";
            }
            if (canConnectToDb)
            {
                Utils.GSMigrationInit();
                canConnectToDb = Utils.CanConnectGS();
            }
            if (canConnectToDb)
            {
                if (specificSystem.Length > 0)
                {
                    switch (specificSystem)
                    {
                        case "npcs":
                            {
                                MigrateNPCs();
                                break;
                            }
                        case "maps":
                            {
                                MigrateMaps();
                                break;
                            }
                        case "portals":
                            {
                                MigratePortals();
                                break;
                            }
                        case "guilds":
                            {
                                MigrateGuilds();
                                break;
                            }
                        case "clans":
                            {
                                MigrateClans();
                                break;
                            }
                        case "tutors":
                            {
                                MigrateTutorType();
                                break;
                            }
                        case "tutorbattle":
                            {
                                MigrateTutorBattleLimitType();
                                break;
                            }
                        case "transformations":
                            {
                                MigrateTransformations();
                                break;
                            }
                        case "crimes":
                            {
                                MigrateCrimes();
                                break;
                            }
                        case "arena":
                            {
                                MigrateArenaAndTeamArena();
                                break;
                            }
                        case "staticstatues":
                            {
                                MigrateStaticStatues();
                                break;
                            }
                        case "players":
                            {
                                MigratePlayers();
                                break;
                            }
                        case "playeritems":
                            {
                                MigratePlayerItems();
                                break;
                            }
                        case "playerspells":
                            {
                                MigratePlayerSpells();
                                break;
                            }
                        case "playerprofs":
                            {
                                MigratePlayerProfs();
                                break;
                            }
                        case "vendorshops":
                            {
                                MigrateVendorShops();
                                break;
                            }
                        case "teamelitepks":
                            {
                                MigrateTeamElitePKS();
                                break;
                            }
                        case "classpkwars":
                            {
                                MigrateClassPKWars();
                                break;
                            }
                        case "elitepks":
                            {
                                MigrateElitePKS();
                                break;
                            }
                        case "couples":
                            {
                                MigrateCouples();
                                break;
                            }
                        case "citywars":
                            {
                                MigrateCityWars();
                                break;
                            }
                        case "guildwars":
                            {
                                MigrateGuildWars();
                                break;
                            }
                        case "vipshares":
                            {
                                MigrateVIPShares();
                                break;
                            }
                        case "koboardranks":
                            {
                                MigrateKOBoardRanks();
                                break;
                            }
                        case "associates":
                            {
                                MigrateAssociates();
                                break;
                            }
                    }
                } else
                {
                    MigrateNPCs();
                    MigrateMaps();
                    MigratePortals();
                    MigrateGuilds();
                    MigrateClans();
                    MigrateTutorType();
                    MigrateTutorBattleLimitType();
                    MigrateTransformations();
                    MigrateCrimes();
                    MigrateArenaAndTeamArena();
                    MigrateStaticStatues();
                    MigratePlayers();
                    MigratePlayerItems();
                    MigratePlayerSpells();
                    MigratePlayerProfs();
                    MigrateVendorShops();
                    MigrateTeamElitePKS();
                    MigrateClassPKWars();
                    MigrateElitePKS();
                    MigrateCouples();
                    MigrateCityWars();
                    MigrateGuildWars();
                    MigrateVIPShares();
                    MigrateKOBoardRanks();
                    MigrateAssociates();
                }
            } else
            {
                Console.WriteLine("--- INFO MIGRATION ---", ConsoleColor.White);
                Console.WriteLine("Cannot connect to MySQL. Check configuration.", ConsoleColor.DarkRed);
                Console.WriteLine("--- INFO MIGRATION ---", ConsoleColor.White);
            }
            return canConnectToDb;
        }
        public void MigratedMessage(int TotalMigrated, string MigrationType)
        {
            Console.WriteLine($"Migrated to Mysql: {TotalMigrated} {MigrationType}", ConsoleColor.DarkGreen);
        }
    }
}
