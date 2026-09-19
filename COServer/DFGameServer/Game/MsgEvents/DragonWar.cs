using GameServer.MadeByDaRkFox;
using System;
using System.Linq;

namespace GameServer.Game.MsgTournaments
{
    public class DragonWar : ITournament
    {
        public static string LastWinner = "[NONE]";
        public ProcesType Process { get; set; }
        public TournamentType Type { get; set; }
        public DateTime StartTimer = new DateTime();
        public DateTime InfoTimer = new DateTime();
        public DateTime ScoreStamp = new DateTime();
        public KillerSystem KillSystem;
        public uint Secounds = 60;
        public uint DinamicID = 0;
        public Role.GameMap Map;

        public DragonWar(TournamentType _type)
        {
            Type = _type;
            Process = ProcesType.Dead;
        }
        public void Open()
        {
            if (Process == ProcesType.Dead)
            {
                LastWinner = "[NONE]";
                KillSystem = new KillerSystem();
                if (Map == null)
                {
                    Map = Pool.ServerMaps[1767];
                    DinamicID = Map.GenerateDynamicID();
                }
                HasDragonKing = false;
                StartTimer = DateTime.Now;
                Process = ProcesType.Idle;
                MsgSchedules.SendInvitation("DragonWar", "PVPPoints, Prizes, ConquerPoints", 425, 330, 1002, 0, 60);

                InfoTimer = DateTime.Now.AddSeconds(10);
                Secounds = 60;
            }
        }
        public static bool HasDragonKing = false;
        public bool Join(Client.GameClient client, ServerSockets.Packet stream)
        {
            if (Process == ProcesType.Idle)
            {
                //if (MapPlayers().Where(e => e.IP == client.IP && e.Info.HWID == client.Info.HWID).Count() > 0)
                //{
                //    client.Player.MessageBox("You already joined using other character, only 1 character is allowed in there.", null, null);
                //    return false;
                //}
                client.Player.DragonKing = false;
                client.Player.DragonWarPoints = 1;
                client.Player.DragonWarScore = 0;
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
                    if (user.Player.DragonKing)
                        user.Player.DragonWarHits = 3;
                    else
                        user.Player.DragonWarHits = 1;
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
                    MsgSchedules.SendSysMesage("DragonWar has started! signup are now closed.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    StartTimer = DateTime.Now;
                    foreach (var user in MapPlayers())
                        user.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Freeze);
                    Process = ProcesType.Alive;
                }
                else if (DateTime.Now > InfoTimer)
                {
                    Secounds -= 10;
                    MsgSchedules.SendSysMesage("Fight starts in " + Secounds.ToString() + " seconds.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    InfoTimer = DateTime.Now.AddSeconds(10);
                }
            }
            if (Process == ProcesType.Alive)
            {
                if (DateTime.Now > StartTimer.AddSeconds(10))
                {

                    var array12 = MapPlayers();//.OrderByDescending(p => p.Player.DeathMatchScore).ToArray();
                    var array = array12.OrderByDescending(p => p.Player.DragonWarScore).ToArray();

                    if (array12.Where(e => e.Player.DragonKing).Count() > 0)
                        HasDragonKing = true;
                    else
                        HasDragonKing = false;

                    if (array.Length > 0)
                    {
                        #region Rewards
                        var Winner = array.FirstOrDefault();
                        if (Winner != null && (DateTime.Now > StartTimer.AddMinutes(7) || Winner.Player.DragonWarScore >= 300 || array.Length == 1))
                        {
                            MsgSchedules.SendSysMesage("" + Winner.Player.Name + " has won the DragonWar match. ", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                            MsgSchedules.SendSysMesage("" + Winner.Player.Name + " has won the DragonWar match. ", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                            //Winner.Player.ConquerPoints += EventsRewards.DragonWar;
                            //string reward = "[EVENT]" + Winner.Player.Name + " has won and received " + EventsRewards.DeathMatch + " CPs from DragonWar.";
                            Winner.Player.PVPPoints += 1;
                            Winner.Player.ConquerPoints += EventsRewards.EventReward("DragonWar").RewardValue;
                            Winner.Player.Money += 5000000;

                      
                            Winner.Teleport(428, 378, 1002);//done
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                Winner.SendWhisper($"You`ve won the DragonWar and received [1 PVP-Points and {EventsRewards.EventReward("DragonWar").RewardValue} + 5,000,000 Gold].", "[DragonWar]", Winner.Player.Name);
                            }
                            foreach (var user in array)
                                user.Teleport(428, 378, 1002);//done//to do
                            Process = ProcesType.Dead;
                            LastWinner = Winner.Player.Name;
                            foreach (var player in Pool.GamePoll
                                .Values
                                .Where(e => e.Player.ContainFlag(MsgServer.MsgUpdate.Flags.CTF_Flag)))
                            {
                                player.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.CTF_Flag);
                                player.Player.DragonKing = false;
                            }
                            Winner.Player.DragonKing = true;
                        }
                        if (DateTime.Now > StartTimer.AddMinutes(7))
                            Process = ProcesType.Dead;
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
                        var array = MapPlayers().OrderByDescending(p => p.Player.DragonWarScore).ToArray();

                        foreach (var user in MapPlayers())
                        {
                            if (!user.Player.Alive)
                            {
                                if (DateTime.Now > user.Player.DeathStamp.AddSeconds(2))
                                {
                                    Revive(user);
                                    ushort XX = 0, YY = 0;
                                    Map.GetRandCoord(ref XX, ref YY);
                                    user.Teleport(XX, YY, Map.ID, DinamicID);
                                }
                            }
                        }
                        Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("**DragonWar Scoreboard - First to 300 wins**", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                        SendMapPacket(msg.GetArray(stream));

                        int x = 0;
                        foreach (var obj in array)
                        {
                            if (x == 6)
                                break;
                            Game.MsgServer.MsgMessage amsg = new MsgServer.MsgMessage("No " + (x + 1).ToString() + ". " + obj.Player.Name + " (" + obj.Player.DragonWarScore.ToString() + ")", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                            SendMapPacket(amsg.GetArray(stream));

                            x++;
                        }
                        foreach (var user in MapPlayers())
                        {
                            msg = new MsgServer.MsgMessage("My Score: " + user.Player.DragonWarScore.ToString() + "", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                            user.Send(msg.GetArray(stream));
                        }
                    }
                    ScoreStamp = DateTime.Now.AddSeconds(3);
                }
            }
        }
    }
}