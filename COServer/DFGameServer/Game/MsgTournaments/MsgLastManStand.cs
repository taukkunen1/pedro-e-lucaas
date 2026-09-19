using GameServer.MadeByDaRkFox;
using System;
using System.Linq;

namespace GameServer.Game.MsgTournaments
{
    public class MsgLastManStand : ITournament
    {
        public const ushort MapID = 1090;
        public ProcesType Process { get; set; }
        public DateTime StartTimer = new DateTime();
        public DateTime InfoTimer = new DateTime();
        public uint Seconds = 60;
        public Role.GameMap Map;
        public uint DinamicMap = 0;
        public KillerSystem KillSystem;
        public TournamentType Type { get; set; }

        public MsgLastManStand(TournamentType _type)
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
                StartTimer = DateTime.Now;

                MsgSchedules.SendInvitation("Lastmanstanding", "PVPPoints, Prizes, ConquerPoints", 476, 350, 1002, 0, 60);


                if (Map == null)
                {
                    Map = Pool.ServerMaps[1090];
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
                
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                user.Teleport(x, y, Map.ID, DinamicMap);
                Game.MsgTournaments.MsgSchedules.Arena.DoQuit(stream, user);
                if (user.Player.OnTransform)
                    user.Player.TransformInfo.FinishTransform();
                if (user.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                    user.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                return true;
            }
            return false;
        }
        public void CheckUp()
        {
            if (Process == ProcesType.Idle)
            {
                if (DateTime.Now > StartTimer.AddMinutes(1))
                {
                    MsgSchedules.SendSysMesage("Lastmanstanding has started! signup are now closed.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    Process = ProcesType.Alive;
                    StartTimer = DateTime.Now;
                }
                else if (DateTime.Now > InfoTimer.AddSeconds(10))
                {
                    Seconds -= 10;
                    MsgSchedules.SendSysMesage("[Lastmanstanding] Fight starts in " + Seconds.ToString() + " Seconds.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    InfoTimer = DateTime.Now;
                }
            }
            if (Process == ProcesType.Alive)
            {
                if (DateTime.Now > StartTimer.AddMinutes(15))
                {
                    foreach (var user in MapPlayers())
                    {
                        user.Teleport(428, 378, 1002);
                    }
                    MsgSchedules.SendSysMesage("Lastmanstanding has ended.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    Process = ProcesType.Dead;
                }
                var AlivePlayers = MapPlayers().Where(e => e.Player.Alive);
                if (AlivePlayers.Count() == 1)
                {
                    var winner = AlivePlayers.First();

                    MsgSchedules.SendSysMesage("" + winner.Player.Name + " has won the Lastmanstanding.", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                    //winner.Player.ConquerPoints += RewardConquerPoints;
                    //string reward = "[EVENT]" + winner.Player.Name + " has won and received " + RewardConquerPoints + " CPs from Last man Standing match.";

                    winner.Player.PVPPoints += 1;
                    winner.Player.ConquerPoints += EventsRewards.EventReward("LastMan").RewardValue;
                    //winner.SendSysMesage("You received " + RewardConquerPoints.ToString() + " ConquerPoints. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                    winner.SendSysMesage($"You received 1 PvP-Point and got (1 PvP-Point and {EventsRewards.EventReward("LastMan").RewardValue} CPs).", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);

                    winner.Teleport(428, 378, 1002, 0);
                    foreach (var user in MapPlayers())
                    {
                        user.Teleport(428, 378, 1002);
                    }
                    Process = ProcesType.Dead;
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
