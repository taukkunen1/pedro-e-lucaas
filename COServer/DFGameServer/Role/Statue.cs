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
        public int Action = 0;
        public ushort Action2;
        public uint UID;
        public int HitPotion = 0;
        public Client.GameClient user;

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
                    npc.X = x;
                    npc.Y = y;
                    npc.Map = client.Player.Map;
                    npc.MaxHitPoints = (int)(client.Status.MaxHitpoints * 10);
                    npc.HitPoints = client.Player.HitPoints * 10;

                    client.Player.View.SendView(npc.GetArray(stream,false), true);

                    client.Map.View.EnterMap<IMapObj>(npc);
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

            ActionQuery action = new ActionQuery()
            {
                ObjId = UID,
                Type = ActionType.RemoveEntity
            };
            killer.Player.View.SendView(stream.ActionCreate(&action), true);
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
                    RemoveStatue(stream, user, StaticSobNpc.UID, StaticSobNpc);
                    CreateStatue(user, 301, 141, 0, 0, true);
                }
            }
        }
        public static void Save()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Write _wr = new Write("StaticStatue.txt"))
                {
                    if (StaticStatue != null && StaticStatue.StatuePacket != null && StaticSobNpc != null)
                    {
                        int Size = StaticStatue.StatuePacket.Length;
                        WriteLine line = new WriteLine('/');


                        line.Add(Size);
                        for (int x = 0; x < Size; x++)
                            line.Add(StaticStatue.StatuePacket[x]);
                        line.Add(StaticSobNpc.UID).Add(StaticSobNpc.X).Add(StaticSobNpc.Y).Add(StaticSobNpc.Map).Add(StaticSobNpc.MaxHitPoints).Add(StaticSobNpc.HitPoints);
                        _wr.Add(line.Close());
                        _wr.Execute(Database.DBActions.Mode.Open);
                    }
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
                            if (Size != 0)
                            {
                                StaticStatue = new Statue();
                                StaticStatue.Static = true;

                                StaticStatue.StatuePacket = new byte[Size];
                                for (int i = 0; i < StaticStatue.StatuePacket.Length; i++)
                                    StaticStatue.StatuePacket[i] = readerline.Read((byte)0);

                                StaticSobNpc = new SobNpc(StaticStatue);
                                StaticSobNpc.ObjType = MapObjectType.SobNpc;
                                StaticSobNpc.UID = readerline.Read((uint)0);
                                StaticSobNpc.X = readerline.Read((ushort)0);
                                StaticSobNpc.Y = readerline.Read((ushort)0);
                                StaticSobNpc.Map = readerline.Read((ushort)0);
                                StaticSobNpc.MaxHitPoints = readerline.Read((int)0);
                                StaticSobNpc.HitPoints = readerline.Read((int)0);

                                Pool.ServerMaps[StaticSobNpc.Map].View.EnterMap<IMapObj>(StaticSobNpc);
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
                }
            }
        }
    
    }
}
