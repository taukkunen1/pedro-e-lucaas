using GameServer.Game.MsgServer;
using GameServer.MadeByDaRkFox;
using System;

namespace GameServer.Game.MsgTournaments
{
    public class MsgEliteTournament
    {
        public IDataManager DataManager;
        public enum GroupTyp : ushort
        {
            EPK_Lvl100Minus = 0,
            EPK_Lvl100To119 = 1,
            EPK_Lvl120To129 = 2,
            EPK_Lvl130Plus = 3,
            Count = 4
        }

        public class top_typ
        {
            public const byte Elite_PK_Champion__Low_ = 12,
            Elite_PK_2nd_Place_Low_ = 13,
            Elite_PK_3rd_Place_Low_ = 14,
            Elite_PK_Top_8__Low_ = 15,

            Elite_PK_Champion_High_ = 16,
            Elite_PK_2nd_Place_High_ = 17,
            Elite_PK_3rd_Place__High_ = 18,
            Elite_PK_Top_8_High_ = 19;
        }

        public ProcesType Proces;
        public static MsgEliteGroup[] EliteGroups;

        public MsgEliteTournament()
        {
            Create();
            DataManager = new ElitePKDataManager();
        }
        public void Create()
        {
            Proces = ProcesType.Dead;
            EliteGroups = new MsgEliteGroup[(byte)GroupTyp.Count];

            for (GroupTyp x = GroupTyp.EPK_Lvl100Minus; x < GroupTyp.Count; x++)
            {
                EliteGroups[(byte)x] = new MsgEliteGroup(x);
            }
        }
        public void Start()
        {
            if (!Core.Features.FeatureRegistry.IsKept("events.elitepk")) return; // [feature-gate]
            if (Proces == ProcesType.Dead)
            {
                foreach (var group in EliteGroups)
                    group.CreateWaitingMap();

                Proces = ProcesType.Idle;
                if (ServerConfig.IsInterServer)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        MsgInterServer.PipeServer.Send(new MsgMessage("[Cross Elite PK Tournament] begins at 20:00. Get yourself prepared for it!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.BroadcastMessage).GetArray(stream));
                    }
                }
                else
                {
                    foreach (var client in Pool.GamePoll.Values)
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(p => p.Teleport(423, 248, 1002, 0)), null, 60, MsgServer.MsgStaticMessage.Messages.ElitePKTournament);
                    }
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
                GroupTyp group = GetGroup(client);
                EliteGroups[(byte)group].SignUp(client);
                return true;
            }
            return false;
        }
        public GroupTyp GetGroup(Client.GameClient client)
        {
            GroupTyp tournament = GroupTyp.EPK_Lvl100Minus;
            if (client.Player.Level >= 130)
            tournament = GroupTyp.EPK_Lvl130Plus;
            else if (client.Player.Level >= 120)
                tournament = GroupTyp.EPK_Lvl120To129;
            else if (client.Player.Level >= 100)
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

                    ReceiceTitle(tournament, Rank, client);
                    if (tournament.GroupTyp == GroupTyp.EPK_Lvl130Plus)
                    {
                        if (Rank == 1)
                        {
                            if (tournament.GroupTyp == GroupTyp.EPK_Lvl130Plus)
                            {
                                Role.Statue.ElitePkStatue(client);
                            }
                            if (client.Inventory.HaveSpace(3))
                            {
                                client.Inventory.Add(stream, 728919, 1);
                                client.Inventory.Add(stream, 721169, 1);
                                client.Inventory.Add(stream, 723094, 3);
                            }
                            else
                            {
                                client.Inventory.AddReturnedItem(stream, 728919, 1);
                                client.Inventory.AddReturnedItem(stream, 721169, 1);
                                client.Inventory.AddReturnedItem(stream, 723094, 3);
                            }
                            client.Player.ConquerPoints += EventsRewards.EventReward("ElitePK1st").RewardValue;
                            client.CreateBoxDialog($"You've received a {EventsRewards.EventReward("ElitePK1st").RewardValue} ConquerPoints and a few items and a few items.");

                        }
                        else if (Rank == 2)
                        {
                            if (client.Inventory.HaveSpace(2))
                            {
                                client.Inventory.Add(stream, 728919, 1);
                                client.Inventory.Add(stream, 723727, 1);
                                client.Inventory.Add(stream, 723094, 2);
                            }
                            else
                            {
                                client.Inventory.AddReturnedItem(stream, 728919, 1);
                                client.Inventory.AddReturnedItem(stream, 723727, 1);
                                client.Inventory.AddReturnedItem(stream, 723094, 2);
                            }
                            client.Player.ConquerPoints += EventsRewards.EventReward("ElitePK2nd").RewardValue;
                            client.CreateBoxDialog($"You've received a {EventsRewards.EventReward("ElitePK2nd").RewardValue} ConquerPoints and a few items.");
                        }
                        else if (Rank == 3)
                        {
                            if (client.Inventory.HaveSpace(3))
                            {
                                client.Inventory.Add(stream, 720549, 3);
                                client.Inventory.Add(stream, 723094, 2);
                            }
                            else
                            {
                                client.Inventory.AddReturnedItem(stream, 720549, 3);
                                client.Inventory.AddReturnedItem(stream, 723094, 2);
                            }
                            client.Player.ConquerPoints += EventsRewards.EventReward("ElitePK3rd").RewardValue;
                            client.CreateBoxDialog($"You've received a {EventsRewards.EventReward("ElitePK3rd").RewardValue} ConquerPoints and a few items.");

                        }
                        else if (Rank == 4)
                        {
                            if (client.Inventory.HaveSpace(3))
                            {
                                client.Inventory.Add(stream, 720549, 3);
                                client.Inventory.Add(stream, 723094, 2);
                            }
                            else
                            {
                                client.Inventory.AddReturnedItem(stream, 720549, 3);
                                client.Inventory.AddReturnedItem(stream, 723094, 2);
                            }
                            client.Player.ConquerPoints += EventsRewards.EventReward("ElitePK4th").RewardValue;
                            client.CreateBoxDialog($"You've received a {EventsRewards.EventReward("ElitePK4th").RewardValue} ConquerPoints and a few items.");
                        }
                        else if (Rank >= 5 && Rank <= 8)
                        {
                            if (client.Inventory.HaveSpace(3))
                            {
                                client.Inventory.Add(stream, 720549, 3);
                                client.Inventory.Add(stream, 723342, 4);
                            }
                            else
                            {
                                client.Inventory.AddReturnedItem(stream, 720549, 3);
                                client.Inventory.AddReturnedItem(stream, 723342, 4);
                            }
                            client.Player.ConquerPoints += EventsRewards.EventReward("ElitePK8th").RewardValue;
                            client.CreateBoxDialog($"You've received a {EventsRewards.EventReward("ElitePK8th").RewardValue} ConquerPoints and a few items.");
                        }
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
        public void ReceiceTitle(MsgTournaments.MsgEliteGroup tournament, byte Rank, Client.GameClient client)
        {
            if (tournament.GroupTyp == GroupTyp.EPK_Lvl130Plus)
            {
                client.Player.Titles.Clear();
                if (Rank == 1) client.Player.AddTitle(MsgEliteTournament.top_typ.Elite_PK_Champion_High_, true);
                else if (Rank == 2) client.Player.AddTitle(MsgEliteTournament.top_typ.Elite_PK_2nd_Place_High_, true);
                else if (Rank == 3) client.Player.AddTitle(MsgEliteTournament.top_typ.Elite_PK_3rd_Place__High_, true);
                else client.Player.AddTitle(MsgEliteTournament.top_typ.Elite_PK_Top_8_High_, true);
            }
            else
            {
                client.Player.Titles.Clear();
                if (Rank == 1) client.Player.AddTitle(MsgEliteTournament.top_typ.Elite_PK_Champion__Low_, true);
                else if (Rank == 2) client.Player.AddTitle(MsgEliteTournament.top_typ.Elite_PK_2nd_Place_Low_, true);
                else if (Rank == 3) client.Player.AddTitle(MsgEliteTournament.top_typ.Elite_PK_3rd_Place_Low_, true);
                else client.Player.AddTitle(MsgEliteTournament.top_typ.Elite_PK_Top_8__Low_, true);
            }
        }
        public uint GetItemID(MsgEliteGroup tournament, byte Rank)
        {
            return (uint)(720714 + 1 * (byte)tournament.GroupTyp + Math.Min(3, (Rank - 1)) * 4);
        }
    }

}
