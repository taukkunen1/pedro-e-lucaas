using Core;
using GameServer.Client;
using GameServer.Database;
using GameServer.Game.MsgServer;
using GameServer.Game.MsgServer.AttackHandler;
using GameServer.Game.MsgTournaments;
using GameServer.Role.Instance;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
namespace GameServer
{

    public static class Pool
    {
        public static ConcurrentDictionary<uint, GameClient> DisconnectPool = new ConcurrentDictionary<uint, GameClient>();
        public static bool OFFChi = true;
        public static bool OFFSubClass = true;

        private static Random Rand = new Random();
        public static void ExitToTwin(GameClient player)
        {
            player.SendSysMesage("", MsgMessage.ChatMode.FirstRightCorner);
            switch (Rand.Next(14))
            {
                case 1: player.Teleport(424, 377, 1002, 0, true, true); break;
                case 2: player.Teleport(456, 357, 1002, 0, true, true); break;
                case 3: player.Teleport(445, 371, 1002, 0, true, true); break;
                case 4: player.Teleport(447, 383, 1002, 0, true, true); break;
                case 5: player.Teleport(442, 387, 1002, 0, true, true); break;
                case 6: player.Teleport(435, 387, 1002, 0, true, true); break;
                case 7: player.Teleport(431, 390, 1002, 0, true, true); break;
                case 8: player.Teleport(421, 381, 1002, 0, true, true); break;
                case 9: player.Teleport(423, 392, 1002, 0, true, true); break;
                case 10: player.Teleport(464, 377, 1002, 0, true, true); break;
                case 11: player.Teleport(458, 366, 1002, 0, true, true); break;
                case 12: player.Teleport(432, 357, 1002, 0, true, true); break;
                default: player.Teleport(438, 366, 1002, 0, true, true); break;
            }
        }
        public static void AwayMsg(MsgMessage msg, GameClient awayplayer, GameClient client, MsgMessage.ChatMode chatMode)
        {
            string away_msg = "I`m away at the moment, Please contact me later.";
            using (var rec = new global::GameServer.ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                var X = new MsgMessage(
                    away_msg,
                    msg._From,
                    awayplayer.Player.Name,
                    MsgMessage.MsgColor.red,
                    MsgMessage.ChatMode.PopUP);
                var X2 = new MsgMessage(
                    away_msg,
                    msg._From,
                    awayplayer.Player.Name,
                    MsgMessage.MsgColor.red,
                    chatMode);
                X2.Mesh = awayplayer.Player.Mesh;
                X2.Color = 4294967295;
                X2.MessageUID1 = awayplayer.Player.UID;
                client.Send(X.GetArray(stream));
                client.Send(X2.GetArray(stream));
            }
        }
        static int _last = 0;
        public static int Online
        {
            get
            {
                int current = GamePoll.Count;
                if (current > _last)
                    _last = current;
                return current;
            }
        }
        public static int MaxOnline
        {
            get { return _last; }
        }
        public static IniFileHelper MonsterFileTxt;
        public static DateTime LastServerPulse, LastSavePulse, LastGuildPulse;
        public static DateTime SaveServerDatabase;
        public static DateTime UpdateServerStatus = DateTime.Now;
        public static uint DefaultItemCounterUID { get; set; } = 1000; // Initial UID
        public static Counter ItemUIDCounter;
        public static string ConnectionString;
        public static string LogginKey = "C238xs65pjy7HU9Q";
        public static bool FullLoading = false;
        public static uint ResetServerDay = 0;
        public static bool ResetedAlready = false;
        public static SendGlobalPacket SendGlobalPackets;
        public static Random GetRandom = new Random();
        public static RandomLite LiteRandom = new RandomLite();
        public static MyRandom Rnd = new MyRandom();
        public static DateTime ResetRandom = new DateTime();
        public static List<uint> OutMaps = new List<uint>() { 700, 1767, 501, 5263, 1234, 1801, 1737, 1036, 1616, 1234, 1801 };
        //public static List<Game.MsgEvents.Events> Events = new List<Game.MsgEvents.Events>();
        public static List<uint> NoDropItems = new List<uint>() { 1764, 700, 3954, 1616, 1950, 1737 };
        public static List<uint> FreePkMap = new List<uint>() { 2071, 5262, 1111, 1112, 1113, 1114, 5261, 3826, 3998, 5215, 3071, 6000, 501, 5263, 1767, 6001, 1505, 1005, 1038, 700, 1508/*PkWar*/, MsgCaptureTheFlag.MapID, MsgLastManStand.MapID };
        public static List<uint> BlockAttackMap = new List<uint>()
        {
            1700, 3825,3830, 3831,1616,
            3832,3834,3826,3827,3828,
            3829,3833, 9995,1068, 4020,
            4000, 4003, 4006, 4008, 4009 ,
            1860 ,1858, 1801, 1780, 1779/*Ghost Map*/,
            9972, 1806, 1002, 3954, 3081, 1036, 1004,
            1008, 601, 1006, 1511, 1039, 700, 1737,
            MsgEliteGroup.WaitingAreaID
        };
        public static List<uint> BlockTeleportMap = new List<uint>() { 601, 6000, 6001, 1005, 700, 1858, 1860, 3852, MsgEliteGroup.WaitingAreaID, 1768, 1038, 1111, 1112, 1113, 1114, 1508, 501, 8839 };
        public static ushort[] PriceUpdateProf = new ushort[] { 600, 600, 600, 600, 600, 600, 1200, 1800, 3000, 3600, 6000, 7200, 7200, 7200, 7200, 8318, 12170, 17735, 25639, 31537 };
        public static Dictionary<DBLevExp.Sort, Dictionary<byte, DBLevExp>> LevelInfo = new Dictionary<DBLevExp.Sort, Dictionary<byte, DBLevExp>>();
        public static Dictionary<uint, string> MapName = new Dictionary<uint, string>() { { 1015, "BirdIsland" }, { 1011, "PhoenixCastle" }, { 1000, "DesertCity" }, { 1020, "ApeMountain" }, { 1001, "MysticCastle" } };
        public static ConcurrentDictionary<uint, TheCrimeTable> TheCrimePoll = new ConcurrentDictionary<uint, TheCrimeTable>();
        public static Dictionary<ushort, ushort> WeaponSpells = new Dictionary<ushort, ushort>();
        public static MagicType Magic = new MagicType();
        public static LotteryTable NewLottery = new LotteryTable();
        public static SubProfessionInfo SubClassInfo = new SubProfessionInfo();
        public static Dictionary<uint, Game.MsgMonster.MonsterFamily> MonsterFamilies = new Dictionary<uint, Game.MsgMonster.MonsterFamily>();
        public static Refinery RefineryItems;
        public static RefinaryBoxes DBRerinaryBoxes;
        public static ItemType ItemsBase;
        public static MapDictionary<uint, Role.GameMap> ServerMaps;
        public static ConcurrentDictionary<uint, GameClient> GamePoll;
        public static List<int> NameUsed;
        public static List<byte[]> LoadPackets = new List<byte[]>();
        public static List<uint> ProtectMapSpells = new List<uint>() { 1038 };
        public static List<uint> MapCounterHits = new List<uint>() { 1005, 6000 };
        public static RebornInfomations RebornInfo;
        public static ArenaTable Arena = new ArenaTable();
        public static TeamArenaTable TeamArena = new TeamArenaTable();
        public static Counter ClientCounter = new Counter(1000000);
        public static ConfiscatorTable QueueContainer = new ConfiscatorTable();
        public static Nobility.NobilityRanking NobilityRanking = new Nobility.NobilityRanking();
        public static ChiRank ChiRanking = new ChiRank();
        public static Flowers.FlowersRankingToday FlowersRankToday = new Flowers.FlowersRankingToday();
        public static Flowers.FlowerRanking GirlsFlowersRanking = new Flowers.FlowerRanking();
        public static Flowers.FlowerRanking BoysFlowersRanking = new Flowers.FlowerRanking(false);
        public static Dictionary<TournamentType, ITournament> Tournaments = new Dictionary<TournamentType, ITournament>();
        public static List<coords> LavaBeast = new List<coords>() {
        new coords(313,250) { },
        new coords(262,241) { },
        new coords(205,231) { },
        new coords(241,201) { },
        new coords(209,143) { },
        new coords(280,176) { },
        new coords(328,166) { },
        new coords(328,120) { },
        new coords(329,067) { },
        new coords(297,087) { },
        new coords(401,185) { },
        new coords(441,225) { },
        new coords(487,249) { },
        new coords(451,289) { },
        new coords(442,348) { },
        new coords(518,296) { },
        new coords(566,334) { },
        new coords(605,322) { },
        new coords(656,359) { },
        new coords(652,407) { },
        new coords(706,414) { },
        new coords(721,471) { },
        new coords(720,518) { },
        new coords(653,512) { },
        new coords(620,549) { },
        new coords(644,599) { },
        new coords(541,510) { },
        new coords(526,466) { },
        new coords(488,431) { },
        new coords(440,408) { },
        new coords(438,484) { },
        new coords(389,483) { },
        new coords(326,475) { },
        new coords(279,441) { },
        new coords(259,478) { },
        new coords(201,435) { },
        new coords(195,375) { },
        new coords(178,324) { },
        new coords(198,285) { },
        new coords(149,435) { },
        new coords(105,407) { },
        new coords(135,367) { },
        new coords(092,348) { },
        new coords(030,321) { },
        new coords(120,314) { },
        new coords(096,256) { },
        new coords(128,251) { },
        new coords(363,525) { },
        new coords(393,558) { },
        new coords(387,592) { },
        new coords(374,643) { },
        new coords(308,625) { },
        new coords(309,572) { },
        new coords(287,540) { },
        new coords(428,643) { },
        new coords(474,642) { },
        new coords(461,683) { },
        new coords(467,735) { },
        new coords(534,670) { },
        new coords(550,630) { },
        new coords(524,588) { },
        new coords(578,601) { }
        };
    }
}
