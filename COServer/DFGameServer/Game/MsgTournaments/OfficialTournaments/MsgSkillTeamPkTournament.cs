using GameServer.Game.MsgServer;
using GameServer.MadeByDaRkFox;
using System;
using static GameServer.Game.MsgTournaments.MsgEliteTournament;

namespace GameServer.Game.MsgTournaments
{
    public class MsgSkillTeamPkTournament
    {
        public ProcesType Proces;
        public static MsgTeamEliteGroup[] EliteGroups;
        public IDataManager DataManager;

        public MsgSkillTeamPkTournament()
        {
            Create();
            DataManager = new SkillTeamPKDataManager();
        }
        public void Create()
        {
            Proces = ProcesType.Dead;
            EliteGroups = new MsgTeamEliteGroup[(byte)MsgEliteTournament.GroupTyp.Count];

            for (MsgEliteTournament.GroupTyp x = MsgEliteTournament.GroupTyp.EPK_Lvl100Minus; x < MsgEliteTournament.GroupTyp.Count; x++)
            {
                EliteGroups[(byte)x] = new MsgTeamEliteGroup(x, GamePackets.SkillElitePKMatchUI);
            }
        }
        public void Start()
        {
            if (!Core.Features.FeatureRegistry.IsKept("events.skilltournament")) return; // [feature-gate]
            if (Proces == ProcesType.Dead)
            {
                foreach (var group in EliteGroups)
                    group.CreateWaitingMap();

                Proces = ProcesType.Idle;
                foreach (var client in Pool.GamePoll.Values)
                {
                    client.Player.MessageBox("The Skill Team PK Tournament will start at 20:00. Prepare yourself and sign up for it as a team!", new Action<Client.GameClient>(p => p.Teleport(442, 273, 1002, 0)), null, 60, MsgServer.MsgStaticMessage.Messages.None);
                }
            }
        }
        public void Save()
        {
            DataManager.Save();
        }
        public void Load()
        {
            DataManager.Load();
        }
        public bool SignUp(Client.GameClient client)
        {
            if (Proces == ProcesType.Idle)
            {
                if (client.Team == null)
                    return false;
                GroupTyp group = GetGroup(client);
                EliteGroups[(byte)group].SignUp(client);
                return true;
            }
            return false;
        }
        public GroupTyp GetGroup(Client.GameClient client)
        {
            GroupTyp tournament = GroupTyp.EPK_Lvl100Minus;
            ushort level = client.Team.Leader.Player.Level;
            if (level >= 130)
                tournament = GroupTyp.EPK_Lvl130Plus;
            else if (level >= 120)
                tournament = GroupTyp.EPK_Lvl120To129;
            else if (level >= 100)
                tournament = GroupTyp.EPK_Lvl100To119;
            return tournament;
        }

        public bool GetReward(Client.GameClient client, ServerSockets.Packet stream)
        {
            foreach (var tournament in EliteGroups)
            {
                byte Rank = 0;
                if (tournament.GetReward(client, out Rank))
                {
                    if (Rank == 1)
                    {
                        client.Player.ConquerPoints += EventsRewards.EventReward("TeamPK1st").RewardValue;
                        client.CreateBoxDialog("You've received " + EventsRewards.EventReward("TeamPK1st").RewardValue + " ConquerPoints and wonderful items.");
                        if (client.Inventory.HaveSpace(2))
                        {
                            client.Inventory.Add(stream, 723094, 5);
                            client.Inventory.Add(stream, 728919, 2);
                        }
                        else
                        {
                            client.Inventory.AddReturnedItem(stream, 723094, 5);
                            client.Inventory.AddReturnedItem(stream, 728919, 2);
                        }
                        string MSG = "Congratulation to " + client.Player.Name + " ! he/she managed to get rank " + Rank + " on TeamTournament.";
                        Program.SendGlobalPackets.Enqueue(new MsgMessage(MSG, MsgMessage.MsgColor.red, MsgMessage.ChatMode.System).GetArray(stream));
                    }
                    else if (Rank == 2)
                    {
                        client.Player.ConquerPoints += EventsRewards.EventReward("TeamPK2nd").RewardValue;
                        if (client.Inventory.HaveSpace(2))
                        {
                            client.Inventory.Add(stream, 723094, 3);
                            client.Inventory.Add(stream, 728919, 2);
                        }
                        else
                        {
                            client.Inventory.AddReturnedItem(stream, 723094, 3);
                            client.Inventory.AddReturnedItem(stream, 728919, 2);
                        }
                        client.CreateBoxDialog("You've received " + EventsRewards.EventReward("TeamPK2nd").RewardValue + " ConquerPoints and wonderful items.");
                        string MSG = "Congratulation to " + client.Player.Name + " ! he/she managed to get rank " + Rank + " on  TeamTournament.";
                        Program.SendGlobalPackets.Enqueue(new MsgMessage(MSG, MsgMessage.MsgColor.red, MsgMessage.ChatMode.System).GetArray(stream));
                    }
                    else if (Rank == 3)
                    {
                        client.Player.ConquerPoints += EventsRewards.EventReward("TeamPK3rd").RewardValue;
                        if (client.Inventory.HaveSpace(2))
                        {
                            client.Inventory.Add(stream, 723094, 2);
                            client.Inventory.Add(stream, 728919, 2);
                        }
                        else
                        {
                            client.Inventory.AddReturnedItem(stream, 723094, 2);
                            client.Inventory.AddReturnedItem(stream, 728919, 2);
                        }
                        client.CreateBoxDialog($"You've received {EventsRewards.EventReward("TeamPK3rd").RewardValue} ConquerPoints and wonderful items..");
                        string MSG = "Congratulation to " + client.Player.Name + " ! he/she managed to get rank " + Rank + " on TeamTournament.";
                        Program.SendGlobalPackets.Enqueue(new MsgMessage(MSG, MsgMessage.MsgColor.red, MsgMessage.ChatMode.System).GetArray(stream));
                    }
                    else if (Rank == 4)
                    {
                        client.Player.ConquerPoints += EventsRewards.EventReward("TeamPK4th").RewardValue;
                        if (client.Inventory.HaveSpace(2))
                        {
                            client.Inventory.Add(stream, 723342, 4);
                            client.Inventory.Add(stream, 720549, 2);
                        }
                        else
                        {
                            client.Inventory.AddReturnedItem(stream, 723342, 4);
                            client.Inventory.AddReturnedItem(stream, 720549, 2);
                        }
                        client.CreateBoxDialog($"You've received {EventsRewards.EventReward("TeamPK4th").RewardValue} ConquerPoints and wonderful items..");
                        string MSG = "Congratulation to " + client.Player.Name + " ! he/she managed to get rank " + Rank + " on TeamTournament.";
                        Program.SendGlobalPackets.Enqueue(new MsgMessage(MSG, MsgMessage.MsgColor.red, MsgMessage.ChatMode.System).GetArray(stream));
                    }
                    else if (Rank >= 5 && Rank <= 8)
                    {
                        client.Player.ConquerPoints += EventsRewards.EventReward("TeamPK8th").RewardValue;
                        if (client.Inventory.HaveSpace(2))
                        {
                            client.Inventory.Add(stream, 723342, 4);
                            client.Inventory.Add(stream, 720549, 2);
                        }
                        else
                        {
                            client.Inventory.AddReturnedItem(stream, 723342, 4);
                            client.Inventory.AddReturnedItem(stream, 720549, 2);
                        }
                        client.CreateBoxDialog($"You've received {EventsRewards.EventReward("TeamPK8th").RewardValue} ConquerPoints and wonderful items..");
                        string MSG = "Congratulation to " + client.Player.Name + " ! he/she managed to get rank " + Rank + " on TeamTournament.";
                        Program.SendGlobalPackets.Enqueue(new MsgMessage(MSG, MsgMessage.MsgColor.red, MsgMessage.ChatMode.System).GetArray(stream));
                    }
                    return true;
                }
            }
            return false;
        }
        public void GetTitle(Client.GameClient client, ServerSockets.Packet stream)
        {
            if (!GetReward(client, stream))
            {
                foreach (var tournament in EliteGroups)
                {
                    byte Rank = 0;
                    if (!tournament.GetReward(client, out Rank) && Rank != 0)
                    {
                        ReceiceTitle(tournament, Rank, client);
                        break;
                    }
                }
            }
        }
        public uint GetItemID(MsgTeamEliteGroup tournament, byte Rank)
        {
            return (uint)(721300 + 1 * (byte)tournament.GroupTyp + Math.Min(3, (Rank - 1)) * 4);
        }
        public void ReceiceTitle(MsgTournaments.MsgTeamEliteGroup tournament, byte Rank, Client.GameClient client)
        {

        }
    }
}
