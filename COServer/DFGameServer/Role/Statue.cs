using Core;
using GameServer.Database.DBActions;
using GameServer.Game.MsgServer;
using System;
using System.Collections.Generic;
using System.Drawing;
using CoreModels = Core.Models.GameServer;

namespace GameServer.Role
{
    public unsafe class Statue
    {
        public unsafe static Counter CounterUID = new(105175);
        public static Statue StaticStatue = null;
        public static SobNpc StaticSobNpc = null;
        public static bool ContainStatue(GameMap map, ushort x, ushort y)
        {
            foreach (var npc in map.View.Roles(MapObjectType.SobNpc, x,y))
                if (npc.X == x && npc.Y == y)
                    return true;
            return false;
        }
        /// <summary>Todas as estatuas vivas (a do Elite PK e as de guild). Persistidas em StaticStatue.txt.</summary>
        public static readonly System.Collections.Concurrent.ConcurrentDictionary<uint, SobNpc> Statues = new System.Collections.Concurrent.ConcurrentDictionary<uint, SobNpc>();

        public int Action = 0;
        public ushort Action2;
        public uint UID;
        public uint GuildID;
        public int HitPotion = 0;
        public Client.GameClient user;

        /// <summary>
        /// Pacote de spawn congelado no momento da criacao. Antes so a estatua do Elite PK
        /// era congelada; as de guild eram montadas a partir do jogador vivo (mudavam de
        /// equipamento/angulo junto com ele, ficavam sem nome e sumiam no restart).
        /// </summary>
        public byte[] StatuePacket;
        public bool Static = false;
        public unsafe static void CreateStatue(Client.GameClient client, ushort x, ushort y, int Action, int action2, bool Static = false)
        {
            try
            {
                Statue stat = new Statue();
                stat.user = client;

                stat.UID = CounterUID.Next;
                stat.HitPotion = client.Player.HitPoints / 100;
                stat.Action = Action;
                stat.Action2 = (ushort)action2;
                stat.GuildID = client.Player.GuildID;
                stat.Static = Static;
                if (stat.Static)
                {
                    stat.user.Player.Action = Flags.ConquerAction.Sit;
                    stat.user.Player.Angle = Flags.ConquerAngle.South;
                    StaticStatue = stat;
                }
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    SobNpc npc = new SobNpc(stat);
                    npc.ObjType = MapObjectType.SobNpc;
                    npc.UID = stat.UID;
                    npc.Name = client.Player.Name;
                    npc.X = x;
                    npc.Y = y;
                    npc.Map = client.Player.Map;
                    npc.MaxHitPoints = (int)(client.Status.MaxHitpoints * 10);
                    npc.HitPoints = client.Player.HitPoints * 10;

                    // GetArray congela o pacote (StatuePacket) na primeira chamada.
                    client.Player.View.SendView(npc.GetArray(stream, false), true);
                    stat.user = null; // a estatua nao depende mais do jogador online

                    client.Map.View.EnterMap<IMapObj>(npc);
                    Statues[npc.UID] = npc;
                    if (Static)
                        StaticSobNpc = npc;
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public unsafe static void RemoveStatue(ServerSockets.Packet stream, Client.GameClient killer, uint UID, IMapObj obj)
        {
            GameMap map;
            if (killer.Map != null)
                map = killer.Map;
            else
              map =  Pool.ServerMaps[1002];

            map.View.LeaveMap(obj);
            Forget(UID);

            ActionQuery action = new ActionQuery()
            {
                ObjId = UID,
                Type = ActionType.RemoveEntity
            };
            killer.Player.View.SendView(stream.ActionCreate(&action), true);
        }

        static void Forget(uint uid)
        {
            SobNpc removed;
            Statues.TryRemove(uid, out removed);
            if (StaticSobNpc != null && StaticSobNpc.UID == uid)
            {
                StaticSobNpc = null;
                StaticStatue = null;
            }
        }

        /// <summary>Remove, sem precisar de um jogador, as estatuas de guild de um mapa que nao sao da guild informada.</summary>
        public unsafe static void RemoveGuildStatuesExcept(uint map, uint keepGuildId)
        {
            foreach (var npc in Statues.Values)
            {
                if (npc.Map != map || npc.statue == null || npc.statue.Static || npc.statue.GuildID == keepGuildId)
                    continue;
                GameMap gameMap;
                if (Pool.ServerMaps.TryGetValue(npc.Map, out gameMap))
                    gameMap.View.LeaveMap(npc);
                Forget(npc.UID);
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    ActionQuery action = new ActionQuery()
                    {
                        ObjId = npc.UID,
                        Type = ActionType.RemoveEntity
                    };
                    npc.SendScrennPacket(stream.ActionCreate(&action));
                }
            }
        }

        public static void ElitePkStatue(Client.GameClient user)
        {
            if (StaticStatue == null && StaticSobNpc == null)
            {
                CreateStatue(user, 301, 141, 0, 0, true);
            }
            else
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    if (StaticSobNpc != null)
                        RemoveStatue(stream, user, StaticSobNpc.UID, StaticSobNpc);
                    CreateStatue(user, 301, 141, 0, 0, true);
                }
            }
        }
        public static void Save()
        {
            if (ServerConfig.DbFromFiles)
            {
                // Formato por linha: tamanho/bytes.../UID/X/Y/Map/MaxHP/HP/Static/GuildID
                using (Write _wr = new Write("StaticStatue.txt"))
                {
                    foreach (var npc in Statues.Values)
                    {
                        var st = npc.statue;
                        if (st == null || st.StatuePacket == null)
                            continue;
                        WriteLine line = new WriteLine('/');
                        line.Add(st.StatuePacket.Length);
                        for (int x = 0; x < st.StatuePacket.Length; x++)
                            line.Add(st.StatuePacket[x]);
                        line.Add(npc.UID).Add(npc.X).Add(npc.Y).Add(npc.Map).Add(npc.MaxHitPoints).Add(npc.HitPoints)
                            .Add(st.Static ? 1 : 0).Add(st.GuildID);
                        _wr.Add(line.Close());
                    }
                    _wr.Execute(Database.DBActions.Mode.Open);
                }
            } else
            {
                List<CoreModels.StaticStatue> staticStatues = new List<CoreModels.StaticStatue>();
                if (StaticStatue != null && StaticStatue.StatuePacket != null && StaticSobNpc != null)
                {
                    int Size = StaticStatue.StatuePacket.Length;
                    WriteLine line = new WriteLine('/');
                    line.Add(Size);
                    for (int x = 0; x < Size; x++)
                        line.Add(StaticStatue.StatuePacket[x]);
                    CoreModels.StaticStatue staticStatueToSave = new CoreModels.StaticStatue() { StatueSize = (uint)Size, StatuePackets = line.Close() };
                    staticStatueToSave.UID = StaticSobNpc.UID;
                    staticStatueToSave.X = StaticSobNpc.X;
                    staticStatueToSave.Y = StaticSobNpc.Y;
                    staticStatueToSave.Map = StaticSobNpc.Map;
                    staticStatueToSave.MaxHitPoints = (uint)StaticSobNpc.MaxHitPoints;
                    staticStatueToSave.HitPoints = (uint)StaticSobNpc.HitPoints;
                    staticStatues.Add(staticStatueToSave);
                }
            }
        }
        public static void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Read r = new Read("StaticStatue.txt"))
                {
                    if (r.Reader())
                    {
                        int count = r.Count;
                        for (uint x = 0; x < count; x++)
                        {

                            ReadLine readerline = new ReadLine(r.ReadString(""), '/');
                            int Size = readerline.Read((int)0);
                            if (Size == 0)
                                continue;

                            Statue st = new Statue();
                            st.StatuePacket = new byte[Size];
                            for (int i = 0; i < st.StatuePacket.Length; i++)
                                st.StatuePacket[i] = readerline.Read((byte)0);

                            SobNpc npc = new SobNpc(st);
                            npc.ObjType = MapObjectType.SobNpc;
                            npc.UID = readerline.Read((uint)0);
                            npc.X = readerline.Read((ushort)0);
                            npc.Y = readerline.Read((ushort)0);
                            npc.Map = readerline.Read((ushort)0);
                            npc.MaxHitPoints = readerline.Read((int)0);
                            npc.HitPoints = readerline.Read((int)0);
                            st.Static = readerline.Read((byte)1) == 1; // arquivos antigos so tinham a estatua do Elite PK
                            st.GuildID = readerline.Read((uint)0);
                            st.UID = npc.UID;

                            GameMap gameMap;
                            if (!Pool.ServerMaps.TryGetValue(npc.Map, out gameMap))
                                continue;
                            gameMap.View.EnterMap<IMapObj>(npc);
                            Statues[npc.UID] = npc;
                            if (npc.UID >= CounterUID.Count)
                                CounterUID.Set(npc.UID + 1);
                            if (st.Static)
                            {
                                StaticStatue = st;
                                StaticSobNpc = npc;
                            }
                        }
                    }
                }
            } else
            {
                List<CoreModels.StaticStatue> staticStatuesApi = RestApiHelper.GetRequest<List<CoreModels.StaticStatue>>("StaticStatue/Get");
                foreach(var staticStatue in staticStatuesApi)
                {
                    StaticStatue = new Statue
                    {
                        Static = true,
                        StatuePacket = new byte[staticStatue.StatueSize]
                    };
                    ReadLine readerline = new ReadLine(staticStatue.StatuePackets, '/');
                    for (int i = 0; i < StaticStatue.StatuePacket.Length; i++)
                        StaticStatue.StatuePacket[i] = readerline.Read((byte)0);

                    StaticSobNpc = new SobNpc(StaticStatue);
                    StaticSobNpc.ObjType = MapObjectType.SobNpc;
                    StaticSobNpc.UID = staticStatue.UID;
                    StaticSobNpc.X = staticStatue.X;
                    StaticSobNpc.Y = staticStatue.Y;
                    StaticSobNpc.Map = staticStatue.Map;
                    StaticSobNpc.MaxHitPoints = (int)staticStatue.MaxHitPoints;
                    StaticSobNpc.HitPoints = (int)staticStatue.HitPoints;

                    Pool.ServerMaps[StaticSobNpc.Map].View.EnterMap<IMapObj>(StaticSobNpc);
                    Statues[StaticSobNpc.UID] = StaticSobNpc;
                }
            }
        }
    
    }
}
