using Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static GameServer.Role.Flags;

namespace GameServer.Models
{
    [Table("entities")]
    public class Player
    {
        [Key]
        public uint UID { get; set; }
        public uint Body { get; set; }
        public uint Face { get; set; }
        public string Name { get; set; }
        public string Spouse { get; set; }
        public byte Class { get; set; }
        public byte FirstClass { get; set; }
        public byte SecondClass { get; set; }
        public ushort Avatar { get; set; }
        public uint Map { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public uint MiningAttempts { get; set; }
        public uint PMap { get; set; }
        public ushort PMapX { get; set; }
        public ushort PMapY { get; set; }
        public ushort Agility { get; set; }
        public ushort Strength { get; set; }
        public ushort Spirit { get; set; }
        public ushort Vitality { get; set; }
        public ushort Atributes { get; set; }
        public byte Reborn { get; set; }
        public ushort Level { get; set; }
        public ushort Hair { get; set; }
        public ulong Experience { get; set; }
        public uint HitPoints { get; set; }
        public ushort Mana { get; set; }
        public uint ConquerPoints { get; set; }
        public uint BoundConquerPoints { get; set; }
        public uint ArenaCPS { get; set; }
        public uint Money { get; set; }
        public uint VirtutePoints { get; set; }
        public uint VirtuteEntries { get; set; }
        public uint PKPoints { get; set; }
        public uint PVPPoints { get; set; }
        [NotMapped]
        public Role.Instance.DaysNobility PayNoblitySystem { get; set; }

        public uint JailerUID { get; set; }
        public uint QuizPoints { get; set; }
        public ushort Enilghten { get; set; }
        public ushort EnlightenReceive { get; set; }
        public ulong DailySignUpDays { get; set; }
        public byte DailyMonth { get; set; }
        public byte DailySignUpRewards { get; set; }
        public byte VipLevel { get; set; }
        public DateTime ExpireVip { get; set; }
        public DateTime LastDragonPill { get; set; }
        public string Archivement { get; set; }

        public ulong WHMoney { get; set; }
        public uint BlessTime { get; set; }
        public uint TrinityPoints { get; set; }
        public uint SpouseUID { get; set; }
        public uint HeavenBlessing { get; set; }
        public DateTime HeavenBlessTime { get; set; }
        public uint HuntingBlessing { get; set; }
        public uint OnlineTrainingPoints { get; set; }
        public DateTime JoinOnflineTG { get; set; }
        public uint RateExp { get; set; }
        public uint DExpTime { get; set; }
        public uint Day { get; set; }
        public byte BDExp { get; set; }
        public byte ExpBallUsed { get; set; }
        public byte MysteryFruit { get; set; }
        public DateTime FreeVIP { get; set; }
        public uint GuildID { get; set; }
        public GuildMemberRank GuildRank { get; set; }
        public uint GuildBattlePower { get; set; }
        [NotMapped]
        public Role.Instance.Guild.Member MyGuildMember { get; set; }
        [NotMapped]
        public Role.Instance.SubClass SubClass { get; set; }
        [NotMapped]
        public Role.Instance.Chi MyChi { get; set; }
        [NotMapped]
        public Role.Instance.Flowers Flowers { get; set; }
        [NotMapped]
        public uint FreeFlowers { get; set; }
        public uint ClanID { get; set; }
        public byte FirstRebornLevel { get; set; }
        public byte SecondRebornLevel { get; set; }
        public bool Reincarnation { get; set; }
        public byte LotteryEntries { get; set; }
        public byte QuestEntries { get; set; }
        public byte Quest2Entries { get; set; }
        public byte Quest3Entries { get; set; }
        public byte MonsterEntries { get; set; }
        public byte MonsterEntries2 { get; set; }
        public byte RemoveWeapon { get; set; }
        public bool DbTry { get; set; }
        public string DemonEx { get; set; }
        public uint PkUID { get; set; }
        public string PkName { get; set; }
        public uint Cursed { get; set; }
        public uint Enervant { get; set; }
        public uint AparenceType { get; set; }
        public uint TKills { get; set; }
        public uint OnlineMinutes { get; set; }
        public uint HistoryChampionPoints { get; set; }
        public uint ChampionPoints { get; set; }
        public uint TodayChampionPoints { get; set; }
        public uint DailySpiritBeadItem { get; set; }
        public string SecurityPass { get; set; }
        public byte TCCaptainTimes { get; set; }
        public uint LastMan { get; set; }
        public uint PTB { get; set; }
        public uint Get5Out { get; set; }
        public uint FreezeWar { get; set; }
        public uint Infection { get; set; }
        public uint TheCaptain { get; set; }
        public uint Kungfu { get; set; }
        public uint VampireWar { get; set; }
        public uint WhackTheThief { get; set; }
        public uint SSFB { get; set; }
        public uint RacePoints { get; set; }
        public ushort NameEditCount { get; set; }
        public uint MainFlag { get; set; }
        public ushort CountryID { get; set; }
        public ushort InventorySashCount { get; set; }
        public uint MyFootBallPoints { get; set; }
        public uint ExpProtection { get; set; }
        public byte BanCount { get; set; }
        public ushort ExtraAtributes { get; set; }
        public byte OpenHousePack { get; set; }
        public DateTime JoinPowerArenaStamp { get; set; }
        public uint GiveFlowersToPerformer { get; set; }
        public byte UseChiToken { get; set; }
        public uint OnlinePoints { get; set; }
        public uint VotePoints { get; set; }
        public uint TotalMobsKilled { get; set; }
        public uint TotalMobsKilled2 { get; set; }
        public uint TotalSouls { get; set; }
        public uint DragonPills { get; set; }
        public uint BossPoints { get; set; }
        public uint PVEPoints { get; set; }
    }
}
