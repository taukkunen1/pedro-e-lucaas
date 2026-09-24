using Core;
using Core.Interfaces.GameServer;
using Core.Models.GameServer;
using GameServer.Role.Instance;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static GameServer.Database.ClientSpells;
using static GameServer.Database.Server;
using static GameServer.Role.Flags;
namespace GameServer.Database
{
    public class ServerDatabase
    {
        public static void SaveDBPayers(DateTime clock)
        {
            if (Pool.FullLoading && !ServerConfig.IsInterServer)
            {
                foreach (var user in Pool.GamePoll.Values)
                {
                    if (user.OnInterServer)
                        continue;
                    if ((user.ClientFlag & Client.ServerFlag.LoginFull) == Client.ServerFlag.LoginFull)
                    {
                        user.ClientFlag |= Client.ServerFlag.QueuesSave;
                        Database.ServerDatabase.LoginQueue.TryEnqueue(user);
                    }
                }
                SaveDatabase();
                Console.WriteLine("Database got saved ! ");
            }
        }
        public static void ResetingEveryDay(Client.GameClient client)
        {
            try
            {
                if (DateTime.Now.DayOfYear != client.Player.Day)
                {
                    client.Player.UseChiToken = 0;
                    client.Player.TodayChampionPoints = 0;

                    if (client.Player.DailyMonth == 0)
                        client.Player.DailyMonth = (byte)DateTime.Now.Month;
                    if (client.Player.DailyMonth != DateTime.Now.Month)
                    {
                        client.Player.DailySignUpRewards = 0;
                        client.Player.DailySignUpDays = 0;
                        client.Player.DailyMonth = (byte)DateTime.Now.Month;
                    }
                    client.Player.QuestGUI.RemoveQuest(6126);
                    client.Player.OpenHousePack = 0;
                    client.Player.DbTry = false;
                    client.Player.LotteryEntries = 0;
                    client.Player.QuestEntries = 0;
                    client.Player.Quest2Entries = 0;
                    client.Player.Quest3Entries = 0;
                    client.Player.MonsterEntries = 0;
                    client.Player.MonsterEntries2 = 0;
                    client.Player.BDExp = 0;
                    client.Player.ExpBallUsed = 0;
                    client.Player.MysteryFruit = 0;
                    client.Player.TCCaptainTimes = 0;
                    client.DemonExterminator.FinishToday = 0;
                    if (ServerConfig.EnabledChi) // [feature-gate progression.chi]
                    if (client.Player.MyChi != null && DateTime.Now.DayOfYear > client.Player.Day)
                        client.Player.MyChi.ChiPoints = client.Player.MyChi.ChiPoints + Math.Min(((DateTime.Now.DayOfYear - client.Player.Day) * 300), 4000);
                    else
                        client.Player.MyChi.ChiPoints = client.Player.MyChi.ChiPoints + 300;

                    client.Player.Flowers.FreeFlowers = 1;
                    foreach (var flower in client.Player.Flowers)
                        flower.Amount2day = 0;

                    if (client.Player.Level >= 90)
                    {
                        client.Player.Enilghten = CalculateEnlighten(client.Player);
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            client.Player.SendUpdate(stream, client.Player.Enilghten, Game.MsgServer.MsgUpdate.DataType.EnlightPoints);
                        }
                    }
                    client.Player.QuestGUI.RemoveQuest(35024);
                    client.Player.QuestGUI.RemoveQuest(35007);
                    client.Player.QuestGUI.RemoveQuest(35025);
                    client.Player.QuestGUI.RemoveQuest(35028);
                    client.Player.QuestGUI.RemoveQuest(35034);

                    //---- reset Quests
                    client.Player.QuestGUI.RemoveQuest(6390);
                    client.Player.QuestGUI.RemoveQuest(6329);
                    client.Player.QuestGUI.RemoveQuest(6245);
                    client.Player.QuestGUI.RemoveQuest(6049);
                    client.Player.QuestGUI.RemoveQuest(6366);
                    client.Player.QuestGUI.RemoveQuest(6014);
                    client.Player.QuestGUI.RemoveQuest(2375);
                    client.Player.QuestGUI.RemoveQuest(6126);
                    client.Player.DailyHeavenChance = client.Player.DailyMagnoliaChance
                        = client.Player.DailyMagnoliaItemId
                        = client.Player.DailyHeavenChance = client.Player.DailySpiritBeadCount = client.Player.DailyRareChance = 0;
                    //
                    client.Player.Day = DateTime.Now.DayOfYear;
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public static ushort CalculateEnlighten(Role.Player player)
        {
            if (player.Level < 90)
                return 0;
            ushort val = 100;
            if (player.NobilityRank == Role.Instance.Nobility.NobilityRank.Knight || player.NobilityRank == Role.Instance.Nobility.NobilityRank.Baron)
                val += 100;
            if (player.NobilityRank == Role.Instance.Nobility.NobilityRank.Earl || player.NobilityRank == Role.Instance.Nobility.NobilityRank.Duke)
                val += 200;
            if (player.NobilityRank == Role.Instance.Nobility.NobilityRank.Prince)
                val += 300;
            if (player.NobilityRank == Role.Instance.Nobility.NobilityRank.King)
                val += 400;
            if (player.VipLevel <= 3)
                val += 100;
            if (player.VipLevel > 3 && player.VipLevel <= 5)
                val += 200;
            if (player.VipLevel > 5)
                val += 300;

            return val;
        }
        public static void SaveClient(Client.GameClient client)
        {
            if (ServerConfig.DbFromFiles)
            {
                try
                {
                    IniFileHelper write = new IniFileHelper(Path.Combine(ServerConfig.DbLocation, "Users", client.Player.UID + ".ini"));

                    if ((client.ClientFlag & Client.ServerFlag.LoginFull) != Client.ServerFlag.LoginFull)
                    {

                        if (client.Map != null)
                            client.Map.Denquer(client);

                    }

                    if (HouseTable.InHouse(client.Player.Map) && client.Player.DynamicID != 0 || client.Player.DynamicID != 0)
                    {
                        if (client.Socket != null && client.Socket.Alive == false)
                        {
                            client.Player.Map = 1002;
                            client.Player.X = 428;
                            client.Player.Y = 378;
                        }
                    }
                    if ((client.ClientFlag & Client.ServerFlag.Disconnect) == Client.ServerFlag.Disconnect)
                    {
                        if (client.Player.Map == 1234 || client.Player.Map == 1767 || client.Player.Map == 1017 || client.Player.Map == 5263 || client.Player.Map == 1081 || client.Player.Map == 2060 || client.Player.Map == 9972
                        || client.Player.Map == 1080 || client.Player.Map == 1999 || client.Player.Map == 3954 || client.Player.Map == 1806 || client.Player.Map == 1508 || client.Player.Map == 1036 || client.Player.Map == 1768
                        || client.Player.Map == 1505 || client.Player.Map == 1506 || client.Player.Map == 1509 || client.Player.Map == 1508 || client.Player.Map == 1507 || client.Player.Map == 1801 || client.Player.Map == 1780
                        || client.Player.Map == 1779 || client.Player.Map == 3071 || client.Player.Map == 1068 || client.Player.Map == 3830 || client.Player.Map == 3831 || client.Player.Map == 3832 || client.Player.Map == 3834
                        || client.Player.Map == 3826 || client.Player.Map == 3827 || client.Player.Map == 3828 || client.Player.Map == 3829 || client.Player.Map == 3833 || client.Player.Map == 3825 || client.Player.Map == 10088
                        || client.Player.Map == 10089 || client.Player.Map == 10090 || client.Player.Map == 44455 || client.Player.Map == 44456 || client.Player.Map == 44457 || client.Player.Map == 1111 || client.Player.Map == 1112
                        || client.Player.Map == 1113 || client.Player.Map == 1114 || client.Player.Map == 44460 || client.Player.Map == 44461 || client.Player.Map == 44462 || client.Player.Map == 44463)
                        {
                            client.Player.Map = 1002;
                            client.Player.X = 428;
                            client.Player.Y = 378;
                        }
                        if (client.Player.Map == 4000 || client.Player.Map == 4003 || client.Player.Map == 4006 || client.Player.Map == 1767 || client.Player.Map == 4008 || client.Player.Map == 4009)
                        {
                            client.Player.Map = 1002;
                            client.Player.X = 428;
                            client.Player.Y = 378;
                        }
                        if (client.Player.Map == 1038 && Game.MsgTournaments.MsgSchedules.GuildWar.Proces == Game.MsgTournaments.ProcesType.Alive)
                        {
                            client.Player.Map = 1002;
                            client.Player.X = 428;
                            client.Player.Y = 378;
                        }
                    }

                    if (!client.FullLoading)
                        return;
                    write.Write<uint>("Character", "UID", client.Player.UID);
                    write.Write<ushort>("Character", "Body", client.Player.Body);
                    write.Write<ushort>("Character", "Face", client.Player.Face);
                    write.WriteString("Character", "Name", client.Player.Name);
                    write.WriteString("Character", "Spouse", client.Player.Spouse);
                    write.Write<byte>("Character", "Class", client.Player.Class);
                    write.Write<byte>("Character", "FirstClass", client.Player.FirstClass);
                    write.Write<byte>("Character", "SecoundeClass", client.Player.SecondClass);
                    write.Write<ushort>("Character", "Avatar", client.Player.Avatar);
                    write.Write<uint>("Character", "Map", client.Player.Map);
                    write.Write<ushort>("Character", "X", client.Player.X);
                    write.Write<ushort>("Character", "Y", client.Player.Y);
                    write.Write<uint>("Character", "PMap", client.Player.PMap);
                    write.Write<ushort>("Character", "PMapX", client.Player.PMapX);
                    write.Write<ushort>("Character", "PMapY", client.Player.PMapY);
                    write.Write<ushort>("Character", "Agility", client.Player.Agility);
                    write.Write<ushort>("Character", "Strength", client.Player.Strength);
                    write.Write<ushort>("Character", "Vitaliti", client.Player.Vitality);
                    write.Write<ushort>("Character", "Spirit", client.Player.Spirit);
                    write.Write<ushort>("Character", "Atributes", client.Player.Atributes);
                    write.Write<byte>("Character", "Reborn", client.Player.Reborn);
                    write.Write<ushort>("Character", "Level", client.Player.Level);
                    write.Write<ushort>("Character", "Haire", client.Player.Hair);
                    write.Write<ulong>("Character", "Experience", client.Player.Experience);
                    write.Write<int>("Character", "MinHitPoints", client.Player.HitPoints);
                    write.Write<ushort>("Character", "MinMana", client.Player.Mana);
                    write.Write<uint>("Character", "ConquerPoints", client.Player.ConquerPoints);
                    write.Write<int>("Character", "BoundConquerPoints", client.Player.BoundConquerPoints);
                    write.Write<uint>("Character", "ArenaCPS", client.Player.ArenaCPS);
                    write.Write<long>("Character", "Money", client.Player.Money);
                    write.Write<uint>("Character", "MiningAttempts", client.MiningAttempts);
                    write.Write<uint>("Character", "VirtutePoints", client.Player.VirtutePoints);
                    write.Write<uint>("Character", "VirtuteEntries", client.Player.VirtuteEntries);
                    write.Write<ushort>("Character", "PkPoints", client.Player.PKPoints);
                    write.Write<uint>("Character", "JailerUID", client.Player.JailerUID);
                    write.Write<uint>("Character", "QuizPoints", client.Player.QuizPoints);
                    write.Write<ushort>("Character", "Enilghten", client.Player.Enilghten);
                    write.Write<ushort>("Character", "EnlightenReceive", client.Player.EnlightenReceive);
                    write.Write<ulong>("Character", "DailySignUpDays", client.Player.DailySignUpDays);
                    write.Write<byte>("Character", "DailyMonth", client.Player.DailyMonth);
                    write.Write<byte>("Character", "DailySignUpRewards", client.Player.DailySignUpRewards);
                    write.Write<byte>("Character", "VipLevel", client.Player.VipLevel);
                    write.Write<long>("Character", "VipTime", client.Player.ExpireVip.Ticks);
                    write.Write<ushort>("AutoHunt", "Radius", AutoHunting.NormalizeRadius(client.AutoHunting.HuntRadius));
                    write.Write<bool>("AutoHunt", "UseSkills", client.AutoHunting.UseSkills);
                    write.Write<byte>("AutoHunt", "HpPotionPercent", client.AutoHunting.HpPotionPercent);
                    write.Write<byte>("AutoHunt", "MpPotionPercent", client.AutoHunting.MpPotionPercent);
                    write.Write<byte>("AutoHunt", "ExpDeliveryMode", (byte)client.AutoHunting.ExpDeliveryMode);
                    write.Write<bool>("AutoHunt", "FastMode", client.AutoHunting.FastMode);
                    write.Write<bool>("AutoHunt", "DBalls", client.AutoHunting.DBalls);
                    write.Write<bool>("AutoHunt", "Meteors", client.AutoHunting.Meteors);
                    write.Write<bool>("AutoHunt", "PlusItems", client.AutoHunting.PlusItems);
                    write.Write<bool>("AutoHunt", "QualityItems", client.AutoHunting.QualityItems);
                    write.Write<bool>("AutoHunt", "ExpBallEventItems", client.AutoHunting.ExpBallEventItems);
                    write.Write<bool>("AutoHunt", "SocketedItems", client.AutoHunting.SocketedItems);
                    write.Write<bool>("AutoHunt", "BlessedItems", client.AutoHunting.BlessedItems);
                    write.Write<bool>("AutoHunt", "MaterialItems", client.AutoHunting.MaterialItems);
                    write.Write<bool>("AutoHunt", "SoulItems", client.AutoHunting.SoulItems);
                    write.Write<bool>("AutoHunt", "LootMoney", client.AutoHunting.LootMoney);
                    write.Write<long>("Character", "LastDragonPill", client.Player.LastDragonPill.Ticks);
                    client.Player.Achievement.Save(client.Achievement);
                    write.WriteString("Character", "Achivement", client.Achievement.ToString());
                    write.Write<long>("Character", "WHMoney", client.Player.WHMoney);
                    write.Write<uint>("Character", "TrinityPoints", client.Player.TrinityPoints);
                    write.Write<uint>("Character", "BlessTime", client.Player.BlessTime);
                    write.Write<uint>("Character", "SpouseUID", client.Player.SpouseUID);
                    write.Write<int>("Character", "HeavenBlessing", client.Player.HeavenBlessing);
                    write.Write<long>("Character", "LostTimeBlessing", client.Player.HeavenBlessTime.Ticks);
                    write.Write<uint>("Character", "HuntingBlessing", client.Player.HuntingBlessing);
                    write.Write<uint>("Character", "OnlineTrainingPoints", client.Player.OnlineTrainingPoints);
                    write.Write<long>("Character", "JoinOnflineTG", client.Player.JoinOnflineTG.Ticks);
                    write.Write<int>("Character", "Day", client.Player.Day);
                    /*Paid Donation System Coded */
                    write.Write<uint>("PaidNobility", "Identifier", client.Player.PayNobilitySystem.Identifier);
                    write.Write<ulong>("PaidNobility", "LastNobilityDonation", client.Player.PayNobilitySystem.LastNobilityDonation);
                    write.Write<byte>("PaidNobility", "PaidRank", (byte)client.Player.PayNobilitySystem.PaidRank);
                    write.Write<long>("PaidNobility", "PeriodTime", client.Player.PayNobilitySystem.PeriodTime.ToBinary());
                    write.Write<bool>("PaidNobility", "IsActive", client.Player.PayNobilitySystem.IsActive);
                    /*End of the code*/
                    write.Write<byte>("Character", "BDExp", client.Player.BDExp);
                    write.Write<uint>("Character", "RateExp", client.Player.RateExp);
                    write.Write<uint>("Character", "DExpTime", client.Player.DExpTime);
                    write.Write<byte>("Character", "ExpBallUsed", client.Player.ExpBallUsed);
                    write.Write<byte>("Character", "MysteryFruit", client.Player.MysteryFruit);
                    write.WriteString("Character", "SubProfInfo", client.Player.SubClass.ToString());
                    write.WriteString("Character", "Dragon", client.Player.MyChi.Dragon.ToString());
                    write.WriteString("Character", "Pheonix", client.Player.MyChi.Phoenix.ToString());
                    write.WriteString("Character", "Turtle", client.Player.MyChi.Turtle.ToString());
                    write.WriteString("Character", "Tiger", client.Player.MyChi.Tiger.ToString());
                    write.Write<int>("Character", "ChiPoints", client.Player.MyChi.ChiPoints);

                    write.WriteString("Character", "Flowers", client.Player.Flowers.ToString());
                    write.Write<ulong>("Character", "DonationNobility", client.Player.Nobility.Donation);
                    write.Write<long>("Character", "FreeVIP", client.Player.FreeVIP.Ticks);
                    write.Write<uint>("Character", "GuildID", client.Player.GuildID);
                    write.Write<ushort>("Character", "GuildRank", (ushort)client.Player.GuildRank);
                    write.Write<string>("Character", "EnabledTitles", string.Join(';', client.Player.Titles));
                    if (client.Player.MyGuildMember != null)
                    {
                        client.Player.MyGuildMember.LastLogin = DateTime.Now.Ticks;
                        write.Write<uint>("Character", "CpsDonate", client.Player.MyGuildMember.CpsDonate);
                        write.Write<long>("Character", "MoneyDonate", client.Player.MyGuildMember.MoneyDonate);
                        write.Write<uint>("Character", "PkDonation", client.Player.MyGuildMember.PkDonation);
                        write.Write<long>("Character", "LastLogin", client.Player.MyGuildMember.LastLogin);

                        write.Write<uint>("Character", "CTF_Exploits", client.Player.MyGuildMember.CTF_Exploits);
                        write.Write<uint>("Character", "CTF_RCPS", client.Player.MyGuildMember.RewardConquerPoints);
                        write.Write<uint>("Character", "CTF_RM", client.Player.MyGuildMember.RewardMoney);
                        write.Write<byte>("Character", "CTF_R", client.Player.MyGuildMember.CTF_Claimed);
                    }
                    if (client.Player.MyClan != null)
                    {
                        write.Write<uint>("Character", "ClanID", client.Player.MyClan.ID);
                        write.Write<ushort>("Character", "ClanRank", client.Player.ClanRank);
                        if (client.Player.MyClanMember != null)
                        {
                            write.Write<uint>("Character", "ClanDonation", client.Player.MyClanMember.Donation);
                        }
                    }
                    write.Write<byte>("Character", "FRL", client.Player.FirstRebornLevel);
                    write.Write<byte>("Character", "SRL", client.Player.SecoundeRebornLevel);
                    write.Write<bool>("Character", "Reincanation", client.Player.Reincarnation);
                    write.Write<byte>("Character", "LotteryEntries", client.Player.LotteryEntries);
                    write.Write<byte>("Character", "QuestEntries", client.Player.QuestEntries);
                    write.Write<byte>("Character", "Quest2Entries", client.Player.Quest2Entries);
                    write.Write<byte>("Character", "Quest3Entries", client.Player.Quest3Entries);
                    write.Write<byte>("Character", "MonsterEntries", client.Player.MonsterEntries);
                    write.Write<byte>("Character", "MonsterEntries2", client.Player.MonsterEntries2);
                    write.Write<byte>("Character", "RemoveWeapon", client.Player.RemoveWeapon);
                    write.Write<bool>("Character", "DbTry", client.Player.DbTry);
                    write.WriteString("Character", "DemonEx", client.DemonExterminator.ToString());
                    write.WriteString("Character", "PkName", client.Player.MyKillerName);
                    write.Write<uint>("Character", "PkUID", client.Player.MyKillerUID);
                    write.Write<int>("Character", "Cursed", client.Player.CursedTimer);
                    write.Write<uint>("Character", "AparenceType", client.Player.AparenceType);
                    write.Write<uint>("Character", "TKills", client.Player.TournamentKills);
                    write.Write<uint>("Character", "OnlineMinutes", client.Player.OnlineMinutes);
                    write.Write<uint>("Character", "HistoryChampionPoints", client.Player.HistoryChampionPoints);
                    write.Write<uint>("Character", "TodayChampionPoints", client.Player.TodayChampionPoints);
                    write.Write<uint>("Character", "ChampionPoints", client.Player.ChampionPoints);
                    write.Write<uint>("Character", "DailySpiritBeadItem", client.Player.DailySpiritBeadItem);
                    write.WriteString("Character", "SecurityPass", GetSecurityPassword(client));
                    write.Write<byte>("Character", "TCT", (byte)client.Player.TCCaptainTimes);
                    write.Write<uint>("Character", "RacePoints", client.Player.DonationPoints);
                    write.Write<uint>("Character", "LastMan", client.Player.LastMan);
                    write.Write<uint>("Character", "PTB", client.Player.PTB);
                    write.Write<uint>("Character", "Get5Out", client.Player.Get5Out);
                    write.Write<uint>("Character", "FreezeWar", client.Player.FreezeWar);
                    write.Write<uint>("Character", "Infection", client.Player.Infection);
                    write.Write<uint>("Character", "TheCaptain", client.Player.TheCaptain);
                    write.Write<uint>("Character", "Kungfu", client.Player.Kungfu);
                    write.Write<uint>("Character", "VampireWar", client.Player.VampireWar);
                    write.Write<uint>("Character", "WhackTheThief", client.Player.WhackTheThief);
                    write.Write<uint>("Character", "SSFB", client.Player.SSFB);
                    write.Write<ushort>("Character", "NameEditCount", client.Player.NameEditCount);
                    write.Write<uint>("Character", "ClaimStateGift", (uint)client.Player.MainFlag);
                    write.Write<uint>("Character", "enervant", client.Player.AtiveQuestApe);
                    write.Write<ushort>("Character", "InventorySashCount", client.Player.InventorySashCount);
                    write.Write<ushort>("Character", "CountryID", client.Player.CountryID);
                    write.Write<uint>("Character", "MyFootBallPoints", client.Player.MyFootBallPoints);
                    write.Write<uint>("Character", "ExpProtection", client.Player.ExpProtection);
                    write.Write<uint>("Character", "BanCount", client.BanCount);
                    write.Write<ushort>("Character", "ExtraAtributes", client.Player.ExtraAtributes);
                    write.Write<byte>("Character", "OpenHousePack", client.Player.OpenHousePack);
                    write.Write<long>("Character", "JPAStamp", client.Player.JoinPowerArenaStamp.Ticks);
                    write.Write<int>("Character", "GiveFlowersToPerformer", client.Player.GiveFlowersToPerformer);
                    write.Write<byte>("Character", "UseChiToken", client.Player.UseChiToken);
                    write.Write<string>("Character", "Agates", client.Player.AgatesString());
                    write.Write<uint>("Character", "OnlinePoints", client.Player.OnlinePoints);
                    write.Write<uint>("Character", "VotePoints", client.Player.VotePoints);
                    write.Write<uint>("Character", "PVPPoints", client.Player.PVPPoints);
                    write.Write<uint>("Character", "PVEPoints", client.Player.PVEPoints);
                    write.Write<uint>("Character", "TotalMobsKilled", client.TotalMobsKilled);
                    write.Write<uint>("Character", "TotalMobsKilled2", client.TotalMobsKilled2);
                    write.Write<uint>("Character", "TotalSouls", client.TotalSouls);
                    write.Write<int>("Character", "DragonPills", client.Player.DragonPills);
                    write.Write<int>("Character", "BossPoints", client.Player.BossPoints);
                    SaveClientItems(client);
                    SaveClientSpells(client);
                    SaveClientProfs(client);
                    RoleQuests.Save(client);
                    Role.Instance.House.Save(client);
                    if ((client.ClientFlag & Client.ServerFlag.Disconnect) == Client.ServerFlag.Disconnect)
                    {
                        Client.GameClient user;
                        Pool.GamePoll.TryRemove(client.Player.UID, out user);
                        Pool.DisconnectPool.TryRemove(client.Player.UID, out user);
                    }
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            } else
            {
                try
                {
                    Player player = RestApiHelper.GetPlayer(client.Player.UID);
                    if (player != null)
                    {
                        if ((client.ClientFlag & Client.ServerFlag.LoginFull) != Client.ServerFlag.LoginFull)
                        {
                            if (client.Map != null) {
                                client.Map.Denquer(client);
                            }

                        }
                        if (HouseTable.InHouse(client.Player.Map) && client.Player.DynamicID != 0 || client.Player.DynamicID != 0)
                        {
                            if (client.Socket != null && client.Socket.Alive == false)
                            {
                                client.Player.Map = 1002;
                                client.Player.X = 428;
                                client.Player.Y = 378;
                            }
                        }
                        if ((client.ClientFlag & Client.ServerFlag.Disconnect) == Client.ServerFlag.Disconnect)
                        {
                            if (client.Player.Map == 1234 || client.Player.Map == 1767 || client.Player.Map == 1017 || client.Player.Map == 5263 || client.Player.Map == 1081 || client.Player.Map == 2060 || client.Player.Map == 9972
                            || client.Player.Map == 1080 || client.Player.Map == 1999 || client.Player.Map == 3954 || client.Player.Map == 1806 || client.Player.Map == 1508 || client.Player.Map == 1036 || client.Player.Map == 1768
                            || client.Player.Map == 1505 || client.Player.Map == 1506 || client.Player.Map == 1509 || client.Player.Map == 1508 || client.Player.Map == 1507 || client.Player.Map == 1801 || client.Player.Map == 1780
                            || client.Player.Map == 1779 || client.Player.Map == 3071 || client.Player.Map == 1068 || client.Player.Map == 3830 || client.Player.Map == 3831 || client.Player.Map == 3832 || client.Player.Map == 3834
                            || client.Player.Map == 3826 || client.Player.Map == 3827 || client.Player.Map == 3828 || client.Player.Map == 3829 || client.Player.Map == 3833 || client.Player.Map == 3825 || client.Player.Map == 10088
                            || client.Player.Map == 10089 || client.Player.Map == 10090 || client.Player.Map == 44455 || client.Player.Map == 44456 || client.Player.Map == 44457 || client.Player.Map == 1111 || client.Player.Map == 1112
                            || client.Player.Map == 1113 || client.Player.Map == 1114 || client.Player.Map == 44460 || client.Player.Map == 44461 || client.Player.Map == 44462 || client.Player.Map == 44463)
                            {
                                client.Player.Map = 1002;
                                client.Player.X = 428;
                                client.Player.Y = 378;
                            }
                            if (client.Player.Map == 4000 || client.Player.Map == 4003 || client.Player.Map == 4006 || client.Player.Map == 1767 || client.Player.Map == 4008 || client.Player.Map == 4009)
                            {
                                client.Player.Map = 1002;
                                client.Player.X = 428;
                                client.Player.Y = 378;
                            }
                            if (client.Player.Map == 1038 && Game.MsgTournaments.MsgSchedules.GuildWar.Proces == Game.MsgTournaments.ProcesType.Alive)
                            {
                                client.Player.Map = 1002;
                                client.Player.X = 428;
                                client.Player.Y = 378;
                            }
                        }
                        if (!client.FullLoading)
                            return;

                        player.UID = client.Player.UID;
                        player.Body = client.Player.Body;
                        player.Face = client.Player.Face;
                        player.Name = client.Player.Name;
                        player.Spouse = client.Player.Spouse;
                        player.Class = client.Player.Class;
                        player.FirstClass = client.Player.FirstClass;
                        player.SecondClass = client.Player.SecondClass;
                        player.Avatar = client.Player.Avatar;
                        player.Map = client.Player.Map;
                        player.X = client.Player.X;
                        player.Y = client.Player.Y;
                        player.PMap = client.Player.PMap;
                        player.PMapX = client.Player.PMapX;
                        player.PMapY = client.Player.PMapY;
                        player.Agility = client.Player.Agility;
                        player.Strength = client.Player.Strength;
                        player.Vitality = client.Player.Vitality;
                        player.Spirit = client.Player.Spirit;
                        player.Attributes = client.Player.Atributes;
                        player.Reborn = client.Player.Reborn;
                        player.Level = (byte)client.Player.Level;
                        player.Hair = client.Player.Hair;
                        player.Experience = client.Player.Experience;
                        player.HitPoints = client.Player.HitPoints;
                        player.Mana = client.Player.Mana;
                        player.ConquerPoints = client.Player.ConquerPoints;
                        player.BoundConquerPoints = client.Player.BoundConquerPoints;
                        player.ArenaCPS = client.Player.ArenaCPS;
                        player.Money = client.Player.Money;
                        player.MiningAttempts = client.MiningAttempts;
                        player.VirtuePoints = client.Player.VirtutePoints;
                        player.VirtueEntries = client.Player.VirtuteEntries;
                        player.PKPoints = client.Player.PKPoints;
                        player.JailerUID = client.Player.JailerUID;
                        player.QuizPoints = client.Player.QuizPoints;
                        player.Enlighten = client.Player.Enilghten;
                        player.EnlightenReceive = client.Player.EnlightenReceive;
                        player.DailySignUpDays = client.Player.DailySignUpDays;
                        player.DailyMonth = client.Player.DailyMonth;
                        player.DailySignUpRewards = client.Player.DailySignUpRewards;
                        player.VipLevel = client.Player.VipLevel;
                        player.ExpireVip = client.Player.ExpireVip;
                        player.LastDragonPill = client.Player.LastDragonPill;
                        client.Player.Achievement.Save(client.Achievement);
                        player.Archivement = client.Achievement.ToString();
                        player.WHMoney = client.Player.WHMoney;
                        player.TrinityPoints = client.Player.TrinityPoints;
                        player.BlessTime = client.Player.BlessTime;
                        player.SpouseUID = client.Player.SpouseUID;
                        player.HeavenBlessing = client.Player.HeavenBlessing;
                        player.HeavenBlessTime = client.Player.HeavenBlessTime;
                        player.OnlineTrainingPoints = client.Player.OnlineTrainingPoints;
                        player.JoinOfflineTG = client.Player.JoinOnflineTG;
                        player.Day = client.Player.Day;
                        if (client.Player.PayNobilitySystem != null)
                        {
                            player.NobilityLastDonation = client.Player.PayNobilitySystem.LastNobilityDonation;
                            player.NobilityPaidRank = (byte)client.Player.PayNobilitySystem.PaidRank;
                            player.NobilityPeriodTime = (ulong)client.Player.PayNobilitySystem.PeriodTime.ToBinary();
                            player.NobilityIsActive = client.Player.PayNobilitySystem.IsActive;
                        }
                        player.BDExp = client.Player.BDExp;
                        player.RateExp = client.Player.RateExp;
                        player.DExpTime = client.Player.DExpTime;
                        player.ExpBallUsed = client.Player.ExpBallUsed;
                        player.MysteryFruit = client.Player.MysteryFruit;
                        player.SubProfInfo = client.Player.SubClass.ToString();
                        player.ChiDragon = client.Player.MyChi.Dragon.ToString();
                        player.ChiPhoenix = client.Player.MyChi.Phoenix.ToString();
                        player.ChiTurtle = client.Player.MyChi.Turtle.ToString();
                        player.ChiTiger = client.Player.MyChi.Tiger.ToString();
                        player.ChiPoints = (uint)client.Player.MyChi.ChiPoints;
                        player.Flowers = client.Player.Flowers.ToString();
                        player.NobilityDonation = client.Player.Nobility.Donation;
                        player.FreeVIP = client.Player.FreeVIP;
                        player.GuildID = client.Player.GuildID;
                        player.GuildRank = (Core.Interfaces.GameServer.GuildMemberRank)client.Player.GuildRank;
                        player.EnabledTitles = string.Join(';', client.Player.Titles);
                        if (client.Player.MyGuildMember != null)
                        {
                            client.Player.MyGuildMember.LastLogin = DateTime.Now.Ticks;
                            player.GuildCPsDonate = client.Player.MyGuildMember.CpsDonate;
                            player.GuildMoneyDonate = (ulong)client.Player.MyGuildMember.MoneyDonate;
                            player.GuildPKDonation = client.Player.MyGuildMember.PkDonation;
                            player.GuildLastLogin = (ulong)client.Player.MyGuildMember.LastLogin;
                            player.GuildCTFExploits = client.Player.MyGuildMember.CTF_Exploits;
                            player.GuildCTFConquerPointsReward = client.Player.MyGuildMember.RewardConquerPoints;
                            player.GuildCTFMoneyReward = client.Player.MyGuildMember.RewardMoney;
                            player.GuildCTFClaimed = client.Player.MyGuildMember.CTF_Claimed;
                        }
                        if (client.Player.MyClan != null)
                        {
                            player.ClanUID = client.Player.MyClan.ID;
                            player.ClanRank = client.Player.ClanRank;
                            if (client.Player.MyClanMember != null)
                            {
                                player.ClanDonation = client.Player.MyClanMember.Donation;
                            }
                        }
                        player.FirstRebornLevel = client.Player.FirstRebornLevel;
                        player.SecondRebornLevel = client.Player.SecoundeRebornLevel;
                        player.Reincarnation = client.Player.Reincarnation;
                        player.LotteryEntries = client.Player.LotteryEntries;
                        player.QuestEntries = client.Player.QuestEntries;
                        player.Quest2Entries = client.Player.Quest2Entries;
                        player.Quest3Entries = client.Player.Quest3Entries;
                        player.MonsterEntries = client.Player.MonsterEntries;
                        player.MonsterEntries2 = client.Player.MonsterEntries2;
                        player.RemoveWeapon = client.Player.RemoveWeapon;
                        player.DbTry = client.Player.DbTry;
                        player.DemonExterminator = client.DemonExterminator.ToString();
                        player.MyKillerName = client.Player.MyKillerName;
                        player.MyKillerUID = client.Player.MyKillerUID;
                        player.CursedTimer = client.Player.CursedTimer;
                        player.AparenceType = client.Player.AparenceType;
                        player.TournamentKills = client.Player.TournamentKills;
                        player.OnlineMinutes = client.Player.OnlineMinutes;
                        player.HistoryChampionPoints = client.Player.HistoryChampionPoints;
                        player.TodayChampionPoints = client.Player.TodayChampionPoints;
                        player.ChampionPoints = client.Player.ChampionPoints;
                        player.DailySpiritBeadItem = client.Player.DailySpiritBeadItem;
                        player.TodayChampionPoints = client.Player.TodayChampionPoints;
                        player.SecurityPass = GetSecurityPassword(client);
                        player.TCCaptainTimes = (byte)client.Player.TCCaptainTimes;
                        player.DonationPoints = client.Player.DonationPoints;
                        player.LastMan = client.Player.LastMan;
                        player.PTB = client.Player.PTB;
                        player.Get5Out = client.Player.Get5Out;
                        player.FreezeWar = client.Player.FreezeWar;
                        player.Infection = client.Player.Infection;
                        player.TheCaptain = client.Player.TheCaptain;
                        player.Kungfu = client.Player.Kungfu;
                        player.VampireWar = client.Player.VampireWar;
                        player.WhackTheThief = client.Player.WhackTheThief;
                        player.SSFB = client.Player.SSFB;
                        player.NameEditCount = client.Player.NameEditCount;
                        player.MainFlag = (MainFlagType)client.Player.MainFlag;
                        player.AtiveQuestApe = client.Player.AtiveQuestApe;
                        player.InventorySashCount = client.Player.InventorySashCount;
                        player.CountryID = client.Player.CountryID;
                        player.MyFootBallPoints = client.Player.MyFootBallPoints;
                        player.ExpProtection = client.Player.ExpProtection;
                        player.BanCount = client.BanCount;
                        player.ExtraAttributes = client.Player.ExtraAtributes;
                        player.OpenHousePack = client.Player.OpenHousePack;
                        player.JoinPowerArenaStamp = client.Player.JoinPowerArenaStamp;
                        player.GiveFlowersToPerformer = client.Player.GiveFlowersToPerformer;
                        player.UseChiToken = client.Player.UseChiToken;
                        player.Agates = client.Player.AgatesString();
                        player.OnlinePoints = client.Player.OnlinePoints;
                        player.VotePoints = client.Player.VotePoints;
                        player.PVPPoints = client.Player.PVPPoints;
                        player.PVEPoints = client.Player.PVEPoints;
                        player.TotalMobsKilled = client.TotalMobsKilled;
                        player.TotalMobsKilled2 = client.TotalMobsKilled2;
                        player.TotalSouls = client.TotalSouls;
                        player.DragonPills = client.Player.DragonPills;
                        player.BossPoints = client.Player.BossPoints;
                        SaveClientItems(client);
                        SaveClientSpells(client);
                        SaveClientProfs(client);
                        RoleQuests.Save(client);
                        Role.Instance.House.Save(client);
                        if ((client.ClientFlag & Client.ServerFlag.Disconnect) == Client.ServerFlag.Disconnect)
                        {
                            Client.GameClient user;
                            Pool.GamePoll.TryRemove(client.Player.UID, out user);
                            Pool.DisconnectPool.TryRemove(client.Player.UID, out user);
                        }
                        RestApiHelper.UpdatePlayer(player);
                    }
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }
        public static string GetSecurityPassword(Client.GameClient user)
        {
            Database.DBActions.WriteLine writer = new DBActions.WriteLine(',');
            writer.Add(user.Player.SecurityPassword);
            writer.Add(user.Player.OnReset);
            writer.Add(user.Player.ResetSecurityPassowrd.Ticks);
            return writer.Close();
        }
        public static void LoadSecurityPassword(string line, Client.GameClient user)
        {
            Database.DBActions.ReadLine reader = new DBActions.ReadLine(line, ',');
            user.Player.SecurityPassword = reader.Read((uint)0);
            user.Player.OnReset = reader.Read((uint)0);
            if (user.Player.OnReset == 1)
            {
                user.Player.ResetSecurityPassowrd = DateTime.FromBinary(reader.Read((long)0));
                if (DateTime.Now > user.Player.ResetSecurityPassowrd)
                {
                    user.Player.OnReset = 0;
                    user.Player.SecurityPassword = 0;
                }
            }

        }


        public static void LoadCharacter(Client.GameClient client, uint UID)
        {
            client.Player.UID = UID;
            if (ServerConfig.DbFromFiles)
            {
                IniFileHelper reader = new(Path.Combine(ServerConfig.DbLocation, "Users", UID + ".ini"));
                client.Player.Body = reader.ReadUInt16("Character", "Body", 1002);
                client.Player.Face = reader.ReadUInt16("Character", "Face", 0);
                client.Player.Name = reader.ReadString("Character", "Name", "None");
                client.Player.Spouse = reader.ReadString("Character", "Spouse", "None");
                client.Player.Class = reader.ReadByte("Character", "Class", 0);
                client.Player.FirstClass = reader.ReadByte("Character", "FirstClass", 0);
                client.Player.SecondClass = reader.ReadByte("Character", "SecoundeClass", 0);
                client.Player.Avatar = reader.ReadUInt16("Character", "Avatar", 0);
                client.Player.Map = reader.ReadUInt32("Character", "Map", 1002);
                client.Player.X = reader.ReadUInt16("Character", "X", 429);
                client.Player.Y = reader.ReadUInt16("Character", "Y", 378);
                client.MiningAttempts = reader.ReadUInt16("Character", "MiningAttempts", 200);
                client.Player.PMap = reader.ReadUInt32("Character", "PMap", 1002);
                client.Player.PMapX = reader.ReadUInt16("Character", "PMapX", 300);
                client.Player.PMapY = reader.ReadUInt16("Character", "PMapY", 300);


                if (Pool.OutMaps.Contains(client.Player.Map) || client.Player.Map == 1234 || client.Player.Map == 1767 || client.Player.Map == 1017 || client.Player.Map == 1081 || client.Player.Map == 2060 || client.Player.Map == 9972 || client.Player.Map == 1080 || client.Player.Map == 1999 || client.Player.Map == 3954
                || client.Player.Map == 1806 || client.Player.Map == 1508 || client.Player.Map == 1036 || client.Player.Map == 1768 || client.Player.Map == 1505 || client.Player.Map == 1506 || client.Player.Map == 1509 || client.Player.Map == 1508 || client.Player.Map == 1507
                || client.Player.Map == 1801 || client.Player.Map == 1780 || client.Player.Map == 1779 || client.Player.Map == 3071 || client.Player.Map == 1068 || client.Player.Map == 3830 || client.Player.Map == 3831 || client.Player.Map == 3832 || client.Player.Map == 3834 || client.Player.Map == 3826 || client.Player.Map == 3827 || client.Player.Map == 3828 || client.Player.Map == 3829
                || client.Player.Map == 3833 || client.Player.Map == 3825 || client.Player.Map == 10088 || client.Player.Map == 10089 || client.Player.Map == 10090 || client.Player.Map == 44455 || client.Player.Map == 44456 || client.Player.Map == 44457
                || client.Player.Map == 44460 || client.Player.Map == 44461 || client.Player.Map == 44462 || client.Player.Map == 44463 || client.Player.Map == 4000 || client.Player.Map == 4003 || client.Player.Map == 4006 || client.Player.Map == 4008 || client.Player.Map == 4009)
                {
                    client.Player.Map = 1002;
                    client.Player.X = 428;
                    client.Player.Y = 378;
                }
                if (client.Player.Map == 1038 && Game.MsgTournaments.MsgSchedules.GuildWar.Proces == Game.MsgTournaments.ProcesType.Alive)
                {
                    client.Player.Map = 1002;
                    client.Player.X = 428;
                    client.Player.Y = 378;
                }

                client.Player.Agility = reader.ReadUInt16("Character", "Agility", 0);
                client.Player.Strength = reader.ReadUInt16("Character", "Strength", 0);
                client.Player.Spirit = reader.ReadUInt16("Character", "Spirit", 0);
                client.Player.Vitality = reader.ReadUInt16("Character", "Vitaliti", 0);
                client.Player.Atributes = reader.ReadUInt16("Character", "Atributes", 0);
                client.Player.Reborn = reader.ReadByte("Character", "Reborn", 0);
                client.Player.Level = reader.ReadUInt16("Character", "Level", 0);
                client.Player.Hair = reader.ReadUInt16("Character", "Haire", 0);
                client.Player.Experience = (ulong)reader.ReadInt64("Character", "Experience", 0);
                client.Player.HitPoints = reader.ReadInt32("Character", "MinHitPoints", 0);
                client.Player.Mana = reader.ReadUInt16("Character", "MinMana", 0);
                client.Player.ConquerPoints = reader.ReadUInt32("Character", "ConquerPoints", 0);
                client.Player.ArenaCPS = reader.ReadUInt32("Character", "ArenaCPS", 0);
                client.Player.BoundConquerPoints = reader.ReadInt32("Character", "BoundConquerPoints", 0);
                client.Player.Money = reader.ReadUInt32("Character", "Money", 0);
                client.Player.VirtutePoints = reader.ReadUInt32("Character", "VirtutePoints", 0);
                client.Player.VirtuteEntries = reader.ReadByte("Character", "VirtuteEntries", 0);
                client.Player.PKPoints = reader.ReadUInt16("Character", "PkPoints", 0);
                client.Player.PVPPoints = reader.ReadUInt16("Character", "PVPPoints", 0);
                /*Paid Donation System */
                client.Player.PayNobilitySystem.SetData(UID,
                    reader.ReadUInt64("PaidNobility", "LastNobilityDonation", 0),
                    reader.ReadByte("PaidNobility", "PaidRank", 0),
                    reader.ReadInt64("PaidNobility", "PeriodTime", 0),
                    reader.ReadBool("PaidNobility", "IsActive", false));
                /*End of the code*/
                client.Player.JailerUID = reader.ReadUInt32("Character", "JailerUID", 0);
                client.Player.QuizPoints = reader.ReadUInt32("Character", "QuizPoints", 0);
                client.Player.Enilghten = reader.ReadUInt16("Character", "Enilghten", 0);
                client.Player.EnlightenReceive = reader.ReadUInt16("Character", "EnlightenReceive", 0);
                client.Player.DailySignUpDays = reader.ReadUInt64("Character", "DailySignUpDays", 0);
                client.Player.DailyMonth = reader.ReadByte("Character", "DailyMonth", 0);
                client.Player.DailySignUpRewards = reader.ReadByte("Character", "DailySignUpRewards", 0);
                client.Player.VipLevel = reader.ReadByte("Character", "VipLevel", 0);
                client.Player.ExpireVip = DateTime.FromBinary(reader.ReadInt64("Character", "VipTime", 0));
                client.AutoHunting.HuntRadius = AutoHunting.NormalizeRadius(reader.ReadUInt16("AutoHunt", "Radius", 0));
                client.AutoHunting.UseSkills = reader.ReadBool("AutoHunt", "UseSkills", true);
                client.AutoHunting.HpPotionPercent = Math.Min((byte)100, reader.ReadByte("AutoHunt", "HpPotionPercent", 40));
                client.AutoHunting.MpPotionPercent = Math.Min((byte)100, reader.ReadByte("AutoHunt", "MpPotionPercent", 30));
                byte expDelivery = reader.ReadByte("AutoHunt", "ExpDeliveryMode", 0);
                client.AutoHunting.ExpDeliveryMode = expDelivery == 1 ? AutoHunting.AutoHuntExpDelivery.Instant : AutoHunting.AutoHuntExpDelivery.OnStop;
                client.AutoHunting.FastMode = reader.ReadBool("AutoHunt", "FastMode", false);
                client.AutoHunting.DBalls = reader.ReadBool("AutoHunt", "DBalls", false);
                client.AutoHunting.Meteors = reader.ReadBool("AutoHunt", "Meteors", false);
                client.AutoHunting.PlusItems = reader.ReadBool("AutoHunt", "PlusItems", false);
                client.AutoHunting.QualityItems = reader.ReadBool("AutoHunt", "QualityItems", false);
                client.AutoHunting.ExpBallEventItems = reader.ReadBool("AutoHunt", "ExpBallEventItems", false);
                client.AutoHunting.SocketedItems = reader.ReadBool("AutoHunt", "SocketedItems", false);
                client.AutoHunting.BlessedItems = reader.ReadBool("AutoHunt", "BlessedItems", false);
                client.AutoHunting.MaterialItems = reader.ReadBool("AutoHunt", "MaterialItems", false);
                client.AutoHunting.SoulItems = reader.ReadBool("AutoHunt", "SoulItems", false);
                client.AutoHunting.LootMoney = reader.ReadBool("AutoHunt", "LootMoney", false);
                client.Player.LastDragonPill = DateTime.FromBinary(reader.ReadInt64("Character", "LastDragonPill", 0));
                if (DateTime.Now > client.Player.ExpireVip)
                {
                    if (client.Player.VipLevel >= 1)
                        client.Player.VipLevel = 0;
                }
                client.Achievement = new AchievementCollection();
                client.Achievement.Load(reader.ReadString("Character", "Achivement", ""));
                client.Player.WHMoney = reader.ReadInt64("Character", "WHMoney", 0);
                client.Player.BlessTime = reader.ReadUInt32("Character", "BlessTime", 0);
                client.Player.TrinityPoints = reader.ReadUInt32("Character", "TrinityPoints", 0);
                client.Player.SpouseUID = reader.ReadUInt32("Character", "SpouseUID", 0);
                client.Player.HeavenBlessing = reader.ReadInt32("Character", "HeavenBlessing", 0);
                client.Player.HeavenBlessTime = new DateTime(reader.ReadInt64("Character", "LostTimeBlessing", 0));
                client.Player.HuntingBlessing = reader.ReadUInt32("Character", "HuntingBlessing", 0);
                client.Player.OnlineTrainingPoints = reader.ReadUInt32("Character", "OnlineTrainingPoints", 0);
                client.Player.JoinOnflineTG = DateTime.FromBinary(reader.ReadInt64("Character", "JoinOnflineTG", 0));
                client.Player.RateExp = reader.ReadUInt32("Character", "RateExp", 0);
                client.Player.DExpTime = reader.ReadUInt32("Character", "DExpTime", 0);
                client.Player.Day = reader.ReadInt32("Character", "Day", 0);
                client.Player.BDExp = reader.ReadByte("Character", "BDExp", 0);
                client.Player.ExpBallUsed = reader.ReadByte("Character", "ExpBallUsed", 0);
                client.Player.MysteryFruit = reader.ReadByte("Character", "MysteryFruit", 0);
                client.Player.FreeVIP = DateTime.FromBinary(reader.ReadInt64("Character", "FreeVIP", 0));
                DataCore.LoadClient(client.Player);
                client.Player.GuildID = reader.ReadUInt32("Character", "GuildID", 0);
                client.Player.GuildRank = (Role.Flags.GuildMemberRank)reader.ReadUInt32("Character", "GuildRank", 200);
                client.Player.EnabledTitles = reader.ReadString("Character", "EnabledTitles", "");
                if (client.Player.EnabledTitles.Length > 0)
                {
                    // Load enables titles
                    string[] titleIDs = client.Player.EnabledTitles.Split(';');
                    foreach (string titleId in titleIDs)
                    {
                        client.Player.AddTitle(byte.Parse(titleId), true);
                    }
                }
                if (client.Player.GuildID != 0)
                {
                    Role.Instance.Guild myguild;
                    if (Role.Instance.Guild.GuildPoll.TryGetValue(client.Player.GuildID, out myguild))
                    {
                        client.Player.MyGuild = myguild;
                        Role.Instance.Guild.Member member;
                        if (myguild.Members.TryGetValue(client.Player.UID, out member))
                        {
                            member.IsOnline = true;
                            client.Player.GuildID = (ushort)myguild.Info.GuildID;
                            client.Player.MyGuildMember = member;
                            client.Player.GuildRank = member.Rank;
                            client.Player.GuildBattlePower = myguild.ShareMemberPotency(member.Rank);
                        }
                        else
                        {
                            client.Player.MyGuild = null;
                            client.Player.GuildID = 0;
                            client.Player.GuildRank = (Role.Flags.GuildMemberRank)0;
                        }
                    }
                    else
                    {
                        client.Player.MyGuild = null;
                        client.Player.GuildID = 0;
                        client.Player.GuildRank = (Role.Flags.GuildMemberRank)0;
                    }
                }


                client.Player.SubClass = new Role.Instance.SubClass();
                client.Player.SubClass.Load(reader.ReadString("Character", "SubProfInfo", ""));
                client.Player.SubClass.CreateSpawn(client);

                if (Role.Instance.Chi.ChiPool.ContainsKey(UID))
                {
                    client.Player.MyChi = Role.Instance.Chi.ChiPool[UID];
                    Role.Instance.Chi.ComputeStatus(client.Player.MyChi);
                }
                else
                    client.Player.MyChi = new Role.Instance.Chi(UID);

                if (Role.Instance.Flowers.ClientPoll.ContainsKey(UID))
                    client.Player.Flowers = Role.Instance.Flowers.ClientPoll[UID];
                else
                    client.Player.Flowers = new Role.Instance.Flowers(UID, client.Player.Name);
                string flowerStr = reader.ReadString("Character", "Flowers", "");
                Database.DBActions.ReadLine Linereader = new DBActions.ReadLine(flowerStr, '/');
                client.Player.Flowers.FreeFlowers = Linereader.Read((uint)0);
                Role.Instance.Nobility nobility;


                if (client.Player.PayNobilitySystem.IsActive == false)
                {
                    //Role.Instance.Nobility nobility;
                    if (Pool.NobilityRanking.TryGetValue(UID, out nobility))
                    {
                        client.Player.Nobility = nobility;
                        client.Player.NobilityRank = client.Player.Nobility.Rank;
                    }
                    else
                    {
                        client.Player.Nobility = new Role.Instance.Nobility(client);
                        client.Player.Nobility.Donation = reader.ReadUInt64("Character", "DonationNobility", 0);
                        client.Player.NobilityRank = client.Player.Nobility.Rank;
                    }
                }
                else
                {
                    if (DateTime.Now >= client.Player.PayNobilitySystem.PeriodTime)
                    {

                        client.Player.Nobility = new Role.Instance.Nobility(client);
                        client.Player.Nobility.Donation = client.Player.PayNobilitySystem.LastNobilityDonation;
                        client.Player.NobilityRank = client.Player.Nobility.Rank;
                        client.Player.PayNobilitySystem.SetDataAnyWay(0, 0, 0, 0, false);
                    }
                    else
                    {
                        client.Player.Nobility = new Role.Instance.Nobility(client);
                        switch (client.Player.PayNobilitySystem.PaidRank)
                        {
                            case global::GameServer.Role.Instance.Nobility.NobilityRank.King:
                                {
                                    client.Player.Nobility.Donation = 0;
                                    client.Player.Nobility.Position = 0;
                                    client.Player.NobilityRank = client.Player.Nobility.Rank;
                                    break;
                                }
                            case global::GameServer.Role.Instance.Nobility.NobilityRank.Prince:
                                {
                                    client.Player.Nobility.Donation = 0;
                                    client.Player.Nobility.Position = 4;
                                    client.Player.NobilityRank = client.Player.Nobility.Rank;
                                    break;
                                }
                            case global::GameServer.Role.Instance.Nobility.NobilityRank.Duke:
                                {
                                    client.Player.Nobility.Donation = 0;
                                    client.Player.Nobility.Position = 15;
                                    client.Player.NobilityRank = client.Player.Nobility.Rank;
                                    break;
                                }
                        }
                    }
                }

                //Role.Instance.Nobility nobility;
                //if (Pool.NobilityRanking.TryGetValue(UID, out nobility))
                //{
                //    client.Player.Nobility = nobility;
                //    client.Player.NobilityRank = client.Player.Nobility.Rank;
                //}
                //else
                //{
                //    client.Player.Nobility = new Role.Instance.Nobility(client);
                //    client.Player.Nobility.Donation = reader.ReadUInt64("Character", "DonationNobility", 0);
                //    client.Player.NobilityRank = client.Player.Nobility.Rank;
                //}
                Role.Instance.AssociateGS.MyAsociats Associate;
                if (Role.Instance.AssociateGS.Associates.TryGetValue(client.Player.UID, out Associate))
                {
                    client.Player.Associate = Associate;
                    client.Player.Associate.MyClient = client;
                    client.Player.Associate.Online = true;
                    if (client.Player.Associate.Associat.ContainsKey(Role.Instance.AssociateGS.Mentor))
                    {
                        foreach (var member in client.Player.Associate.Associat[Role.Instance.AssociateGS.Mentor].Values)
                        {
                            if (member.UID != 0)
                            {
                                Role.Instance.AssociateGS.MyAsociats mentor;
                                if (Role.Instance.AssociateGS.Associates.TryGetValue(member.UID, out mentor))
                                {
                                    client.Player.MyMentor = mentor;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    client.Player.Associate = new Role.Instance.AssociateGS.MyAsociats(client.Player.UID);
                    client.Player.Associate.MyClient = client;
                    client.Player.Associate.Online = true;
                }
                client.Player.ClanUID = reader.ReadUInt32("Character", "ClanID", 0);
                if (client.Player.ClanUID != 0)
                {
                    Role.Instance.Clan myclan;
                    if (Role.Instance.Clan.Clans.TryGetValue(client.Player.ClanUID, out myclan))
                    {
                        client.Player.MyClan = myclan;
                        Role.Instance.Clan.Member member;
                        if (myclan.Members.TryGetValue(client.Player.UID, out member))
                        {
                            member.Online = true;
                            client.Player.ClanName = myclan.Name;
                            client.Player.MyClanMember = member;
                            client.Player.ClanRank = (ushort)member.Rank;
                        }
                        else
                        {
                            client.Player.MyClan = null;
                            client.Player.ClanUID = 0;
                            client.Player.ClanRank = 0;
                        }
                    }
                    else
                        client.Player.ClanUID = 0;
                }
                client.Player.FirstRebornLevel = reader.ReadByte("Character", "FRL", 0);
                client.Player.SecoundeRebornLevel = reader.ReadByte("Character", "SRL", 0);
                client.Player.Reincarnation = reader.ReadBool("Character", "Reincanation", false);
                client.Player.LotteryEntries = reader.ReadByte("Character", "LotteryEntries", 0);
                client.Player.QuestEntries = reader.ReadByte("Character", "QuestEntries", 0);
                client.Player.Quest2Entries = reader.ReadByte("Character", "Quest2Entries", 0);
                client.Player.Quest3Entries = reader.ReadByte("Character", "Quest3Entries", 0);
                client.Player.MonsterEntries = reader.ReadByte("Character", "MonsterEntries", 0);
                client.Player.MonsterEntries2 = reader.ReadByte("Character", "MonsterEntries2", 0);
                client.Player.RemoveWeapon = reader.ReadByte("Character", "RemoveWeapon", 0);
                client.Player.DbTry = reader.ReadBool("Character", "DbTry", false);
                client.DemonExterminator.ReadLine(reader.ReadString("Character", "DemonEx", "0/0/"));
                if (uint.TryParse(reader.ReadString("Character", "PkName", "0"), out uint canParseMyKillerUID))
                {
                    client.Player.MyKillerUID = uint.Parse(reader.ReadString("Character", "PkName", "0"));
                }
                client.Player.MyKillerName = reader.ReadString("Character", "PkName", "None");
                client.Player.CursedTimer = reader.ReadInt32("Character", "Cursed", 0);
                client.Player.AtiveQuestApe = reader.ReadUInt32("Character", "enervant", 0);
                client.Player.AparenceType = reader.ReadUInt32("Character", "AparenceType", 0);
                client.Player.TournamentKills = reader.ReadUInt32("Character", "TKills", 0);
                client.Player.OnlineMinutes = reader.ReadUInt32("Character", "OnlineMinutes", 0);
                client.Player.HistoryChampionPoints = reader.ReadUInt32("Character", "HistoryChampionPoints", 0);
                client.Player.AddChampionPoints(reader.ReadUInt32("Character", "ChampionPoints", 0), false);
                client.Player.TodayChampionPoints = reader.ReadUInt32("Character", "TodayChampionPoints", 0);
                client.Player.DailySpiritBeadItem = reader.ReadUInt32("Character", "DailySpiritBeadItem", 0);
                LoadSecurityPassword(reader.ReadString("Character", "SecurityPass", "0,0,0"), client);
                client.Player.TCCaptainTimes = reader.ReadByte("Character", "TCT", 0);
                client.Player.LastMan = reader.ReadUInt32("Character", "LastMan", 0);
                client.Player.PTB = reader.ReadUInt32("Character", "PTB", 0);
                client.Player.Get5Out = reader.ReadUInt32("Character", "Get5Out", 0);
                client.Player.FreezeWar = reader.ReadUInt32("Character", "FreezeWar", 0);
                client.Player.Infection = reader.ReadUInt32("Character", "Infection", 0);
                client.Player.TheCaptain = reader.ReadUInt32("Character", "TheCaptain", 0);
                client.Player.Kungfu = reader.ReadUInt32("Character", "Kungfu", 0);
                client.Player.VampireWar = reader.ReadUInt32("Character", "VampireWar", 0);
                client.Player.WhackTheThief = reader.ReadUInt32("Character", "WhackTheThief", 0);
                client.Player.SSFB = reader.ReadUInt32("Character", "SSFB", 0);
                client.Player.DonationPoints = reader.ReadUInt32("Character", "RacePoints", 0);
                client.Player.NameEditCount = reader.ReadUInt16("Character", "NameEditCount", 0);
                client.Player.MainFlag = (Role.Player.MainFlagType)reader.ReadUInt32("Character", "ClaimStateGift", 0);
                client.Player.CountryID = reader.ReadUInt16("Character", "CountryID", 0);
                client.Player.InventorySashCount = reader.ReadUInt16("Character", "InventorySashCount", 0);
                client.Player.MyFootBallPoints = reader.ReadUInt32("Character", "MyFootBallPoints", 0);
                client.Player.ExpProtection = reader.ReadUInt32("Character", "ExpProtection", 0);
                client.BanCount = reader.ReadByte("Character", "BanCount", 0);
                client.Player.ExtraAtributes = reader.ReadUInt16("Character", "ExtraAtributes", 0);
                client.Player.OpenHousePack = reader.ReadByte("Character", "OpenHousePack", 0);
                client.Player.JoinPowerArenaStamp = DateTime.FromBinary(reader.ReadInt64("Character", "JPAStamp", 0));
                client.Player.GiveFlowersToPerformer = reader.ReadInt32("Character", "GiveFlowersToPerformer", 0);
                client.Player.UseChiToken = reader.ReadByte("Character", "UseChiToken", 0);
                client.Player.OnlinePoints = reader.ReadUInt32("Character", "OnlinePoints", 0);
                client.Player.VotePoints = reader.ReadUInt32("Character", "VotePoints", 0);
                client.TotalMobsKilled = reader.ReadUInt32("Character", "TotalMobsKilled", 0);
                client.TotalMobsKilled2 = reader.ReadUInt32("Character", "TotalMobsKilled2", 0);
                client.TotalSouls = reader.ReadUInt32("Character", "TotalSouls", 0);
                client.Player.DragonPills = reader.ReadInt32("Character", "DragonPills", 0);
                client.Player.BossPoints = reader.ReadInt32("Character", "BossPoints", 0);
                client.Player.PVEPoints = reader.ReadUInt32("Character", "PVEPoints", 0);
                LoadClientItems(client);
                client.Player.LoadAgates(reader.ReadString("Character", "Agates", ""));
                LoadClientSpells(client);
                LoadClientProfs(client);
                RoleQuests.Load(client);
                Role.Instance.House.Load(client);
                ResetingEveryDay(client);
                Role.Instance.Confiscator Container;
                if (Pool.QueueContainer.PollContainers.TryGetValue(client.Player.UID, out Container))
                    client.Confiscator = Container;
                try
                {
                    client.Player.Associate.OnLoading(client);
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
                if (Game.MsgTournaments.MsgArena.ArenaPoll.TryGetValue(client.Player.UID, out client.ArenaStatistic))
                {
                    client.ArenaStatistic.ApplayInfo(client.Player);
                }
                else
                {
                    client.ArenaStatistic = new Game.MsgTournaments.MsgArena.User();
                    client.ArenaStatistic.ApplayInfo(client.Player);
                    client.ArenaStatistic.Info.ArenaPoints = 4000;
                    Game.MsgTournaments.MsgArena.ArenaPoll.TryAdd(client.Player.UID, client.ArenaStatistic);
                }
                if (Game.MsgTournaments.MsgTeamArena.ArenaPoll.TryGetValue(client.Player.UID, out client.TeamArenaStatistic))
                {
                    client.TeamArenaStatistic.ApplayInfo(client.Player);
                }
                else
                {
                    client.TeamArenaStatistic = new Game.MsgTournaments.MsgTeamArena.User();
                    client.TeamArenaStatistic.ApplayInfo(client.Player);
                    client.TeamArenaStatistic.Info.ArenaPoints = 4000;
                    Game.MsgTournaments.MsgTeamArena.ArenaPoll.TryAdd(client.Player.UID, client.TeamArenaStatistic);
                }
                client.FullLoading = true;
            }
            else
            {
                var player = RestApiHelper.GetPlayer(UID);
                client.Player.Body = player.Body;
                client.Player.Face = player.Face;
                client.Player.Name = player.Name;
                client.Player.Spouse = player.Spouse;
                client.Player.Class = player.Class;
                client.Player.FirstClass = player.FirstClass;
                client.Player.SecondClass = player.SecondClass;
                client.Player.Avatar = player.Avatar;
                client.Player.Map = player.Map;
                client.Player.X = player.X;
                client.Player.Y = player.Y;
                client.MiningAttempts = player.MiningAttempts;
                client.Player.PMap = player.PMap;
                client.Player.PMapX = player.PMapX;
                client.Player.PMapY = player.PMapY;

                if (Pool.OutMaps.Contains(client.Player.Map) || client.Player.Map == 1234 || client.Player.Map == 1767 || client.Player.Map == 1017 || client.Player.Map == 1081 || client.Player.Map == 2060 || client.Player.Map == 9972 || client.Player.Map == 1080 || client.Player.Map == 1999 || client.Player.Map == 3954
                 || client.Player.Map == 1806 || client.Player.Map == 1508 || client.Player.Map == 1036 || client.Player.Map == 1768 || client.Player.Map == 1505 || client.Player.Map == 1506 || client.Player.Map == 1509 || client.Player.Map == 1508 || client.Player.Map == 1507
                 || client.Player.Map == 1801 || client.Player.Map == 1780 || client.Player.Map == 1779 || client.Player.Map == 3071 || client.Player.Map == 1068 || client.Player.Map == 3830 || client.Player.Map == 3831 || client.Player.Map == 3832 || client.Player.Map == 3834 || client.Player.Map == 3826 || client.Player.Map == 3827 || client.Player.Map == 3828 || client.Player.Map == 3829
                 || client.Player.Map == 3833 || client.Player.Map == 3825 || client.Player.Map == 10088 || client.Player.Map == 10089 || client.Player.Map == 10090 || client.Player.Map == 44455 || client.Player.Map == 44456 || client.Player.Map == 44457
                 || client.Player.Map == 44460 || client.Player.Map == 44461 || client.Player.Map == 44462 || client.Player.Map == 44463 || client.Player.Map == 4000 || client.Player.Map == 4003 || client.Player.Map == 4006 || client.Player.Map == 4008 || client.Player.Map == 4009)
                {
                    client.Player.Map = 1002;
                    client.Player.X = 428;
                    client.Player.Y = 378;
                }
                if (client.Player.Map == 1038 && Game.MsgTournaments.MsgSchedules.GuildWar.Proces == Game.MsgTournaments.ProcesType.Alive)
                {
                    client.Player.Map = 1002;
                    client.Player.X = 428;
                    client.Player.Y = 378;
                }

                client.Player.Agility = player.Agility;
                client.Player.Strength = player.Strength;
                client.Player.Spirit = player.Spirit;
                client.Player.Vitality = player.Vitality;
                client.Player.Atributes = player.Attributes;
                client.Player.Reborn = player.Reborn;
                client.Player.Level = player.Level;
                client.Player.Hair = player.Hair;
                client.Player.Experience = player.Experience;
                client.Player.HitPoints = player.HitPoints;
                client.Player.Mana = player.Mana;
                client.Player.ConquerPoints = player.ConquerPoints;
                client.Player.ArenaCPS = player.ArenaCPS;
                client.Player.BoundConquerPoints = player.BoundConquerPoints;
                client.Player.Money = player.Money;
                client.Player.VirtutePoints = player.VirtuePoints;
                client.Player.VirtuteEntries = player.VirtueEntries;
                client.Player.PKPoints = player.PKPoints;
                client.Player.PVPPoints = player.PVPPoints;
                /*Paid Donation System */
                client.Player.PayNobilitySystem.SetData(UID,
                    player.NobilityLastDonation,
                    player.NobilityPaidRank,
                    (long)player.NobilityPeriodTime,
                    player.NobilityIsActive);
                /*End of the code*/
                client.Player.JailerUID = player.JailerUID;
                client.Player.QuizPoints = player.QuizPoints;
                client.Player.Enilghten = player.Enlighten;
                client.Player.EnlightenReceive = player.EnlightenReceive;
                client.Player.DailySignUpDays = player.DailySignUpDays;
                client.Player.DailyMonth = player.DailyMonth;
                client.Player.DailySignUpRewards = player.DailySignUpRewards;
                client.Player.VipLevel = player.VipLevel;
                client.Player.ExpireVip = player.ExpireVip;
                client.Player.LastDragonPill = player.LastDragonPill;
                if (DateTime.Now > client.Player.ExpireVip)
                {
                    if (client.Player.VipLevel >= 1)
                        client.Player.VipLevel = 0;
                }
                client.Achievement = new AchievementCollection();
                client.Achievement.Load(player.Archivement);
                client.Player.WHMoney = player.WHMoney;
                client.Player.BlessTime = player.BlessTime;
                client.Player.TrinityPoints = player.TrinityPoints;
                client.Player.SpouseUID = player.SpouseUID;
                client.Player.HeavenBlessing = player.HeavenBlessing;
                client.Player.HeavenBlessTime = player.HeavenBlessTime;
                client.Player.HuntingBlessing = player.HuntingBlessing;
                client.Player.OnlineTrainingPoints = player.OnlineTrainingPoints;
                client.Player.JoinOnflineTG = player.JoinOfflineTG;
                client.Player.RateExp = player.RateExp;
                client.Player.DExpTime = player.DExpTime;
                client.Player.Day = player.Day;
                client.Player.BDExp = player.BDExp;
                client.Player.ExpBallUsed = player.ExpBallUsed;
                client.Player.MysteryFruit = player.MysteryFruit;
                client.Player.FreeVIP = player.FreeVIP;
                DataCore.LoadClient(client.Player);
                client.Player.GuildID = player.GuildID;
                client.Player.GuildRank = (Role.Flags.GuildMemberRank)player.GuildRank;
                client.Player.EnabledTitles = player.EnabledTitles;
                if (client.Player.EnabledTitles.Length > 0)
                {
                    // Load enables titles
                    string[] titleIDs = client.Player.EnabledTitles.Split(';');
                    foreach (string titleId in titleIDs)
                    {
                        client.Player.AddTitle(byte.Parse(titleId), true);
                    }
                }
                if (client.Player.GuildID != 0)
                {
                    Role.Instance.Guild myguild;
                    if (Role.Instance.Guild.GuildPoll.TryGetValue(client.Player.GuildID, out myguild))
                    {
                        client.Player.MyGuild = myguild;
                        Role.Instance.Guild.Member member;
                        if (myguild.Members.TryGetValue(client.Player.UID, out member))
                        {
                            member.IsOnline = true;
                            client.Player.GuildID = (ushort)myguild.Info.GuildID;
                            client.Player.MyGuildMember = member;
                            client.Player.GuildRank = member.Rank;
                            client.Player.GuildBattlePower = myguild.ShareMemberPotency(member.Rank);
                        }
                        else
                        {
                            client.Player.MyGuild = null;
                            client.Player.GuildID = 0;
                            client.Player.GuildRank = (Role.Flags.GuildMemberRank)0;
                        }
                    }
                    else
                    {
                        client.Player.MyGuild = null;
                        client.Player.GuildID = 0;
                        client.Player.GuildRank = (Role.Flags.GuildMemberRank)0;
                    }
                }
                client.Player.SubClass = new Role.Instance.SubClass();
                client.Player.SubClass.Load(player.SubProfInfo);
                client.Player.SubClass.CreateSpawn(client);
                if (Role.Instance.Chi.ChiPool.ContainsKey(UID))
                {
                    client.Player.MyChi = Role.Instance.Chi.ChiPool[UID];
                    Role.Instance.Chi.ComputeStatus(client.Player.MyChi);
                }
                else
                    client.Player.MyChi = new Role.Instance.Chi(UID);
                if (Role.Instance.Flowers.ClientPoll.ContainsKey(UID))
                    client.Player.Flowers = Role.Instance.Flowers.ClientPoll[UID];
                else
                    client.Player.Flowers = new Role.Instance.Flowers(UID, client.Player.Name);
                DBActions.ReadLine Linereader = new DBActions.ReadLine(player.Flowers, '/');
                client.Player.Flowers.FreeFlowers = Linereader.Read((uint)0);
                Role.Instance.Nobility nobility;
                if (client.Player.PayNobilitySystem.IsActive == false)
                {
                    if (Pool.NobilityRanking.TryGetValue(UID, out nobility))
                    {
                        client.Player.Nobility = nobility;
                        client.Player.NobilityRank = client.Player.Nobility.Rank;
                    }
                    else
                    {
                        client.Player.Nobility = new Role.Instance.Nobility(client);
                        client.Player.Nobility.Donation = player.NobilityDonation;
                        client.Player.NobilityRank = client.Player.Nobility.Rank;
                    }
                }
                else
                {
                    if (DateTime.Now >= client.Player.PayNobilitySystem.PeriodTime)
                    {

                        client.Player.Nobility = new Role.Instance.Nobility(client);
                        client.Player.Nobility.Donation = client.Player.PayNobilitySystem.LastNobilityDonation;
                        client.Player.NobilityRank = client.Player.Nobility.Rank;
                        client.Player.PayNobilitySystem.SetDataAnyWay(0, 0, 0, 0, false);
                    }
                    else
                    {
                        client.Player.Nobility = new Role.Instance.Nobility(client);
                        switch (client.Player.PayNobilitySystem.PaidRank)
                        {
                            case global::GameServer.Role.Instance.Nobility.NobilityRank.King:
                                {
                                    client.Player.Nobility.Donation = 0;
                                    client.Player.Nobility.Position = 0;
                                    client.Player.NobilityRank = client.Player.Nobility.Rank;
                                    break;
                                }
                            case global::GameServer.Role.Instance.Nobility.NobilityRank.Prince:
                                {
                                    client.Player.Nobility.Donation = 0;
                                    client.Player.Nobility.Position = 4;
                                    client.Player.NobilityRank = client.Player.Nobility.Rank;
                                    break;
                                }
                            case global::GameServer.Role.Instance.Nobility.NobilityRank.Duke:
                                {
                                    client.Player.Nobility.Donation = 0;
                                    client.Player.Nobility.Position = 15;
                                    client.Player.NobilityRank = client.Player.Nobility.Rank;
                                    break;
                                }
                        }
                    }
                }
                Role.Instance.AssociateGS.MyAsociats Associate;
                if (Role.Instance.AssociateGS.Associates.TryGetValue(client.Player.UID, out Associate))
                {
                    client.Player.Associate = Associate;
                    client.Player.Associate.MyClient = client;
                    client.Player.Associate.Online = true;
                    if (client.Player.Associate.Associat.ContainsKey(Role.Instance.AssociateGS.Mentor))
                    {
                        foreach (var member in client.Player.Associate.Associat[Role.Instance.AssociateGS.Mentor].Values)
                        {
                            if (member.UID != 0)
                            {
                                Role.Instance.AssociateGS.MyAsociats mentor;
                                if (Role.Instance.AssociateGS.Associates.TryGetValue(member.UID, out mentor))
                                {
                                    client.Player.MyMentor = mentor;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    client.Player.Associate = new Role.Instance.AssociateGS.MyAsociats(client.Player.UID);
                    client.Player.Associate.MyClient = client;
                    client.Player.Associate.Online = true;
                }
                client.Player.ClanUID = player.ClanUID;
                if (client.Player.ClanUID != 0)
                {
                    Role.Instance.Clan myclan;
                    if (Role.Instance.Clan.Clans.TryGetValue(client.Player.ClanUID, out myclan))
                    {
                        client.Player.MyClan = myclan;
                        Role.Instance.Clan.Member member;
                        if (myclan.Members.TryGetValue(client.Player.UID, out member))
                        {
                            member.Online = true;
                            client.Player.ClanName = myclan.Name;
                            client.Player.MyClanMember = member;
                            client.Player.ClanRank = (ushort)member.Rank;
                        }
                        else
                        {
                            client.Player.MyClan = null;
                            client.Player.ClanUID = 0;
                            client.Player.ClanRank = 0;
                        }
                    }
                    else
                        client.Player.ClanUID = 0;
                }
                client.Player.FirstRebornLevel = player.FirstRebornLevel;
                client.Player.SecoundeRebornLevel = player.SecondRebornLevel;
                client.Player.Reincarnation = player.Reincarnation;
                client.Player.LotteryEntries = player.LotteryEntries;
                client.Player.QuestEntries = player.QuestEntries;
                client.Player.Quest2Entries = player.Quest2Entries;
                client.Player.Quest3Entries = player.Quest3Entries;
                client.Player.MonsterEntries = player.MonsterEntries;
                client.Player.MonsterEntries2 = player.MonsterEntries2;
                client.Player.RemoveWeapon = player.RemoveWeapon;
                client.Player.DbTry = player.DbTry;
                client.DemonExterminator.ReadLine(player.DemonExterminator);
                client.Player.MyKillerUID = player.MyKillerUID;
                client.Player.MyKillerName = player.MyKillerName;
                client.Player.CursedTimer = player.CursedTimer;
                client.Player.AtiveQuestApe = player.AtiveQuestApe;
                client.Player.AparenceType = player.AparenceType;
                client.Player.TournamentKills = player.TournamentKills;
                client.Player.OnlineMinutes = player.OnlineMinutes;
                client.Player.HistoryChampionPoints = player.HistoryChampionPoints;
                client.Player.AddChampionPoints(player.ChampionPoints, false);
                client.Player.TodayChampionPoints = player.TodayChampionPoints;
                client.Player.DailySpiritBeadItem = player.DailySpiritBeadItem;
                LoadSecurityPassword(player.SecurityPass, client);
                client.Player.TCCaptainTimes = player.TCCaptainTimes;
                client.Player.LastMan = player.LastMan;
                client.Player.PTB = player.PTB;
                client.Player.Get5Out = player.Get5Out;
                client.Player.FreezeWar = player.FreezeWar;
                client.Player.Infection = player.Infection;
                client.Player.TheCaptain = player.TheCaptain;
                client.Player.Kungfu = player.Kungfu;
                client.Player.VampireWar = player.VampireWar;
                client.Player.WhackTheThief = player.WhackTheThief;
                client.Player.SSFB = player.SSFB;
                client.Player.DonationPoints = player.DonationPoints;
                client.Player.NameEditCount = player.NameEditCount;
                client.Player.MainFlag = (Role.Player.MainFlagType)player.MainFlag;
                client.Player.CountryID = (ushort)player.CountryID;
                client.Player.InventorySashCount = player.InventorySashCount;
                client.Player.MyFootBallPoints = player.MyFootBallPoints;
                client.Player.ExpProtection = player.ExpProtection;
                client.BanCount = player.BanCount;
                client.Player.ExtraAtributes = player.ExtraAttributes;
                client.Player.OpenHousePack = player.OpenHousePack;
                client.Player.JoinPowerArenaStamp = player.JoinPowerArenaStamp;
                client.Player.GiveFlowersToPerformer = player.GiveFlowersToPerformer;
                client.Player.UseChiToken = player.UseChiToken;
                client.Player.OnlinePoints = player.OnlinePoints;
                client.Player.VotePoints = player.VotePoints;
                client.TotalMobsKilled = player.TotalMobsKilled;
                client.TotalMobsKilled2 = player.TotalMobsKilled2;
                client.TotalSouls = player.TotalSouls;
                client.Player.DragonPills = player.DragonPills;
                client.Player.BossPoints = player.BossPoints;
                client.Player.PVEPoints = player.PVEPoints;
                LoadClientItems(client);
                client.Player.LoadAgates(player.Agates);
                LoadClientSpells(client);
                LoadClientProfs(client);
                RoleQuests.Load(client);
                Role.Instance.House.Load(client);
                ResetingEveryDay(client);
                Role.Instance.Confiscator Container;
                if (Pool.QueueContainer.PollContainers.TryGetValue(client.Player.UID, out Container))
                    client.Confiscator = Container;
                try
                {
                    client.Player.Associate.OnLoading(client);
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
                if (Game.MsgTournaments.MsgArena.ArenaPoll.TryGetValue(client.Player.UID, out client.ArenaStatistic))
                {
                    client.ArenaStatistic.ApplayInfo(client.Player);
                }
                else
                {
                    client.ArenaStatistic = new Game.MsgTournaments.MsgArena.User();
                    client.ArenaStatistic.ApplayInfo(client.Player);
                    client.ArenaStatistic.Info.ArenaPoints = 4000;
                    Game.MsgTournaments.MsgArena.ArenaPoll.TryAdd(client.Player.UID, client.ArenaStatistic);
                }
                if (Game.MsgTournaments.MsgTeamArena.ArenaPoll.TryGetValue(client.Player.UID, out client.TeamArenaStatistic))
                {
                    client.TeamArenaStatistic.ApplayInfo(client.Player);
                }
                else
                {
                    client.TeamArenaStatistic = new Game.MsgTournaments.MsgTeamArena.User();
                    client.TeamArenaStatistic.ApplayInfo(client.Player);
                    client.TeamArenaStatistic.Info.ArenaPoints = 4000;
                    Game.MsgTournaments.MsgTeamArena.ArenaPoll.TryAdd(client.Player.UID, client.TeamArenaStatistic);
                }
                client.FullLoading = true;
            }
        }

        public static void LoadClientItems(Client.GameClient client)
        {
            if (ServerConfig.DbFromFiles)
            {
                string p = Path.Combine(ServerConfig.DbLocation, "PlayersItems", client.Player.UID + ".bin");
                BinaryFileHelper binHelper = new(p);
                int ItemCount = binHelper.ReadInt();
                for (int x = 0; x < ItemCount; x++)
                {
                    ClientItems.DBItem Item = binHelper.Read<ClientItems.DBItem>();
                    if (!Item.Fake && Item.ITEM_ID != 0)
                    {
                        if (Item.ITEM_ID == 750000)//demonExterminator jar
                            client.DemonExterminator.ItemUID = Item.UID;
                        Game.MsgServer.MsgGameItem ClienItem = Item.GetDataItem();
                        if (Database.ItemType.ItemPosition(Item.ITEM_ID) != (ushort)Role.Flags.ConquerItem.RidingCrop)
                        {
                            if (Item.Bless > 1)
                                Item.Bless = 1;
                        }
                        if (Item.ITEM_ID == 720598 || (Item.ITEM_ID >= 2100065 && Item.ITEM_ID <= 2100095))
                            ClienItem.Bound = 0;
                        if (Item.WH_ID != 0)
                        {
                            if (Item.WH_ID == 100)
                            {
                                if (Item.Position > 0 && Item.Position <= (ushort)Role.Flags.ConquerItem.AlternateGarment)
                                {
                                    client.Equipment.ClientItems.TryAdd(Item.UID, ClienItem);
                                }
                            }
                            else
                            {
                                if (!client.Warehouse.ClientItems.ContainsKey(Item.WH_ID))
                                    client.Warehouse.ClientItems.TryAdd(Item.WH_ID, new System.Collections.Concurrent.ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());
                                if (client.Player.GuildID == 0)
                                    ClienItem.Inscribed = 0;
                                client.Warehouse.ClientItems[Item.WH_ID].TryAdd(Item.UID, ClienItem);
                            }
                        }
                        else
                        {
                            if (Item.Position > 0 && Item.Position <= (ushort)Role.Flags.ConquerItem.AlternateGarment)
                            {
                                if (client.Player.GuildID == 0)
                                    ClienItem.Inscribed = 0;
                                client.Equipment.ClientItems.TryAdd(Item.UID, ClienItem);
                            }
                            else if (Item.Position == 0)
                            {
                                if (client.Player.GuildID == 0)
                                    ClienItem.Inscribed = 0;
                                client.Inventory.AddDBItem(ClienItem);
                            }
                        }
                    }
                }
                binHelper.Close();
            } else
            {
                List<PlayerItem> playerItems = RestApiHelper.GetPlayerItems(client.Player.UID);
                foreach (PlayerItem Item in playerItems)
                {
                    Game.MsgServer.MsgGameItem ClientItem = new();
                    ClientItem.UID = Item.Uid;
                    ClientItem.ITEM_ID = Item.ItemId;
                    ClientItem.Durability = Item.Durability;
                    ClientItem.MaximDurability = Item.MaxDurability;
                    ClientItem.Position = Item.Position;
                    ClientItem.SocketProgress = Item.SocketProgress;
                    ClientItem.SocketOne = (Gem)Item.SocketOne;
                    ClientItem.SocketTwo = (Gem)Item.SocketTwo;
                    ClientItem.Effect = (ItemEffect)Item.Effect;
                    ClientItem.Plus = Item.Plus;
                    ClientItem.Bless = Item.Bless;
                    ClientItem.Bound = Item.Bound;
                    ClientItem.Enchant = Item.Enchant;
                    ClientItem.Suspicious = Item.Suspicious;
                    ClientItem.Locked = Item.Locked;
                    ClientItem.PlusProgress = Item.PlusProgress;
                    ClientItem.Inscribed = Item.Inscribed;
                    ClientItem.Activate = Item.Activate;
                    ClientItem.TimeLeftInMinutes = Item.TimeLeftInMinutes;
                    ClientItem.StackSize = Item.StackSize;
                    ClientItem.WH_ID = Item.WarehouseId;
                    ClientItem.Color = (Color)Item.Color;
                    ClientItem.IDEvent = Item.IDEvent;

                    // Load purification (Souls)
                    ClientItem.Purification.ItemUID = Item.Uid;
                    ClientItem.Purification.PurificationItemID = Item.PurificationItemID;
                    ClientItem.Purification.PurificationLevel = Item.PurificationLevel;
                    ClientItem.Purification.PurificationDuration = Item.PurificationDuration;
                    try
                    {
                        ClientItem.Purification.AddedOn = DateTime.FromBinary(Item.PurificationAddedOn);
                    }
                    catch
                    {
                        ClientItem.Purification.AddedOn = DateTime.FromBinary(0);
                    }
                    if (!ClientItem.Purification.InLife)
                        ClientItem.Purification = new Game.MsgServer.MsgItemExtra.Purification();

                    // Load Refinary (Enchantments)
                    bool failed = false;
                    ClientItem.Refinary.ItemUID = Item.Uid;
                    if (Item.EffectID != 0)
                    {
                        Refinery.Item BaseAddingItem;
                        if (Pool.RefineryItems.TryGetValue(Item.EffectID, out BaseAddingItem))
                        {
                            if (ItemType.ItemPosition(Item.ItemId) != BaseAddingItem.ForItemPosition && BaseAddingItem.Name != "(Heavy)Ring")
                                failed = true;
                            if (BaseAddingItem.Name == "(Heavy)Ring")
                            {
                                if (Item.ItemId / 1000 != 151)
                                    failed = true;
                            }

                            if (BaseAddingItem.Name == "2-Handed" || BaseAddingItem.Name == "2-Handed")
                            {
                                if (!ItemType.IsTwoHand(Item.ItemId))
                                    failed = true;
                            }
                            if (BaseAddingItem.Name == "Bow" || BaseAddingItem.Name == "Bow")
                            {
                                if (!ItemType.IsBow(Item.ItemId))
                                    failed = true;
                            }
                            if (BaseAddingItem.Name == "Bracelet" || BaseAddingItem.Name == "Bracelet")
                            {
                                if (!ItemType.IsBraclet(Item.ItemId))
                                    failed = true;
                            }
                            if (BaseAddingItem.Name == "Ring" || BaseAddingItem.Name == "Ring")
                            {
                                if (!ItemType.IsRing(Item.ItemId) && !ItemType.IsHeavyRing(Item.ItemId))
                                    failed = true;
                            }
                        }
                        if (!failed)
                        {
                            ClientItem.Refinary.EffectID = Item.EffectID;
                            ClientItem.Refinary.EffectLevel = Item.EffectLevel;
                            ClientItem.Refinary.EffectPercent = Item.EffectPercent;
                            ClientItem.Refinary.EffectPercent2 = Item.EffectPercent2;
                            ClientItem.Refinary.EffectDuration = Item.EffectDuration;
                        }
                    }
                    try
                    {
                        ClientItem.Refinary.AddedOn = DateTime.FromBinary(Item.EffectAddedOn);
                    }
                    catch
                    {
                        ClientItem.Refinary.AddedOn = DateTime.FromBinary(0);
                    }
                    if (!ClientItem.Refinary.InLife)
                    {
                        ClientItem.Refinary = new Game.MsgServer.MsgItemExtra.Refinery();
                    }
                    ClientItem.Fake = Item.Fake;
                    ClientItem.UnLockTimer = Item.UnlockTimer;
                    ClientItem.EndDate = DateTime.FromBinary(Item.Expiration);
                    if (Item.ItemId == 750000)//demonExterminator jar
                        client.DemonExterminator.ItemUID = Item.Uid;
                    if (ItemType.ItemPosition(Item.ItemId) != (ushort)Role.Flags.ConquerItem.RidingCrop)
                    {
                        if (Item.Bless > 1)
                            Item.Bless = 1;
                    }
                    if (Item.ItemId == 720598 || (Item.ItemId >= 2100065 && Item.ItemId <= 2100095))
                        ClientItem.Bound = 0;
                    if (Item.ItemId == 721758)
                        ClientItem.Bound = 1;
                    if (Item.WarehouseId != 0)
                    {
                        if (Item.WarehouseId == 100)
                        {
                            if (Item.Position > 0 && Item.Position <= (ushort)Role.Flags.ConquerItem.AlternateGarment)
                            {
                                client.Equipment.ClientItems.TryAdd(Item.Uid, ClientItem);
                            }
                        }
                        else
                        {
                            if (!client.Warehouse.ClientItems.ContainsKey(Item.WarehouseId))
                                client.Warehouse.ClientItems.TryAdd(Item.WarehouseId, new System.Collections.Concurrent.ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());
                            if (client.Player.GuildID == 0)
                                ClientItem.Inscribed = 0;
                            client.Warehouse.ClientItems[Item.WarehouseId].TryAdd(Item.Uid, ClientItem);
                        }
                    }
                    else
                    {
                        if (Item.Position > 0 && Item.Position <= (ushort)Role.Flags.ConquerItem.AlternateGarment)
                        {
                            if (client.Player.GuildID == 0)
                                ClientItem.Inscribed = 0;
                            client.Equipment.ClientItems.TryAdd(Item.Uid, ClientItem);
                        }
                        else if (Item.Position == 0)
                        {
                            if (client.Player.GuildID == 0)
                                ClientItem.Inscribed = 0;
                            client.Inventory.AddDBItem(ClientItem);
                        }
                    }
                }
            }
        }

        public static Dictionary<uint, DateTime> SaveClientItemsTimes = new();
        public static void SaveClientItems(Client.GameClient client)
        {
            if (ServerConfig.DbFromFiles)
            {
                BinaryFileHelper binary = new();
                if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, "PlayersItems", client.Player.UID + ".bin"), FileMode.Create))
                {
                    ClientItems.DBItem DBItem = new ClientItems.DBItem();
                    int ItemCount;
                    ItemCount = client.GetItemsCount();
                    binary.WriteInt(ItemCount);
                    foreach (var item in client.AllMyItems())
                    {
                        if (!item.Fake && item.ITEM_ID != 0)
                        {
                            DBItem.GetDBItem(item);
                            binary.Write(DBItem);
                        }
                    }
                    binary.Close();
                }
            } else
            {
                DateTime SaveTime = DateTime.Now;
                SaveClientItemsTimes.TryGetValue(client.EntityID, out SaveTime);
                SaveClientItemsTimes[client.EntityID] = DateTime.Now;
                List<PlayerItem> PlayerItems = new List<PlayerItem>();
                if (DateTime.Now > SaveTime.AddSeconds(10)) // Prevent massive updates for the same EntityID
                {
                    foreach (var item in client.AllMyItems())
                    {
                        if (!item.Fake && item.ITEM_ID != 0)
                        {
                            PlayerItem pItem = new(item, client.Player.UID);
                            PlayerItems.Add(pItem);
                        }
                    }
                    RestApiHelper.UpdatePlayerItems(new UpdatePlayerItem() { EntityUid = client.Player.UID, PlayerItems = PlayerItems });
                }
            }
        }
        public unsafe static void LoadClientProfs(Client.GameClient client)
        {
            if (ServerConfig.DbFromFiles)
            {
                BinaryFileHelper binary = new();
                if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, "PlayersProfs", client.Player.UID + ".bin"), FileMode.Open))
                {
                    ClientProficiency.DBProf DBProf;
                    int CountProf = binary.ReadInt();
                    for (int x = 0; x < CountProf; x++)
                    {
                        DBProf = binary.Read<ClientProficiency.DBProf>();
                        var ClientProf = DBProf.GetClientProf();
                        client.MyProfs.ClientProf.TryAdd(ClientProf.ID, ClientProf);
                    }
                    binary.Close();
                }
            } else
            {
                List<PlayerProficiency> playerProfs = RestApiHelper.GetPlayerProfs(client.Player.UID);
                foreach (PlayerProficiency playerProf in playerProfs)
                {
                    ClientProficiency.DBProf DBProf = new ClientProficiency.DBProf() { ID = playerProf.TypeID, Experience = playerProf.Experience, Level = playerProf.Level, PreviousLevel = playerProf.PreviousLevel };
                    var ClientProf = DBProf.GetClientProf();
                    client.MyProfs.ClientProf.TryAdd(ClientProf.ID, ClientProf);
                }
            }
        }
        public unsafe static void SaveClientProfs(Client.GameClient client)
        {
            if (ServerConfig.DbFromFiles)
            {
                BinaryFileHelper binary = new();
                if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, "PlayersProfs", client.Player.UID + ".bin"), FileMode.Create))
                {
                    ClientProficiency.DBProf DBProf = new ClientProficiency.DBProf();
                    int CountProf;
                    CountProf = client.MyProfs.ClientProf.Count;
                    binary.Write(CountProf);
                    foreach (var prof in client.MyProfs.ClientProf.Values)
                    {
                        DBProf.GetDBSpell(prof);
                        binary.Write(DBProf);
                    }
                    binary.Close();
                }
            } else
            {
                List<PlayerProficiency> profsToSave = new();
                foreach (var prof in client.MyProfs.ClientProf.Values)
                {
                    profsToSave.Add(new PlayerProficiency() { Level = prof.Level, PlayerUid = client.Player.UID, TypeID = prof.ID, Experience = prof.Experience, PreviousLevel = prof.PreviousLevel });
                }
                RestApiHelper.AddUpdatePlayerProfs(client.Player.UID, profsToSave);
            }
        }
        public static void LoadClientSpells(Client.GameClient client)
        {
            if (ServerConfig.DbFromFiles)
            {
                BinaryFileHelper binary = new();
                if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, "PlayersSpells", client.Player.UID + ".bin"), FileMode.Open))
                {
                    ClientSpells.DBSpell DBSpell;
                    int CountSpell = binary.ReadInt();
                    for (int x = 0; x < CountSpell; x++)
                    {
                        DBSpell = binary.Read<ClientSpells.DBSpell>();
                        var clientSpell = DBSpell.GetClientSpell();
                        if (Pool.Magic.ContainsKey(clientSpell.ID))
                        {
                            client.MySpells.ClientSpells.TryAdd(clientSpell.ID, clientSpell);
                        }
                    }
                    binary.Close();
                }
            } else
            {
                List<PlayerSpell> playerSpells = RestApiHelper.GetPlayerSpells(client.Player.UID);
                foreach(PlayerSpell playerSpell in playerSpells)
                {
                    ClientSpells.DBSpell DBSpell = new ClientSpells.DBSpell() { ID = playerSpell.TypeID, Experience = playerSpell.Experience, Level = playerSpell.Level, PreviousLevel = playerSpell.PreviousLevel, SoulLevel = playerSpell.SoulLevel, UseJiangSpell = playerSpell.UseJiangSpell};
                    var clientSpell = DBSpell.GetClientSpell();
                    if (Pool.Magic.ContainsKey(clientSpell.ID))
                    {
                        client.MySpells.ClientSpells.TryAdd(clientSpell.ID, clientSpell);
                    }
                }
            }
        }
        public static void SaveClientSpells(Client.GameClient client)
        {
            if (ServerConfig.DbFromFiles)
            {
                BinaryFileHelper binary = new();
                if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, "PlayersSpells", client.Player.UID + ".bin"), FileMode.Create))
                {
                    ClientSpells.DBSpell DBSpell = new ClientSpells.DBSpell();
                    int SpellCount;
                    SpellCount = client.MySpells.ClientSpells.Count;
                    binary.WriteInt(SpellCount);
                    foreach (var spell in client.MySpells.ClientSpells.Values)
                    {
                        if (Pool.Magic.ContainsKey(spell.ID))
                        {
                            DBSpell.GetDBSpell(spell);
                            binary.Write(DBSpell);
                        }
                    }
                    binary.Close();
                }
            } else
            {
                List<PlayerSpell> spellsToSave = new();
                foreach (var spell in client.MySpells.ClientSpells.Values)
                {
                    ClientSpells.DBSpell DBSpell = new ClientSpells.DBSpell();
                    if (Pool.Magic.ContainsKey(spell.ID))
                    {
                        DBSpell.GetDBSpell(spell);
                        spellsToSave.Add(new PlayerSpell() { Level = spell.Level, PlayerUid = client.Player.UID, TypeID = spell.ID, Experience = spell.Experience, PreviousLevel = spell.PreviousLevel, SoulLevel = spell.SoulLevel, UseJiangSpell = spell.UseSpellSoul });
                    }
                }
                RestApiHelper.AddUpdatePlayerSpells(client.Player.UID, spellsToSave);
            }
        }
        public static void CreateCharacter(Client.GameClient client)
        {
            if (ServerConfig.DbFromFiles)
            {
                string characterPath = Path.Combine(ServerConfig.DbLocation, "Users", client.Player.UID + ".ini");
                File.Create(characterPath).Close();
                IniFileHelper write = new(characterPath);
                write.Write<uint>("Character", "UID", client.Player.UID);
                write.Write<ushort>("Character", "Body", client.Player.Body);
                write.Write<ushort>("Character", "Face", client.Player.Face);
                write.WriteString("Character", "Name", client.Player.Name);
                write.Write<byte>("Character", "Class", client.Player.Class);
                write.Write<uint>("Character", "Map", client.Player.Map);
                write.Write<ushort>("Character", "X", client.Player.X);
                write.Write<ushort>("Character", "Y", client.Player.Y);
            } else
            {
                RestApiHelper.CreatePlayer(new Player() { UID = client.Player.UID, Body = client.Player.Body, Face = client.Player.Face, Name = client.Player.Name, Class = client.Player.Class, Map = client.Player.Map, X = client.Player.X, Y = client.Player.Y });
            }

            client.ArenaStatistic = new Game.MsgTournaments.MsgArena.User();
            client.ArenaStatistic.ApplayInfo(client.Player);
            client.ArenaStatistic.Info.ArenaPoints = 4000;
            Game.MsgTournaments.MsgArena.ArenaPoll.TryAdd(client.Player.UID, client.ArenaStatistic);

            client.Player.Nobility = new Role.Instance.Nobility(client);

            client.TeamArenaStatistic = new Game.MsgTournaments.MsgTeamArena.User();
            client.TeamArenaStatistic.ApplayInfo(client.Player);
            client.TeamArenaStatistic.Info.ArenaPoints = 4000;

            Game.MsgTournaments.MsgTeamArena.ArenaPoll.TryAdd(client.Player.UID, client.TeamArenaStatistic);

            client.Player.Associate = new Role.Instance.AssociateGS.MyAsociats(client.Player.UID);
            client.Player.Associate.MyClient = client;
            client.Player.Associate.Online = true;


            client.Player.Flowers = new Role.Instance.Flowers(client.Player.UID, client.Player.Name);
            client.Player.SubClass = new Role.Instance.SubClass();
            client.Player.MyChi = new Role.Instance.Chi(client.Player.UID);
            client.Achievement = new AchievementCollection();

            client.FullLoading = true;

        }

        public static bool AllowCreate(uint UID)
        {
            return !BaseFunc.UserExists(UID);
        }
        public static void UpdateGuildMember(Role.Instance.Guild.Member Member)
        {
            if (ServerConfig.DbFromFiles)
            {
                IniFileHelper write = new(Path.Combine(ServerConfig.DbLocation, "Users", Member.UID + ".ini"));
                write.Write<ushort>("Character", "GuildRank", 0);
            } else
            {
                Player p = RestApiHelper.GetPlayer(Member.UID);
                p.GuildRank = 0;
                RestApiHelper.UpdatePlayer(p);
            }
        }
        public static void UpdateGuildMember(Role.Instance.Guild.UpdateDB Member)
        {
            if (ServerConfig.DbFromFiles)
            {
                IniFileHelper write = new(Path.Combine(ServerConfig.DbLocation, "Users", Member.UID + ".ini"));
                write.Write<ushort>("Character", "GuildRank", 0);
                write.Write<ushort>("Character", "GuildID", 0);
            } else
            {
                Player p = RestApiHelper.GetPlayer(Member.UID);
                p.GuildRank = 0;
                p.GuildID = 0;
                RestApiHelper.UpdatePlayer(p);
            }
        }

        public static void UpdateMapRace(Role.GameMap map)
        {
            IniFileHelper write = new(Path.Combine(ServerConfig.DbLocation, "maps", map.ID + ".ini"));
            write.Write<uint>("info", "race_record", map.RecordSteedRace);
        }
        public static void UpdateClanMember(Role.Instance.Clan.Member Member)
        {
            if (ServerConfig.DbFromFiles)
            {
                IniFileHelper write = new(Path.Combine(ServerConfig.DbLocation, "Users", Member.UID + ".ini"));
                write.Write<uint>("Character", "ClanID", 0);
                write.Write<ushort>("Character", "ClanRank", 0);
                write.Write<uint>("Character", "ClanDonation", 0);
            }
            else
            {
                Player p = RestApiHelper.GetPlayer(Member.UID);
                p.ClanUID = 0;
                p.ClanRank = 0;
                p.ClanDonation = 0;
                RestApiHelper.UpdatePlayer(p);
            }
        }
        public static void DestroySpouse(Client.GameClient client)
        {
            if (client.Player.SpouseUID != 0)
            {
                if (ServerConfig.DbFromFiles)
                {
                    IniFileHelper write = new(Path.Combine(ServerConfig.DbLocation, "Users", client.Player.SpouseUID + ".ini"));
                    write.Write<uint>("Character", "SpouseUID", 0);
                    write.WriteString("Character", "Spouse", "None");
                }
                else
                {
                    Player p = RestApiHelper.GetPlayer(client.Player.SpouseUID);
                    p.SpouseUID = 0;
                    p.Spouse = "None";
                    RestApiHelper.UpdatePlayer(p);
                }
                client.Player.SpouseUID = 0;
            }

            client.Player.Spouse = "None";
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.Spouse, false, new string[1] { "None" });
            }
        }
        public static string GenerateDate()
        {
            DateTime now = DateTime.Now;
            return now.Year.ToString() + "and" + now.Month.ToString() + "and" + now.Day.ToString() + "and" + now.Hour.ToString() + "and" + now.Minute.ToString() + "and" + now.Second.ToString();
        }
        public static void UpdateSpouse(Client.GameClient client)
        {
            if (client.Player.SpouseUID != 0)
            {
                if (ServerConfig.DbFromFiles)
                {
                    IniFileHelper write = new(Path.Combine(ServerConfig.DbLocation, "Users", client.Player.SpouseUID + ".ini"));
                    write.WriteString("Character", "Spouse", client.Player.Name);
                }
                else
                {
                    Player p = RestApiHelper.GetPlayer(client.Player.SpouseUID);
                    p.Spouse = client.Player.Name;
                    RestApiHelper.UpdatePlayer(p);
                }
            }
        }
        public static ExecuteLogin LoginQueue = new ExecuteLogin();

        public class ExecuteLogin : ConcurrentSmartThreadQueue<object>
        {
            public object SynRoot = new object();
            public ExecuteLogin()
                : base(5)
            {
                Start(10);
            }
            public void TryEnqueue(object obj)
            {
                //  lock (SynRoot)
                {

                    base.Enqueue(obj);
                }
            }
            protected unsafe override void OnDequeue(object obj, int time)
            {
                try
                {

                    if (obj is string)
                    {
                        string text = obj as string;
                        if (text.StartsWith("[EVENT]"))
                        {
                            string UnhandledExceptionsPath = "EventsLogs" + Path.DirectorySeparatorChar;

                            var dt = DateTime.Now;
                            string date = "EVENT" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(Path.Combine(Program.StartupPath, UnhandledExceptionsPath)))
                                Directory.CreateDirectory(Path.Combine(Program.StartupPath, UnhandledExceptionsPath));
                            if (!Directory.Exists(Path.Combine(Program.StartupPath, UnhandledExceptionsPath, date)))
                                Directory.CreateDirectory(Path.Combine(Program.StartupPath, UnhandledExceptionsPath, date));

                            string fullPath = Path.Combine(Program.StartupPath, UnhandledExceptionsPath, date);

                            if (!File.Exists(Path.Combine(fullPath, date + ".txt")))
                            {
                                File.WriteAllLines(Path.Combine(fullPath, date + ".txt"), new string[0]);
                            }

                            using (var SW = File.AppendText(Path.Combine(fullPath, date + ".txt")))
                            {
                                SW.WriteLine(dt.ToShortTimeString() + " :: " + text);
                                SW.Close();
                            }
                        }
                        else if (text.StartsWith("[DemonBox]"))
                        {

                            string UnhandledExceptionsPath = "Logs" + Path.DirectorySeparatorChar;

                            var dt = DateTime.Now;
                            string date = "DemonBox" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(Path.Combine(Program.StartupPath, UnhandledExceptionsPath)))
                                Directory.CreateDirectory(Path.Combine(Program.StartupPath, UnhandledExceptionsPath));
                            if (!Directory.Exists(Path.Combine(Program.StartupPath, UnhandledExceptionsPath, date)))
                                Directory.CreateDirectory(Path.Combine(Program.StartupPath, UnhandledExceptionsPath, date));

                            string fullPath = Path.Combine(Program.StartupPath, UnhandledExceptionsPath + date);

                            if (!File.Exists(Path.Combine(fullPath, date + ".txt")))
                            {
                                File.WriteAllLines(Path.Combine(fullPath, date + ".txt"), new string[0]);
                            }


                            using (var SW = File.AppendText(Path.Combine(fullPath, date + ".txt")))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        else if (text.StartsWith("[Chat]"))
                        {

                            string UnhandledExceptionsPath = "Logs" + Path.DirectorySeparatorChar;

                            var dt = DateTime.Now;
                            string date = "Chat" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(Program.StartupPath + UnhandledExceptionsPath))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);

                            string fullPath = Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date + "" + Path.DirectorySeparatorChar + "";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        else if (text.StartsWith("[CHEAT]"))
                        {

                            string UnhandledExceptionsPath = "Logs" + Path.DirectorySeparatorChar;

                            var dt = DateTime.Now;
                            string date = "CHEAT" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(Program.StartupPath + UnhandledExceptionsPath))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);

                            string fullPath = Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date + "" + Path.DirectorySeparatorChar + "";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        else if (text.StartsWith("[Item]"))
                        {

                            string UnhandledExceptionsPath = "Logs" + Path.DirectorySeparatorChar;

                            var dt = DateTime.Now;
                            string date = "Item" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(Program.StartupPath + UnhandledExceptionsPath))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);

                            string fullPath = Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date + "" + Path.DirectorySeparatorChar + "";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        else if (text.StartsWith("[CallStack]"))
                        {

                            string UnhandledExceptionsPath = "Logs" + Path.DirectorySeparatorChar;

                            var dt = DateTime.Now;
                            string date = "CallStack" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(Program.StartupPath + UnhandledExceptionsPath))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);

                            string fullPath = Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date + "" + Path.DirectorySeparatorChar + "";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        else if (text.StartsWith("[PVEPoints]"))
                        {

                            string UnhandledExceptionsPath = "Logs" + Path.DirectorySeparatorChar;

                            var dt = DateTime.Now;
                            string date = "PVEPoints" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(Program.StartupPath + UnhandledExceptionsPath))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);

                            string fullPath = Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date + "" + Path.DirectorySeparatorChar + "";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        else if (text.StartsWith("[Poker]"))
                        {
                            string UnhandledExceptionsPath = "Logs" + Path.DirectorySeparatorChar;

                            var dt = DateTime.Now;
                            string date = "Poker" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(Program.StartupPath + UnhandledExceptionsPath))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);

                            string fullPath = Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date + "" + Path.DirectorySeparatorChar + "";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        if (text.StartsWith("[GMLogs]") || text.StartsWith("[HDLogs]"))
                        {
                            string UnhandledExceptionsPath = "Logs" + Path.DirectorySeparatorChar;

                            var dt = DateTime.Now;
                            string date = "GMLogs";

                            if (!Directory.Exists(Program.StartupPath + UnhandledExceptionsPath))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);

                            string fullPath = Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date + "" + Path.DirectorySeparatorChar + "";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        if (text.StartsWith("[TradeConquerPoints]"))
                        {
                            string UnhandledExceptionsPath = "Logs" + Path.DirectorySeparatorChar;
                            var dt = DateTime.Now;
                            string date = "TradeConquerPoints";
                            if (!Directory.Exists(Program.StartupPath + UnhandledExceptionsPath))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);

                            string fullPath = Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date + "" + Path.DirectorySeparatorChar + "";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        if (text.StartsWith("[TradeMoney]"))
                        {
                            string UnhandledExceptionsPath = "Logs" + Path.DirectorySeparatorChar;
                            var dt = DateTime.Now;
                            string date = "TradeMoney";
                            if (!Directory.Exists(Program.StartupPath + UnhandledExceptionsPath))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);

                            string fullPath = Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date + "" + Path.DirectorySeparatorChar + "";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        if (text.StartsWith("[TradeItem]"))
                        {
                            string UnhandledExceptionsPath = "Logs" + Path.DirectorySeparatorChar;
                            var dt = DateTime.Now;
                            string date = "TradeItem";
                            if (!Directory.Exists(Program.StartupPath + UnhandledExceptionsPath))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);

                            string fullPath = Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date + "" + Path.DirectorySeparatorChar + "";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        if (text.StartsWith("[FullTrade]"))
                        {
                            string UnhandledExceptionsPath = "Logs" + Path.DirectorySeparatorChar;
                            var dt = DateTime.Now;
                            string date = "FullTrade";
                            if (!Directory.Exists(Program.StartupPath + UnhandledExceptionsPath))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date);

                            string fullPath = Program.StartupPath + "" + Path.DirectorySeparatorChar + "" + UnhandledExceptionsPath + date + "" + Path.DirectorySeparatorChar + "";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }
                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }

                    }
                    else if (obj is Role.GameMap)
                    {
                        UpdateMapRace(obj as Role.GameMap);
                    }
                    else if (obj is Role.Instance.Guild.Member)
                    {
                        UpdateGuildMember(obj as Role.Instance.Guild.Member);
                    }
                    else if (obj is Role.Instance.Guild.UpdateDB)
                    {
                        UpdateGuildMember(obj as Role.Instance.Guild.UpdateDB);
                    }
                    else if (obj is Role.Instance.Clan.Member)
                    {
                        UpdateClanMember(obj as Role.Instance.Clan.Member);
                    }
                    else
                    {
                        Client.GameClient client = obj as Client.GameClient;
                        if (client.Player != null && client.Player.Delete)
                        {
                            if (client.Map != null)
                                client.Map.View.LeaveMap<Role.Player>(client.Player);
                            DateTime Now64 = DateTime.Now;
                            Console.WriteLine("Client " + client.Player.Name + " deleted their player character.");
                            if (ServerConfig.DbFromFiles)
                            {
                                string pathHouses = Path.Combine(ServerConfig.DbLocation, "Houses", client.Player.UID + ".bin");
                                string pathHousesNew = Path.Combine(ServerConfig.DbLocation, "Backup", "Houses", client.Player.UID + "date" + GenerateDate() + ".bin");
                                string pathQuests = Path.Combine(ServerConfig.DbLocation, "Quests", client.Player.UID + ".ini");
                                string pathQuestsNew = Path.Combine(ServerConfig.DbLocation, "Backup", "Houses", client.Player.UID + "date" + GenerateDate() + ".ini");
                                string pathUsers = Path.Combine(ServerConfig.DbLocation, "Users", client.Player.UID + ".ini");
                                string pathUsersNew = Path.Combine(ServerConfig.DbLocation, "Backup", "Users", client.Player.UID + "date" + GenerateDate() + ".ini");
                                string pathPlayerSpells = Path.Combine(ServerConfig.DbLocation, "PlayersSpells", client.Player.UID + ".bin");
                                string pathPlayerSpellsNew = Path.Combine(ServerConfig.DbLocation, "Backup", "PlayersSpells", client.Player.UID + "date" + GenerateDate() + ".bin");
                                string pathPlayerProfs = Path.Combine(ServerConfig.DbLocation, "PlayersProfs", client.Player.UID + ".bin");
                                string pathPlayerProfsNew = Path.Combine(ServerConfig.DbLocation, "Backup", "PlayersProfs", client.Player.UID + "date" + GenerateDate() + ".bin");
                                string pathPlayerItems = Path.Combine(ServerConfig.DbLocation, "PlayersItems", client.Player.UID + ".bin");
                                string pathPlayerItemsNew = Path.Combine(ServerConfig.DbLocation, "Backup", "PlayersItems", client.Player.UID + "date" + GenerateDate() + ".bin");
                                if (File.Exists(pathHouses))
                                    File.Copy(pathHouses, pathHousesNew, true);
                                if (File.Exists(pathQuests))
                                    File.Copy(pathQuests, pathQuestsNew, true);
                                if (File.Exists(pathUsers))
                                    File.Copy(pathUsers, pathUsersNew, true);
                                if (File.Exists(pathPlayerSpells))
                                    File.Copy(pathPlayerSpells, pathPlayerSpellsNew, true);
                                if (File.Exists(pathPlayerProfs))
                                    File.Copy(pathPlayerProfs, pathPlayerProfsNew, true);
                                if (File.Exists(pathPlayerItems))
                                    File.Copy(pathPlayerItems, pathPlayerItemsNew);
                                if (File.Exists(pathUsers))
                                    File.Delete(pathUsers);
                                if (File.Exists(pathPlayerSpells))
                                    File.Delete(pathPlayerSpells);
                                if (File.Exists(pathPlayerProfs))
                                    File.Delete(pathPlayerProfs);
                                if (File.Exists(pathPlayerItems))
                                    File.Delete(pathPlayerItems);
                                try
                                {
                                    if (File.Exists(pathQuests))
                                    {
                                        File.Delete(pathQuests);
                                    }
                                }
                                catch
                                {

                                }
                                Role.Instance.House house;
                                if (client.MyHouse != null && Role.Instance.House.HousePoll.ContainsKey(client.Player.UID))
                                    Role.Instance.House.HousePoll.TryRemove(client.Player.UID, out house);

                                if (File.Exists(pathHouses))
                                {
                                    File.Delete(pathHouses);
                                }
                            } else
                            {
                                // TODO with mysql mode that not are saved, maybe in future
                            }
                            Role.Instance.Chi chi;
                            if (Role.Instance.Chi.ChiPool.ContainsKey(client.Player.UID))
                            {
                                Role.Instance.Chi.ChiPool.TryRemove(client.Player.UID, out chi);
                                if (ServerConfig.DbFromFiles)
                                {
                                    IniFileHelper write = new IniFileHelper(Path.Combine(ServerConfig.DbLocation, "BackUp", "ChiInfo.txt"));
                                    write.WriteString(client.Player.UID.ToString() + "date" + GenerateDate() + "", "Dragon", chi.Dragon.ToString());
                                    write.WriteString(client.Player.UID.ToString() + "date" + GenerateDate() + "", "Phoenix", chi.Phoenix.ToString());
                                    write.WriteString(client.Player.UID.ToString() + "date" + GenerateDate() + "", "Turtle", chi.Turtle.ToString());
                                    write.WriteString(client.Player.UID.ToString() + "date" + GenerateDate() + "", "Tiger", chi.Tiger.ToString());
                                }
                            }
                            Role.Instance.Flowers flow;
                            if (Role.Instance.Flowers.ClientPoll.ContainsKey(client.Player.UID))
                            {
                                Role.Instance.Flowers.ClientPoll.TryRemove(client.Player.UID, out flow);
                            }
                            Role.Instance.AssociateGS.MyAsociats Associate;
                            if (Role.Instance.AssociateGS.Associates.TryGetValue(client.Player.UID, out Associate))
                            {
                                Role.Instance.AssociateGS.Associates.TryRemove(client.Player.UID, out Associate);
                            }
                            Client.GameClient user;
                            if (Pool.GamePoll.TryRemove(client.Player.UID, out user))
                            {
                                if (Pool.NameUsed.Contains(client.Player.Name.GetHashCode()))
                                {
                                    lock (Pool.NameUsed)
                                        Pool.NameUsed.Remove(client.Player.Name.GetHashCode());
                                }
                            }
                            return;
                        }
                        if ((client.ClientFlag & Client.ServerFlag.RemoveSpouse) == Client.ServerFlag.RemoveSpouse)
                        {
                            DestroySpouse(client);
                            client.ClientFlag &= ~Client.ServerFlag.RemoveSpouse;
                            return;
                        }
                        if ((client.ClientFlag & Client.ServerFlag.UpdateSpouse) == Client.ServerFlag.UpdateSpouse)
                        {
                            UpdateSpouse(client);
                            client.ClientFlag &= ~Client.ServerFlag.UpdateSpouse;
                            return;
                        }
                        if ((client.ClientFlag & Client.ServerFlag.SetLocation) != Client.ServerFlag.SetLocation && (client.ClientFlag & Client.ServerFlag.OnLoggion) == Client.ServerFlag.OnLoggion)
                        {
                            Game.MsgServer.MsgLoginClient.LoginHandler(client, client.OnLogin);
                        }
                        else if ((client.ClientFlag & Client.ServerFlag.QueuesSave) == Client.ServerFlag.QueuesSave)
                        {
                            if (client.Player.OnTransform)
                            {
                                client.Player.HitPoints = Math.Min(client.Player.HitPoints, (int)client.Status.MaxHitpoints);
                                client.Player.Mana = (ushort)Math.Min(client.Player.Mana, (int)client.Status.MaxMana);
                            }
                            SaveClient(client);
                        }
                    }
                }
                catch (Exception e) { Console.SaveException(e); }
            }
        }

    }
}