using GameServer.MadeByDaRkFox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgTournaments
{
    public class MsgPkWar
    {
        public const int RewardConquerPoints = 2500, FinishMinutes = 20 , EndSignTime = 19;

        private ProcesType Mode;
        public static DateTime EndSignTimer = new DateTime();
        private DateTime FinishTimer = new DateTime();

        public uint WinnerUID = 0;
        public MsgPkWar()
        {
            Mode = ProcesType.Dead;
        }


        public void Open()
        {
            if (!Core.Features.FeatureRegistry.IsKept("events.pkwar")) return; // [feature-gate]
            if (Mode == ProcesType.Dead)
            {
                Mode = ProcesType.Idle;
                MsgSchedules.SendInvitation("WeeklyPK", "CPs,PowerExpBall", 452, 294, 1002, 0, 60);
                FinishTimer = DateTime.Now.AddMinutes(FinishMinutes);
                EndSignTimer = DateTime.Now.AddMinutes(EndSignTime);
            }
        }
        public bool AllowJoin()
        {
            return Mode == ProcesType.Idle && EndSignTimer > DateTime.Now;
        }

        public bool Started()
        {
            return Mode == ProcesType.Idle;
        }

        public void CheckUp()
        {
            if (Mode == ProcesType.Idle)
            {
                if (DateTime.Now > FinishTimer)
                {
                    Mode = ProcesType.Dead;
                    MsgSchedules.SendSysMesage("WeeklyPK has started! signup are now closed.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);                   
                }
            }
        }
        public bool IsFinished() { return Mode == ProcesType.Dead; }
        public bool TheLastPlayer()
        {
            return Pool.GamePoll.Values.Where(p => p.Player.Map == 1508 && p.Player.Alive).Count() == 1;
        }
        public void GiveReward(Client.GameClient client, ServerSockets.Packet stream)
        {
            WinnerUID = client.Player.UID;
            var value = EventsRewards.EventReward("WeeklyPKWar").RewardValue * (uint)Pool.GamePoll.Count;
            if (Pool.GamePoll.Count > 40)
                value = EventsRewards.EventReward("WeeklyPKWar").RewardValue * 40;
            client.SendSysMesage("You received " + value.ToString() + " ConquerPoints and 4 PowerExpBalls. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
            MsgSchedules.SendSysMesage("" + client.Player.Name + " Won  WeeklyPK War , he received " + RewardConquerPoints.ToString() + " ConquerPoints and 4-PowerExpBalls!", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
            string reward = "[EVENT]" + client.Player.Name + " has received " + value + " from WeeklyPK.";
            Database.ServerDatabase.LoginQueue.Enqueue(reward);
            client.Player.ConquerPoints += value;
            if (client.Inventory.HaveSpace(4))
            {
                client.Inventory.Add(stream, Database.ItemType.PowerExpBall, 4);
            }
            else
            {
                client.Inventory.AddReturnedItem(stream, Database.ItemType.PowerExpBall, 4);
            }
            AddTop(client);
            client.Teleport(430, 269, 1002, 0);
        }
        public void AddTop(Client.GameClient client)
        {
            if(WinnerUID == client.Player.UID)
                client.Player.AddFlag(MsgServer.MsgUpdate.Flags.WeeklyPKChampion, Role.StatusFlagsBigVector32.PermanentFlag, false);
        }
    }
}
