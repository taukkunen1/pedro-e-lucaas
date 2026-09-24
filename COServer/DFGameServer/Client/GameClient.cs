using GameServer.Cryptography;
using GameServer.Game.MsgServer;
using GameServer.Game.MsgTournaments;
using GameServer.Role;
using GameServer.Role.Instance;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Client
{
    [Flags]
    public enum ServerFlag : ushort
    {
        None = 0,
        AcceptLogin = 1 << 0,
        CreateCharacter = 1 << 1,
        CreateCharacterSucces = 1 << 2,
        LoginFull = 1 << 3,
        SetLocation = 1 << 4,
        OnLoggion = 1 << 5,
        QueuesSave = 1 << 6,
        RemoveSpouse = 1 << 7,
        Disconnect = 1 << 8,
        UpdateSpouse = 1 << 9
    }
    public unsafe class GameClient
    {
        public TQCast5 Crypto;
        public DateTime StaminStamp = DateTime.Now.AddMilliseconds(MapGroupThread.User_Stamina);
        public AutoHunting AutoHunting;
        public DateTime LoaderTime = DateTime.Now;
        public uint StampThread = 0;
        public bool TerminateLoader = false;
        public bool ActiveClient = false;
        public uint EncryptTokenSpell = 0;
        public List<string> OpenedProcesses = new List<string>();
        /// <summary>
        /// /end 
        /// </summary>
        public bool QuenchesTheTree = false;
        public DateTime StampAutoAttackCallback = DateTime.Now;
        public DateTime StampPlayer_BuffersCallback = DateTime.Now;
        public DateTime StampItemsCallBack = DateTime.Now;
        public DateTime StampMiningCallBack = DateTime.Now;
        public DateTime StampSecondsCallback = DateTime.Now;
        public DateTime StampStaminaCallback = DateTime.Now;
        public DateTime StampAliveMonstersCallback = DateTime.Now;
        public DateTime StampMonster_BuffersCallback = DateTime.Now;
        public DateTime StampGuardsCallback = DateTime.Now;
        public DateTime StampReviversCallback = DateTime.Now;
        public void AutoAttackCallback() { Threading.AutoAttackCallback.Handle(this, 0); }
        public void BufferCallback() { Threading.BufferCallback.Handle(this, 0); }
        public void ItemsCallBack() { Threading.ItemsCallBack.Handle(this, 0); }
        public void MiningCallBack() { Threading.MiningCallBack.Handle(this, 0); }
        public void SecondsCallback() { Threading.SecondsCallback.Handle(this, 0); }
        public void StaminaCallback() { Threading.StaminaCallback.Handle(this, 0); }
        public void AliveMonstersCallback() { Threading.AliveMonstersCallback.Handle(this, 0); }
        public void GuardsCallback() { Threading.GuardsCallback.Handle(this, 0); }
        public void ReviversCallback() { Threading.ReviversCallback.Handle(this, 0); }
        public void BuffersCallback() { Threading.BuffersCallback.Handle(this, 0); }

        public IDisposable[] TimerSubscriptions;
        public object TimerSyncRoot;
        //public Game.ArenaDuel Arena;
        //public Game.MsgEvents.Events EventBase;
        public bool Mining = false;
        public DateTime NextMine;
        public uint MiningAttempts = 200;
        public uint ReduceDurabilityPercent = 0;
        public void StopMining()
        {
            Mining = false;
        }
        private uint totalmobskilled;
        public uint TotalMobsKilled//10,000 monster kill
        {
            get { return totalmobskilled; }
            set
            {
                totalmobskilled = value;
                if (Player != null && Player.QuestGUI != null)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Player.QuestGUI.SetQuestObjectives(stream, 2, value);
                    }
                }
            }
        }
        private uint totalmobskilled2;
        public uint TotalMobsKilled2//300,000 monster kill
        {
            get { return totalmobskilled2; }
            set
            {
                totalmobskilled2 = value;
                if (Player != null && Player.QuestGUI != null)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Player.QuestGUI.SetQuestObjectives(stream, 3, value);
                    }
                }
            }
        }
        public uint MobsKilled = 0;
        public DateTime StartQuizTimer = new DateTime();
        public int QuizRank = 0;
        public int GetQuizTimer()
        {
            TimeSpan now = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan old = new TimeSpan(StartQuizTimer.Ticks);
            return (int)(now.TotalSeconds - old.TotalSeconds);
        }
        public ushort QuizShowPoints = 0;
        public byte RightAnswer = 1;
        public byte BanCount = 0;
        public MsgInterServer.PipeServer.User PipeServer;
        public Game.MsgNpc.Npc OnRemoveNpc;
        public int TerainMask = 0;
        public MsgInterServer.PipeClient PipeClient = null;
        public bool IsConnectedInterServer() { return PipeClient != null; }
        public ulong ExpOblivion = 0;
        public byte TRyDisconnect = 2;
        public Database.AchievementCollection Achievement;
        public DateTime LastVIPTeleport = new DateTime();
        public DateTime CoolStamp = DateTime.Now;
        public DateTime LastVIPTeamTeleport = new DateTime();
        public Role.Instance.SlotMachine SlotMachine = null;
        public Game.MsgTournaments.MsgTeamArena.User TeamArenaStatistic;
        public Game.MsgTournaments.MsgArena.User ArenaStatistic;
        public Game.MsgTournaments.MsgArena.Match ArenaMatch;
        public Game.MsgTournaments.MsgArena.Match ArenaWatchingGroup;
        public Game.MsgTournaments.MsgTeamArena.Match TeamArenaWatchingGroup;
        public Game.MsgTournaments.MsgTeamEliteGroup.Match TeamElitePkWatchingGroup;
        public Game.MsgTournaments.MsgEliteGroup.FighterStats ElitePKStats;
        Game.MsgTournaments.MsgEliteGroup.Match _tet;
        public Game.MsgTournaments.MsgEliteGroup.Match ElitePkMatch
        {

            get { return _tet; }
            set
            {
                _tet = value;
            }
        }
        public Game.MsgTournaments.MsgEliteGroup.Match ElitePkWatchingGroup;

        #region ItemsTime
        public IEnumerable<Game.MsgServer.MsgGameItem> AllMyItemsInvEquip()
        {
            foreach (var item in Inventory.ClientItems.Values)
            {
                if (item == null)
                    continue;
                if (item.Activate == 1)
                {
                    yield return item;
                }
            }
            foreach (var item in Equipment.ClientItems.Values)
            {
                if (item == null)
                    continue;
                if (item.Activate == 1)
                {
                    yield return item;
                }
            }
        }
        public IEnumerable<Game.MsgServer.MsgGameItem> AllItemsTimeWarehouse()
        {
            foreach (var Wh in Warehouse.ClientItems.Values)
            {
                foreach (var item in Wh.Values)
                {
                    if (item == null)
                        continue;
                    if (item.Activate == 1)
                    {
                        yield return item;
                    }
                }
            }
        }
        #endregion
        public bool InSkillTeamPk()
        {
            return Team != null && Team.PkMatch != null && Team.PkMatch.elitepkgroup.PKTournamentID == Game.GamePackets.SkillElitePKMatchUI && Player.InTeamPk;
        }
        public uint ArenaPoints
        {
            get
            {
                if (ArenaStatistic == null)
                    return 0;//for facke accounts
                return ArenaStatistic.Info.ArenaPoints;
            }
            set
            {
                if (ArenaStatistic != null)//for facke accounts
                    ArenaStatistic.Info.ArenaPoints = value;
            }
        }
        public uint TeamArenaPoints
        {
            get
            {
                if (TeamArenaStatistic == null)
                    return 0;//for facke accounts
                return TeamArenaStatistic.Info.ArenaPoints;
            }
            set
            {
                if (TeamArenaStatistic != null)//for facke accounts
                    TeamArenaStatistic.Info.ArenaPoints = value;
            }
        }
        public uint HonorPoints
        {
            get
            {
                if (ArenaStatistic == null)
                    return 0;//for facke accounts
                return ArenaStatistic.Info.CurrentHonor;
            }
            set
            {
                if (ArenaStatistic != null)//for facke accounts
                    ArenaStatistic.Info.CurrentHonor = value;
            }
        }
        public uint TeamArenaHonorPoints
        {
            get
            {
                if (TeamArenaStatistic == null)
                    return 0;//for facke accounts
                return TeamArenaStatistic.Info.CurrentHonor;
            }
            set
            {
                if (TeamArenaStatistic != null)//for facke accounts
                    TeamArenaStatistic.Info.CurrentHonor = value;
            }
        }
        internal bool IsWatching()
        {
            return ArenaWatchingGroup != null || TeamArenaWatchingGroup != null || ElitePkWatchingGroup != null || TeamElitePkWatchingGroup != null;
        }
        internal bool InQualifier()
        {
            return
                ArenaStatistic.ArenaState != Game.MsgTournaments.MsgArena.User.StateType.None && ArenaMatch != null
                || Team != null && Team.TeamArenaMatch != null
                || (ElitePkMatch != null)
                || (Team != null && Team.PkMatch != null);
        }
        internal bool InTeamQualifier()
        {
            return Team != null && (Team.TeamArenaMatch != null || Team.PkMatch != null);
        }
        internal void EndQualifier()
        {
            if (ArenaMatch != null)
                ArenaMatch.End(this);
            if (Team != null)
            {
                if (Team.TeamArenaMatch != null)
                {
                    if (Team.TeamLider(this))
                    {
                        if (Team.Members.Count <= 1)
                        {
                            Team.TeamArenaMatch.End(this.Team);
                            return;
                        }
                    }
                    if (Team.IsDead(700))
                        Team.TeamArenaMatch.End(Team);
                }
            }
            if (ElitePkMatch != null)
            {
                ElitePkMatch.End(this, true);
            }
            if (Team != null)
            {
                if (Team.PkMatch != null)
                {
                    if (Team.TeamLider(this))
                    {
                        if (Team.Members.Count <= 1)
                        {
                            Team.PkMatch.End(this.Team, true);
                            return;
                        }
                    }
                    if (Team.IsDead(700))
                        Team.PkMatch.End(this.Team, true);
                }
            }
        }
        internal void UpdateQualifier(GameClient client, GameClient target, uint damage)
        {
            if (client.Player.Map == 700)
            {
                if (ArenaMatch != null)
                {
                    client.ArenaStatistic.Damage += damage;
                    ArenaMatch.SendScore();
                }
                if (Team != null)
                {
                    if (Team.TeamArenaMatch != null)
                    {
                        Team.Damage += damage;
                        Team.TeamArenaMatch.SendScore();
                    }
                    if (Team.PkMatch != null)
                    {
                        Team.PKStats.Points += damage;
                        Team.PkMatch.UpdateScore();
                    }
                }
                if (ElitePKStats != null && ElitePkMatch != null)
                {
                    ElitePKStats.Points += damage;
                    ElitePkMatch.UpdateScore();
                }
            }
        }
        public bool IsInSpellRange(uint UID, byte range)
        {
            if (range == 0)
                range = 10;
            Role.IMapObj target;
            if (Player.View.TryGetValue(UID, out target, Role.MapObjectType.Monster))
            {
                return Role.Core.GetDistance(Player.X, Player.Y, target.X, target.Y) <= range;
            }
            else if (Player.View.TryGetValue(UID, out target, Role.MapObjectType.Player))
            {
                return Role.Core.GetDistance(Player.X, Player.Y, target.X, target.Y) <= range;
            }
            else if (Player.View.TryGetValue(UID, out target, Role.MapObjectType.SobNpc))
            {
                return Role.Core.GetDistance(Player.X, Player.Y, target.X, target.Y) <= range;
            }
            return false;
        }
        internal void LoseDeadExperience(Client.GameClient killer)
        {
            if (Fake)
                return;

            if (Player.Level >= Game.Era1.Era1Progression.MaxLevel)
                return;

            if (Player.ExpProtection > 0)
                return;

            var nextlevel = Pool.LevelInfo[Database.DBLevExp.Sort.User][(byte)(Player.Level)];
            if (nextlevel.Experience == 0)
            {
                return;//Era 1 max level. Error divide by 0;
            }
            ulong loseexp = (ulong)((Player.Experience * (uint)(nextlevel.UpLevTime * nextlevel.MentorUpLevTime)) / nextlevel.Experience);
            double LoseExpPercent = (double)((double)loseexp / (double)nextlevel.Experience);

            if (Player.Experience > loseexp)
            {
                Player.Experience -= loseexp;//exp;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Player.SendUpdate(stream, (long)Player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience);
                }
            }

            // to do : increase kill experince
            if (killer.Player.Level < Player.Level)
            {
                var killernextlevel = Pool.LevelInfo[Database.DBLevExp.Sort.User][(byte)(killer.Player.Level)];
                if (killernextlevel.Experience == 0)
                {
                    return;//Era 1 max level. Error divide by 0;
                }
                double GetExp = (double)((double)100 / (double)killernextlevel.Experience) * (double)(loseexp * 100);
                killer.Player.Experience += (uint)GetExp;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    killer.Player.SendUpdate(stream, (long)killer.Player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience);
                }
            }
        }
        unsafe internal bool UpdateSpellSoul(ServerSockets.Packet stream, Role.Flags.SpellID SpellID, byte MaxLevel)
        {
            Game.MsgServer.MsgSpell spell;
            if (MySpells.ClientSpells.TryGetValue((ushort)SpellID, out spell))
            {
                if (spell.SoulLevel >= MaxLevel)
                {
                    CreateBoxDialog("Sorry, you spell " + SpellID.ToString() + " is max level.");
                    return false;
                }

                ActionQuery action = new ActionQuery()
                {
                    ObjId = Player.UID,
                    dwParam = (ushort)SpellID,
                    Type = ActionType.RemoveSpell
                };
                Send(stream.ActionCreate(&action));

                spell.SoulLevel++;
                spell.UseSpellSoul = spell.SoulLevel;

                Send(stream.SpellCreate(spell));

                return true;
            }
            else
            {
                CreateBoxDialog("Sorry, you not have the spell " + SpellID.ToString() + ".");
                return false;
            }
        }
        public const int DefaultDefense2 = 10000;
        public Role.Instance.House MyHouse;
        //For anti proxy --------------
        public ushort MoveNpcMesh;
        public uint MoveNpcUID;
        public uint UseItem = 0;
        //-----------------------------
        public System.Collections.Concurrent.ConcurrentDictionary<RoleStatus.StatusType, RoleStatus> ExtraStatus;
        public DemonExterminator DemonExterminator;
        public uint RebornGem = 0;
        public Vendor MyVendor;
        public bool IsVendor
        {
            get
            {
                if (MyVendor != null)
                    return MyVendor.InVending;
                return false;
            }
        }
        public Trade MyTrade;
        public bool EditNPC { get; set; }
        public uint EditNPCID { get; set; }
        public bool ProjectManager
        {
            get
            {
                if (Program.TestServer)
                    return true;
                return Player.Name.Contains("[PM]");
            }
            set { ProjectManager = value; }
        }
        public bool GameMaster
        {
            get
            {
                return Player.Name.Contains("[GM]");
            }
        }
        public bool NormalPlayer
        {
            get
            {
                return !ProjectManager && !GameMaster;
            }
        }
        public bool InTrade
        {
            get
            {
                if (MyTrade != null)
                    return MyTrade.WindowOpen;
                return false;
            }
        }
        public bool FullLoading = false;
        public uint Vigor;
        public bool AllowUseSpellOnSteed(ushort Spell)
        {
            if (!Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Ride))
                return true;
            if (Equipment.RidingCrop != 0)
                return true;//all spells
            else if (Spell == (ushort)Role.Flags.SpellID.Spook || Spell == (ushort)Role.Flags.SpellID.WarCry
                || Spell == (ushort)Role.Flags.SpellID.Riding)
                return true;
            return false;
        }
        public ulong GainExperience(double Experience, ushort targetlevel)
        {
            var deltaLevel = Player.Level - targetlevel;
            if (deltaLevel >= 3)//green
            {
                if (deltaLevel >= 3 && deltaLevel <= 5)
                    Experience *= .7;
                else if (deltaLevel > 5 && deltaLevel <= 10)
                    Experience *= .2;
                else if (deltaLevel > 10 && deltaLevel <= 20)
                    Experience *= .1;
                else if (deltaLevel > 20)
                    Experience *= .05;
            }
            else if (deltaLevel < -15)
                Experience *= 1.8;
            else if (deltaLevel < -8)
                Experience *= 1.5;
            else if (deltaLevel < -5)
                Experience *= 1.3;

            return (ulong)Experience;
        }
        public void IncreaseExperience(ServerSockets.Packet stream, double Experience, Role.Flags.ExperienceEffect effect = Role.Flags.ExperienceEffect.None)
        {
            if (Player.CursedTimer > 2)
            {
                return;
            }
            if (Player.Level < Game.Era1.Era1Progression.MaxLevel)
            {
                if (effect != Role.Flags.ExperienceEffect.None)
                {
                    Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.Effect, true, new string[1] { effect.ToString() });

                }
                Experience = CalculateFinalExperience(Experience);
                Player.Experience += (ulong)Experience;
                while (Player.Experience >= Pool.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level].Experience)
                {
                    Player.Experience -= Pool.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level].Experience;
                    ushort newlev = (ushort)(Player.Level + 1);
                    UpdateLevel(stream, newlev);
                    if (Player.Level >= Game.Era1.Era1Progression.MaxLevel)
                    {
                        Player.Experience = 0;
                        break;
                    }
                }
                UpdateRebornLastLevel(stream);
                Player.SendUpdate(stream, (long)Player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience, false);
            }
        }
        public ulong CalculateFinalExperience(double experience)
        {
            if (Player.CursedTimer > 2 || Player.Level >= Game.Era1.Era1Progression.MaxLevel)
                return 0;

            experience *= ServerConfig.UserExpRate;
            experience += experience * GemValues(Role.Flags.Gem.NormalRainbowGem) / 100;
            if (Player.DExpTime > 0)
                experience *= Player.RateExp;
            return (ulong)Math.Max(0, experience);
        }

        public void IncreaseExperienceRaw(ServerSockets.Packet stream, ulong experience)
        {
            if (experience == 0 || Player.Level >= Game.Era1.Era1Progression.MaxLevel)
                return;

            Player.Experience += experience;
            while (Player.Level < Game.Era1.Era1Progression.MaxLevel &&
                   Player.Experience >= Pool.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level].Experience)
            {
                Player.Experience -= Pool.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level].Experience;
                UpdateLevel(stream, (ushort)(Player.Level + 1));
                if (Player.Level >= Game.Era1.Era1Progression.MaxLevel)
                {
                    Player.Experience = 0;
                    break;
                }
            }
            UpdateRebornLastLevel(stream);
            Player.SendUpdate(stream, (long)Player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience, false);
        }

        public void UpdateRebornLastLevel(ServerSockets.Packet stream)
        {
            if (Player.Reborn > 0)
            {
                if (Player.Reincarnation)
                {
                    if (Player.Level >= 110 && Player.Level < Player.SecoundeRebornLevel)
                    {
                        UpdateLevel(stream, Player.SecoundeRebornLevel, true);
                    }
                }
                else
                {
                    if (Player.Reborn == 1)
                    {
                        if (Player.Level >= 130 && Player.Level < Player.FirstRebornLevel)
                        {
                            UpdateLevel(stream, Player.FirstRebornLevel, true);
                        }
                    }
                    else if (Player.Reborn == 2)
                    {
                        if (Player.Level >= 130 && Player.Level < Player.SecoundeRebornLevel)
                            UpdateLevel(stream, Player.SecoundeRebornLevel, true);
                    }
                }
            }
        }
        public unsafe void DisconectStopFunctions()
        {
            try
            {
                if (AutoHunting != null && AutoHunting.PendingExperience > 0)
                    Catching.FlushPendingExperience(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            try
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    // Remove entity in map
                    if (this.Map != null)
                    {
                        this.Map.RemoveEntity(this);
                        this.Map.Denquer(this);
                    }
                    // Send Action Remove Object in map
                    var actionVending = new ActionQuery()
                    {
                        ObjId = this.Player.UID,
                        Type = ActionType.StopVending,
                        dwParam = this.Player.Map,
                        wParam1 = this.Player.X,
                        dwParam2 = this.Player.Y,
                        dwParam3 = this.Player.Map
                    };
                    this.Send(stream.ActionCreate(&actionVending));
                    // Remove vendor boths in map
                    if (this.MyVendor != null)
                        this.MyVendor.StopVending(stream);
                    if (this.MyTrade != null)
                        this.MyTrade.CloseTrade();
                    if (this.Player.Associate != null)
                        this.Player.Associate.Online = false;
                    // Remove mentor bp
                    if (this.Player.MyMentor != null)
                        this.Player.MyMentor.Online = false;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                //important action
                Database.ServerDatabase.SaveClient(this);
            }
        }
        public string InfoLevelUpdate(double amount = 600)
        {
            if (Player.Level >= Game.Era1.Era1Progression.MaxLevel)
                return Game.Era1.Era1Progression.MaxLevel + " (MAX)";

            ulong ReceiveExperience = GainExpBall(amount, false, Role.Flags.ExperienceEffect.None, true);
            ulong MyExperince = Player.Experience;
            byte MyLevel = (byte)Player.Level;
            MyExperince += ReceiveExperience;
            while (MyLevel < Game.Era1.Era1Progression.MaxLevel
                && MyExperince >= Pool.LevelInfo[Database.DBLevExp.Sort.User][(byte)MyLevel].Experience)
            {
                MyExperince -= Pool.LevelInfo[Database.DBLevExp.Sort.User][(byte)MyLevel].Experience;
                MyLevel++;
            }
            if (MyLevel >= Game.Era1.Era1Progression.MaxLevel)
                return Game.Era1.Era1Progression.MaxLevel + " (MAX)";
            float Percentaj = (float)(Pool.LevelInfo[Database.DBLevExp.Sort.User][(byte)MyLevel].Experience / MyExperince);
            return "" + MyLevel + " (" + Percentaj + "%)";
        }
        public ulong GainExpBall(double amount = 600, bool sendMsg = false, Role.Flags.ExperienceEffect effect = Role.Flags.ExperienceEffect.None
            , bool JustCalculate = false, bool mentorexp = true)
        {
            if (Player.Level >= Game.Era1.Era1Progression.MaxLevel)
                return 0;
            if (sendMsg)
            {
                SendSysMesage("You have gained experience worth " + (amount * 1.0) / 600 + " exp ball(s).", Game.MsgServer.MsgMessage.ChatMode.System);
            }
            if (effect != Role.Flags.ExperienceEffect.None)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.Effect, true, new string[1] { effect.ToString() });
                }
            }

            var LevelDBExp = Pool.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level];
            if (LevelDBExp == null)
                return 0;

            var ReceiveExp = (long)Player.Experience * LevelDBExp.UpLevTime / (double)LevelDBExp.Experience;
            ReceiveExp += amount;

            byte IncreaseLevel = (byte)Player.Level;
            //LevelDBExp = LevelInfo[Database.DBLevExp.Sort.User][IncreaseLevel];
            var times = LevelDBExp.UpLevTime;

            while (IncreaseLevel < Game.Era1.Era1Progression.MaxLevel)
            {
                if (ReceiveExp < times)
                    break;
                ReceiveExp -= times;
                IncreaseLevel++;

                LevelDBExp = Pool.LevelInfo[Database.DBLevExp.Sort.User][IncreaseLevel];
                if (LevelDBExp == null)
                    break;

                times = LevelDBExp.UpLevTime;
            }
            if (times < 1) return 0;
            if (!JustCalculate)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    UpdateLevel(stream, IncreaseLevel, false, mentorexp);
                }
            }
            ReceiveExp /= times;

            LevelDBExp = Pool.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level];
            if (LevelDBExp == null)
                return 0;

            ulong CalculateEXp = (ulong)(ReceiveExp * LevelDBExp.Experience);
            if (!JustCalculate)
            {
                Player.Experience = CalculateEXp;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Player.SendUpdate(stream, (long)Player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience, false);
                }
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    UpdateRebornLastLevel(stream);
                }
            }
            return CalculateEXp;
        }
        public ulong CalcExpBall(double amount, out ushort nextlevel)
        {
            if (Player.Level >= Game.Era1.Era1Progression.MaxLevel)
            { nextlevel = 0; return 0; }

            var LevelDBExp = Pool.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level];
            if (LevelDBExp == null)
            { nextlevel = 0; return 0; }

            var ReceiveExp = (long)Player.Experience * LevelDBExp.UpLevTime / (double)LevelDBExp.Experience;
            ReceiveExp += amount;

            byte IncreaseLevel = (byte)Player.Level;
            //LevelDBExp = LevelInfo[Database.DBLevExp.Sort.User][IncreaseLevel];
            var times = LevelDBExp.UpLevTime;

            while (IncreaseLevel < Game.Era1.Era1Progression.MaxLevel)
            {
                if (ReceiveExp < times)
                    break;
                ReceiveExp -= times;
                IncreaseLevel++;

                LevelDBExp = Pool.LevelInfo[Database.DBLevExp.Sort.User][IncreaseLevel];
                if (LevelDBExp == null)
                    break;

                times = LevelDBExp.UpLevTime;
            }

            if (times < 1) { nextlevel = IncreaseLevel; return 0; }
            ReceiveExp /= times;

            LevelDBExp = Pool.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level];
            if (LevelDBExp == null)
            { nextlevel = IncreaseLevel; return 0; }

            ulong CalculateEXp = (ulong)(ReceiveExp * LevelDBExp.Experience);

            nextlevel = IncreaseLevel;
            return CalculateEXp;
        }
        public unsafe Game.MsgServer.InteractQuery AutoAttack = default(Game.MsgServer.InteractQuery);
        private bool _OnAutoAttack = false;
        public bool OnAutoAttack
        {
            get { return _OnAutoAttack; }
            set
            {
                _OnAutoAttack = value;
                if (value)
                {
                }
            }

        }
        public uint AcceptedGuildID = 0;

        public uint ConnectionUID = 0;
        public Game.MsgServer.MsgLoginClient OnLogin = default(Game.MsgServer.MsgLoginClient);
        public uint ActiveNpc = 0;
        public ServerFlag ClientFlag = ServerFlag.None;
        public Role.Player Player;
        public Cryptography.DiffieHellman DHKey;
        public Cryptography.TQCast5 Cryptography;
        public Cryptography.DHKeyExchange.ServerKeyExchange DHKeyExchance;
        public ServerSockets.SecuritySocket Socket;
        public Role.GameMap Map = null;
        public Role.Instance.Team Team = null;
        public ushort[] Gems = new ushort[13];
        public void AddProf()
        {
            if (MyProfs == null) 
                return;

            Game.MsgServer.MsgProficiency prof;

            uint ProfRightWeapon = Equipment.RightWeapon / 1000;
            uint PorfLeftWeapon = Equipment.LeftWeapon / 1000;

            if (ProfRightWeapon != PorfLeftWeapon)
            {
                if (ProfRightWeapon != 0)
                {
                    if (MyProfs.ClientProf.TryGetValue(ProfRightWeapon, out prof))
                    {
                        CalcProfsWeap(prof.Level);
                    }
                }
                if (PorfLeftWeapon != 0)
                {
                    if (MyProfs.ClientProf.TryGetValue(PorfLeftWeapon, out prof))
                    {
                        CalcProfsWeap(prof.Level);
                    }
                }
            }
            else
            {
                if (ProfRightWeapon != 0)
                {
                    if (MyProfs.ClientProf.TryGetValue(ProfRightWeapon, out prof))
                    {
                        CalcProfsWeap(prof.Level);
                    }
                }
            }
        }
        public void CalcProfsWeap(uint lev)
        {
            if (lev > 12)
            {
                uint calc = (uint)(lev - 12);
                if (calc <= 8)
                    Status.PhysicalPercent += calc;
            }
        }
        public void AddGem(Role.Flags.Gem gem, ushort value)
        {
            if (value == 15 || value == 10 || value == 5)
            {
                if (gem == Role.Flags.Gem.NormalDragonGem || gem == Role.Flags.Gem.RefinedDragonGem || gem == Role.Flags.Gem.SuperDragonGem)
                    Status.PhysicalPercent += value;
                else
                {
                    if (gem == Role.Flags.Gem.NormalKylinGem || gem == Role.Flags.Gem.RefinedKylinGem || gem == Role.Flags.Gem.SuperKylinGem)
                    {
                        ReduceDurabilityPercent = value;
                    } else
                    {
                        Status.MagicPercent += value;
                    }
                }
            }
            Gems[(byte)((byte)gem / 10)] += value;
        }
        public uint GemValues(Role.Flags.Gem gem)
        {
            return Gems[(byte)((byte)gem / 10)];
        }
        public uint AjustDefense
        {
            get
            {
                uint defence = (uint)(Status.Defence);
                uint nDefence = 0;
                if (Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Shield) || Player.OnDefensePotion)
                {
                    nDefence += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)defence, 120, 100) - defence;////(uint)(defence * 1.3);// + 30% dmg
                }
                return defence + nDefence;
            }
        }
        public uint AjustAttack(uint Damage)
        {
            uint nAttack = 0;

            if (Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Stigma) || Player.OnAttackPotion)
            {
                nAttack += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, 130, 100) - Damage;
            }
            if (Status.PhysicalPercent > 0)
            {
                nAttack += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, (int)Status.PhysicalPercent, 100);// -Damage / 2;
                //(uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, (int)Status.PhysicalPercent, 100);
            }
            if (Player.Intensify)
            {
                Player.Intensify = false;//IntensifyDamage
                nAttack += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, Player.IntensifyDamage, 100) - Damage;
            }
            return Damage + nAttack;
        }
        public int GetDefense2()
        {
            return Player.Reborn >= 2 ? 5000 : DefaultDefense2;
        }
        public uint AjustCriticalStrike()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatusType.IncreasePStrike, out Power))
            {
                if (Power)
                    return Status.CriticalStrike + Power;
            }
            return Status.CriticalStrike;
        }
        public uint AjustMCriticalStrike()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatusType.IncreaseMStrike, out Power))
            {
                if (Power)
                    return Status.SkillCStrike + Power;
            }
            return Status.SkillCStrike;
        }
        public uint AjustImunity()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatusType.IncreaseImunity, out Power))
            {
                if (Power)
                    return Status.Immunity + Power;
            }
            return Status.Immunity;
        }
        public uint AjustBreakthrough()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatusType.IncreaseBreack, out Power))
            {
                if (Power)
                    return Status.Breakthrough + Power;
            }
            return Status.Breakthrough;
        }
        public uint AjustAntiBreack()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatusType.IncreaseAntiBreack, out Power))
            {
                if (Power)
                    return Status.Counteraction + Power;
            }
            return Status.Counteraction;
        }
        public uint AjustMagicDamageIncrease()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatusType.IncreaseFinalMAttack, out Power))
            {
                if (Power)
                    return Status.MagicDamageIncrease + Power;
            }
            return Status.MagicDamageIncrease;
        }
        public uint AjustMagicDamageDecrease()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatusType.IncreaseFinalMDamage, out Power))
            {
                if (Power)
                    return Status.MagicDamageDecrease + Power;
            }
            return Status.MagicDamageDecrease;
        }
        public uint AjustPhysicalDamageIncrease()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatusType.IncreaseFinalPAttack, out Power))
            {
                if (Power)
                    return Status.PhysicalDamageIncrease + Power;
            }
            return Status.PhysicalDamageIncrease;
        }
        public uint AjustPhysicalDamageDecrease()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatusType.IncreaseFinalPDamage, out Power))
            {
                if (Power)
                    return Status.PhysicalDamageDecrease + Power;
            }
            return Status.PhysicalDamageDecrease;
        }
        public uint AjustMagicAttack()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatusType.IncreaseMAttack, out Power))
            {
                if (Power)
                    return Status.MagicAttack + Power;
            }
            return (uint)(Status.MagicAttack);
        }
        public uint AjustMaxHitpoints()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatusType.IncreaseMaxHp, out Power))
            {
                if (Power)
                    return Status.MaxHitpoints + Power;
            }
            return Status.MaxHitpoints;
        }
        public uint AjustMaxAttack(uint damage)
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatusType.IncreasePAttack, out Power))
            {
                if (Power)
                    return damage + Power;
            }
            return (uint)(damage);
        }
        public void ClanShareBP()
        {
            if (Team != null)
            {
                if (Team.TeamLider(this))
                {
                    foreach (var memeber in Team.GetMembers())
                        Team.GetClanShareBp(memeber);
                }
                else
                {
                    Team.GetClanShareBp(this);
                }
            }
        }
        public void Shift(ushort X, ushort Y, ServerSockets.Packet stream, bool SendData = true)
        {
            Player.Px = Player.X;
            Player.Py = Player.Y;

            if (SendData)
            {

                ActionQuery action = new ActionQuery()
                {
                    ObjId = Player.UID,
                    Type = ActionType.FlashStep,
                    wParam1 = X,
                    wParam2 = Y
                };
                Player.View.SendView(stream.ActionCreate(&action), true);

                Map.View.MoveTo<Role.IMapObj>(Player, X, Y);
                Player.X = X;
                Player.Y = Y;
                Player.View.Role(false, stream);
            }
            else
            {
                Map.View.MoveTo<Role.IMapObj>(Player, X, Y);
                Player.X = X;
                Player.Y = Y;
                Player.View.Role(false, null);
            }
        }
        public Game.MsgServer.MsgStatus Status = new MsgStatus();
        public Role.Instance.Warehouse Warehouse;
        public Role.Instance.Equip Equipment;
        public Role.Instance.Inventory Inventory;
        public Role.Instance.Proficiency MyProfs;
        public Role.Instance.Spell MySpells;
        public Role.Instance.Confiscator Confiscator;
        public bool OnInterServer;
        public GameClient(ServerSockets.SecuritySocket _socket, bool _OnInterServer = false)
        {
            OnInterServer = _OnInterServer;
            ExtraStatus = new System.Collections.Concurrent.ConcurrentDictionary<Role.Instance.RoleStatus.StatusType, Role.Instance.RoleStatus>();
            ArenaStatistic = new Game.MsgTournaments.MsgArena.User();
            DemonExterminator = new Role.Instance.DemonExterminator();
            Confiscator = new Role.Instance.Confiscator();
            AutoHunting = new AutoHunting();

            ClientFlag |= ServerFlag.None;
            TimerSyncRoot = new object();
            if (_socket != null)
            {
                Socket = _socket;
                if (OnInterServer == false)
                {

                    Socket.Client = this;
                    Socket.Game = this;
                    DHKey = new Cryptography.DiffieHellman(DHKeyExchange.KeyExchange.Str_P, DHKeyExchange.KeyExchange.Str_G);
                    Crypto = new Cryptography.TQCast5();
                    Crypto.GenerateKey(Encoding.UTF8.GetBytes(Pool.LogginKey));
                    Socket.SetCrypto(Crypto);
                }
            }
            Player = new Role.Player(this);
            if (OnInterServer == false)
            {
                DHKeyExchance = new Cryptography.DHKeyExchange.ServerKeyExchange();
                if (_socket != null)
                {
                    Send(DHKeyExchance.CreateServerKeyPacket(DHKey));
                }
            }
        }
        public unsafe void Send(ServerSockets.Packet msg)
        {
            try
            {
                if (Fake || Socket.Alive == false)
                    return;

                Socket.Send(msg);

            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }
        }
        public unsafe void Send(byte[] buffer)
        {
            try
            {
                if (Fake || Socket.Alive == false)
                    return;

                ushort length = BitConverter.ToUInt16(buffer, 0);
                if (length == 0)
                {
                    Poker.Packets.Packet.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                }
                Poker.Packets.Packet.WriteString("TQServer", buffer.Length - 8, buffer);

                ServerSockets.Packet stream = new ServerSockets.Packet(buffer);
                Socket.Send(stream);

            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }
        }
        public bool Fake = false;
        public void SendSysMesage(string Messaj, Game.MsgServer.MsgMessage.ChatMode ChatType = Game.MsgServer.MsgMessage.ChatMode.TopLeftSystem
            , Game.MsgServer.MsgMessage.MsgColor color = Game.MsgServer.MsgMessage.MsgColor.red, bool SendScren = false, bool self = false)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                if (!self)
                {
                    if (SendScren)
                        Player.View.SendView(new Game.MsgServer.MsgMessage(Messaj, color, ChatType).GetArray(stream), true);
                    else
                        Send(new Game.MsgServer.MsgMessage(Messaj, color, ChatType).GetArray(stream));
                }
                else
                {
                    if (SendScren)
                        Player.View.SendView(new Game.MsgServer.MsgMessage(Messaj, Player.Name, color, ChatType).GetArray(stream), true);
                    else
                        Send(new Game.MsgServer.MsgMessage(Messaj, Player.Name, color, ChatType).GetArray(stream));
                }
            }
        }
        public void SendWhisper(string Messaj, string from, string to)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                var X = new Game.MsgServer.MsgMessage(Messaj, to, from, MsgMessage.MsgColor.red, MsgMessage.ChatMode.Whisper);
                X.Mesh = 1531003;
                X.Color = 4294967295;
                X.MessageUID1 = 550;
                var x2 = X.GetArray(stream);
                Send(x2);
            }
        }
        public void SendScreen(byte[] msg, bool self = true)
        {
            Player.View.SendView(msg, self);
        }
        public void CreateDialog(ServerSockets.Packet stream, string Text, string OptionText)
        {
            Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(this, stream);
            dialog.AddText(Text);
            if (OptionText != "")
                dialog.AddOption(OptionText, 255);
            dialog.FinalizeDialog();
        }
        public void CreateBoxDialog(string Text)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(this, stream);
                dialog.CreateMessageBox(Text).FinalizeDialog(true);
            }
        }
        public IEnumerable<Game.MsgServer.MsgGameItem> GetAllMainItems()
        {
            foreach (var item in Inventory.ClientItems.Values)
                if (!item.Fake)
                    yield return item;
            foreach (var item in Equipment.ClientItems.Values)
                if (!item.Fake)
                    yield return item;
        }
        public IEnumerable<Game.MsgServer.MsgGameItem> AllMyItems()
        {
            foreach (var item in Inventory.ClientItems.Values)
                if (!item.Fake)
                    yield return item;
            
            foreach (var item in Equipment.ClientItems.Values)
                if (!item.Fake)
                    yield return item;

            foreach (var Wh in Warehouse.ClientItems.Values)
            {
                foreach (var item in Wh.Values)
                    if (!item.Fake)
                        yield return item;
            }

        }
        public int GetItemsCount()
        {
            int count = 0;
            int FakeCount = 0;
            count += Inventory.ClientItems.Count;
            count += Equipment.ClientItems.Count;
            foreach (var Wh in Warehouse.ClientItems.Values)
            {
                count += Wh.Count;
            }

            foreach (var item in Equipment.ClientItems.Values)
                if (item.Fake)
                    FakeCount++;
            foreach (var item in Inventory.ClientItems.Values)
                if (item.Fake)
                    FakeCount++;

            return count - FakeCount;
        }
        public bool TryGetItem(uint UID, out Game.MsgServer.MsgGameItem item)
        {
            if (Equipment.TryGetValue(UID, out item))
                return true;
            if (Inventory.TryGetItem(UID, out item))
                return true;

            item = null; return false;
        }
        public ushort CalculateHitPoint()
        {
            ushort valor = 0;
            switch (Player.Class)
            {
                case 11:
                    valor += (ushort)(Player.Agility * 3.15 + Player.Spirit * 3.15 + Player.Strength * 3.15 + Player.Vitality * 25.2);
                    break;
                case 12:
                    valor += (ushort)(Player.Agility * 3.24 + Player.Spirit * 3.24 + Player.Strength * 3.24 + Player.Vitality * 25.9);
                    break;
                case 13:
                    valor += (ushort)(Player.Agility * 3.30 + Player.Spirit * 3.30 + Player.Strength * 3.30 + Player.Vitality * 26.4);
                    break;
                case 14:
                    valor += (ushort)(Player.Agility * 3.36 + Player.Spirit * 3.36 + Player.Strength * 3.36 + Player.Vitality * 26.8);
                    break;
                case 15:
                    valor += (ushort)(Player.Agility * 3.45 + Player.Spirit * 3.45 + Player.Strength * 3.45 + Player.Vitality * 27.6);
                    break;
                default:
                    valor += (ushort)(Player.Agility * 3 + Player.Spirit * 3 + Player.Strength * 3 + Player.Vitality * 24);
                    break;
            }
            return valor;


        }
        public ushort CalculateMana()
        {
            ushort valor = 0;
            switch (Player.Class)
            {
                case 142:
                case 132: valor += (ushort)(Player.Spirit * 15); break;
                case 143:
                case 133: valor += (ushort)(Player.Spirit * 20); break;
                case 144:
                case 134: valor += (ushort)(Player.Spirit * 25); break;
                case 145:
                case 135: valor += (ushort)(Player.Spirit * 30); break;
                default: valor += (ushort)(Player.Spirit * 5); break;
            }
            return valor;
        }
        public void Pullback()
        {
            Teleport(Player.X, Player.Y, Player.Map, Player.DynamicID, false,false);
        }
        public void TeleportCallBack()
        {
            Teleport(Player.PMapX, Player.PMapY, Player.PMap, Player.PDinamycID);
        }
        public void Teleport(ushort x, ushort y, uint MapID, uint DynamicID = 0, bool revive = true, bool CanTeleport = false)
        {

            if (Player.Name == DragonWar.LastWinner)
            {
                if (!Player.DragonKing)
                    Player.DragonKing = true;
            }
            else if (Player.DragonKing)
                Player.DragonKing = false;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var msg = rec.GetStream();
                Player.SendString(msg, MsgStringPacket.StringID.Effect, true, "moveback");
            }
            if (MapID == 1036 && Player.TransformInfo != null)
                Player.TransformInfo.FinishTransform();
            if (MapID == 1036 && Player.ContainFlag(MsgUpdate.Flags.Cyclone))
                Player.RemoveFlag(MsgUpdate.Flags.Cyclone);
            if (MapID == 700 || MapID == 1005 || MapID == 1036 || MapID == 2071 || MapID == 1764)
            {
                if (Player.ContainFlag(MsgUpdate.Flags.Ride))
                    Player.RemoveFlag(MsgUpdate.Flags.Ride);
            }
            if (!ProjectManager)
            {
                if (Player.Map == 6001 && CanTeleport == false)
                    return;
            }
            if (Player.Map == 1038 && Player.Alive == false && CanTeleport == false)
                return;
            //if (EventBase != null)
            //{
            //    var events = Pool.Events.Find(e => e.EventTitle == EventBase.EventTitle);
            //    if (events != null)
            //    {
            //        if(!events.InTournament(this,true, MapID, DinamycID))
            //        {
            //            events.RemovePlayer(this, false, false);
            //        }
            //    }
            //}
                if (Mining)
                StopMining();
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                if (Player.SetLocationType != 1)
                {
                    if (!Player.OnMyOwnServer && MapID != 1002 && MapID != 3935 && !InQualifier() && !IsWatching())
                        return;
                }
                if (MapID == 1011)
                {
                    //375, 48
                    if (x == 375 && y == 48)
                    {
                        if (this.Player.QuestGUI.CheckQuest(1352, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                            Player.QuestGUI.IncreaseQuestObjectives(stream, 1352, 1);
                    }
                }
                if (Pool.MapCounterHits.Contains(Player.Map) || Player.Map == 1038 || Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(this))
                {
                    if (MapID != Player.Map)
                    {
                        SendSysMesage("", MsgMessage.ChatMode.FirstRightCorner);
                    }
                }

                if (Socket != null)//!= null for facke accounts.
                {
                    if (Socket.Alive == false)
                        return;
                }


                if (IsWatching() && Player.Map == 700)
                {
                    if (ArenaWatchingGroup != null)
                        ArenaWatchingGroup.DoLeaveWatching(this);
                    else if (TeamArenaWatchingGroup != null)
                        TeamArenaWatchingGroup.DoLeaveWatching(this);
                    else if (ElitePkWatchingGroup != null)
                        ElitePkWatchingGroup.DoLeaveWatching(this);

                }

#if TEST
                if (ProjectManager)
                    Console.WriteLine("Name= " + Player.Name + " Tele to map = " + MapID + " X = " + x.ToString() + " Y = " + y.ToString() + "");
#endif
                if (IsVendor)
                    MyVendor.StopVending(stream);
                if (InTrade)
                    MyTrade.CloseTrade();

                if (MapID == 601 || MapID == 1039)
                {
                    if (Player.HeavenBlessing > 0)
                    {
                        Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.InTraining, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);
                    }
                }
                if (Player.Map == 601 || Player.Map == 1039)
                {
                    if (MapID != 601 && MapID != 1039)
                        Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.Review, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);
                }
                if (!Role.GameMap.CheckMap(MapID))
                {
                    MapID = 1002;
                    x = 429;
                    y = 378;
                }
                Role.GameMap GameMap;
                Role.GameMap.EnterMap((int)MapID);
                if (Pool.ServerMaps.TryGetValue(MapID, out GameMap))
                {
                    OnAutoAttack = false;
                    Player.RemoveBuffersMovements(stream);

                    Player.View.Clear(stream);


                    if (GameMap.BaseID != 0)
                    {
                        ActionQuery daction = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.Teleport,
                            dwParam = GameMap.BaseID,
                            wParam1 = x,
                            wParam2 = y,
                            dwParam3 = GameMap.BaseID
                        };
                        Send(stream.ActionCreate(&daction));

                    }
                    else
                    {
                        ActionQuery aaction = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.Teleport,
                            dwParam = MapID,
                            wParam1 = x,
                            wParam2 = y,
                            dwParam3 = MapID
                        };
                        Send(stream.ActionCreate(&aaction));
                    }
                    if (Player.Map != 700)
                    {
                        var aaaction = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = (ActionType)157,
                            dwParam = 2,
                            wParam1 = x,
                            wParam2 = y,
                            dwParam3 = MapID
                        };
                        Send(stream.ActionCreate(&aaaction));
                    }

                    var action = new ActionQuery()
                    {
                        ObjId = Player.UID,
                        Type = ActionType.StopVending,
                        dwParam = MapID,
                        wParam1 = x,
                        wParam2 = y,
                        dwParam3 = MapID
                    };
                    Send(stream.ActionCreate(&action));

                    if (MapID == 1780 && GameMap.BaseID == 0)
                    {
                        action = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.SetMapColor,
                            dwParam = 0x323232,
                            wParam1 = x,
                            wParam2 = y
                        };
                        Send(stream.ActionCreate(&action));

                    }
                    else if (MapID == 3846)
                    {
                        action = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.SetMapColor,
                            dwParam = 16755370,
                            wParam1 = x,
                            wParam2 = y
                        };
                        Send(stream.ActionCreate(&action));

                    }
                    else if (MapID == 10088 || MapID == 44455 || MapID == 44456)
                    {
                        action = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.SetMapColor,
                            dwParam = 14535867,
                            wParam1 = x,
                            wParam2 = y
                        };
                        Send(stream.ActionCreate(&action));
                    }
                    else
                    {

                        if (GameMap.ID == 3830 || GameMap.ID == 3831 || GameMap.ID == 3832)
                        {
                            action = new ActionQuery()
                            {
                                ObjId = Player.UID,
                                Type = ActionType.SetMapColor,
                                dwParam = GameMap.MapColor,
                                wParam1 = x,
                                wParam2 = y

                            };
                            Send(stream.ActionCreate(&action));
                        }
                        else
                        {
                            action = new ActionQuery()
                            {
                                ObjId = Player.UID,
                                Type = ActionType.SetMapColor,
                                dwParam = 0,
                                wParam1 = x,
                                wParam2 = y

                            };
                            Send(stream.ActionCreate(&action));
                        }
                    }
                    if (MapID == 1002)
                    {
                        Player.DynamicID = 0;
                    }
                    if (MapID == Player.Map && Player.DynamicID == DynamicID)
                    {
                        Map.Denquer(this);
                        // Map.View.MoveTo<Role.IMapObj>(Player, x, y);
                        Player.X = x;
                        Player.Y = y;
                        Pool.ServerMaps[MapID].Enquer(this);
                    }
                    else
                    {
                        Player.PDinamycID = Player.DynamicID;
                        Player.PMapX = Player.X;
                        Player.PMapY = Player.Y;

                        Map.Denquer(this);

                        Player.DynamicID = DynamicID;
                        Player.X = x;
                        Player.Y = y;
                        Player.PMap = Player.Map;

                        Player.Map = MapID;


                        Pool.ServerMaps[MapID].Enquer(this);
                    }

                    if (Player.Map == 700)
                    {
                        if (InTeamQualifier())
                            Send(stream.MapStatusCreate(Map.ID, Map.ID, 19568946643047));
                        else if (ElitePkWatchingGroup != null || ElitePkMatch != null)
                            Send(stream.MapStatusCreate(Map.ID, Map.ID, 18173880847630407));
                        else
                            Send(stream.MapStatusCreate(Map.ID, Map.ID, (uint)Map.TypeStatus));
                    }
                    else if (GameMap.BaseID != 0)
                        Send(stream.MapStatusCreate(Map.BaseID, Map.BaseID, (uint)Map.TypeStatus));
                    else
                    {
                        if (Player.Map == 3935)
                            Send(stream.MapStatusCreate(Map.ID, Map.ID, 846641133264903));
                        else
                            Send(stream.MapStatusCreate(Map.ID, Map.ID, (uint)Map.TypeStatus));
                    }

                    Player.View.Role(true);

                    if (!Player.Alive && revive && Player.Map != 1038)
                    {
                        Player.Revive(stream);
                    }
                    if (Player.ObjInteraction != null)
                    {
                        if (Role.Core.IsBoy(Player.Body))
                        {
                            Player.ObjInteraction.Teleport(x, y, MapID, DynamicID);
                        }
                    }
                    if (Player.Map == 1038 || Player.Map == 3868 || Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(this))
                        if (Player.ContainFlag(MsgUpdate.Flags.Ride))
                            Player.RemoveFlag(MsgUpdate.Flags.Ride);
                    if (Player.ContainFlag(MsgUpdate.Flags.Cyclone))
                        Player.RemoveFlag(MsgUpdate.Flags.Cyclone);
                    if (Player.ContainFlag(MsgUpdate.Flags.Superman))
                        Player.RemoveFlag(MsgUpdate.Flags.Superman);

                    if (Player.Map == 1038 || Player.Map == 3868)
                    {
                        if (Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                            Player.RemoveFlag(MsgUpdate.Flags.FatalStrike);
                    }
                    Player.UpdateSurroundings(stream, true);
                    // Send(stream.WeatherCreate(MsgWeather.WeatherType.Snow, 1000, 3, 0, 0));
                }

            }
            if (Player.Map == 6000 && !Map.ValidLocation(x, y))
                Teleport(32, 73, 6000);
            //ClanShareBP();
            Player.ProtectAttack(2000);
        }
        public void UpdateLevel(ServerSockets.Packet stream, ushort Level, bool REsetExp = false, bool mentorexp = true)
        {
            Level = Game.Era1.Era1Progression.ClampLevel(Level);

            if (Level == Player.Level)
                return;
            if (Player.MyGuildMember != null)
            {
                Player.MyGuildMember.Level = Level;
            }
            if (REsetExp)
                Player.Experience = 0;
            uint OldLevel = Player.Level;
            Player.Level = Level;


            Player.SendUpdate(stream, Player.Level, Game.MsgServer.MsgUpdate.DataType.Level);
            ActionQuery action = new ActionQuery()
            {
                Type = ActionType.Leveled,
                ObjId = Player.UID,
                wParam1 = Level
            };
            Player.View.SendView(stream.ActionCreate(&action), true);


            if (Player.Reborn == 0 && (
                Database.AtributesStatus.IsWater(Player.Class)
                ? (Level < 111 || OldLevel < 110 && Level > 110)
                : (Level < 121 || OldLevel < 120 && Level > 120)))
            {
                Database.DataCore.AtributeStatus.GetStatus(Player);
                Player.SendUpdate(stream, Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                Player.SendUpdate(stream, Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                Player.SendUpdate(stream, Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                Player.SendUpdate(stream, Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                Player.SendUpdate(stream, Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

            }
            else
            {
                if (OldLevel < Level)
                {
                    ushort artibute = (ushort)((Level - OldLevel) * 3);
                    Player.Atributes += artibute;
                    Player.SendUpdate(stream, Player.Atributes, MsgUpdate.DataType.Atributes);
                }
            }
            Pool.RebornInfo.Reborn(this.Player, 0, stream);
            if (Player.MyMentor != null && mentorexp)
            {
                var LevelUp = Pool.LevelInfo[Database.DBLevExp.Sort.User][(byte)OldLevel];
                Player.MyMentor.Mentor_ExpBalls += (uint)LevelUp.MentorUpLevTime;
                AssociateGS.Member mee;
                if (Player.MyMentor.Associat.ContainsKey(AssociateGS.Apprentice))
                {
                    if (Player.MyMentor.Associat[AssociateGS.Apprentice].TryGetValue(Player.UID, out mee))
                    {

                        mee.ExpBalls += (uint)LevelUp.MentorUpLevTime;
                    }
                }
            }
            Equipment.QueryEquipment(Equipment.Alternante, false);
            Player.HitPoints = (int)Status.MaxHitpoints;
            Player.Mana = (ushort)Status.MaxMana;
            if (Player.Level <= 70 && Team != null)
            {
                var teamleader = Team.Leader;
                if (teamleader.Player.UID != Player.UID)
                {
                    if (Role.Core.GetDistance(teamleader.Player.X, teamleader.Player.Y, Player.X, Player.Y) < Role.RoleView.ViewThreshold)
                    {
                        if (teamleader.Player.Map != Player.Map)
                            return;
                        if (!teamleader.Player.Alive || teamleader.Player.Level < 70)
                            return;

                        teamleader.Player.VirtutePoints += (uint)(Player.Level * 10);
                        Team.SendTeam(new MsgMessage("Congratulations to leader, he have earned " + (Player.Level * 10).ToString() + " VirtuePoints by leveling up newbies!", MsgMessage.MsgColor.white, MsgMessage.ChatMode.Team).GetArray(stream), 0);

                    }
                }
            }
            UpdateRebornLastLevel(stream);
        }
        public bool RaceGuard { get { return Player.ContainFlag(MsgUpdate.Flags.GodlyShield); } }
        public bool RaceDecelerated
        {
            get
            {
                return Player.ContainFlag(MsgUpdate.Flags.Deceleration);
            }
        }
        public bool RaceExcitement { get { return Player.ContainFlag(MsgUpdate.Flags.Accelerated); } }
        public int MaxChains = 0;
        public int Arena2;
        public uint EntityID { get { return Player.UID; } }
        public object Name { get { return Player.Name; } }
        public uint Dueler { get; internal set; }
        public int Shots { get; internal set; }
        public bool Hit { get; internal set; }
        public int DirectionChange { get; internal set; }

        public int Hits = 0;
        public int Chains = 0;
        public bool RaceDizzy, RaceFrightened;
        public DateTime RaceExcitementStamp, GuardStamp, DizzyStamp, FrightenStamp, ExtraVigorStamp, DecelerateStamp;
        public uint RaceExcitementAmount, RaceExtraVigor;
        public void ApplyRacePotion(Game.MsgServer.MsgRacePotion.RaceItemType type, uint target)
        {
            Console.WriteLine("Less > ApplyRacePotion " + type);
            //switch (type)
            //{
            //    case Game.MsgServer.MsgRacePotion.RaceItemType.FrozenTrap:
            //        {
            //            if (target != uint.MaxValue)
            //            {
            //                if (Map.IsFlagPresent(Player.X, Player.Y, Role.MapFlagType.Valid) == false)
            //                {
            //                    Role.StaticRole role = new Role.StaticRole(Player.X, Player.Y);
            //                    role.DoFrozenTrap(Player.UID);
            //                    Map.AddStaticRole(role);

            //                    using (var rec = new ServerSockets.RecycledPacket())
            //                    {

            //                        var stream = rec.GetStream();
            //                        Player.View.SendView(stream, true);
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                Player.AddFlag(MsgUpdate.Flags.Freeze, 4, true);
            //            }
            //            break;
            //        }
            //    case Game.MsgServer.MsgRacePotion.RaceItemType.RestorePotion:
            //        {
            //            Vigor += 2000;
            //            if (Vigor > Status.MaxVigor)
            //                Vigor = Status.MaxVigor;

            //            using (var rec = new ServerSockets.RecycledPacket())
            //            {
            //                var stream = rec.GetStream();
            //                Send(stream.ServerInfoCreate(MsgServerInfo.Action.Vigor, Vigor));
            //            }
            //            break;
            //        }
            //    case Game.MsgServer.MsgRacePotion.RaceItemType.ExcitementPotion:
            //        {
            //            if (RaceDecelerated)
            //                Player.RemoveFlag(MsgUpdate.Flags.Deceleration);

            //            Player.AddFlag(MsgUpdate.Flags.Accelerated, 15, true, 0, 50, 25);
            //            RaceExcitementAmount = 50;
            //            break;
            //        }
            //    case Game.MsgServer.MsgRacePotion.RaceItemType.SuperExcitementPotion:
            //        {
            //            if (RaceDecelerated)
            //                Player.RemoveFlag(MsgUpdate.Flags.Deceleration);

            //            Player.AddFlag(MsgUpdate.Flags.Accelerated, 15, true, 0, 200, 100);
            //            break;
            //        }
            //    case Game.MsgServer.MsgRacePotion.RaceItemType.GuardPotion:
            //        {

            //            Player.AddFlag(MsgUpdate.Flags.GodlyShield, 10, true);
            //            break;
            //        }
            //    case Game.MsgServer.MsgRacePotion.RaceItemType.DizzyHammer:
            //        {
            //            Role.IMapObj obj;
            //            if (Player.View.TryGetValue(target, out obj, Role.MapObjectType.Player))
            //            {
            //                var user = obj as Role.Player;
            //                if (user != null)
            //                {
            //                    if (!user.Owner.RaceGuard && !user.Owner.RaceFrightened)
            //                    {
            //                        user.AddFlag(MsgUpdate.Flags.Dizzy, 5, true);
            //                    }
            //                }
            //            }

            //            break;
            //        }
            //    case Game.MsgServer.MsgRacePotion.RaceItemType.ScreamBomb:
            //        {
            //            using (var rec = new ServerSockets.RecycledPacket())
            //            {

            //                var stream = rec.GetStream();
            //                MsgSpellAnimation MsgSpell = new MsgSpellAnimation(Player.UID, 0, Player.X, Player.Y, 9989, 0, 0);
            //                MsgSpellAnimation.SpellObj AnimationObj = new MsgSpellAnimation.SpellObj(Player.UID, 0, MsgAttackPacket.AttackEffect.None);
            //                MsgSpell.SetStream(stream);
            //                MsgSpell.Send(this);
            //            }

            //            foreach (var user in Player.View.Roles(Role.MapObjectType.Player))
            //            {
            //                if (Role.Core.GetDistance(Player.X, Player.Y, user.X, user.Y) < 10)
            //                {
            //                    var obj = user as Role.Player;
            //                    if (!obj.Owner.RaceGuard && !obj.Owner.RaceDizzy)
            //                    {
            //                        obj.AddFlag(MsgUpdate.Flags.Frightened, 20, false);
            //                    }
            //                }
            //            }
            //            break;
            //        }
            //    case Game.MsgServer.MsgRacePotion.RaceItemType.SpiritPotion:
            //        {

            //            break;
            //        }
            //    case Game.MsgServer.MsgRacePotion.RaceItemType.ChaosBomb:
            //        {
            //            using (var rec = new ServerSockets.RecycledPacket())
            //            {

            //                var stream = rec.GetStream();
            //                MsgSpellAnimation MsgSpell = new MsgSpellAnimation(Player.UID, 0, Player.X, Player.Y, 9989, 0, 0);
            //                MsgSpellAnimation.SpellObj AnimationObj = new MsgSpellAnimation.SpellObj(Player.UID, 0, MsgAttackPacket.AttackEffect.None);
            //                MsgSpell.SetStream(stream);
            //                MsgSpell.Send(this);
            //            }

            //            foreach (var user in Player.View.Roles(Role.MapObjectType.Player))
            //            {
            //                if (Role.Core.GetDistance(Player.X, Player.Y, user.X, user.Y) < 10)
            //                {
            //                    var obj = user as Role.Player;
            //                    if (!obj.Owner.RaceGuard)
            //                    {
            //                        obj.RemoveFlag(MsgUpdate.Flags.Dizzy);
            //                        obj.AddFlag(MsgUpdate.Flags.Confused, 15, false);
            //                    }
            //                }
            //            }
            //            break;
            //        }
            //    case Game.MsgServer.MsgRacePotion.RaceItemType.SluggishPotion:
            //        {
            //            using (var rec = new ServerSockets.RecycledPacket())
            //            {

            //                var stream = rec.GetStream();
            //                MsgSpellAnimation MsgSpell = new MsgSpellAnimation(Player.UID, 0, Player.X, Player.Y, 9989, 0, 0);
            //                MsgSpellAnimation.SpellObj AnimationObj = new MsgSpellAnimation.SpellObj(Player.UID, 0, MsgAttackPacket.AttackEffect.None);
            //                MsgSpell.SetStream(stream);
            //                MsgSpell.Send(this);
            //            }

            //            foreach (var user in Player.View.Roles(Role.MapObjectType.Player))
            //            {
            //                if (Role.Core.GetDistance(Player.X, Player.Y, user.X, user.Y) < 10)
            //                {
            //                    var obj = user as Role.Player;
            //                    if (!obj.Owner.RaceGuard)
            //                    {
            //                        if (obj.Owner.RaceExcitement)
            //                            obj.RemoveFlag(MsgUpdate.Flags.Accelerated);

            //                        obj.AddFlag(MsgUpdate.Flags.Deceleration, 10, true, 0, 50, 25);
            //                    }
            //                }
            //            }

            //            break;
            //        }
            //    case Game.MsgServer.MsgRacePotion.RaceItemType.TransformItem:
            //        {
            //            for (int i = 0; i < 5; i++)
            //            {
            //                if (Player.RacePotions[i] != null)
            //                {
            //                    if (Player.RacePotions[i].Type != MsgRacePotion.RaceItemType.TransformItem)
            //                    {
            //                        using (var rec = new ServerSockets.RecycledPacket())
            //                        {

            //                            var stream = rec.GetStream();
            //                            Send(stream.CreateRecePotion(new MsgRacePotion.RacePotion() { Amount = 0, Location = i + 1, PotionType = Player.RacePotions[i].Type }));
            //                        }
            //                        Player.RacePotions[i] = null;
            //                    }
            //                }
            //            }
            //            //for (int i = 0; i < 5; i++)
            //            {
            //                int i = 0;
            //                if (Player.RacePotions[i] == null)
            //                {
            //                    int val = (int)MsgRacePotion.RaceItemType.TransformItem;
            //                    while (val == (int)MsgRacePotion.RaceItemType.TransformItem)
            //                        val = Pool.GetRandom.Next((int)MsgRacePotion.RaceItemType.ChaosBomb, (int)MsgRacePotion.RaceItemType.SuperExcitementPotion);
            //                    Player.RacePotions[i] = new Game.MsgTournaments.MsgSteedRace.UsableRacePotion();
            //                    Player.RacePotions[i].Count = 1;
            //                    Player.RacePotions[i].Type = (MsgRacePotion.RaceItemType)val;

            //                    using (var rec = new ServerSockets.RecycledPacket())
            //                    {

            //                        var stream = rec.GetStream();
            //                        Send(stream.CreateRecePotion(new MsgRacePotion.RacePotion() { Amount = 1, Location = i + 1, PotionType = Player.RacePotions[i].Type }));
            //                    }
            //                }
            //            }
            //            break;
            //        }
            //}
        }
        public Poker.Structures.PokerStructs.Player PokerPlayer;
        public bool CanPlayPoker()
        {
            if (InTrade)
                return false;
            if (Map.ID != 1858 && Map.ID != 1860)
                return false;
            return true;
        }
        internal static unsafe GameClient CharacterFromName(string p)
        {
            foreach (var x in Pool.GamePoll.Values)
            {
                if (p == x.Player.Name)
                    return x;
            }
            return null;
        }
        public bool Intrn = false;
        public DateTime LastOnlineStamp = DateTime.Now;
        internal DateTime LastCheatPacket = DateTime.Now;
        internal bool LootEnduranceBooks = true;
        internal bool LootRefenieryPacks = true;
        internal bool StoneLoot = true;
        internal bool RemoveSteed = false;
        internal DateTime RemoveSteedFlag = DateTime.Now;
        internal uint TotalSouls = 0;
        public DateTime ItemStamp;
        internal int FruitsMobs = 0, CityMobs = 0, MaxFruits = 250, MaxCity = 400;
        internal string IP;
    }
}
