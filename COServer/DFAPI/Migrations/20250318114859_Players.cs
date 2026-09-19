using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Players : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UID = table.Column<uint>(type: "int unsigned", nullable: false),
                    Body = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Face = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Spouse = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Class = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FirstClass = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SecondClass = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Avatar = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Map = table.Column<uint>(type: "int unsigned", nullable: false),
                    X = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Y = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    MiningAttempts = table.Column<uint>(type: "int unsigned", nullable: false),
                    PMap = table.Column<uint>(type: "int unsigned", nullable: false),
                    PMapX = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    PMapY = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Agility = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Strength = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Spirit = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Vitality = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Attributes = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Reborn = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Hair = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Experience = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    HitPoints = table.Column<int>(type: "int", nullable: false),
                    Mana = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ConquerPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    ArenaCPS = table.Column<uint>(type: "int unsigned", nullable: false),
                    BoundConquerPoints = table.Column<int>(type: "int", nullable: false),
                    Money = table.Column<uint>(type: "int unsigned", nullable: false),
                    VirtuePoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    VirtueEntries = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PKPoints = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    PVPPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    NobilityLastDonation = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    NobilityPaidRank = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NobilityPeriodTime = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    NobilityIsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    JailerUID = table.Column<uint>(type: "int unsigned", nullable: false),
                    QuizPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    Enlighten = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    EnlightenReceive = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    DailySignUpDays = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    DailyMonth = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DailySignUpRewards = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    VipLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ExpireVip = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastDragonPill = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Archivement = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WHMoney = table.Column<long>(type: "bigint", nullable: false),
                    BlessTime = table.Column<uint>(type: "int unsigned", nullable: false),
                    TrinityPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    SpouseUID = table.Column<uint>(type: "int unsigned", nullable: false),
                    HeavenBlessing = table.Column<int>(type: "int", nullable: false),
                    HeavenBlessTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    HuntingBlessing = table.Column<uint>(type: "int unsigned", nullable: false),
                    OnlineTrainingPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    JoinOfflineTG = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RateExp = table.Column<uint>(type: "int unsigned", nullable: false),
                    DExpTime = table.Column<uint>(type: "int unsigned", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    BDExp = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ExpBallUsed = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MysteryFruit = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FreeVIP = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    GuildID = table.Column<uint>(type: "int unsigned", nullable: false),
                    GuildRank = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    EnabledTitles = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubProfInfo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Flowers = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NobilityDonation = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ClanUID = table.Column<uint>(type: "int unsigned", nullable: false),
                    ClanRank = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ClanDonation = table.Column<uint>(type: "int unsigned", nullable: false),
                    FirstRebornLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SecondRebornLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Reincarnation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LotteryEntries = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    QuestEntries = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Quest2Entries = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Quest3Entries = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MonsterEntries = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MonsterEntries2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RemoveWeapon = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DbTry = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DemonExterminator = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MyKillerUID = table.Column<uint>(type: "int unsigned", nullable: false),
                    MyKillerName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CursedTimer = table.Column<int>(type: "int", nullable: false),
                    AtiveQuestApe = table.Column<uint>(type: "int unsigned", nullable: false),
                    AparenceType = table.Column<uint>(type: "int unsigned", nullable: false),
                    TournamentKills = table.Column<uint>(type: "int unsigned", nullable: false),
                    OnlineMinutes = table.Column<uint>(type: "int unsigned", nullable: false),
                    HistoryChampionPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    ChampionPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    TodayChampionPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    DailySpiritBeadItem = table.Column<uint>(type: "int unsigned", nullable: false),
                    SecurityPass = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TCCaptainTimes = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    LastMan = table.Column<uint>(type: "int unsigned", nullable: false),
                    PTB = table.Column<uint>(type: "int unsigned", nullable: false),
                    Get5Out = table.Column<uint>(type: "int unsigned", nullable: false),
                    FreezeWar = table.Column<uint>(type: "int unsigned", nullable: false),
                    Infection = table.Column<uint>(type: "int unsigned", nullable: false),
                    TheCaptain = table.Column<uint>(type: "int unsigned", nullable: false),
                    Kungfu = table.Column<uint>(type: "int unsigned", nullable: false),
                    VampireWar = table.Column<uint>(type: "int unsigned", nullable: false),
                    WhackTheThief = table.Column<uint>(type: "int unsigned", nullable: false),
                    SSFB = table.Column<uint>(type: "int unsigned", nullable: false),
                    DonationPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    NameEditCount = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    MainFlag = table.Column<uint>(type: "int unsigned", nullable: false),
                    CountryID = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    InventorySashCount = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    MyFootBallPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    ExpProtection = table.Column<uint>(type: "int unsigned", nullable: false),
                    BanCount = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ExtraAttributes = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    OpenHousePack = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    JoinPowerArenaStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    GiveFlowersToPerformer = table.Column<int>(type: "int", nullable: false),
                    UseChiToken = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    OnlinePoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    VotePoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    TotalMobsKilled = table.Column<uint>(type: "int unsigned", nullable: false),
                    TotalMobsKilled2 = table.Column<uint>(type: "int unsigned", nullable: false),
                    TotalSouls = table.Column<uint>(type: "int unsigned", nullable: false),
                    DragonPills = table.Column<int>(type: "int", nullable: false),
                    BossPoints = table.Column<int>(type: "int", nullable: false),
                    PVEPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    Agates = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChiPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    ChiDragon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChiPhoenix = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChiTurtle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChiTiger = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GuildCPsDonate = table.Column<uint>(type: "int unsigned", nullable: false),
                    GuildMoneyDonate = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    GuildPKDonation = table.Column<uint>(type: "int unsigned", nullable: false),
                    GuildLastLogin = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    GuildCTFExploits = table.Column<uint>(type: "int unsigned", nullable: false),
                    GuildCTFConquerPointsReward = table.Column<uint>(type: "int unsigned", nullable: false),
                    GuildCTFMoneyReward = table.Column<uint>(type: "int unsigned", nullable: false),
                    GuildCTFClaimed = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "players");
        }
    }
}
