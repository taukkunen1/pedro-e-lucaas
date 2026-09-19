using GameServer.Game.MsgServer;
using GameServer.MadeByDaRkFox;
using GameServer.ServerSockets;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GameServer.Game.MsgTournaments
{
    public class FrozenSky : ITournament
    {

        public uint RewardConquerPoints = 0;
        public ProcesType Process { get; set; }
        public DateTime StartTimer = new DateTime();
        public DateTime InfoTimer = new DateTime();
        public uint Seconds = 60;
        public Role.GameMap Map;
        public uint DinamicMap = 0;
        public KillerSystem KillSystem;
        public TournamentType Type { get; set; }
        public FrozenSky(TournamentType _type)
        {
            Type = _type;
            Process = ProcesType.Dead;
            RewardConquerPoints = EventsRewards.EventReward("FrozenSky").RewardValue;
        }

        public void Open()
        {
            if (!Core.Features.FeatureRegistry.IsKept("events.custom-minigames")) return; // [feature-gate]
            if (Process == ProcesType.Dead)
            {
                KillSystem = new KillerSystem();
                StartTimer = DateTime.Now;

                MsgSchedules.SendInvitation("FrozenSky", "PVPPoints, Prizes, ConquerPoints", 469, 352, 1002, 0, 60);


                if (Map == null)
                {
                    Map = Pool.ServerMaps[700];
                    DinamicMap = Map.GenerateDynamicID();
                }
                InfoTimer = DateTime.Now;
                Seconds = 60;
                Process = ProcesType.Idle;
            }
        }
        public bool Join(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (Process == ProcesType.Idle)
            {
                user.Player.SkyFight = 0;
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                user.Teleport(x, y, Map.ID, DinamicMap);
                Game.MsgTournaments.MsgSchedules.Arena.DoQuit(stream, user);
                return true;
            }
            return false;
        }
        public void SendMapPacket(ServerSockets.Packet stream)
        {
            foreach (var user in MapPlayers())
                user.Send(stream);
        }
        DateTime ScoreStamp;
        public void CheckUp()
        {
            if (Process == ProcesType.Idle)
            {
                if (DateTime.Now > StartTimer.AddMinutes(1))
                {
                    MsgSchedules.SendSysMesage("FrozenSky has started! signup are now closed.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    Process = ProcesType.Alive;
                    StartTimer = DateTime.Now;
                    SanitizePlayers();
                }
                else if (DateTime.Now > InfoTimer.AddSeconds(10))
                {
                    Seconds -= 10;
                    MsgSchedules.SendSysMesage("[FrozenSky] Fight starts in " + Seconds.ToString() + " Seconds.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    InfoTimer = DateTime.Now;
                    //if (Seconds == 10)
                    //{
                    //    using (var rec = new RecycledPacket())
                    //    {
                    //        var stream = rec.GetStream();
                    //        Task.Run(() =>
                    //        {
                    //            for (var i = 9; i >= 0; i--)
                    //            {
                    //                var players = MapPlayers().OrderByDescending(e => e.Player.SkyFight).ToList();
                    //                foreach (var clientPlayer in players)
                    //                {
                    //                    ushort cordX = (ushort)(clientPlayer.Player.X - 5);
                    //                    ushort cordY = (ushort)(clientPlayer.Player.Y - 5);
                    //                    clientPlayer.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "upnumeber" + i);
                    //                }
                    //                System.Threading.Thread.Sleep(1000);
                    //            }
                    //        });
                    //    }
                    //}
                }
            }
            if (Process == ProcesType.Alive)
            {
                if (DateTime.Now > StartTimer.AddMinutes(5))
                {
                    var players = MapPlayers().OrderByDescending(e => e.Player.SkyFight).ToList();
                    if (players.Count() > 0)
                    {
                        var winner = players[0];
                        MsgSchedules.SendSysMesage("" + winner.Player.Name + " has won FrozenSky and received " + RewardConquerPoints.ToString() + " ConquerPoints and 1 PVPPoints.", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                        winner.Player.ConquerPoints += RewardConquerPoints;
                        winner.Player.PVPPoints += 1;
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            if (winner.Inventory.HaveSpace(1))
                                winner.Inventory.Add(stream, 722057);
                        }
                        winner.SendSysMesage("You received " + RewardConquerPoints.ToString() + " ConquerPoints and 1 PVPPoints. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);

                        winner.Teleport(428, 378, 1002, 0);
                        winner.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Freeze);
                        winner.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                    }

                    foreach (var user in MapPlayers())
                    {
                        user.Teleport(428, 378, 1002);
                    }
                    MsgSchedules.SendSysMesage("FrozenSky has ended. Players has been teleported to TwinCity.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    Process = ProcesType.Dead;
                }
                if (DateTime.Now > ScoreStamp)
                {

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        var array = MapPlayers().OrderByDescending(p => p.Player.SkyFight).ToArray();


                        Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("*_ScoreBoard for FrozenSky_*", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                        SendMapPacket(msg.GetArray(stream));

                        int x = 0;
                        foreach (var obj in array)
                        {
                            if (x == 4)
                                break;
                            Game.MsgServer.MsgMessage amsg = new MsgServer.MsgMessage("Nº " + (x + 1).ToString() + ". " + obj.Player.Name + " (" + obj.Player.SkyFight.ToString() + ")", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                            SendMapPacket(amsg.GetArray(stream));

                            x++;
                        }
                        foreach (var user in MapPlayers())
                        {
                            msg = new MsgServer.MsgMessage("My Score: " + user.Player.SkyFight.ToString() + "", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                            user.Send(msg.GetArray(stream));
                        }
                    }
                    ScoreStamp = DateTime.Now.AddSeconds(3);
                }
                if (MapPlayers().Length == 1)
                {
                    var winner = MapPlayers().First();

                    MsgSchedules.SendSysMesage("" + winner.Player.Name + " has won FrozenSky and received " + RewardConquerPoints.ToString() + " ConquerPoints and 3 PVE Points.", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                    winner.Player.ConquerPoints += RewardConquerPoints;
                    winner.Player.PVEPoints += 3;
                    string reward = "[EVENT]" + winner.Player.Name + " has won and received " + RewardConquerPoints + " CPs and 3PVE Points from FrozenSky.";

                    Database.ServerDatabase.LoginQueue.Enqueue(reward);
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        if (winner.Inventory.HaveSpace(1))
                            winner.Inventory.Add(stream, 722057);
                    }
                    winner.SendSysMesage("You received " + RewardConquerPoints.ToString() + " ConquerPoints, 3PVE Points and a PowerExpBall. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);

                    winner.Teleport(428, 378, 1002, 0);

                    Process = ProcesType.Dead;
                }

                DateTime Timer = DateTime.Now;
                foreach (var user in MapPlayers())
                {
                    if (user.Player.Alive == false)
                    {
                        if (user.Player.DeadStamp.AddSeconds(4) < Timer)
                            user.Teleport(428, 378, 1002);
                    }
                }
            }


        }

        public void SanitizePlayers()
        {
            var players = MapPlayers().OrderByDescending(e => e.Player.SkyFight).ToList();
            foreach (var clientPlayer in players)
            {
                if (clientPlayer.AjustDefense < 8000)
                {
                    clientPlayer.Teleport(428, 378, 1002, 0);
                    clientPlayer.Player.AddFlag(MsgServer.MsgUpdate.Flags.Confused, 3, true);
                    clientPlayer.SendSysMesage("Sorry but you not have 8K or more of defense for participate. You need more defense for play in the event.", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                }
            }
        }

        public Client.GameClient[] MapPlayers()
        {
            return Map.Values.Where(p => p.Player.DynamicID == DinamicMap && p.Player.Map == Map.ID).ToArray();
        }

        public bool InTournament(Client.GameClient user)
        {
            if (Map == null) return false;
            return user.Player.Map == Map.ID && user.Player.DynamicID == DinamicMap;
        }
    }
}
