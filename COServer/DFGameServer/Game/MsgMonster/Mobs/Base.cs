using GameServer.Game.MsgMonster;
using GameServer.Game.MsgTournaments;
using static GameServer.Pool;

namespace GameServer.Mobs
{
    public class Base
    {
        public virtual void Hit() { }
        public uint ID;
        public string MapName;
        public ushort MapID;
        public ushort X;
        public ushort Y;
        public MonsterFamily Mob;
        public Base(MonsterFamily _mob)
        {
            Mob = _mob;
        }
        public virtual void Run()
        {
            if (!SendInvitationStillAlive())
            {
                SendInvitationFirstLife();
            }
        }
        public virtual void Reward(MonsterRole MobRole, Client.GameClient killer)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                string msg = "The " + MobRole.Name + " has been destroyed by " + killer.Player.Name.ToString() + "! " + MapName + " at (" + X + "," + Y + "), and dropped ConquerPoints, Stones, Soul etc..";
                MobRole.DistributeBossPoints();
                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.WhiteVibrate).GetArray(stream));
                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.pink, Game.MsgServer.MsgMessage.ChatMode.World).GetArray(stream));
                MobRole.GMap.EmptyCelly(MobRole.X, MobRole.Y);
                if (ID == 20160)//ThrillingSpook
                {
                    MobRole.GMap.EmptyCelly(41, 40);
                }
                else MobRole.GMap.EmptyCelly(X, Y);
            }
        }
        public void SendInvitationFirstLife()
        {

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                string msg = Mob.Name + " has spawned! " + MapName + " at (" + X + "," + Y + ")! Hurry and kill it.";
                #region ThrillingSpook and SnowBashee
                if (ID == 20160)//ThrillingSpook
                {
                    var mapa = Pool.ServerMaps[MapID];
                    if (!mapa.ContainMobID(ID))
                    {
                        Role.GameMap.EnterMap(2090);
                        msg = "Thrilling Spook has spawned and terrify the world! (Join from market) at (245,153).";
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                        Database.Server.AddMapMonster(stream, ServerMaps[2090], 20160, 41, 40, 1, 1, 1);
                        MsgSchedules.SendInvitation("Thrilling Spook has spawned and terrify the world!", "ConquerPoints, Stones, Soul etc..", 245, 153, 1036, 0, 60, Game.MsgServer.MsgStaticMessage.Messages.None);



                    }
                    return;
                }
                if(ID == 20070)//SnowBashee
                {
                    var mapa = Pool.ServerMaps[MapID];
                    if (!mapa.ContainMobID(ID))
                    {
                        Role.GameMap.EnterMap((int)MapID);
                        msg = "SnowBashee has spawned in FrozenGrotto2 (407,433) ! Hurry and kill it.";
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                        Database.Server.AddMapMonster(stream, ServerMaps[MapID], ID, X, Y, 1, 1, 1);
                        MsgSchedules.SendInvitation2(Mob.Name, X, Y, MapID, 0, 60, Game.MsgServer.MsgStaticMessage.Messages.None, 1, 99);



                    }
                    return;
                }
                #endregion
                #region Others
                if (ID == 3737 || ID == 3738)
                {
                    var mapa = Pool.ServerMaps[MapID];
                    if (!mapa.ContainMobID(ID))
                    {
                        Role.GameMap.EnterMap((int)MapID);
                        Database.Server.AddMapMonster(stream, ServerMaps[MapID], ID, X, Y, 1, 1, 1);
                        MsgSchedules.SendInvitation2(Mob.Name, X, Y, MapID, 0, 60, Game.MsgServer.MsgStaticMessage.Messages.None, 1, 99);
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

                    }
                    return;
                }
                if (ID == 3130 || ID == 3739)
                {
                    var mapa = Pool.ServerMaps[MapID];
                    if (!mapa.ContainMobID(ID))
                    {
                        Role.GameMap.EnterMap((int)MapID);
                        Database.Server.AddMapMonster(stream, ServerMaps[MapID], ID, X, Y, 1, 1, 1);
                        MsgSchedules.SendInvitation2(Mob.Name, X, Y, MapID, 0, 60, Game.MsgServer.MsgStaticMessage.Messages.None, 1, 99);
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

                    }
                    return;

                }
                var mapa2 = Pool.ServerMaps[MapID];//conatins
                if (!mapa2.ContainMobID(ID))
                {
                    Role.GameMap.EnterMap((int)MapID);
                    Database.Server.AddMapMonster(stream, ServerMaps[MapID], ID, X, Y, 1, 1, 1);
                    MsgSchedules.SendInvitation2(Mob.Name, X, Y, MapID, 0, 60, Game.MsgServer.MsgStaticMessage.Messages.None);
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

                }
                #endregion
            }



        }
        public bool SendInvitationStillAlive()
        {
            if (ID == 20160)//ThrillingSpook
            {
                var loc = ServerMaps[2090].GetMobLoc(ID);
                if (loc != "")
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        string msg = "Thrilling Spook is still alive in the Spook's land (Join from market) at (245,153).";
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));

                        return true;
                    }
                }
            }
            else
            {
                Role.GameMap.EnterMap((int)MapID);
                var loc = ServerMaps[MapID].GetMobLoc(ID);
                if (loc != "")
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        string msg = Mob.Name + " is still alive " + MapName + " at " + loc;
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(msg, Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
