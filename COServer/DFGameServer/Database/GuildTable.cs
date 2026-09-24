using Core;
using Core.Models.GameServer;
using GameServer.MadeByDaRkFox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameServer.Database
{
    public class GuildTable
    {
        private static SqlMigrator migrator = new SqlMigrator();
        //save ----------------------
        internal static void Save()
        {
            List<Core.Models.GameServer.Guild> guildToDb = new List<Core.Models.GameServer.Guild>();
            foreach (var obj in Role.Instance.Guild.GuildPoll)
            {
                if (obj.Value.CanSave == false)
                    continue;
                var guild = obj.Value;
                if (ServerConfig.DbFromFiles)
                {
                    using (DBActions.Write writer = new DBActions.Write(Path.Combine("Guilds", obj.Key + ".txt")))
                    {
                        writer.Add(guild.ToString()).Add(guild.Recruit.ToString())
                            .Add(guild.AdvertiseRecruit.ToString()).Add(ToStringAlly(guild)).Add(ToStringEnemy(guild))
                            .Add(guild.MyArsenal.ToString())
                            .Add(guild.CTF_Exploits.ToString())
                            .Add(guild.CTF_Next_ConquerPoints.ToString())
                            .Add(guild.CTF_Next_Money.ToString())
                            .Add(guild.CTF_Rank.ToString())
                            .Add(guild.ClaimCtfReward.ToString());
                        writer.Execute(DBActions.Mode.Open);
                    }
                } else
                {
                    Core.Models.GameServer.Guild g = migrator.Mapper.Map<Core.Models.GameServer.Guild>(guild);
                    g.GuildName = guild.GuildName;
                    g.AdvertiseRecruit = guild.AdvertiseRecruit.ToString();
                    g.Recruit = guild.Recruit.ToString();
                    g.Allies = ToStringAlly(guild);
                    g.Enemies = ToStringAlly(guild);
                    guildToDb.Add(g);
                }
            }
            if (!ServerConfig.DbFromFiles)
            {
                RestApiHelper.PostRequestSuccessful("Guilds/Update", guildToDb);
            }
        }
        public static string ToStringAlly(Role.Instance.Guild guild)
        {
            DBActions.WriteLine writer = new DBActions.WriteLine('/');
            writer.Add(guild.Ally.Count);
            foreach (Role.Instance.Guild ally in guild.Ally.Values)
                writer.Add(ally.Info.GuildID);
            return writer.Close();
        }
        public static string ToStringEnemy(Role.Instance.Guild guild)
        {
            DBActions.WriteLine writer = new DBActions.WriteLine('/');
            writer.Add(guild.Enemy.Count);
            foreach (Role.Instance.Guild enemy in guild.Enemy.Values)
                writer.Add(enemy.Info.GuildID);
            return writer.Close();
        }
        //------------------------


        internal static void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                foreach (string fname in System.IO.Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Guilds")))
                {
                    using (DBActions.Read reader = new DBActions.Read(fname, true))
                    {
                        if (reader.Reader())
                        {
                            //--------- guild info ------------------
                            DBActions.ReadLine GuildReader = new DBActions.ReadLine(reader.ReadString("0/"), '/');
                            uint ID = GuildReader.Read((uint)0);
                            if (ID > 100000)
                                continue;
                            if (ID > Role.Instance.Guild.Counter.Count)
                                Role.Instance.Guild.Counter.Set(ID);
                            Role.Instance.Guild guild = new Role.Instance.Guild(null, GuildReader.Read("None"), null);
                            guild.Info.GuildID = ID;
                            guild.Info.LeaderName = GuildReader.Read("None");
                            guild.Info.SilverFund = GuildReader.Read((long)0);
                            guild.Info.ConquerPointFund = GuildReader.Read((uint)0);
                            guild.Info.CreateTime = GuildReader.Read((uint)0);
                            guild.Bulletin = GuildReader.Read("None");
                            guild.UseAdvertise = GuildReader.Read((byte)0) == 1;
                            guild.BuletinEnrole = GuildReader.Read((int)0);


                            //----------------------------------

                            //----------load requit and advertise ----------------
                            guild.Recruit.Load(reader.ReadString("0/"));
                            guild.AdvertiseRecruit.Load(reader.ReadString("0/"));
                            //----------------------------------------------------

                            //---------load ally ---------------------
                            LoadGuildAlly(ID, reader.ReadString("0/"));
                            //-----------------------------------

                            //---------load enemy --------------------
                            LoadGuildEnemy(ID, reader.ReadString("0/"));
                            //----------------------------------------

                            //---------load arsenals ------------------
                            guild.MyArsenal.Load(reader.ReadString("0/"));
                            //-----------------------------------------
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
                            if (guild.UseAdvertise)
                                Role.Instance.Guild.Advertise.Add(guild);
                            if (!Role.Instance.Guild.GuildPoll.ContainsKey(guild.Info.GuildID))
                                Role.Instance.Guild.GuildPoll.TryAdd(guild.Info.GuildID, guild);
                            guild.MyArsenal.CheckLoad();



                        }
                    }
                }
                ExecuteAllyAndEnemy();
                LoadMembers();
                LoadArsenals();
                LoadArsenals();
                foreach (var guilds in Role.Instance.Guild.GuildPoll.Values)
                {
                    guilds.CreateMembersRank();
                    guilds.UpdateGuildInfo();
                }
                Pool.LastGuildPulse = DateTime.Now;
                enemy.Clear();
                ally.Clear();
                Console.WriteLine("Loading [" + Role.Instance.Guild.GuildPoll.Count + "] Guilds");
                GC.Collect();
            } else
            {
                List<Core.Models.GameServer.Guild> guilds = RestApiHelper.GetRequest<List<Core.Models.GameServer.Guild>>("Guilds/Get");
                foreach(Core.Models.GameServer.Guild guild in guilds)
                {
                    Role.Instance.Guild g = new Role.Instance.Guild(null, guild.GuildName, null);
                    g.Info.GuildID = guild.GuildID;
                    g.Info.LeaderName = guild.LeaderName;
                    g.Info.SilverFund = guild.SilverFund;
                    g.Info.ConquerPointFund = guild.ConquerPointFund;
                    g.Info.CreateTime = guild.CreateTime;
                    g.Bulletin = guild.Bulletin;
                    g.UseAdvertise = guild.UseAdvertise;
                    g.BuletinEnrole = (int)guild.BuletinEnrole;
                    g.Recruit.Load(guild.Recruit);
                    g.AdvertiseRecruit.Load(guild.AdvertiseRecruit);
                    LoadGuildAlly(g.Info.GuildID, guild.Allies);
                    LoadGuildEnemy(g.Info.GuildID, guild.Enemies);
                    g.MyArsenal.Load(guild.Arsenal);
                    g.CTF_Exploits = guild.CTFExploits;
                    try
                    {
                        g.CTF_Next_ConquerPoints = guild.CTFNextConquerPoints;
                        g.CTF_Next_Money = guild.CTFNextMoney;
                        g.CTF_Rank = guild.CTFRank;
                        g.ClaimCtfReward = guild.ClaimCtfReward;
                    }
                    catch
                    {

                    }
                    g.GuildName = guild.GuildName;
                    if (guild.UseAdvertise)
                        Role.Instance.Guild.Advertise.Add(g);
                    if (!Role.Instance.Guild.GuildPoll.ContainsKey(g.Info.GuildID))
                        Role.Instance.Guild.GuildPoll.TryAdd(g.Info.GuildID, g);
                    g.MyArsenal.CheckLoad();
                }
                ExecuteAllyAndEnemy();
                LoadMembers();
                LoadArsenals();
                LoadArsenals();
                foreach (var guildsValue in Role.Instance.Guild.GuildPoll.Values)
                {
                    guildsValue.CreateMembersRank();
                    guildsValue.UpdateGuildInfo();
                }
                Pool.LastGuildPulse = DateTime.Now;
                enemy.Clear();
                ally.Clear();
                Console.WriteLine("Loading [" + Role.Instance.Guild.GuildPoll.Count + "] Guilds");
            }
        }
        private unsafe static void LoadArsenals()
        {
            if (ServerConfig.DbFromFiles)
            {
                foreach (var guild in Role.Instance.Guild.GuildPoll.Values)
                {
                    foreach (var member in guild.Members.Values)
                    {
                        BinaryFileHelper binary = new();
                        if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, "PlayersItems", member.UID + ".bin"), System.IO.FileMode.Open))
                        {
                            ClientItems.DBItem Item;
                            int ItemCount = binary.ReadInt();
                            for (int x = 0; x < ItemCount; x++)
                            {
                                Item = binary.Read<ClientItems.DBItem>();
                                Game.MsgServer.MsgGameItem ClienItem = Item.GetDataItem();
                                if (ClienItem.Inscribed == 1)
                                {
                                    guild.MyArsenal.Add(Role.Instance.Guild.Arsenal.GetArsenalPosition(ClienItem.ITEM_ID)
                                        , new Role.Instance.Guild.Arsenal.InscribeItem() { BaseItem = ClienItem, Name = member.Name, UID = member.UID });
                                    member.ArsenalDonation += GetItemDonation(ClienItem);
                                }
                            }
                            binary.Close();
                        }
                    }
                }
            } else
            {
                foreach (var guild in Role.Instance.Guild.GuildPoll.Values)
                {
                    foreach (var member in guild.Members.Values)
                    {
                        List<PlayerItem> playerItems = RestApiHelper.GetPlayerItems(member.UID);
                        foreach(PlayerItem Item in playerItems.Where(x => x.Inscribed == 1))
                        {
                            Game.MsgServer.MsgGameItem ClientItem = new();
                            ClientItem.UID = Item.Uid;
                            ClientItem.ITEM_ID = Item.ItemId;
                            ClientItem.Durability = Item.Durability;
                            ClientItem.MaximDurability = Item.MaxDurability;
                            ClientItem.Position = Item.Position;
                            ClientItem.SocketProgress = Item.SocketProgress;
                            ClientItem.SocketOne = (Role.Flags.Gem)Item.SocketOne;
                            ClientItem.SocketTwo = (Role.Flags.Gem)Item.SocketTwo;
                            ClientItem.Effect = (Role.Flags.ItemEffect)Item.Effect;
                            ClientItem.Plus = Item.Plus;
                            ClientItem.Bless = Item.Bless;
                            ClientItem.Bound = Item.Bound;
                            ClientItem.Enchant = Item.Enchant;
                            ClientItem.Suspicious = 0; // Era 1: itens suspeitos vieram no Patch 5022
                            ClientItem.Locked = Item.Locked;
                            ClientItem.PlusProgress = Item.PlusProgress;
                            ClientItem.Inscribed = Item.Inscribed;
                            ClientItem.Activate = Item.Activate;
                            ClientItem.TimeLeftInMinutes = Item.TimeLeftInMinutes;
                            ClientItem.StackSize = Item.StackSize;
                            ClientItem.WH_ID = Item.WarehouseId;
                            ClientItem.Color = (Role.Flags.Color)Item.Color;
                            ClientItem.IDEvent = Item.IDEvent;
                            guild.MyArsenal.Add(Role.Instance.Guild.Arsenal.GetArsenalPosition(Item.ItemId)
                                       , new Role.Instance.Guild.Arsenal.InscribeItem() { BaseItem = ClientItem, Name = member.Name, UID = member.UID });
                            member.ArsenalDonation += GetItemDonation(ClientItem);
                        }
                    }
                }
            }
        }
        private static uint GetItemDonation(Game.MsgServer.MsgGameItem Item)//1395660 on full item
        {
            uint Return = 0;
            int id = (int)(Item.ITEM_ID % 10);
            switch (id)
            {
                case 8: Return = 1000; break;
                case 9: Return = 16660; break;
            }
            if (Item.SocketOne > 0 && Item.SocketTwo == 0)
                Return += 33330;
            if (Item.SocketOne > 0 && Item.SocketTwo > 0)
                Return += 133330;

            switch (Item.Plus)
            {
                case 1: Return += 90; break;
                case 2: Return += 490; break;
                case 3: Return += 1350; break;
                case 4: Return += 4070; break;
                case 5: Return += 12340; break;
                case 6: Return += 37030; break;
                case 7: Return += 111110; break;
                case 8: Return += 333330; break;
                case 9: Return += 1000000; break;
                case 10: Return += 1033330; break;
                case 11: Return += 1101230; break;
                case 12: Return += 1212340; break;
                default: break;
            }

            return Return;
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
                    uint GuildID = ini.ReadUInt32("Character", "GuildID", 0);
                    if (GuildID != 0)
                    {
                        Role.Instance.Guild Guild;
                        if (Role.Instance.Guild.GuildPoll.TryGetValue(GuildID, out Guild))
                        {
                            ushort Body = ini.ReadUInt16("Character", "Body", 1002);
                            ushort Face = ini.ReadUInt16("Character", "Face", 0);

                            Role.Instance.Guild.Member member = new Role.Instance.Guild.Member();
                            member.UID = UID;
                            member.Mesh = (uint)(Face * 10000 + Body);
                            member.Name = Name;
                            member.Rank = (Role.Flags.GuildMemberRank)ini.ReadUInt32("Character", "GuildRank", 200);
                            member.Class = ini.ReadByte("Character", "Class", 0);
                            member.CpsDonate = ini.ReadUInt32("Character", "CpsDonate", 0);
                            member.MoneyDonate = ini.ReadInt64("Character", "MoneyDonate", 0);
                            member.PkDonation = ini.ReadUInt32("Character", "PkDonation", 0);
                            member.LastLogin = ini.ReadInt64("Character", "LastLogin", 0);
                            member.Level = ini.ReadUInt16("Character", "Level", 0);
                            //------------------------------- CTF--------------
                            member.CTF_Exploits = ini.ReadUInt32("Character", "CTF_Exploits", 0);
                            member.RewardConquerPoints = ini.ReadUInt32("Character", "CTF_RCPS", 0);
                            member.RewardMoney = ini.ReadUInt32("Character", "CTF_RM", 0);
                            member.CTF_Claimed = ini.ReadByte("Character", "CTF_R", 0);
                            //-----------------------------------------------


                            Role.Instance.Flowers flower;
                            if (Role.Instance.Flowers.ClientPoll.TryGetValue(UID, out flower))
                            {
                                member.Lilies = flower.Lilies;
                                member.Orchids = flower.Orchids;
                                member.Rouses = flower.RedRoses;
                                member.Tulips = flower.Tulips;
                            }
                            ulong nobilitydonation = ini.ReadUInt64("Character", "DonationNobility", 0);
                            Role.Instance.Nobility nobility;
                            if (Pool.NobilityRanking.TryGetValue(UID, out nobility))
                            {
                                member.NobilityRank = (uint)nobility.Rank;
                            }
                            else
                            {
                                if (nobilitydonation >= 200000000)
                                    member.NobilityRank = (uint)Role.Instance.Nobility.NobilityRank.Earl;
                                else if (nobilitydonation >= 100000000)
                                    member.NobilityRank = (uint)Role.Instance.Nobility.NobilityRank.Baron;
                                else if (nobilitydonation >= 30000000)
                                    member.NobilityRank = (uint)Role.Instance.Nobility.NobilityRank.Knight;
                            }
                            if (!Guild.Members.ContainsKey(member.UID))
                                Guild.Members.TryAdd(member.UID, member);
                        }
                    }
                }
            } else
            {
                List<Player> players = RestApiHelper.GetPlayers();
                foreach(Player player in players)
                {
                    uint UID = player.UID;
                    string Name = player.Name;
                    uint GuildID = player.GuildID;
                    if (GuildID != 0)
                    {
                        Role.Instance.Guild Guild;
                        if (Role.Instance.Guild.GuildPoll.TryGetValue(GuildID, out Guild))
                        {
                            ushort Body = player.Body;
                            ushort Face = player.Face;
                            Role.Instance.Guild.Member member = new Role.Instance.Guild.Member();
                            member.UID = UID;
                            member.Mesh = (uint)(Face * 10000 + Body);
                            member.Name = Name;
                            member.Rank = (Role.Flags.GuildMemberRank)player.GuildRank;
                            member.Class = player.Class;
                            member.CpsDonate = player.GuildCPsDonate;
                            member.MoneyDonate = (long)player.GuildMoneyDonate;
                            member.PkDonation = player.GuildPKDonation;
                            member.LastLogin = (long)player.GuildLastLogin;
                            member.Level = player.Level;
                            //------------------------------- CTF--------------
                            member.CTF_Exploits = player.GuildCTFExploits;
                            member.RewardConquerPoints = player.GuildCTFConquerPointsReward;
                            member.RewardMoney = player.GuildCTFMoneyReward;
                            member.CTF_Claimed = player.GuildCTFClaimed;
                            //-----------------------------------------------
                            Role.Instance.Flowers flower;
                            if (Role.Instance.Flowers.ClientPoll.TryGetValue(UID, out flower))
                            {
                                member.Lilies = flower.Lilies;
                                member.Orchids = flower.Orchids;
                                member.Rouses = flower.RedRoses;
                                member.Tulips = flower.Tulips;
                            }
                            ulong nobilitydonation = player.NobilityDonation;
                            Role.Instance.Nobility nobility;
                            if (Pool.NobilityRanking.TryGetValue(UID, out nobility))
                            {
                                member.NobilityRank = (uint)nobility.Rank;
                            }
                            else
                            {
                                if (nobilitydonation >= 200000000)
                                    member.NobilityRank = (uint)Role.Instance.Nobility.NobilityRank.Earl;
                                else if (nobilitydonation >= 100000000)
                                    member.NobilityRank = (uint)Role.Instance.Nobility.NobilityRank.Baron;
                                else if (nobilitydonation >= 30000000)
                                    member.NobilityRank = (uint)Role.Instance.Nobility.NobilityRank.Knight;
                            }
                            if (!Guild.Members.ContainsKey(member.UID))
                                Guild.Members.TryAdd(member.UID, member);
                        }
                    }
                }
            }
        }
        public static void ExecuteAllyAndEnemy()
        {
            foreach (var obj in ally)
            {
                foreach (var guild in obj.Value)
                {
                    Role.Instance.Guild alyguild;
                    if (Role.Instance.Guild.GuildPoll.TryGetValue(guild, out alyguild))
                    {
                        if (Role.Instance.Guild.GuildPoll.ContainsKey(obj.Key))
                        {
                            Role.Instance.Guild.GuildPoll[obj.Key].Ally.TryAdd(alyguild.Info.GuildID, alyguild);
                        }
                    }
                }
            }
            foreach (var obj in enemy)
            {
                foreach (var guild in obj.Value)
                {
                    Role.Instance.Guild alyenemy;
                    if (Role.Instance.Guild.GuildPoll.TryGetValue(guild, out alyenemy))
                    {
                        if (Role.Instance.Guild.GuildPoll.ContainsKey(obj.Key))
                        {
                            Role.Instance.Guild.GuildPoll[obj.Key].Enemy.TryAdd(alyenemy.Info.GuildID, alyenemy);
                        }
                    }
                }
            }
        }
        public static Dictionary<uint, List<uint>> ally = new Dictionary<uint, List<uint>>();
        public static Dictionary<uint, List<uint>> enemy = new Dictionary<uint, List<uint>>();
        public static void LoadGuildAlly(uint id, string line)
        {
            Database.DBActions.ReadLine reader = new DBActions.ReadLine(line, '/');
            int count = reader.Read(0);
            for (int x = 0; x < count; x++)
            {
                if (ally.ContainsKey(id))
                    ally[id].Add(reader.Read((uint)0));
                else
                    ally.Add(id, new List<uint>() { reader.Read((uint)0) });
            }
        }
        public static void LoadGuildEnemy(uint id, string line)
        {
            Database.DBActions.ReadLine reader = new DBActions.ReadLine(line, '/');
            int count = reader.Read(0);
            for (int x = 0; x < count; x++)
            {
                if (enemy.ContainsKey(id))
                    enemy[id].Add(reader.Read((uint)0));
                else
                    enemy.Add(id, new List<uint>() { reader.Read((uint)0) });
            }
        }
    }
}
