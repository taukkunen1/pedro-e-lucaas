using System;

namespace Core.Interfaces.GameServer
{
    public interface IPlayer
    {
        public uint UID { get; set; }
        public ushort Body { get; set; }
        public ushort Face { get; set; }
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
        public ushort Attributes { get; set; }
        public byte Reborn { get; set; }
        public byte Level { get; set; }
        public ushort Hair { get; set; }
        public ulong Experience { get; set; }
        public int HitPoints { get; set; }
        public ushort Mana { get; set; }
        public uint ConquerPoints { get; set; }
        public uint ArenaCPS { get; set; }
        public int BoundConquerPoints { get; set; }
        public uint Money { get; set; }
        public uint VirtuePoints { get; set; }
        public byte VirtueEntries { get; set; }
        public ushort PKPoints { get; set; }
        public uint PVPPoints { get; set; }
        public ulong NobilityLastDonation { get; set; }
        public byte NobilityPaidRank { get; set; }
        public ulong NobilityPeriodTime { get; set; }
        public bool NobilityIsActive { get; set; }
        public uint JailerUID { get; set; }
        public uint QuizPoints { get; set; }
        public ushort Enlighten { get; set; }
        public ushort EnlightenReceive { get; set; }
        public ulong DailySignUpDays { get; set; }
        public byte DailyMonth { get; set; }
        public byte DailySignUpRewards { get; set; }
        public byte VipLevel { get; set; }
        public DateTime ExpireVip { get; set; }
        public DateTime LastDragonPill { get; set; }
        public string Archivement { get; set; }
        public long WHMoney { get; set; }
        public uint BlessTime { get; set; }
        public uint TrinityPoints { get; set; }
        public uint SpouseUID { get; set; }
        public int HeavenBlessing { get; set; }
        public DateTime HeavenBlessTime { get; set; }
        public uint HuntingBlessing { get; set; }
        public uint OnlineTrainingPoints { get; set; }
        public DateTime JoinOfflineTG { get; set; }
        public uint RateExp { get; set; }
        public uint DExpTime { get; set; }
        public int Day { get; set; }
        public byte BDExp { get; set; }
        public byte ExpBallUsed { get; set; }
        public byte MysteryFruit { get; set; }
        public DateTime FreeVIP { get; set; }
        public uint GuildID { get; set; }
        public GuildMemberRank GuildRank { get; set; }
        public string EnabledTitles { get; set; }
        public string SubProfInfo { get; set; }
        public string Flowers { get; set; }
        public ulong NobilityDonation { get; set; }
        public uint ClanUID { get; set; }
        public ushort ClanRank { get; set; }
        public uint ClanDonation { get; set; }
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
        public string DemonExterminator { get; set; }
        public uint MyKillerUID { get; set; }
        public string MyKillerName { get; set; }
        public int CursedTimer { get; set; }
        public uint AtiveQuestApe { get; set; }
        public uint AparenceType { get; set; }
        public uint TournamentKills { get; set; }
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
        public uint DonationPoints { get; set; }
        public ushort NameEditCount { get; set; }
        public MainFlagType MainFlag { get; set; }
        public ushort CountryID { get; set; }
        public ushort InventorySashCount { get; set; }
        public uint MyFootBallPoints { get; set; }
        public uint ExpProtection { get; set; }
        public byte BanCount { get; set; }
        public ushort ExtraAttributes { get; set; }
        public byte OpenHousePack { get; set; }
        public DateTime JoinPowerArenaStamp { get; set; }
        public int GiveFlowersToPerformer { get; set; }
        public byte UseChiToken { get; set; }
        public uint OnlinePoints { get; set; }
        public uint VotePoints { get; set; }
        public uint TotalMobsKilled { get; set; }
        public uint TotalMobsKilled2 { get; set; }
        public uint TotalSouls { get; set; }
        public int DragonPills { get; set; }
        public int BossPoints { get; set; }
        public uint PVEPoints { get; set; }
        public string Agates { get; set; }
        // CHI
        public uint ChiPoints { get; set; }
        public string ChiDragon {get; set; }
        public string ChiPhoenix { get; set; }
        public string ChiTurtle { get; set; }
        public string ChiTiger { get; set; }
        // Guild Donations
        public uint GuildCPsDonate { get; set; }
        public ulong GuildMoneyDonate { get; set; }
        public uint GuildPKDonation { get; set; }
        public ulong GuildLastLogin { get; set; }
        // CaptureTheFlag
        public uint GuildCTFExploits { get; set; }
        public uint GuildCTFConquerPointsReward { get; set; }
        public uint GuildCTFMoneyReward { get; set; }
        public byte GuildCTFClaimed { get; set; }
    }
    public enum GuildMemberRank : ushort
    {
        GuildLeader = 1000,
        DeputyLeader = 990,
        HDeputyLeader = 980,
        LeaderSpouse = 920,
        Manager = 890,
        HonoraryManager = 880,

        TSupervisor = 859,//tulips
        OSupervisor = 858,//orchides
        CPSupervisor = 857,//cps
        ASupervisor = 856,//cred ca e donation super visor
        SSupervisor = 855,////silver
        GSupervisor = 854,//guide
        PKSupervisor = 853,//pk supervisor
        RoseSupervisor = 852,//rose
        LilySupervisor = 851,///lili
        Supervisor = 850,//toata donatia la guild sa fie mare
        HonorarySuperv = 840,

        Steward = 690,
        HonorarySteward = 680,
        DeputySteward = 650,
        DLeaderSpouse = 620,
        DLeaderAide = 611,
        LSpouseAide = 610,
        Aide = 602,

        TulipAgent = 599,
        OrchidAgent = 598,
        CPAgent = 597,
        ArsenalAgent = 596,
        SilverAgent = 595,
        GuideAgent = 594,
        PKAgent = 593,
        RoseAgent = 592,
        LilyAgent = 591,
        Agent = 590,

        SupervSpouse = 521,
        ManagerSpouse = 520,
        SupervisorAide = 511,
        ManagerAide = 510,

        TulipFollower = 499,
        OrchidFollower = 498,
        CPFollower = 497,
        ArsFollower = 496,
        SilverFollower = 495,
        GuideFollower = 494,
        PKFollower = 493,
        RoseFollower = 492,
        LilyFollower = 491,
        Follower = 490,

        StewardSpouse = 420,
        SeniorMember = 210,
        Member = 200,
        None = 0
    }
    public enum MainFlagType : uint
    {
        None = 0,
        CanClaim = 1 << 0,
        ShowSpecialItems = 1 << 1,
        ClaimGift = 1 << 2,
        OnMeleeAttack = 1 << 3,
    }
}
