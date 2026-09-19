using Core;
using GameServer.MadeByDaRkFox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgTournaments
{
    public class PassTheBomb : ITournament
    {
        public ProcesType Process { get; set; }
        public DateTime StartTimer = new DateTime();
        public DateTime InfoTimer = new DateTime();
        internal static DateTime NextStamp;
        public uint Seconds = 60;
        public Role.GameMap Map;
        public uint DinamicMap = 0;
        public KillerSystem KillSystem;
        public TournamentType Type { get; set; }
        public PassTheBomb(TournamentType _type)
        {
            Type = _type;
            Process = ProcesType.Dead;
        }

        public void Open()
        {
            if (!Core.Features.FeatureRegistry.IsKept("events.custom-minigames")) return; // [feature-gate]
            if (Process == ProcesType.Dead)
            {
                Hunted = "None";
                KillSystem = new KillerSystem();
                StartTimer = DateTime.Now;

                MsgSchedules.SendInvitation("PassTheBomb", "PVPPoints, Prizes, ConquerPoints", 443, 335, 1002, 0, 60);


                if (Map == null)
                {
                    Map = Pool.ServerMaps[700];
                    DinamicMap = Map.GenerateDynamicID();
                }
                SecondsLeft = 12;
                InfoTimer = DateTime.Now;
                Seconds = 60;
                Process = ProcesType.Idle;
            }
        }
        public static string Hunted = "None";
        public void SendMapPacket(ServerSockets.Packet stream)
        {
            foreach (var user in MapPlayers())
                user.Send(stream);
        }
        public DateTime ScoreStamp;
        public void ChooseRandomLeader()
        {
            var playersList = MapPlayers();
            if (playersList.Where(e => e.Player.HasTheBomb).Count() == 0 && playersList.Count() > 0)
            {
                SetTime();
                var uid = playersList[Pool.GetRandom.Next(0, playersList.Count())].Player.UID;
                Client.GameClient client;
                if (Pool.GamePoll.TryGetValue(uid, out client))
                {
                    client.Player.HasTheBomb = true;
                    Hunted = client.Player.Name;
                }

            }
        }
        public bool Join(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (Process == ProcesType.Idle)
            {
                //if (MapPlayers().Where(e => e.IP == user.IP && e.Info.HWID == user.Info.HWID).Count() > 0)
                //{
                //    user.Player.MessageBox("You already joined using other character, only 1 character is allowed in there.", null, null);
                //    return false;
                //}
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                user.Teleport(x, y, Map.ID, DinamicMap);
                Game.MsgTournaments.MsgSchedules.Arena.DoQuit(stream, user);
                user.Player.HasTheBomb = false;

                if (user.Player.OnTransform)
                    user.Player.TransformInfo.FinishTransform();
                if (user.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                    user.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                return true;
            }
            return false;
        }
        public static int SecondsLeft = 15;
        public void SetTime()
        {
            var count = MapPlayers().Count();
            if (count <= 5)
                SecondsLeft = 7;
            else if (count <= 10)
                SecondsLeft = 10;
            else SecondsLeft = 12;

        }
        public void CheckUp()
        {
            if (Process == ProcesType.Idle)
            {
                if (DateTime.Now > StartTimer.AddMinutes(1))
                {
                    MsgSchedules.SendSysMesage("PassTheBomb has started! signup are now closed.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    Process = ProcesType.Alive;
                    StartTimer = DateTime.Now;
                }
                else if (DateTime.Now > InfoTimer.AddSeconds(10))
                {
                    Seconds -= 10;
                    MsgSchedules.SendSysMesage("[PassTheBomb] Fight starts in " + Seconds.ToString() + " Seconds.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    InfoTimer = DateTime.Now;
                }
            }
            if (Process == ProcesType.Alive)
            {
                ChooseRandomLeader();
                if (DateTime.Now > StartTimer.AddMinutes(15))
                {
                    foreach (var user in MapPlayers())
                    {
                        user.Teleport(428, 378, 1002);
                    }
                    MsgSchedules.SendSysMesage("PassTheBomb has ended.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    Process = ProcesType.Dead;
                }
                if (DateTime.Now > ScoreStamp.AddSeconds(2))
                {
                    ScoreStamp = DateTime.Now;
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage($"Current players left: {MapPlayers().Count()}", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                        SendMapPacket(msg.GetArray(stream));
                        msg = new MsgServer.MsgMessage($"Bomb Holder: {Hunted} T.L: {SecondsLeft - (int)(DateTime.Now - NextStamp).TotalSeconds}", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                        SendMapPacket(msg.GetArray(stream));
                    }
                }
                if (MapPlayers().Length == 1)
                {
                    var winner = MapPlayers().First();

                    MsgSchedules.SendSysMesage("" + winner.Player.Name + " has won the PassTheBomb.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                    MsgSchedules.SendSysMesage("" + winner.Player.Name + " has won the PassTheBomb.", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.red);

                    winner.Player.PVPPoints += 1;
                    winner.Player.ConquerPoints += EventsRewards.EventReward("PassTheBomb").RewardValue;

                    winner.SendWhisper($"You`ve won the event and got 1 PVP Point {EventsRewards.EventReward("PassTheBomb").RewardValue} CPs.", "[PassTheBomb]", winner.Player.Name);
                    winner.Player.HasTheBomb = false;
                  

                    winner.SendSysMesage("You received 1 PVP Point from PassTheBomb.", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);

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
