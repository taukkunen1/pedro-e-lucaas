using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;

namespace GameServer.Telemetry
{
    public enum Currency : byte { Gold = 0, CP = 1, BoundCP = 2 }

    /// <summary>De onde vem a moeda: escopos abertos nos pontos de entrada (NPC, uso de item, pacote de item).</summary>
    public enum SourceKind : byte { None = 0, Npc = 1, ItemUsage = 2, ItemUse = 3, Compose = 4, EmbedSocket = 5 }

    /// <summary>
    /// Telemetria de economia (mint/burn) de Gold, CPs e Bound CPs.
    ///
    /// Ponto unico de captura: os setters Player.Money / ConquerPoints / BoundConquerPoints chamam Record().
    /// O motivo (reason) vem, em ordem: (1) escopo aberto no ponto de entrada (Npc, ItemUsage, ItemUse);
    /// (2) automaticamente do stack de chamadas (Classe.Metodo); regras em EconomyTelemetry.json classificam
    /// o motivo em "system" e dizem se e' mint/burn (sinal do delta) ou transferencia entre jogadores.
    ///
    /// Mint     = moeda criada (delta positivo fora de transferencia)
    /// Burn     = moeda destruida (delta negativo fora de transferencia)
    /// Transfer = troca/armazem/barraca/poker: soma esperada ~0; residuo = vazamento (imposto ou exploit)
    ///
    /// Saidas em Database\Telemetry\economy\: yyyy-MM-dd.ndjson (1 linha por evento) e summary-yyyy-MM-dd.json.
    /// </summary>
    public static class Economy
    {
        #region config
        public class Settings
        {
            [JsonProperty("enabled")] public bool Enabled { get; set; } = true;
            [JsonProperty("folder")] public string Folder { get; set; } = "Telemetry/economy";
            [JsonProperty("autoReasonFromStack")] public bool AutoReasonFromStack { get; set; } = true;
            [JsonProperty("logMinDeltaGold")] public long LogMinDeltaGold { get; set; } = 1;
            [JsonProperty("logMinDeltaCps")] public long LogMinDeltaCps { get; set; } = 1;
            [JsonProperty("alertGold")] public long AlertGold { get; set; } = 500_000_000;
            [JsonProperty("alertCps")] public long AlertCps { get; set; } = 5_000;
            [JsonProperty("summarySeconds")] public int SummarySeconds { get; set; } = 300;
            [JsonProperty("queueSize")] public int QueueSize { get; set; } = 200_000;
        }
        public class Rule
        {
            /// <summary>Regex aplicada ao reason (ex.: "Npc:ArenaGuard#3", "ItemUsage:BuyItem", "Code:MsgTrade.Process").</summary>
            [JsonProperty("match")] public string Match { get; set; }
            [JsonProperty("system")] public string System { get; set; }
            /// <summary>auto (sinal do delta: mint/burn) | transfer | ignore</summary>
            [JsonProperty("flow")] public string Flow { get; set; } = "auto";
        }
        class Config
        {
            [JsonProperty("_readme")] public string Readme { get; set; } = "Regras: a primeira que casar com o reason vence. flow: auto|transfer|ignore. Sem regra => system 'Other'.";
            [JsonProperty("settings")] public Settings Settings { get; set; } = new();
            [JsonProperty("rules")] public List<Rule> Rules { get; set; } = DefaultRules();
        }
        public static List<Rule> DefaultRules() => new()
        {
            R("^ItemUsage:(DepositWarehouse|WarehouseWithdraw|ShowWarehouseMoney)$", "Warehouse", "transfer"),
            R("^ItemUsage:DropGold$", "FloorDrop"),
            R("^ItemUsage:(BuyVendingItem|AddVendingItem\\w*|RemoveVendingItem|ShowVendingList)$", "Booth", "transfer"),
            R("^ItemUsage:(RedeemGear|ClaimGear)$", "PkRedemption", "transfer"),
            R("^ItemUsage:(BuyItem|BuyItemFromForging|GarmentShop|SellItem)$", "NpcShop"),
            R("^ItemUsage:Repair", "Repair"),
            R("^Compose:", "Forging"),
            R("^EmbedSocket:", "Forging"),
            R("^ItemUsage:(Upgrade|GemCompose|Socket|CreateSocket|AddBless|UpdatePurity|ToristSuper|Alternante|UnAlternante|Merge|Embed)", "Forging"),
            R("^ItemUsage:", "ItemUsage.Other"),
            R("^ItemUse:", "ItemUse"),
            R("^Npc:", "Npc"),
            R("^Code:(MsgTrade|Trade)\\b", "Trade", "transfer"),
            R("^Code:MsgItemPacket\\b", "FloorPickup"),
            R("^Code:(DropMob|MonsterRole|MobCollection|MonsterFamily)\\b", "MonsterDrop"),
            R("^Code:(Poker|PokerHandler|PokerTable|PokerPlayer)\\b", "Poker", "transfer"),
            R("^Code:(PipeClient|PipeServer|StaticConnexion)\\b", "InterServer", "transfer"),
            R("^Code:(MsgNobility)\\b", "Nobility"),
            R("^Code:MsgGuildProces\\b", "GuildTreasury", "transfer"),
            R("^Code:Guild\\w*\\b", "Guild"),
            R("^Code:MsgOsShop\\b", "OsShop"),
            R("^Code:MsgChiInfo\\b", "Chi"),
            R("^Code:MsgNameChange\\b", "NameChange"),
            R("^Code:MsgMachine\\b", "Machine"),
            R("^Code:MsgBroadcast\\b", "Broadcast"),
            R("^Code:MsgMessage\\b", "GM/Commands"),
            R("^Code:(Msg\\w*(Tournament|Arena|War|TopFight|Flag|Couples|Group)\\w*|KingOfTheHill|PassTheBomb|FrozenSky|LastManStand|TreasureThief|DragonWar|CityWars|FortressWars|Fivenout|KillerSystem)\\b", "Events"),
            R("^Code:(ServerDatabase|DataManager|EventsRewards|ItemsByTime|BulletinManager)\\b", "ServerSystems"),
            R("^Code:PoolProcesor\\b", "PoolTimers"),
        };
        static Rule R(string m, string s, string f = "auto") => new() { Match = m, System = s, Flow = f };
        #endregion

        #region estado
        static Settings _cfg = new();
        static (Regex rx, string system, string flow)[] _rules = Array.Empty<(Regex, string, string)>();
        static readonly ConcurrentDictionary<string, (string system, string flow)> _classified = new();
        static readonly ConcurrentDictionary<(byte, uint, uint), string> _scopeNames = new();
        static readonly ConcurrentDictionary<MethodBase, string> _stackNames = new();
        static BlockingCollection<Ev> _queue;
        static Thread _writer;
        static volatile bool _stop;
        static string _dir;
        static long _dropped;
        static bool _started;

        struct Ev
        {
            public DateTime T; public uint Uid; public string Name; public uint Map; public Currency Cur;
            public long Before, After; public string Flow, System, Reason, Resource; public bool Alert;
        }
        class Agg { public long Count, In, Out; }
        class PlayerAgg { public string Name; public readonly long[] Mint = new long[3]; public readonly long[] Burn = new long[3]; }

        static ConcurrentDictionary<(Currency cur, string system, string reason, bool transfer), Agg> _agg = new();
        static ConcurrentDictionary<uint, PlayerAgg> _players = new();
        static ConcurrentDictionary<(string resource, string system, string reason, bool transfer, uint map), Agg> _resources = new();
        static DateTime _day = DateTime.Now.Date;
        static readonly object _swap = new();
        #endregion

        #region escopos
        public struct Ctx { public SourceKind Kind; public uint Id, Sub; }
        [ThreadStatic] static Ctx _cur;

        public readonly struct EconomyScope : IDisposable
        {
            readonly Ctx _prev;
            internal EconomyScope(Ctx prev) { _prev = prev; }
            public void Dispose() { _cur = _prev; }
        }

        /// <summary>Abre um escopo: tudo que mexer em gold/CP nesta thread ate o Dispose() e' atribuido a este motivo.</summary>
        public static EconomyScope Scope(SourceKind kind, uint id, uint sub = 0)
        {
            var prev = _cur;
            _cur = new Ctx { Kind = kind, Id = id, Sub = sub };
            return new EconomyScope(prev);
        }

        static string BuildScopeName(SourceKind k, uint id, uint sub)
        {
            switch (k)
            {
                case SourceKind.Npc:
                    {
                        string n = Enum.GetName(typeof(Game.MsgNpc.NpcID), id) ?? id.ToString();
                        return $"Npc:{n}#{sub}";
                    }
                case SourceKind.ItemUsage:
                    return "ItemUsage:" + (Enum.GetName(typeof(Game.MsgServer.MsgItemUsuagePacket.ItemUsageID), id) ?? id.ToString());
                case SourceKind.ItemUse:
                    {
                        string name = "";
                        try { if (Pool.ItemsBase.TryGetValue(id, out var it)) name = " " + it.Name; } catch { }
                        return $"ItemUse:{id}{name}";
                    }
                case SourceKind.Compose:
                    return "Compose:" + (Enum.GetName(typeof(Game.MsgServer.MsgUpdateItem.ActionType), (byte)id) ?? id.ToString());
                case SourceKind.EmbedSocket:
                    return "EmbedSocket:" + (Enum.GetName(typeof(Game.MsgServer.AttackHandler.MsgEmbedSocket.ActionMode), (ushort)id) ?? id.ToString()) + "#" + sub;
            }
            return "Unknown";
        }
        #endregion

        #region API
        public static bool Enabled => _started && _cfg.Enabled;

        /// <summary>Chamar uma vez no start, depois que o caminho da Database e' conhecido.</summary>
        public static void Init(string databaseDir)
        {
            if (_started) return;
            try
            {
                string cfgPath = Path.Combine(databaseDir, "EconomyTelemetry.json");
                Config cfg;
                if (File.Exists(cfgPath)) cfg = JsonConvert.DeserializeObject<Config>(File.ReadAllText(cfgPath)) ?? new Config();
                else { cfg = new Config(); File.WriteAllText(cfgPath, JsonConvert.SerializeObject(cfg, Formatting.Indented)); }
                _cfg = cfg.Settings ?? new Settings();
                _rules = (cfg.Rules ?? DefaultRules()).Select(r => (new Regex(r.Match, RegexOptions.Compiled | RegexOptions.CultureInvariant), r.System ?? "Other", (r.Flow ?? "auto").ToLowerInvariant())).ToArray();
                if (!_cfg.Enabled) { Console.WriteLine("[Economy] telemetry disabled (EconomyTelemetry.json)"); return; }
                _dir = Path.GetFullPath(Path.Combine(databaseDir, _cfg.Folder));
                Directory.CreateDirectory(_dir);
                _queue = new BlockingCollection<Ev>(Math.Max(1000, _cfg.QueueSize));
                _writer = new Thread(WriterLoop) { IsBackground = true, Name = "EconomyTelemetry" };
                _writer.Start();
                AppDomain.CurrentDomain.ProcessExit += (s, e) => Shutdown();
                _started = true;
                Console.WriteLine($"[Economy] telemetry on -> {_dir} ({_rules.Length} rules)");
            }
            catch (Exception e) { Console.WriteLine("[Economy] init failed: " + e.Message); }
        }

        /// <summary>Chamado pelos setters. before/after em unidades da moeda (Gold, CP ou BoundCP).</summary>
        public static void Record(uint uid, string name, uint map, Currency cur, long before, long after)
        {
            if (!_started || before == after) return;
            Push(uid, name, map, cur, before, after, null);
        }

        /// <summary>Tracks scarce Era 1 item resources using the same MINT/BURN/TRANSFER classifier as currency.</summary>
        public static void RecordResource(uint uid, string name, uint map, string resource, long delta)
        {
            if (!_started || string.IsNullOrEmpty(resource) || delta == 0) return;
            try
            {
                string reason = ReasonNow();
                var (system, flowMode) = Classify(reason);
                if (flowMode == "ignore") return;
                bool transfer = flowMode == "transfer";
                long abs = System.Math.Abs(delta);
                var a = _resources.GetOrAdd((resource, system, reason, transfer, map), _ => new Agg());
                Interlocked.Increment(ref a.Count);
                if (delta > 0) Interlocked.Add(ref a.In, abs); else Interlocked.Add(ref a.Out, abs);

                string flow = transfer ? "Transfer" : (delta > 0 ? "Mint" : "Burn");
                if (!_queue.TryAdd(new Ev { T = DateTime.Now, Uid = uid, Name = name, Map = map,
                    Cur = Currency.Gold, Before = 0, After = delta, Flow = flow,
                    System = "Resource:" + resource + "/" + system, Reason = reason, Resource = resource }))
                    Interlocked.Increment(ref _dropped);
            }
            catch { }
        }

        /// <summary>Para ganhos/gastos que nao passam pelos setters (ex.: personagem novo antes do login completo).</summary>
        public static void RecordManual(uint uid, string name, uint map, Currency cur, long delta, string reason)
        {
            if (!_started || delta == 0) return;
            Push(uid, name, map, cur, 0, delta, reason);
        }

        static void Push(uint uid, string name, uint map, Currency cur, long before, long after, string manualReason)
        {
            try
            {
                long delta = after - before;
                string reason = manualReason ?? ReasonNow();
                var (system, flowMode) = Classify(reason);
                if (flowMode == "ignore") return;
                bool transfer = flowMode == "transfer";
                string flow = transfer ? "Transfer" : (delta > 0 ? "Mint" : "Burn");
                long abs = Math.Abs(delta);

                var a = _agg.GetOrAdd((cur, system, reason, transfer), _ => new Agg());
                Interlocked.Increment(ref a.Count);
                if (delta > 0) Interlocked.Add(ref a.In, abs); else Interlocked.Add(ref a.Out, abs);

                if (!transfer)
                {
                    var p = _players.GetOrAdd(uid, _ => new PlayerAgg());
                    lock (p) { p.Name = name; if (delta > 0) p.Mint[(int)cur] += abs; else p.Burn[(int)cur] += abs; }
                }

                long min = cur == Currency.Gold ? _cfg.LogMinDeltaGold : _cfg.LogMinDeltaCps;
                bool alert = abs >= (cur == Currency.Gold ? _cfg.AlertGold : _cfg.AlertCps);
                if (alert) Console.WriteLine($"[Economy][ALERT] {name}({uid}) {cur} {delta:+#,0;-#,0} {flow}/{system} <- {reason}");
                if (abs >= min || alert)
                {
                    if (!_queue.TryAdd(new Ev { T = DateTime.Now, Uid = uid, Name = name, Map = map, Cur = cur, Before = before, After = after, Flow = flow, System = system, Reason = reason, Alert = alert }))
                        Interlocked.Increment(ref _dropped);
                }
            }
            catch { /* telemetria nunca pode derrubar o jogo */ }
        }

        static (string system, string flow) Classify(string reason)
            => _classified.GetOrAdd(reason, r =>
            {
                foreach (var (rx, system, flow) in _rules) if (rx.IsMatch(r)) return (system, flow);
                return ("Other", "auto");
            });
        #endregion

        #region reason
        static string ReasonNow()
        {
            var c = _cur;
            if (c.Kind != SourceKind.None)
                return _scopeNames.GetOrAdd(((byte)c.Kind, c.Id, c.Sub), k => BuildScopeName((SourceKind)k.Item1, k.Item2, k.Item3));
            return _cfg.AutoReasonFromStack ? FromStack() : "Code:unknown";
        }

        static string FromStack()
        {
            var st = new StackTrace(2, false);
            int n = Math.Min(st.FrameCount, 14);
            for (int i = 0; i < n; i++)
            {
                var m = st.GetFrame(i).GetMethod();
                var t = m?.DeclaringType;
                if (t == null) continue;
                if (t.Namespace != null && t.Namespace.StartsWith("GameServer.Telemetry")) continue;
                if (m.Name.StartsWith("set_Money") || m.Name.StartsWith("set_ConquerPoints") || m.Name.StartsWith("set_BoundConquerPoints")) continue;
                return _stackNames.GetOrAdd(m, Describe);
            }
            return "Code:unknown";
        }

        static readonly Regex _compilerName = new(@"^<(.+?)>[A-Za-z]__", RegexOptions.Compiled);
        static string Describe(MethodBase m)
        {
            string mn = m.Name;
            var mm = _compilerName.Match(mn);
            if (mm.Success) mn = mm.Groups[1].Value;           // lambda / async: <Handler>b__3_0 -> Handler
            var t = m.DeclaringType;
            while (t != null && t.IsNested && t.Name.StartsWith("<")) t = t.DeclaringType;   // <>c__DisplayClass..
            if (t != null && _compilerName.IsMatch(t.Name)) mn = _compilerName.Match(t.Name).Groups[1].Value;
            return $"Code:{t?.Name ?? "?"}.{mn}";
        }
        #endregion

        #region escrita
        static void WriterLoop()
        {
            StreamWriter w = null; string wDate = null;
            var nextSummary = DateTime.Now.AddSeconds(_cfg.SummarySeconds);
            try
            {
                while (!_stop || _queue.Count > 0)
                {
                    Ev ev;
                    if (_queue.TryTake(out ev, 1000))
                    {
                        int burst = 0;
                        do
                        {
                            string d = ev.T.ToString("yyyy-MM-dd");
                            if (d != wDate) { w?.Dispose(); w = new StreamWriter(new FileStream(Path.Combine(_dir, d + ".ndjson"), FileMode.Append, FileAccess.Write, FileShare.ReadWrite), new UTF8Encoding(false)); wDate = d; }
                            w.WriteLine(Line(ev));
                        } while (++burst < 5000 && _queue.TryTake(out ev));
                    }
                    w?.Flush();
                    if (DateTime.Now >= nextSummary) { WriteSummary(false); nextSummary = DateTime.Now.AddSeconds(_cfg.SummarySeconds); }
                    if (DateTime.Now.Date != _day) RollDay();
                    if (_stop && _queue.Count == 0) break;
                }
            }
            catch (Exception e) { Console.WriteLine("[Economy] writer error: " + e.Message); }
            finally { w?.Dispose(); }
        }

        static string Line(in Ev e)
        {
            var sb = new StringBuilder(220);
            sb.Append("{\"t\":\"").Append(e.T.ToString("yyyy-MM-ddTHH:mm:ss.fff")).Append("\",\"uid\":").Append(e.Uid)
              .Append(",\"name\":").Append(JsonConvert.ToString(e.Name ?? "")).Append(",\"map\":").Append(e.Map)
              .Append(",\"cur\":\"").Append(e.Cur).Append("\"");
            if (!string.IsNullOrEmpty(e.Resource))
                sb.Append(",\"resource\":").Append(JsonConvert.ToString(e.Resource));
            sb.Append(",\"before\":").Append(e.Before).Append(",\"after\":").Append(e.After)
              .Append(",\"delta\":").Append(e.After - e.Before)
              .Append(",\"flow\":\"").Append(e.Flow).Append("\",\"system\":").Append(JsonConvert.ToString(e.System))
              .Append(",\"reason\":").Append(JsonConvert.ToString(e.Reason));
            if (e.Alert) sb.Append(",\"alert\":true");
            return sb.Append('}').ToString();
        }

        static void RollDay()
        {
            WriteSummary(true);
            lock (_swap) { _agg = new(); _players = new(); _resources = new(); _day = DateTime.Now.Date; }
        }

        public static string SnapshotJson()
        {
            var cur = new Dictionary<string, object>();
            foreach (Currency c in Enum.GetValues(typeof(Currency)))
            {
                var rows = _agg.Where(k => k.Key.cur == c).ToList();
                if (rows.Count == 0) continue;
                long minted = rows.Where(r => !r.Key.transfer).Sum(r => r.Value.In);
                long burned = rows.Where(r => !r.Key.transfer).Sum(r => r.Value.Out);
                var bySystem = rows.GroupBy(r => r.Key.system).Select(g => new
                {
                    system = g.Key,
                    minted = g.Where(r => !r.Key.transfer).Sum(r => r.Value.In),
                    burned = g.Where(r => !r.Key.transfer).Sum(r => r.Value.Out),
                    transferIn = g.Where(r => r.Key.transfer).Sum(r => r.Value.In),
                    transferOut = g.Where(r => r.Key.transfer).Sum(r => r.Value.Out),
                    transferResidual = g.Where(r => r.Key.transfer).Sum(r => r.Value.In - r.Value.Out),
                    events = g.Sum(r => r.Value.Count)
                }).OrderByDescending(x => x.minted + x.burned + x.transferIn + x.transferOut).ToList();
                var topReasons = rows.Select(r => new { reason = r.Key.reason, system = r.Key.system, transfer = r.Key.transfer, events = r.Value.Count, @in = r.Value.In, @out = r.Value.Out })
                                     .OrderByDescending(x => x.@in + x.@out).Take(40).ToList();
                var pl = _players.ToArray();
                int i = (int)c;
                cur[c.ToString()] = new
                {
                    minted,
                    burned,
                    net = minted - burned,
                    mintBurnRatio = burned == 0 ? (double?)null : System.Math.Round((double)minted / burned, 4),
                    burnCoveragePct = minted == 0 ? (double?)null : System.Math.Round((double)burned * 100.0 / minted, 2),
                    bySystem, topReasons,
                    topMinters = pl.Where(p => p.Value.Mint[i] > 0).OrderByDescending(p => p.Value.Mint[i]).Take(20).Select(p => new { uid = p.Key, name = p.Value.Name, amount = p.Value.Mint[i] }),
                    topBurners = pl.Where(p => p.Value.Burn[i] > 0).OrderByDescending(p => p.Value.Burn[i]).Take(20).Select(p => new { uid = p.Key, name = p.Value.Name, amount = p.Value.Burn[i] })
                };
            }
            var resources = _resources.GroupBy(r => r.Key.resource).ToDictionary(
                g => g.Key,
                g =>
                {
                    long minted = g.Where(r => !r.Key.transfer).Sum(r => r.Value.In);
                    long burned = g.Where(r => !r.Key.transfer).Sum(r => r.Value.Out);
                    return (object)new
                    {
                        minted,
                        burned,
                        net = minted - burned,
                        mintBurnRatio = burned == 0 ? (double?)null : System.Math.Round((double)minted / burned, 4),
                        burnCoveragePct = minted == 0 ? (double?)null : System.Math.Round((double)burned * 100.0 / minted, 2),
                        transferIn = g.Where(r => r.Key.transfer).Sum(r => r.Value.In),
                        transferOut = g.Where(r => r.Key.transfer).Sum(r => r.Value.Out),
                        bySystem = g.GroupBy(r => r.Key.system).Select(s => new
                        {
                            system = s.Key,
                            minted = s.Where(r => !r.Key.transfer).Sum(r => r.Value.In),
                            burned = s.Where(r => !r.Key.transfer).Sum(r => r.Value.Out),
                            events = s.Sum(r => r.Value.Count)
                        }).OrderByDescending(x => x.minted + x.burned).ToList(),
                        byMap = g.GroupBy(r => r.Key.map).Select(m => new
                        {
                            map = m.Key,
                            minted = m.Where(r => !r.Key.transfer).Sum(r => r.Value.In),
                            burned = m.Where(r => !r.Key.transfer).Sum(r => r.Value.Out),
                            net = m.Where(r => !r.Key.transfer).Sum(r => r.Value.In - r.Value.Out),
                            transferIn = m.Where(r => r.Key.transfer).Sum(r => r.Value.In),
                            transferOut = m.Where(r => r.Key.transfer).Sum(r => r.Value.Out),
                            events = m.Sum(r => r.Value.Count)
                        }).OrderByDescending(x => x.minted + x.burned).ToList()
                    };
                });

            return JsonConvert.SerializeObject(new { day = _day.ToString("yyyy-MM-dd"), generatedAt = DateTime.Now.ToString("s"), droppedEvents = Interlocked.Read(ref _dropped), currencies = cur, resources }, Formatting.Indented);
        }

        static void WriteSummary(bool final)
        {
            try { File.WriteAllText(Path.Combine(_dir, "summary-" + _day.ToString("yyyy-MM-dd") + ".json"), SnapshotJson(), new UTF8Encoding(false)); }
            catch (Exception e) { Console.WriteLine("[Economy] summary error: " + e.Message); }
        }

        public static void Shutdown()
        {
            if (!_started) return;
            _started = false;
            _stop = true;
            try { _writer?.Join(5000); WriteSummary(true); } catch { }
        }
        #endregion
    }
}
