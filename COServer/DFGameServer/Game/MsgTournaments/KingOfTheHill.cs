using GameServer.MadeByDaRkFox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgTournaments
{
    public class MsgKingOfTheHill : ITournament
    {
        public ProcesType Process { get; set; }
        public TournamentType Type { get; set; }
        public DateTime StartTimer = new DateTime();
        public DateTime InfoTimer = new DateTime();
        public DateTime ScoreStamp = new DateTime();
        public KillerSystem KillSystem;
        public uint Secounds = 60;
        public uint DinamicID = 0;
        public Role.GameMap Map;

        public MsgKingOfTheHill(TournamentType _type)
        {
            Type = _type;
            Process = ProcesType.Dead;
        }
        public void Open()
        {
            if (!Core.Features.FeatureRegistry.IsKept("events.custom-minigames")) return; // [feature-gate]
            if (Process == ProcesType.Dead)
            {
                KillSystem = new KillerSystem();
                if (Map == null)
                {
                    Map = Pool.ServerMaps[700];
                    DinamicID = Map.GenerateDynamicID();
                }

                StartTimer = DateTime.Now;
                Process = ProcesType.Idle;
                MsgSchedules.SendInvitation("KingOfTheHill", "PVPPoints, Prizes, ConquerPoints", 461, 352, 1002, 0, 60);

                InfoTimer = DateTime.Now.AddSeconds(10);
                Secounds = 60;
            }
        }
        public bool Join(Client.GameClient client, ServerSockets.Packet stream)
        {
            if (Process == ProcesType.Idle)
            {
                
                client.Player.KingOfTheHillScore = 0;
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                client.Teleport(x, y, Map.ID, DinamicID);
                client.Player.AddFlag(MsgServer.MsgUpdate.Flags.Freeze, 60, true);
                Game.MsgTournaments.MsgSchedules.Arena.DoQuit(stream, client);
                if (client.Player.OnTransform)
                    client.Player.TransformInfo.FinishTransform();
                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                return true;
            }
            return false;
        }
        public bool InTournament(Client.GameClient user)
        {
            if (Map == null)
                return false;
            return user.Player.Map == Map.ID && user.Player.DynamicID == DinamicID;
        }
        public void Revive(Client.GameClient user)
        {
            if (user.Player.Alive == false && Process != ProcesType.Dead)
            {
                if (InTournament(user))
                {
                    ushort x = 0;
                    ushort y = 0;
                    Map.GetRandCoord(ref x, ref y);
                    user.Teleport(x, y, Map.ID, DinamicID);
                }
            }
        }
        public void GetPoints(Client.GameClient user)
        {
            if (Process == ProcesType.Alive)
            {
                if (DateTime.Now > user.Player.KingOfTheHillStamp.AddSeconds(8))
                {
                    short distance = Role.Core.GetDistance(user.Player.X, user.Player.Y, 50, 50);
                    if (distance <= 12)
                    {
                        user.Player.KingOfTheHillScore += 2;
                        user.Player.KingOfTheHillStamp = DateTime.Now;
                    }
                }
            }
        }
        public Client.GameClient[] MapPlayers()
        {
            return Map.Values.Where(p => InTournament(p)).ToArray();
        }
        public void SendMapPacket(ServerSockets.Packet stream)
        {
            foreach (var user in MapPlayers())
                user.Send(stream);
        }
        public void CheckUp()
        {
            if (Process == ProcesType.Idle)
            {
                if (DateTime.Now > StartTimer.AddMinutes(1))
                {
#if Arabic
                     MsgSchedules.SendSysMesage("KingOfTheHill has started! signup are now closed.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                   
#else
                    MsgSchedules.SendSysMesage("KingOfTheHill has started! signup are now closed.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);

#endif
                    StartTimer = DateTime.Now;
                    foreach (var user in MapPlayers())
                        user.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Freeze);
                    Process = ProcesType.Alive;
                }
                else if (DateTime.Now > InfoTimer)
                {
                    Secounds -= 10;
#if Arabic
                      MsgSchedules.SendSysMesage("Fight starts in " + Secounds.ToString() + " Secounds.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                   
#else
                    MsgSchedules.SendSysMesage("Fight starts in " + Secounds.ToString() + " seconds.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);

#endif
                    InfoTimer = DateTime.Now.AddSeconds(10);
                }
            }
            if (Process == ProcesType.Alive)
            {
                if (DateTime.Now > StartTimer.AddSeconds(5))
                {

                    var array = MapPlayers().OrderByDescending(p => p.Player.KingOfTheHillScore).ToArray();
                    if (array.Length > 0)
                    {
                        #region Rewards
                        var Winner = array.FirstOrDefault();
                        if (Winner != null && (Winner.Player.KingOfTheHillScore >= 80 || array.Length == 1 || DateTime.Now > StartTimer.AddMinutes(5)))
                        {

                            MsgSchedules.SendSysMesage("KingOfTheHill has ended.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                            MsgSchedules.SendSysMesage("" + Winner.Player.Name + " has won the king of hill. ", MsgServer.MsgMessage.ChatMode.BroadcastMessage, MsgServer.MsgMessage.MsgColor.white);
                            Winner.Player.ConquerPoints += EventsRewards.EventReward("KingOfTheHill").RewardValue;
                            Winner.Player.PVPPoints += 1;
                            Winner.Teleport(428, 378, 1002);//done
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                Winner.SendSysMesage("You received " + EventsRewards.EventReward("KingOfTheHill").RewardValue + " 1 PvP-Point and ConquerPoints. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                            }
                            foreach (var user in array)
                                user.Teleport(428, 378, 1002);//done//to do
                            Process = ProcesType.Dead;
                        }
                        #endregion
                    }
                    else
                        Process = ProcesType.Dead;
                }

                if (DateTime.Now > ScoreStamp)
                {

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        var array = MapPlayers().OrderByDescending(p => p.Player.KingOfTheHillScore).ToArray();

                        foreach (var user in MapPlayers())
                        {
                            if (user.Player.Alive)
                                GetPoints(user);
                            else
                            {
                                if (DateTime.Now > user.Player.DeathStamp.AddSeconds(2))
                                    Revive(user);
                            }
                        }
                        Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("ScoreBoard: (1st to 80 wins)", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                        SendMapPacket(msg.GetArray(stream));

                        int x = 0;
                        foreach (var obj in array)
                        {
                            if (x == 4)
                                break;
                            Game.MsgServer.MsgMessage amsg = new MsgServer.MsgMessage("No " + (x + 1).ToString() + ". " + obj.Player.Name + " (" + obj.Player.KingOfTheHillScore.ToString() + ")", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                            SendMapPacket(amsg.GetArray(stream));

                            x++;
                        }
                        foreach (var user in MapPlayers())
                        {
                            msg = new MsgServer.MsgMessage("My Score: " + user.Player.KingOfTheHillScore.ToString() + "", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                            user.Send(msg.GetArray(stream));
                        }
                    }
                    ScoreStamp = DateTime.Now.AddSeconds(3);
                }
            }
        }
    }
}
