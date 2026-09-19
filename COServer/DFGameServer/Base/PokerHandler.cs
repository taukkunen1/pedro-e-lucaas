using GameServer.Client;
using Poker.Packets;
using Poker.Structures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer
{
    public unsafe class PokerHandler
    {
        public static void Shutdown(GameClient client)
        {
            if (Poker.Database.Tables != null)
            {
                foreach (var table in Poker.Database.Tables.Values)
                {
                    lock (table.TableSyncRoot)
                    {
                        if (table.OnScreen != null)
                        {
                            uint x = 0;
                            if (table.OnScreen.ContainsKey(client.ConnectionUID))
                                table.OnScreen.TryRemove(client.ConnectionUID, out x);
                        }
                    }
                }
            }
            if (client.PokerPlayer != null)
            {
                Poker.Packets.MsgShowHandExit Respond = new Poker.Packets.MsgShowHandExit(null);
                Respond.Action = 1;
                Respond.PlayerUid = client.ConnectionUID;
                Respond.TableNumber = client.PokerPlayer.Table.Number;
                ServerSockets.Packet stream = new ServerSockets.Packet(Respond.ToArray());
                Handler(client, stream);
            }
        }
        public static void Handler(Client.GameClient client, ServerSockets.Packet stream)
        {
            var packet = new byte[stream.Size];
            System.Runtime.InteropServices.Marshal.Copy(new IntPtr(stream.Memory), packet, 0, stream.Size);
            ushort ID = BitConverter.ToUInt16(packet, 2);
            Poker.Structures.PokerTable Table = null;
            if (client.PokerPlayer != null)
                Table = client.PokerPlayer.Table;
            if (ID == 2171)
            {
                MsgTexasInteractive Respond = new MsgTexasInteractive(packet);
                Table = Poker.Database.Tables[Respond.TableId];
            }
            if (Table == null)
                return;
            if (!Table.TableBusy)
            {
                lock (Table.TableSyncRoot)
                {

                    switch (ID)
                    {
                        #region Kick
                        case 2088:
                            {
                                byte Action = packet[4];
                                uint Target = System.BitConverter.ToUInt32(packet, 9);
                                switch (Action)
                                {
                                    case 0:
                                        {
                                            if (client.PokerPlayer != null)
                                            {
                                                if (!Table.InGame() && Table.CanInterface(client.Player.UID))
                                                {
                                                    if (Table.Time > DateTime.Now)
                                                    {
                                                        if (Table.Kick == null)
                                                        {
                                                            Table.Kick = new PokerStructs.Kick();
                                                            Table.Kick.Starter = client.Player.UID;
                                                            Table.Kick.Target = Target;
                                                            Table.Kick.Accept = new List<uint>();
                                                            Table.Kick.Refuse = new List<uint>();
                                                            Table.Kick.Refuse.AddRange(Table.Players.Keys.ToList());
                                                            Table.Kick.Total = (byte)Table.Players.Count;
                                                            if (Table.Kick.Refuse.Remove(client.Player.UID))
                                                            {
                                                                Table.Kick.Accept.Add(client.Player.UID);
                                                                uint Sec = (uint)new TimeSpan((Table.Time - DateTime.Now).Ticks).TotalSeconds;
                                                                Table.Kick.Time = DateTime.Now.AddSeconds(Sec);
                                                                foreach (var P in Table.PlayersOnTable())
                                                                {
                                                                    if (Pool.GamePoll.ContainsKey(P.Uid))
                                                                    {
                                                                        Pool.GamePoll[P.Uid].Send(Poker.Packets.General.Kick(Table.Kick, 1, Sec, 0));
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    case 2: //Accept
                                        {
                                            if (client.PokerPlayer != null)
                                            {
                                                byte Type = packet[17];
                                                if (!Table.InGame() && Table.CanInterface(client.Player.UID) && client.PokerPlayer.PlayerType == Poker.Enums.PlayerType.Player)
                                                {
                                                    if (Table.Kick != null)
                                                    {
                                                        if (Table.Kick.Time > DateTime.Now)
                                                        {
                                                            if (Type == 1)
                                                            {
                                                                if (Table.Kick.Refuse.Remove(client.Player.UID))
                                                                {
                                                                    Table.Kick.Accept.Add(client.Player.UID);
                                                                    foreach (var P in Table.PlayersOnTable())
                                                                    {
                                                                        if (Pool.GamePoll.ContainsKey(P.Uid))
                                                                        {
                                                                            Pool.GamePoll[P.Uid].Send(Poker.Packets.General.Kick(Table.Kick, 2, client.Player.UID, 1));
                                                                        }
                                                                    }
                                                                }
                                                                if (Table.Kick.Accept.Count > Table.Kick.Refuse.Count)
                                                                {
                                                                    foreach (var P in Table.PlayersOnTable())
                                                                    {
                                                                        if (Pool.GamePoll.ContainsKey(P.Uid))
                                                                        {
                                                                            Pool.GamePoll[P.Uid].Send(Poker.Packets.General.Kick(Table.Kick, 3, 0, 1));
                                                                        }
                                                                    }
                                                                    var C = Pool.GamePoll[Table.Kick.Target];
                                                                    if (C != null)
                                                                    {
                                                                        if (C.PokerPlayer != null)
                                                                        {
                                                                            Poker.Packets.MsgShowHandExit Respond = new Poker.Packets.MsgShowHandExit(null);
                                                                            Respond.Action = 1;
                                                                            Respond.PlayerUid = C.Player.UID;
                                                                            Respond.TableNumber = C.PokerPlayer.Table.Number;
                                                                            stream = new ServerSockets.Packet(Respond.ToArray());
                                                                            Handler(C, stream);
                                                                        }
                                                                    }
                                                                    Table.Kick = null;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                foreach (var P in Table.PlayersOnTable())
                                                                {
                                                                    if (Pool.GamePoll.ContainsKey(P.Uid))
                                                                    {
                                                                        Pool.GamePoll[P.Uid].Send(Poker.Packets.General.Kick(Table.Kick, 2, client.Player.UID, 0));
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                }
                                break;
                            }
                        #endregion
                        #region MsgTexasInteractive
                        case 2171:
                            {
                                MsgTexasInteractive Respond = new MsgTexasInteractive(packet);
                                switch (Respond.InteractiveType)
                                {
                                    case Poker.Enums.TableInteractiveType.Join:
                                        {
                                            if (!client.CanPlayPoker())
                                                return;
                                            if (Respond.PlayerUid == client.Player.UID)
                                            {
                                                if (client.PokerPlayer == null)
                                                {

                                                    if (Table.OnScreen != null)
                                                    {
                                                        if (!Table.OnScreen.ContainsKey(client.Player.UID))
                                                        { Table.OnScreen.TryAdd(client.Player.UID, client.Player.UID); }
                                                    }
                                                    if (Table != null)
                                                    {
                                                        if (!Table.CanInterface(client.Player.UID))
                                                            break;
                                                        ulong Money = 0;
                                                        if (Table.IsCPs == false)
                                                        {
                                                            Money = (ulong)client.Player.Money;
                                                        }
                                                        else if (Table.IsCPs == true)
                                                        {
                                                            Money = client.Player.ConquerPoints;
                                                        }
                                                        client.PokerPlayer = new PokerStructs.Player(client.Player.Name, client.Player.UID);
                                                        client.PokerPlayer.Create(Poker.Enums.PlayerType.Player, Respond.Seat, Table, Money);
                                                        if (Table.AddPlayer(client.PokerPlayer))
                                                        {
                                                            client.SendScreen(Respond.ToArray(), true);
                                                            var P1 = new MsgShowHandEnter(null).ToArray(1, client.PokerPlayer);
                                                            client.Send(P1);
                                                            foreach (var P in Table.PlayersOnTable())
                                                            {
                                                                client.Send(new MsgShowHandEnter(null).ToArray(1, P));
                                                                if (P.Uid != client.Player.UID)
                                                                {
                                                                    if (Pool.GamePoll.ContainsKey(P.Uid))
                                                                    {
                                                                        Pool.GamePoll[P.Uid].Send(P1);
                                                                    }
                                                                }
                                                            }
                                                            var P2 = Table.Update(Poker.Enums.TableUpdate.PlayerCount);
                                                            foreach (uint key in Table.OnScreen.Keys)
                                                            {
                                                                if (Pool.GamePoll.ContainsKey(key))
                                                                {
                                                                    Pool.GamePoll[key].Send(P2);
                                                                }
                                                            }
                                                            if (Table.InGame())
                                                            {
                                                                client.Send(Table.Update(Poker.Enums.TableUpdate.Chips));
                                                                client.Send(Poker.Packets.General.MsgShowHandState(Table));
                                                            }
                                                            client.Player.PokerTableID = client.PokerPlayer.Table.Id;
                                                            client.Player.PokerSeat = client.PokerPlayer.Seat;
                                                        }
                                                        else
                                                        {
                                                            client.Player.PokerTableID = 0;
                                                            client.Player.PokerSeat = 0;
                                                            client.PokerPlayer = null;
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    case Poker.Enums.TableInteractiveType.Watch:
                                        {
                                            if (Respond.PlayerUid == client.Player.UID)
                                            {
                                                if (client.PokerPlayer == null)
                                                {
                                                    if (Table.OnScreen != null)
                                                    {
                                                        if (!Table.OnScreen.ContainsKey(client.Player.UID))
                                                        { Table.OnScreen.TryAdd(client.Player.UID, client.Player.UID); }
                                                    }
                                                    if (Table != null)
                                                    {
                                                        if (!Table.CanInterface(client.Player.UID))
                                                            break;
                                                        ulong Money = 0;
                                                        if (Table.IsCPs == false)
                                                        {
                                                            Money = (ulong)client.Player.Money;
                                                        }
                                                        else if (Table.IsCPs == true)
                                                        {
                                                            Money = client.Player.ConquerPoints;
                                                        }
                                                        client.PokerPlayer = new PokerStructs.Player(client.Player.Name, client.Player.UID);
                                                        client.PokerPlayer.Create(Poker.Enums.PlayerType.Watcher, Respond.Seat, Table, Money);
                                                        if (Table.AddWatcher(client.PokerPlayer))
                                                        {
                                                            var P1 = new MsgShowHandEnter(null).ToArray(1, client.PokerPlayer);
                                                            client.Send(P1);
                                                            foreach (var P in Table.PlayersOnTable())
                                                            {
                                                                client.Send(new MsgShowHandEnter(null).ToArray(1, P));
                                                                if (P.Uid != client.Player.UID)
                                                                {
                                                                    if (Pool.GamePoll.ContainsKey(P.Uid))
                                                                    {
                                                                        Pool.GamePoll[P.Uid].Send(P1);
                                                                    }
                                                                }
                                                            }
                                                            if (Table.InGame())
                                                            {
                                                                client.Send(Table.Update(Poker.Enums.TableUpdate.Chips));
                                                                client.Send(Poker.Packets.General.MsgShowHandState(Table));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            client.PokerPlayer = null;
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            Console.WriteLine("Unhandle Action MsgTexasInteractive " + Respond.InteractiveType);
                                            break;
                                        }
                                }
                                break;
                            }
                        #endregion
                        #region MsgShowHandExit
                        case 2096:
                            {
                                MsgShowHandExit Respond = new MsgShowHandExit(packet);

                                switch (Respond.Action)
                                {
                                    case 1:
                                        {
                                            if (client.PokerPlayer != null)
                                            {
                                                if (Table != null)
                                                {
                                                    if (client.PokerPlayer.PlayerType == Poker.Enums.PlayerType.Player)
                                                    {
                                                        if (!Table.Players.ContainsKey(client.Player.UID))
                                                            break;
                                                        if (Table.PlayerLeave(client.PokerPlayer))
                                                        {
                                                            if (Table.TableType == Poker.Enums.TableType.TexasHoldem)
                                                            {
                                                                if (Table.SmallBlind == client.Player.UID)
                                                                {
                                                                    Table.SmallBlind = Table.NextSeat(client.PokerPlayer.Seat);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (Table.Dealer == client.Player.UID)
                                                                {
                                                                    Table.Dealer = Table.NextSeat(client.PokerPlayer.Seat);
                                                                }
                                                            }
                                                            var P1 = Respond.ToArray(1, client.PokerPlayer);
                                                            client.Send(P1);
                                                            client.SendScreen(new MsgTexasInteractive(null).ToArray(Poker.Enums.TableInteractiveType.Leave, client.PokerPlayer), true);
                                                            foreach (var P in Table.PlayersOnTable())
                                                            {
                                                                if (Pool.GamePoll.ContainsKey(P.Uid))
                                                                {
                                                                    Pool.GamePoll[P.Uid].Send(P1);
                                                                }
                                                            }
                                                            var P2 = Table.Update(Poker.Enums.TableUpdate.PlayerCount);
                                                            foreach (uint key in Table.OnScreen.Keys)
                                                            {
                                                                if (Pool.GamePoll.ContainsKey(key))
                                                                {
                                                                    Pool.GamePoll[key].Send(P2);
                                                                }
                                                            }
                                                            if (Table.InGame())
                                                            {
                                                                if (Table.CurrentPlayer == client.Player.UID)
                                                                {
                                                                    if (client.PokerPlayer.Table.Next(false))
                                                                    {
                                                                        var P3 = Poker.Packets.General.MsgShowHandActivePlayer(client.PokerPlayer.Table, 10, client.PokerPlayer.Table.CurrentPlayer);
                                                                        foreach (var p in client.PokerPlayer.Table.PlayersOnTable())
                                                                        {
                                                                            if (Pool.GamePoll.ContainsKey(p.Uid))
                                                                            {
                                                                                var c = Pool.GamePoll[p.Uid];
                                                                                c.Send(P3);
                                                                            }
                                                                        }
                                                                        client.PokerPlayer.Table.Time = DateTime.Now.AddSeconds(10);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    Table.NextRound();
                                                                }
                                                            }
                                                            client.PokerPlayer = null;
                                                            client.Player.PokerTableID = 0;
                                                            client.Player.PokerSeat = 0;
                                                        }

                                                    }
                                                    else if (client.PokerPlayer.PlayerType == Poker.Enums.PlayerType.Watcher)
                                                    {
                                                        if (!Table.Watchers.ContainsKey(client.Player.UID))
                                                            return;
                                                        if (Table.WatcherLeave(client.PokerPlayer))
                                                        {
                                                            var P1 = Respond.ToArray(1, client.PokerPlayer);
                                                            client.Send(P1);
                                                            foreach (var P in Table.PlayersOnTable())
                                                            {
                                                                if (Pool.GamePoll.ContainsKey(P.Uid))
                                                                {
                                                                    Pool.GamePoll[P.Uid].Send(P1);
                                                                }
                                                            }
                                                            client.PokerPlayer = null;
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            Console.WriteLine("Unhandle Action MsgShowHandExit " + Respond.Action);
                                            break;
                                        }
                                }
                                break;
                            }
                        #endregion
                        #region MsgShowHandEnter
                        case 2090:
                            {
                                MsgShowHandEnter Respond = new MsgShowHandEnter(packet);
                                switch (Respond.Action)
                                {
                                    case 0:
                                        {
                                            if (client.PokerPlayer != null)
                                            {
                                                if (!client.CanPlayPoker())
                                                    return;
                                                if (client.PokerPlayer.PlayerType == Poker.Enums.PlayerType.Watcher)
                                                {
                                                    if (client.PokerPlayer.Table.Number == Respond.TableNumber)
                                                    {
                                                        ulong Money = 0;
                                                        if (client.PokerPlayer.Table.IsCPs == false)
                                                        {
                                                            Money = (ulong)client.Player.Money;
                                                        }
                                                        else if (client.PokerPlayer.Table.IsCPs == true)
                                                        {
                                                            Money = client.Player.ConquerPoints;
                                                        }
                                                        client.PokerPlayer.Create(Poker.Enums.PlayerType.Player, (byte)Respond.Seat, client.PokerPlayer.Table, Money);
                                                        if (client.PokerPlayer.Table.AddPlayer(client.PokerPlayer))
                                                        {
                                                            client.SendScreen(new MsgTexasInteractive(null).ToArray(Poker.Enums.TableInteractiveType.Join, client.PokerPlayer), true);
                                                            client.Send(Respond.ToArray(1, client.PokerPlayer));
                                                            foreach (var P in client.PokerPlayer.Table.PlayersOnTable())
                                                            {
                                                                client.Send(Respond.ToArray(1, P));
                                                                if (P.Uid != client.Player.UID)
                                                                {
                                                                    if (Pool.GamePoll.ContainsKey(P.Uid))
                                                                    {
                                                                        Pool.GamePoll[P.Uid].Send(Respond.ToArray(1, client.PokerPlayer));
                                                                    }
                                                                }
                                                            }
                                                            var P2 = client.PokerPlayer.Table.Update(Poker.Enums.TableUpdate.PlayerCount);
                                                            foreach (uint key in client.PokerPlayer.Table.OnScreen.Keys)
                                                            {
                                                                if (Pool.GamePoll.ContainsKey(key))
                                                                {
                                                                    Pool.GamePoll[key].Send(P2);
                                                                }
                                                            }
                                                            client.Player.PokerTableID = client.PokerPlayer.Table.Id;
                                                            client.Player.PokerSeat = client.PokerPlayer.Seat;
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            Console.WriteLine("Unhandle Action MsgShowHandEnter " + Respond.Action);
                                            break;
                                        }
                                }
                                break;
                            }
                        #endregion
                        #region MsgShowHandCallAction
                        case 2093:
                            {
                                Poker.Packets.MsgShowHandCallAction Respond = new Poker.Packets.MsgShowHandCallAction(packet);
                                if (client.PokerPlayer == null)
                                    return;
                                Respond.Uid = client.PokerPlayer.Uid;
                                if (!Table.Players.ContainsKey(client.Player.UID))
                                    return;
                                if (Table.CurrentPlayer != client.Player.UID)
                                    return;
                                switch (Respond.Action)
                                {
                                    case 1://Bet
                                        {
                                            if (!Table.UnLimited)
                                            {
                                                if (Table.NumberOfRaise == 0)
                                                {
                                                    if (Table.State == Poker.Enums.TableState.Pocket || Table.State == Poker.Enums.TableState.Flop)
                                                    {
                                                        Respond.RoundPot = Table.MinBet;
                                                    }
                                                    else
                                                    {
                                                        Respond.RoundPot = Table.MinBet * 2;
                                                    }
                                                }
                                                else
                                                {
                                                    if (Table.State == Poker.Enums.TableState.Pocket)
                                                    {
                                                        Respond.RoundPot = Table.MinBet;
                                                    }
                                                    else
                                                    {
                                                        Respond.RoundPot = Table.MinBet * 2;
                                                    }
                                                }
                                            }
                                            if (Table.IsCPs ? client.Player.ConquerPoints >= Respond.RoundPot : client.Player.Money >= (long)Respond.RoundPot)
                                            {
                                                if (Table.IsCPs)
                                                {
                                                    client.Player.ConquerPoints -= (uint)(Respond.RoundPot);
                                                }
                                                else
                                                {
                                                    client.Player.Money -= (uint)(Respond.RoundPot);
                                                }
                                                client.PokerPlayer.Decrement(Respond.RoundPot);
                                                client.PokerPlayer.PotinThisRound = true;
                                                var P1 = Respond.ToArray(client.PokerPlayer.RoundPot, client.PokerPlayer.TotalPot);
                                                foreach (var p in Table.PlayersOnTable())
                                                {
                                                    if (Pool.GamePoll.ContainsKey(p.Uid))
                                                    {
                                                        var c = Pool.GamePoll[p.Uid];
                                                        c.Send(P1);
                                                    }
                                                }
                                                var P2 = Table.Update(Poker.Enums.TableUpdate.Chips);
                                                foreach (var p in Table.OnScreen)
                                                {
                                                    if (Pool.GamePoll.ContainsKey(p.Key))
                                                    {
                                                        var c = Pool.GamePoll[p.Key];
                                                        c.Send(P2);
                                                    }
                                                }
                                                if (Table.Next(true))
                                                {
                                                    var P3 = Poker.Packets.General.MsgShowHandActivePlayer(Table, 10, Table.CurrentPlayer);
                                                    foreach (var p in Table.PlayersOnTable())
                                                    {
                                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                                        {
                                                            var c = Pool.GamePoll[p.Uid];
                                                            c.Send(P3);
                                                        }
                                                    }
                                                    Table.Time = DateTime.Now.AddSeconds(10);
                                                }
                                            }
                                            break;
                                        }
                                    //case 2://Call
                                    //    {

                                    //        if (Table.IsCPs ? client.Player.ConquerPoints >= Table.RequiredPot : client.Player.Money >= (long)Table.RequiredPot)
                                    //        {
                                    //            if (Table.IsCPs)
                                    //            {
                                    //                client.Player.ConquerPoints -= (uint)(Table.RequiredPot);
                                    //            }
                                    //            else
                                    //            {
                                    //                client.Player.Money -= (uint)(Table.RequiredPot);
                                    //            }
                                    //            var myTimer = new System.Timers.Timer();
                                    //            client.PokerPlayer.Decrement(Table.RequiredPot);
                                    //            client.PokerPlayer.PotinThisRound = true;
                                    //            var P1 = Respond.ToArray(client.PokerPlayer.RoundPot, client.PokerPlayer.TotalPot);
                                    //            foreach (var p in Table.PlayersOnTable())
                                    //            {
                                    //                if (GamePoll.ContainsKey(p.Uid))
                                    //                {
                                    //                    var c = GamePoll[p.Uid];
                                    //                    c.Send(P1);
                                    //                }
                                    //            }
                                    //            var P2 = Table.Update(Poker.Enums.TableUpdate.Chips);
                                    //            foreach (var p in Table.OnScreen)
                                    //            {
                                    //                if (GamePoll.ContainsKey(p.Key))
                                    //                {
                                    //                    var c = GamePoll[p.Key];
                                    //                    c.Send(P2);
                                    //                }
                                    //            }
                                    //            if (Table.Next(true))
                                    //            {
                                    //                var P3 = Poker.Packets.General.MsgShowHandActivePlayer(Table, 10, Table.CurrentPlayer);
                                    //                if (Table.RoundPot != 0)
                                    //                {
                                    //                    foreach (var p in Table.PlayersOnTable())
                                    //                    {
                                    //                        if (GamePoll.ContainsKey(p.Uid))
                                    //                        {
                                    //                            var c = GamePoll[p.Uid];
                                    //                            c.Send(P3);
                                    //                        }
                                    //                    }
                                    //                }
                                    //                Table.Time = DateTime.Now.AddSeconds(10);
                                    //                myTimer.Elapsed += (o, ea) =>
                                    //                {
                                    //                    foreach (var p in Table.PlayersOnTable())
                                    //                    {
                                    //                        if (GamePoll.ContainsKey(p.Uid))
                                    //                        {
                                    //                            var c = GamePoll[p.Uid];
                                    //                            c.Send(P3);
                                    //                        }
                                    //                    }
                                    //                    Table.Time = DateTime.Now.AddSeconds(10);
                                    //                    myTimer.Stop();
                                    //                };
                                    //                if (Table.RoundPot == 0)
                                    //                {
                                    //                    myTimer.Interval = 2000;
                                    //                    myTimer.Start();
                                    //                }
                                    //            }
                                    //        }
                                    //        break;
                                    //    }
                                    case 4://Fold
                                        {
                                            if (Table.TableType == Poker.Enums.TableType.TexasHoldem)
                                            {
                                                if (Table.SmallBlind == client.Player.UID)
                                                {
                                                    Table.SmallBlind = Table.NextSeat(client.PokerPlayer.Seat);
                                                }
                                            }
                                            else
                                            {
                                                if (Table.Dealer == client.Player.UID)
                                                {
                                                    Table.Dealer = Table.NextSeat(client.PokerPlayer.Seat);
                                                }
                                            }
                                            client.PokerPlayer.PotinThisRound = true;
                                            client.PokerPlayer.Fold = true;
                                            client.PokerPlayer.RoundPot = 0;
                                            var P1 = Respond.ToArray(client.PokerPlayer.RoundPot, client.PokerPlayer.TotalPot);
                                            foreach (var p in Table.PlayersOnTable())
                                            {
                                                if (Pool.GamePoll.ContainsKey(p.Uid))
                                                {
                                                    var c = Pool.GamePoll[p.Uid];
                                                    c.Send(P1);
                                                }
                                            }
                                            var P2 = Table.Update(Poker.Enums.TableUpdate.Chips);
                                            foreach (var p in Table.OnScreen)
                                            {
                                                if (Pool.GamePoll.ContainsKey(p.Key))
                                                {
                                                    var c = Pool.GamePoll[p.Key];
                                                    c.Send(P2);
                                                }
                                            }
                                            if (Table.Next(false))
                                            {
                                                var P3 = Poker.Packets.General.MsgShowHandActivePlayer(Table, 10, Table.CurrentPlayer);
                                                foreach (var p in Table.PlayersOnTable())
                                                {
                                                    if (Pool.GamePoll.ContainsKey(p.Uid))
                                                    {
                                                        var c = Pool.GamePoll[p.Uid];
                                                        c.Send(P3);
                                                    }
                                                }
                                                Table.Time = DateTime.Now.AddSeconds(10);
                                            }
                                            break;
                                        }
                                    case 8://Check
                                        {
                                            client.PokerPlayer.PotinThisRound = true;
                                            var P1 = Respond.ToArray(client.PokerPlayer.RoundPot, client.PokerPlayer.TotalPot);
                                            foreach (var p in Table.PlayersOnTable())
                                            {
                                                if (Pool.GamePoll.ContainsKey(p.Uid))
                                                {
                                                    var c = Pool.GamePoll[p.Uid];
                                                    c.Send(P1);
                                                }
                                            }
                                            var P2 = Table.Update(Poker.Enums.TableUpdate.Chips);

                                            foreach (var p in Table.OnScreen)
                                            {
                                                if (Pool.GamePoll.ContainsKey(p.Key))
                                                {
                                                    var c = Pool.GamePoll[p.Key];
                                                    c.Send(P2);
                                                }
                                            }
                                            var myTimer = new System.Timers.Timer();
                                            if (Table.Next(true))
                                            {
                                                var P3 = Poker.Packets.General.MsgShowHandActivePlayer(Table, 10, Table.CurrentPlayer);
                                                Table.Time = DateTime.Now.AddSeconds(10);
                                                myTimer.Elapsed += (o, ea) =>
                                                {
                                                    foreach (var p in Table.PlayersOnTable())
                                                    {
                                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                                        {
                                                            var c = Pool.GamePoll[p.Uid];
                                                            c.Send(P3);
                                                        }
                                                    }
                                                    Table.Time = DateTime.Now.AddSeconds(10);
                                                    myTimer.Stop();
                                                };
                                                myTimer.Interval = 750;
                                                myTimer.Start();
                                            }
                                            break;
                                        }
                                    case 2://Raise
                                        {
                                            if (!Table.UnLimited)
                                            {
                                                Respond.RoundPot = Table.MinBet;
                                                if (Table.NumberOfRaise == 3)
                                                { return; }
                                            }
                                            if (Table.IsCPs ? client.Player.ConquerPoints >= Table.RequiredPot + Respond.RoundPot : client.Player.Money >= (long)Table.RequiredPot + (long)Respond.RoundPot)
                                            {
                                                if (!Table.UnLimited)
                                                {
                                                    Table.NumberOfRaise++;
                                                }
                                                if (Table.IsCPs)
                                                {
                                                    client.Player.ConquerPoints -= (uint)(Table.RequiredPot + Respond.RoundPot);
                                                }
                                                else
                                                {
                                                    client.Player.Money -= (uint)(Table.RequiredPot + Respond.RoundPot);
                                                }
                                                client.PokerPlayer.Decrement(Table.RequiredPot + Respond.RoundPot);
                                                client.PokerPlayer.PotinThisRound = true;
                                                var P1 = Respond.ToArray(client.PokerPlayer.RoundPot, client.PokerPlayer.TotalPot);
                                                foreach (var p in Table.PlayersOnTable())
                                                {
                                                    if (Pool.GamePoll.ContainsKey(p.Uid))
                                                    {
                                                        var c = Pool.GamePoll[p.Uid];
                                                        c.Send(P1);
                                                    }
                                                }
                                                var P2 = Table.Update(Poker.Enums.TableUpdate.Chips);
                                                foreach (var p in Table.OnScreen)
                                                {
                                                    if (Pool.GamePoll.ContainsKey(p.Key))
                                                    {
                                                        var c = Pool.GamePoll[p.Key];
                                                        c.Send(P2);
                                                    }
                                                }
                                                if (Table.Next(true))
                                                {
                                                    var P3 = Poker.Packets.General.MsgShowHandActivePlayer(Table, 10, Table.CurrentPlayer);
                                                    foreach (var p in Table.PlayersOnTable())
                                                    {
                                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                                        {
                                                            var c = Pool.GamePoll[p.Uid];
                                                            c.Send(P3);
                                                        }
                                                    }
                                                    Table.Time = DateTime.Now.AddSeconds(10);
                                                }
                                            }
                                            break;
                                        }
                                    case 32://Allin
                                        {
                                            if (Table.TableType == Poker.Enums.TableType.TexasHoldem)
                                            {
                                                if (Table.IsCPs)
                                                {
                                                    Respond.RoundPot = client.Player.ConquerPoints;
                                                    client.Player.ConquerPoints = 0;
                                                }
                                                else
                                                {
                                                    Respond.RoundPot = (ulong)client.Player.Money;
                                                    client.Player.Money = 0;
                                                }
                                                if (Table.TableType == Poker.Enums.TableType.TexasHoldem)
                                                {
                                                    if (Table.SmallBlind == client.Player.UID)
                                                    {
                                                        Table.SmallBlind = Table.NextSeat(client.PokerPlayer.Seat);
                                                    }
                                                }
                                                else
                                                {
                                                    if (Table.Dealer == client.Player.UID)
                                                    {
                                                        Table.Dealer = Table.NextSeat(client.PokerPlayer.Seat);
                                                    }
                                                }
                                                client.PokerPlayer.Decrement(Respond.RoundPot);
                                                client.PokerPlayer.PotinThisRound = true;
                                                client.PokerPlayer.IsPotAllin = true;
                                                var P1 = Respond.ToArray(client.PokerPlayer.RoundPot, client.PokerPlayer.TotalPot);
                                                foreach (var p in Table.PlayersOnTable())
                                                {
                                                    if (Pool.GamePoll.ContainsKey(p.Uid))
                                                    {
                                                        var c = Pool.GamePoll[p.Uid];
                                                        c.Send(P1);
                                                    }
                                                }
                                                var P2 = Table.Update(Poker.Enums.TableUpdate.Chips);
                                                foreach (var p in Table.OnScreen)
                                                {
                                                    if (Pool.GamePoll.ContainsKey(p.Key))
                                                    {
                                                        var c = Pool.GamePoll[p.Key];
                                                        c.Send(P2);
                                                    }
                                                }
                                                bool H = false;
                                                if (Table.HighestBet(client.PokerPlayer.Uid, client.PokerPlayer.RoundPot))
                                                {
                                                    H = Table.Next(true);
                                                }
                                                else
                                                {
                                                    H = Table.Next(false);
                                                }
                                                if (H)
                                                {
                                                    var P3 = Poker.Packets.General.MsgShowHandActivePlayer(Table, 10, Table.CurrentPlayer);
                                                    foreach (var p in Table.PlayersOnTable())
                                                    {
                                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                                        {
                                                            var c = Pool.GamePoll[p.Uid];
                                                            c.Send(P3);
                                                        }
                                                    }
                                                    Table.Time = DateTime.Now.AddSeconds(10);
                                                }
                                            }
                                            else
                                            {
                                                if (Table.Showhand == false)
                                                {
                                                    if (client.Player.Money > (long)Table.LowestMoney + (long)Table.RequiredPot)
                                                    {
                                                        Respond.RoundPot = (ulong)Table.LowestMoney + Table.RequiredPot;
                                                        client.Player.Money -= (uint)Table.LowestMoney + (uint)Table.RequiredPot;
                                                    }
                                                    else
                                                    {
                                                        Respond.RoundPot = (ulong)Table.LowestMoney;
                                                        client.Player.Money -= (uint)Table.LowestMoney;
                                                    }
                                                    if (Table.Dealer == client.Player.UID)
                                                    {
                                                        Table.Dealer = Table.NextSeat(client.PokerPlayer.Seat);
                                                    }
                                                    client.PokerPlayer.Decrement(Respond.RoundPot);
                                                    client.PokerPlayer.PotinThisRound = true;
                                                    client.PokerPlayer.IsPotAllin = true;
                                                    Table.Showhand = true;
                                                    Table.ShowhandTotalPot = (long)client.PokerPlayer.TotalPot;
                                                    var P1 = Respond.ToArray(client.PokerPlayer.RoundPot, client.PokerPlayer.TotalPot);
                                                    foreach (var p in Table.PlayersOnTable())
                                                    {
                                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                                        {
                                                            var c = Pool.GamePoll[p.Uid];
                                                            c.Send(P1);
                                                        }
                                                    }
                                                    var P2 = Table.Update(Poker.Enums.TableUpdate.Chips);
                                                    foreach (var p in Table.OnScreen)
                                                    {
                                                        if (Pool.GamePoll.ContainsKey(p.Key))
                                                        {
                                                            var c = Pool.GamePoll[p.Key];
                                                            c.Send(P2);
                                                        }
                                                    }
                                                    bool H = false;
                                                    if (Table.HighestBet(client.PokerPlayer.Uid, client.PokerPlayer.RoundPot))
                                                    {
                                                        H = Table.Next(true);
                                                    }
                                                    else
                                                    {
                                                        H = Table.Next(false);
                                                    }
                                                    if (H)
                                                    {
                                                        var P3 = Poker.Packets.General.MsgShowHandActivePlayer(Table, 10, Table.CurrentPlayer);
                                                        foreach (var p in Table.PlayersOnTable())
                                                        {
                                                            if (Pool.GamePoll.ContainsKey(p.Uid))
                                                            {
                                                                var c = Pool.GamePoll[p.Uid];
                                                                c.Send(P3);
                                                            }
                                                        }
                                                        Table.Time = DateTime.Now.AddSeconds(10);
                                                    }
                                                }
                                                else
                                                {
                                                    Respond.RoundPot = (ulong)Table.RequiredPot;
                                                    client.Player.Money -= (uint)Table.RequiredPot;
                                                    if (Table.Dealer == client.Player.UID)
                                                    {
                                                        Table.Dealer = Table.NextSeat(client.PokerPlayer.Seat);
                                                    }
                                                    client.PokerPlayer.Decrement(Respond.RoundPot);
                                                    client.PokerPlayer.PotinThisRound = true;
                                                    client.PokerPlayer.IsPotAllin = true;
                                                    var P1 = Respond.ToArray(client.PokerPlayer.RoundPot, client.PokerPlayer.TotalPot);
                                                    foreach (var p in Table.PlayersOnTable())
                                                    {
                                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                                        {
                                                            var c = Pool.GamePoll[p.Uid];
                                                            c.Send(P1);
                                                        }
                                                    }
                                                    var P2 = Table.Update(Poker.Enums.TableUpdate.Chips);
                                                    foreach (var p in Table.OnScreen)
                                                    {
                                                        if (Pool.GamePoll.ContainsKey(p.Key))
                                                        {
                                                            var c = Pool.GamePoll[p.Key];
                                                            c.Send(P2);
                                                        }
                                                    }
                                                    bool H = false;
                                                    if (Table.HighestBet(client.PokerPlayer.Uid, client.PokerPlayer.RoundPot))
                                                    {
                                                        H = Table.Next(true);
                                                    }
                                                    else
                                                    {
                                                        H = Table.Next(false);
                                                    }
                                                    if (H)
                                                    {
                                                        var P3 = Poker.Packets.General.MsgShowHandActivePlayer(Table, 10, Table.CurrentPlayer);
                                                        foreach (var p in Table.PlayersOnTable())
                                                        {
                                                            if (Pool.GamePoll.ContainsKey(p.Uid))
                                                            {
                                                                var c = Pool.GamePoll[p.Uid];
                                                                c.Send(P3);
                                                            }
                                                        }
                                                        Table.Time = DateTime.Now.AddSeconds(10);
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                }
                                break;
                            }
                        #endregion
                    }
                }
            }
        }
        public static void ShowHand(PokerTable Table)
        {
            switch (Table.State)
            {
                #region Unopened
                case Poker.Enums.TableState.Unopened:
                    {
                        if (Table.Time < Table.ThreadTime)
                        {
                            #region Kick
                            if (Table.Kick != null)
                            {
                                try
                                {
                                    if (Table.Kick.Accept.Count > Table.Kick.Refuse.Count)
                                    {
                                        var P1 = Poker.Packets.General.Kick(Table.Kick, 3, 0, 2);
                                        foreach (var P in Table.PlayersOnTable())
                                        {
                                            if (Pool.GamePoll.ContainsKey(P.Uid))
                                            {
                                                Pool.GamePoll[P.Uid].Send(P1);
                                            }
                                        }
                                        var C = Pool.GamePoll[Table.Kick.Target];
                                        if (Table.Players.ContainsKey(Table.Kick.Target))
                                        {
                                            if (C != null)
                                            {
                                                if (C.PokerPlayer != null)
                                                {
                                                    Poker.Packets.MsgShowHandExit Respond = new Poker.Packets.MsgShowHandExit(null);
                                                    Respond.Action = 1;
                                                    Respond.PlayerUid = C.Player.UID;
                                                    Respond.TableNumber = C.PokerPlayer.Table.Number;
                                                    ServerSockets.Packet stream = new ServerSockets.Packet(Respond.ToArray());
                                                    Handler(C, stream);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var P1 = Poker.Packets.General.Kick(Table.Kick, 4, 0, 2);
                                        foreach (var P in Table.PlayersOnTable())
                                        {
                                            Pool.GamePoll[P.Uid].Send(P1);
                                        }
                                    }
                                    Table.Kick = null;
                                }
                                catch
                                {
                                    Table.Kick = null;
                                }
                            }
                            #endregion
                        }
                        if (Table.PlayerCount > 1)
                        {
                            if (Table.Time < Table.ThreadTime)
                            {
                                Table.StartNewRound();
                                foreach (var p in Table.Players.Values.Where(p => p.IsPlaying == true))
                                {
                                    if (Pool.GamePoll.ContainsKey(p.Uid))
                                    {
                                        var client = Pool.GamePoll[p.Uid];
                                        if (Table.IsCPs)
                                        {
                                            client.Player.ConquerPoints -= Table.MinBet / 2;
                                        }
                                        else
                                        {
                                            client.Player.Money -= Table.MinBet / 2;
                                        }
                                    }
                                }
                                var P1 = Table.Update(Poker.Enums.TableUpdate.Chips);
                                foreach (var p in Table.OnScreen)
                                {
                                    if (Pool.GamePoll.ContainsKey(p.Key))
                                    {
                                        var client = Pool.GamePoll[p.Key];
                                        client.Send(P1);
                                    }
                                }
                                if (Table.TableIsChange == true)
                                {
                                    Table.TableIsChange = false;
                                }
                                Table.RoundState = 0;
                                Table.StartPocket();

                            }
                        }
                        break;
                    }
                #endregion
                #region Poket
                case Poker.Enums.TableState.Pocket:
                    {
                        if (Table.PlayerCount > 1)
                        {
                            if (Table.Time < Table.ThreadTime)
                            {
                                if (Table.RoundState == 0)
                                {
                                    var P1 = Table.Update(Poker.Enums.TableUpdate.Statue);
                                    foreach (var p in Table.OnScreen)
                                    {
                                        if (Pool.GamePoll.ContainsKey(p.Key))
                                        {
                                            var client = Pool.GamePoll[p.Key];
                                            client.Send(P1);
                                        }
                                    }
                                    var p2 = Poker.Packets.General.MsgShowHandDealtCard(Table, 1, Poker.Packets.General.HandDealtCard.CardUp, 0);
                                    foreach (var p in Table.PlayersOnTable())
                                    {
                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                        {
                                            var client = Pool.GamePoll[p.Uid];
                                            client.Send(Poker.Packets.General.MsgShowHandDealtCard(Table, 0, Poker.Packets.General.HandDealtCard.CardDown, client.Player.UID));
                                            client.Send(p2);
                                        }
                                    }
                                    Table.Time = DateTime.Now.AddSeconds(Table.Players.Values.Where(p => p.IsPlaying).ToList().Count * 1.5);
                                    Table.RoundState = 1;
                                }
                                else if (Table.RoundState == 1)
                                {
                                    var P1 = Poker.Packets.General.MsgShowHandActivePlayer(Table, 10, Table.CurrentPlayer);
                                    foreach (var p in Table.PlayersOnTable())
                                    {
                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                        {
                                            var client = Pool.GamePoll[p.Uid];
                                            client.Send(P1);
                                        }
                                    }
                                    Table.RoundState = 2;
                                    Table.Time = DateTime.Now.AddSeconds(10);
                                }
                                else if (Table.RoundState == 2)
                                {
                                    var player = Table.Players[Table.CurrentPlayer];
                                    Poker.Packets.MsgShowHandCallAction Respond = new Poker.Packets.MsgShowHandCallAction(null);
                                    Respond.Uid = player.Uid;
                                    Respond.Action = 4;
                                    player.PotinThisRound = true;
                                    player.Fold = true;
                                    player.RoundPot = 0;
                                    if (Table.Dealer == player.Uid)
                                    {
                                        Table.Dealer = Table.NextSeat(player.Seat);
                                    }
                                    var P1 = Respond.ToArray(player.RoundPot, player.TotalPot);
                                    foreach (var p in Table.PlayersOnTable())
                                    {
                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                        {
                                            var c = Pool.GamePoll[p.Uid];
                                            c.Send(P1);
                                        }
                                    }
                                    if (Table.Next(false))
                                    {
                                        var P2 = Poker.Packets.General.MsgShowHandActivePlayer(Table, 10, Table.CurrentPlayer);
                                        foreach (var p in Table.PlayersOnTable())
                                        {
                                            if (Pool.GamePoll.ContainsKey(p.Uid))
                                            {
                                                var client = Pool.GamePoll[p.Uid];
                                                client.Send(P2);
                                            }
                                        }
                                        Table.Time = DateTime.Now.AddSeconds(10);
                                    }
                                }
                            }
                        }
                        break;
                    }
                #endregion
                #region Flop<Turn<River
                case Poker.Enums.TableState.Flop:
                case Poker.Enums.TableState.Turn:
                case Poker.Enums.TableState.River:
                    {
                        if (Table.PlayerCount > 1)
                        {
                            if (Table.RoundState == 0)
                            {
                                var P1 = Table.Update(Poker.Enums.TableUpdate.Statue);
                                foreach (var p in Table.OnScreen)
                                {
                                    if (Pool.GamePoll.ContainsKey(p.Key))
                                    {
                                        var client = Pool.GamePoll[p.Key];
                                        client.Send(P1);
                                    }
                                }
                                var P2 = Table.State == Poker.Enums.TableState.Flop ? Poker.Packets.General.MsgShowHandDealtCard(Table, 1, Poker.Packets.General.HandDealtCard.CardUp, 0) : Table.State == Poker.Enums.TableState.Turn ? Poker.Packets.General.MsgShowHandDealtCard(Table, 1, Poker.Packets.General.HandDealtCard.CardUp, 0) : Table.State == Poker.Enums.TableState.River ? Poker.Packets.General.MsgShowHandDealtCard(Table, 1, Poker.Packets.General.HandDealtCard.CardUp, 0) : null;
                                foreach (var p in Table.PlayersOnTable())
                                {
                                    if (Pool.GamePoll.ContainsKey(p.Uid))
                                    {
                                        var client = Pool.GamePoll[p.Uid];
                                        client.Send(P2);
                                    }
                                }
                                Table.RoundState = 1;
                            }
                            if (Table.Time < Table.ThreadTime)
                            {
                                if (Table.RoundState == 1)
                                {
                                    var player = Table.Players[Table.CurrentPlayer];
                                    Poker.Packets.MsgShowHandCallAction Respond = new Poker.Packets.MsgShowHandCallAction(null);
                                    Respond.Uid = player.Uid;
                                    Respond.Action = 4;
                                    player.PotinThisRound = true;
                                    player.Fold = true;
                                    player.RoundPot = 0;
                                    if (Table.Dealer == player.Uid)
                                    {
                                        Table.Dealer = Table.NextSeat(player.Seat);
                                    }
                                    var P1 = Respond.ToArray(player.RoundPot, player.TotalPot);
                                    foreach (var p in Table.PlayersOnTable())
                                    {
                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                        {
                                            var c = Pool.GamePoll[p.Uid];
                                            c.Send(P1);
                                        }
                                    }
                                    if (Table.Next(false))
                                    {
                                        var P2 = Poker.Packets.General.MsgShowHandActivePlayer(Table, 10, Table.CurrentPlayer);
                                        foreach (var p in Table.PlayersOnTable())
                                        {
                                            if (Pool.GamePoll.ContainsKey(p.Uid))
                                            {
                                                var client = Pool.GamePoll[p.Uid];
                                                client.Send(P2);
                                            }
                                        }
                                        Table.Time = DateTime.Now.AddSeconds(10);
                                    }
                                }
                            }
                        }
                        break;
                    }
                #endregion
                #region ShowDown
                case Poker.Enums.TableState.ShowDown:
                    {
                        byte[] P1x = new byte[0];
                        byte[] P2x = new byte[0];
                        byte[] P3x = new byte[0];
                        if (Table.RoundState == 0)
                        {
                            var P0 = Table.Update(Poker.Enums.TableUpdate.Statue);
                            foreach (var p in Table.OnScreen)
                            {
                                if (Pool.GamePoll.ContainsKey(p.Key))
                                {
                                    var client = Pool.GamePoll[p.Key];
                                    client.Send(P0);
                                }
                            }
                            if (Table.PreviousState > 0)
                            {
                                if (Table.PreviousState < Poker.Enums.TableState.Flop)
                                {
                                    P3x = Poker.Packets.General.MsgShowHandDealtCard(Table, 3, Poker.Packets.General.HandDealtCard.CardUp, 0);
                                }
                                if (Table.PreviousState == Poker.Enums.TableState.Flop)
                                {
                                    P2x = Poker.Packets.General.MsgShowHandDealtCard(Table, 2, Poker.Packets.General.HandDealtCard.CardUp, 0);
                                }
                                if (Table.PreviousState == Poker.Enums.TableState.Turn)
                                {
                                    P1x = Poker.Packets.General.MsgShowHandDealtCard(Table, 1, Poker.Packets.General.HandDealtCard.CardUp, 0);
                                }

                            }
                            Table.RoundState = 1;
                        }
                        if (Table.RoundState == 1)
                        {
                            try
                            {
                                Table.GetWinners();
                                Table.RoundState = 2;
                                var Result = Poker.Packets.General.MsgShowHandGameResult(Table);
                                var ShowCards = Poker.Packets.General.MsgShowHandLayCard(Table);
                                Table.TotalPot = 0;
                                var P1 = Table.Update(Poker.Enums.TableUpdate.Chips);
                                foreach (var p in Table.OnScreen)
                                {
                                    if (Pool.GamePoll.ContainsKey(p.Key))
                                    {
                                        var client = Pool.GamePoll[p.Key];
                                        client.Send(P1);
                                    }
                                }
                                if (Table.PreviousState > 0)
                                {
                                    foreach (var p in Table.PlayersOnTable())
                                    {
                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                        {
                                            var client = Pool.GamePoll[p.Uid];
                                            if (Table.PreviousState < Poker.Enums.TableState.Flop)
                                            {
                                                client.Send(P3x);
                                            }
                                            if (Table.PreviousState == Poker.Enums.TableState.Flop)
                                            {
                                                client.Send(P2x);
                                            }
                                            if (Table.PreviousState == Poker.Enums.TableState.Turn)
                                            {
                                                client.Send(P1x);
                                            }
                                        }
                                    }
                                }
                                foreach (var p in Table.PlayersOnTable())
                                {
                                    if (Pool.GamePoll.ContainsKey(p.Uid))
                                    {
                                        var client = Pool.GamePoll[p.Uid];
                                        client.Send(ShowCards);
                                        client.Send(Result);
                                        if (client.PokerPlayer.PlayerType == Poker.Enums.PlayerType.Player)
                                        {
                                            if (Table.IsCPs)
                                            {
                                                client.Player.ConquerPoints = (uint)p.CurrentMoney;
                                                if (client.Player.ConquerPoints < Table.MinBet * 10)
                                                {
                                                    if (Table.PlayerLeave(client.PokerPlayer))
                                                    {
                                                        var P3 = new Poker.Packets.MsgTexasInteractive(null).ToArray(Poker.Enums.TableInteractiveType.Leave, client.PokerPlayer);
                                                        foreach (var px in Table.PlayersOnTable())
                                                        {
                                                            if (Pool.GamePoll.ContainsKey(px.Uid))
                                                            {
                                                                var c = Pool.GamePoll[px.Uid];
                                                                c.Send(P3);
                                                            }
                                                        }
                                                        client.SendScreen(P3, true);
                                                        client.PokerPlayer.Create(Poker.Enums.PlayerType.Watcher, client.PokerPlayer.Seat, Table, (ulong)client.PokerPlayer.CurrentMoney);
                                                        Table.AddWatcher(client.PokerPlayer);
                                                        client.Player.PokerTableID = 0;
                                                        client.Player.PokerSeat = 0;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                client.Player.Money = (uint)p.CurrentMoney;
                                                if (client.Player.Money < Table.MinBet * 10)
                                                {
                                                    if (Table.PlayerLeave(client.PokerPlayer))
                                                    {
                                                        var P3 = new Poker.Packets.MsgTexasInteractive(null).ToArray(Poker.Enums.TableInteractiveType.Leave, client.PokerPlayer);
                                                        foreach (var px in Table.PlayersOnTable())
                                                        {
                                                            if (Pool.GamePoll.ContainsKey(px.Uid))
                                                            {
                                                                var c = Pool.GamePoll[px.Uid];
                                                                c.Send(P3);
                                                            }
                                                        }
                                                        client.SendScreen(P3, true);
                                                        client.PokerPlayer.Create(Poker.Enums.PlayerType.Watcher, client.PokerPlayer.Seat, Table, (ulong)client.PokerPlayer.CurrentMoney);
                                                        Table.AddWatcher(client.PokerPlayer);
                                                        client.Player.PokerTableID = 0;
                                                        client.Player.PokerSeat = 0;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                Table.RoundState = 2;
                            }

                        }
                        if (Table.RoundState == 2)
                        {
                            Table.State = Poker.Enums.TableState.Unopened;
                            Table.RoundState = 0;
                            Table.Clear();
                            var P4 = Table.Update(Poker.Enums.TableUpdate.Statue);
                            var P5 = Table.Update(Poker.Enums.TableUpdate.PlayerCount);
                            foreach (var p in Table.OnScreen)
                            {
                                if (Pool.GamePoll.ContainsKey(p.Key))
                                {
                                    var client = Pool.GamePoll[p.Key];
                                    client.Send(P4);
                                    client.Send(P5);
                                }
                            }
                            Table.Time = DateTime.Now.AddSeconds(10);
                        }
                        break;
                    }
                #endregion
            }
        }
        public static void TexasHoldem(PokerTable Table)
        {
            switch (Table.State)
            {
                #region Unopened
                case Poker.Enums.TableState.Unopened:
                    {
                        if (Table.Time < Table.ThreadTime)
                        {
                            #region Kick
                            if (Table.Kick != null)
                            {
                                try
                                {
                                    if (Table.Kick.Accept.Count > Table.Kick.Refuse.Count)
                                    {
                                        var P1 = Poker.Packets.General.Kick(Table.Kick, 3, 0, 2);
                                        foreach (var P in Table.PlayersOnTable())
                                        {
                                            if (Pool.GamePoll.ContainsKey(P.Uid))
                                            {
                                                Pool.GamePoll[P.Uid].Send(P1);
                                            }
                                        }
                                        var C = Pool.GamePoll[Table.Kick.Target];
                                        if (Table.Players.ContainsKey(Table.Kick.Target))
                                        {
                                            if (C != null)
                                            {
                                                if (C.PokerPlayer != null)
                                                {
                                                    Poker.Packets.MsgShowHandExit Respond = new Poker.Packets.MsgShowHandExit(null);
                                                    Respond.Action = 1;
                                                    Respond.PlayerUid = C.Player.UID;
                                                    Respond.TableNumber = C.PokerPlayer.Table.Number;
                                                    ServerSockets.Packet stream = new ServerSockets.Packet(Respond.ToArray());
                                                    Handler(C, stream);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var P1 = Poker.Packets.General.Kick(Table.Kick, 4, 0, 2);
                                        foreach (var P in Table.PlayersOnTable())
                                        {
                                            Pool.GamePoll[P.Uid].Send(P1);
                                        }
                                    }
                                    Table.Kick = null;
                                }
                                catch
                                {
                                    Table.Kick = null;
                                }
                            }
                            #endregion
                        }
                        if (Table.PlayerCount > 1)
                        {
                            if (Table.Time < Table.ThreadTime)
                            {
                                Table.StartNewRound();
                                foreach (var p in Table.Players.Values.Where(p => p.IsPlaying == true))
                                {
                                    if (Pool.GamePoll.ContainsKey(p.Uid))
                                    {
                                        var client = Pool.GamePoll[p.Uid];
                                        if (Table.IsCPs)
                                        {
                                            client.Player.ConquerPoints -= Table.MinBet / 2;
                                            if (client.Player.UID == Table.BigBlind)
                                                client.Player.ConquerPoints -= Table.MinBet;
                                            if (client.Player.UID == Table.SmallBlind)
                                                client.Player.ConquerPoints -= Table.MinBet / 2;
                                        }
                                        else
                                        {
                                            client.Player.Money -= Table.MinBet / 2;
                                            if (client.Player.UID == Table.BigBlind)
                                                client.Player.Money -= Table.MinBet;
                                            if (client.Player.UID == Table.SmallBlind)
                                                client.Player.Money -= Table.MinBet / 2;
                                        }
                                    }
                                }
                                var P1 = Table.Update(Poker.Enums.TableUpdate.Chips);
                                foreach (var p in Table.OnScreen)
                                {
                                    if (Pool.GamePoll.ContainsKey(p.Key))
                                    {
                                        var client = Pool.GamePoll[p.Key];
                                        client.Send(P1);
                                    }
                                }
                                if (Table.TableIsChange == true)
                                {
                                    var P2 = Poker.Packets.General.TimerTick(7);
                                    var P3 = Poker.Packets.General.MsgShowHandDealtCard(Table, 7, Poker.Packets.General.HandDealtCard.OneCardDraw);
                                    foreach (var p in Table.PlayersOnTable())
                                    {
                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                        {
                                            var client = Pool.GamePoll[p.Uid];
                                            client.Send(P2);
                                            client.Send(P3);
                                        }
                                    }
                                    Table.TableIsChange = false;
                                    Table.Time = DateTime.Now.AddSeconds(7);
                                }
                                Table.RoundState = 0;
                                Table.StartPocket();

                            }
                        }
                        break;
                    }
                #endregion
                #region Poket
                case Poker.Enums.TableState.Pocket:
                    {
                        if (Table.PlayerCount > 1)
                        {
                            if (Table.Time < Table.ThreadTime)
                            {
                                if (Table.RoundState == 0)
                                {
                                    var P1 = Table.Update(Poker.Enums.TableUpdate.Statue);
                                    foreach (var p in Table.OnScreen)
                                    {
                                        if (Pool.GamePoll.ContainsKey(p.Key))
                                        {
                                            var client = Pool.GamePoll[p.Key];
                                            client.Send(P1);
                                        }
                                    }
                                    foreach (var p in Table.PlayersOnTable())
                                    {
                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                        {
                                            var client = Pool.GamePoll[p.Uid];
                                            client.Send(Poker.Packets.General.MsgShowHandDealtCard(Table, 0, Poker.Packets.General.HandDealtCard.TwoCardDraw, p.Uid));
                                        }
                                    }
                                    Table.Time = DateTime.Now.AddSeconds(Table.Players.Values.Where(p => p.IsPlaying).ToList().Count * 1.5);
                                    Table.RoundState = 1;
                                }
                                else if (Table.RoundState == 1)
                                {
                                    var P1 = Poker.Packets.General.MsgShowHandActivePlayer(Table, 10, Table.CurrentPlayer);
                                    foreach (var p in Table.PlayersOnTable())
                                    {
                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                        {
                                            var client = Pool.GamePoll[p.Uid];
                                            client.Send(P1);
                                        }
                                    }
                                    Table.RoundState = 2;
                                    Table.Time = DateTime.Now.AddSeconds(10);
                                }
                                else if (Table.RoundState == 2)
                                {
                                    var player = Table.Players[Table.CurrentPlayer];
                                    Poker.Packets.MsgShowHandCallAction Respond = new Poker.Packets.MsgShowHandCallAction(null);
                                    Respond.Uid = player.Uid;
                                    Respond.Action = 4;
                                    player.PotinThisRound = true;
                                    player.Fold = true;
                                    player.RoundPot = 0;
                                    if (Table.SmallBlind == player.Uid)
                                    {
                                        Table.SmallBlind = Table.NextSeat(player.Seat);
                                    }
                                    var P1 = Respond.ToArray(player.RoundPot, player.TotalPot);
                                    foreach (var p in Table.PlayersOnTable())
                                    {
                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                        {
                                            var c = Pool.GamePoll[p.Uid];
                                            c.Send(P1);
                                        }
                                    }
                                    if (Table.Next(false))
                                    {
                                        var P2 = Poker.Packets.General.MsgShowHandActivePlayer(Table, 10, Table.CurrentPlayer);
                                        foreach (var p in Table.PlayersOnTable())
                                        {
                                            if (Pool.GamePoll.ContainsKey(p.Uid))
                                            {
                                                var client = Pool.GamePoll[p.Uid];
                                                client.Send(P2);
                                            }
                                        }
                                        Table.Time = DateTime.Now.AddSeconds(10);
                                    }
                                }
                            }
                        }
                        break;
                    }
                #endregion
                #region Flop<Turn<River
                case Poker.Enums.TableState.Flop:
                case Poker.Enums.TableState.Turn:
                case Poker.Enums.TableState.River:
                    {
                        if (Table.PlayerCount > 1)
                        {
                            if (Table.RoundState == 0)
                            {
                                var P1 = Table.Update(Poker.Enums.TableUpdate.Statue);
                                foreach (var p in Table.OnScreen)
                                {
                                    if (Pool.GamePoll.ContainsKey(p.Key))
                                    {
                                        var client = Pool.GamePoll[p.Key];
                                        client.Send(P1);
                                    }
                                }
                                var P2 = Table.State == Poker.Enums.TableState.Flop ? Poker.Packets.General.MsgShowHandDealtCard(Table, 0, Poker.Packets.General.HandDealtCard.ThreeCardDraw, 0) : Table.State == Poker.Enums.TableState.Turn ? Poker.Packets.General.MsgShowHandDealtCard(Table, 0, Poker.Packets.General.HandDealtCard.FourCardDraw, 0) : Table.State == Poker.Enums.TableState.River ? Poker.Packets.General.MsgShowHandDealtCard(Table, 0, Poker.Packets.General.HandDealtCard.FiveCardDraw, 0) : Poker.Packets.General.MsgShowHandDealtCard(Table, 0, Poker.Packets.General.HandDealtCard.FiveCardDraw, 0);
                                foreach (var p in Table.PlayersOnTable())
                                {
                                    if (Pool.GamePoll.ContainsKey(p.Uid))
                                    {
                                        var client = Pool.GamePoll[p.Uid];
                                        client.Send(P2);
                                    }
                                }
                                Table.RoundState = 1;
                            }
                            if (Table.Time < Table.ThreadTime)
                            {
                                if (Table.RoundState == 1)
                                {
                                    var player = Table.Players[Table.CurrentPlayer];
                                    Poker.Packets.MsgShowHandCallAction Respond = new Poker.Packets.MsgShowHandCallAction(null);
                                    Respond.Uid = player.Uid;
                                    Respond.Action = 4;
                                    player.PotinThisRound = true;
                                    player.Fold = true;
                                    player.RoundPot = 0;
                                    if (Table.SmallBlind == player.Uid)
                                    {
                                        Table.SmallBlind = Table.NextSeat(player.Seat);
                                    }
                                    var P1 = Respond.ToArray(player.RoundPot, player.TotalPot);
                                    foreach (var p in Table.PlayersOnTable())
                                    {
                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                        {
                                            var c = Pool.GamePoll[p.Uid];
                                            c.Send(P1);
                                        }
                                    }
                                    if (Table.Next(false))
                                    {
                                        var P2 = Poker.Packets.General.MsgShowHandActivePlayer(Table, 10, Table.CurrentPlayer);
                                        foreach (var p in Table.PlayersOnTable())
                                        {
                                            if (Pool.GamePoll.ContainsKey(p.Uid))
                                            {
                                                var client = Pool.GamePoll[p.Uid];
                                                client.Send(P2);
                                            }
                                        }
                                        Table.Time = DateTime.Now.AddSeconds(10);
                                    }
                                }
                            }
                        }
                        break;
                    }
                #endregion
                #region ShowDown
                case Poker.Enums.TableState.ShowDown:
                    {
                        if (Table.RoundState == 0)
                        {
                            try
                            {
                                var P0 = Table.Update(Poker.Enums.TableUpdate.Statue);
                                foreach (var p in Table.OnScreen)
                                {
                                    if (Pool.GamePoll.ContainsKey(p.Key))
                                    {
                                        var client = Pool.GamePoll[p.Key];
                                        client.Send(P0);
                                    }
                                }
                                if (Table.PreviousState > 0)
                                {
                                    var P1 = Poker.Packets.General.MsgShowHandDealtCard(Table, 0, Poker.Packets.General.HandDealtCard.ThreeCardDraw, 0);
                                    var P2 = Poker.Packets.General.MsgShowHandDealtCard(Table, 0, Poker.Packets.General.HandDealtCard.FourCardDraw, 0);
                                    var P3 = Poker.Packets.General.MsgShowHandDealtCard(Table, 0, Poker.Packets.General.HandDealtCard.FiveCardDraw, 0);
                                    foreach (var p in Table.PlayersOnTable())
                                    {
                                        if (Pool.GamePoll.ContainsKey(p.Uid))
                                        {
                                            var client = Pool.GamePoll[p.Uid];
                                            if (Table.PreviousState < Poker.Enums.TableState.Flop)
                                            {
                                                client.Send(P1);
                                                client.Send(P2);
                                                client.Send(P3);
                                            }
                                            if (Table.PreviousState == Poker.Enums.TableState.Flop)
                                            {
                                                client.Send(P2);
                                                client.Send(P3);
                                            }
                                            if (Table.PreviousState == Poker.Enums.TableState.Turn)
                                            {
                                                client.Send(P3);
                                            }
                                        }
                                    }
                                }
                                Table.RoundState = 1;
                            }
                            catch
                            {
                                Table.RoundState = 1;
                            }
                        }
                        if (Table.RoundState == 1)
                        {
                            try
                            {
                                Table.GetWinners();
                                var Result = Poker.Packets.General.MsgShowHandGameResult(Table);
                                var ShowCards = Poker.Packets.General.MsgShowHandLayCard(Table);
                                Table.TotalPot = 0;
                                var P1 = Table.Update(Poker.Enums.TableUpdate.Chips);
                                foreach (var p in Table.OnScreen)
                                {
                                    if (Pool.GamePoll.ContainsKey(p.Key))
                                    {
                                        var client = Pool.GamePoll[p.Key];
                                        client.Send(P1);
                                    }
                                }
                                foreach (var p in Table.PlayersOnTable())
                                {
                                    if (Pool.GamePoll.ContainsKey(p.Uid))
                                    {
                                        var client = Pool.GamePoll[p.Uid];
                                        client.Send(ShowCards);
                                        client.Send(Result);
                                        if (client.PokerPlayer.PlayerType == Poker.Enums.PlayerType.Player)
                                        {
                                            if (Table.IsCPs)
                                            {
                                                client.Player.ConquerPoints = (uint)p.CurrentMoney;
                                                if (client.Player.ConquerPoints < Table.MinBet * 10)
                                                {
                                                    if (Table.PlayerLeave(client.PokerPlayer))
                                                    {
                                                        var P3 = new Poker.Packets.MsgTexasInteractive(null).ToArray(Poker.Enums.TableInteractiveType.Leave, client.PokerPlayer);
                                                        foreach (var px in Table.PlayersOnTable())
                                                        {
                                                            if (Pool.GamePoll.ContainsKey(px.Uid))
                                                            {
                                                                var c = Pool.GamePoll[px.Uid];
                                                                c.Send(P3);
                                                            }
                                                        }
                                                        client.SendScreen(P3, true);
                                                        client.PokerPlayer.Create(Poker.Enums.PlayerType.Watcher, client.PokerPlayer.Seat, Table, (ulong)client.PokerPlayer.CurrentMoney);
                                                        Table.AddWatcher(client.PokerPlayer);
                                                        client.Player.PokerTableID = 0;
                                                        client.Player.PokerSeat = 0;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                client.Player.Money = (uint)p.CurrentMoney;
                                                if (client.Player.Money < Table.MinBet * 10)
                                                {
                                                    if (Table.PlayerLeave(client.PokerPlayer))
                                                    {
                                                        var P3 = new Poker.Packets.MsgTexasInteractive(null).ToArray(Poker.Enums.TableInteractiveType.Leave, client.PokerPlayer);
                                                        foreach (var px in Table.PlayersOnTable())
                                                        {
                                                            if (Pool.GamePoll.ContainsKey(px.Uid))
                                                            {
                                                                var c = Pool.GamePoll[px.Uid];
                                                                c.Send(P3);
                                                            }
                                                        }
                                                        client.SendScreen(P3, true);
                                                        client.PokerPlayer.Create(Poker.Enums.PlayerType.Watcher, client.PokerPlayer.Seat, Table, (ulong)client.PokerPlayer.CurrentMoney);
                                                        Table.AddWatcher(client.PokerPlayer);
                                                        client.Player.PokerTableID = 0;
                                                        client.Player.PokerSeat = 0;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                Table.RoundState = 2;
                            }
                            catch
                            {
                                Table.RoundState = 2;
                            }

                        }
                        if (Table.RoundState == 2)
                        {
                            Table.State = Poker.Enums.TableState.Unopened;
                            Table.RoundState = 0;
                            Table.Clear();
                            var P4 = Table.Update(Poker.Enums.TableUpdate.Statue);
                            var P5 = Table.Update(Poker.Enums.TableUpdate.PlayerCount);
                            foreach (var p in Table.OnScreen)
                            {
                                if (Pool.GamePoll.ContainsKey(p.Key))
                                {
                                    var client = Pool.GamePoll[p.Key];
                                    client.Send(P4);
                                    client.Send(P5);
                                }
                            }
                            Table.Time = DateTime.Now.AddSeconds(10);
                        }
                        break;
                    }
                #endregion
                default:
                    {
                        Console.WriteLine("Unhandle Poker Table State: " + Table.State);
                        break;
                    }
            }
        }
        public static void PokerTablesCallback(PokerTable Table, int time)
        {
            try
            {
                Table.ThreadTime = DateTime.Now;
                if (!Table.TableBusy)
                {
                    lock (Table.TableSyncRoot)
                    {
                        switch (Table.TableType)
                        {
                            case Poker.Enums.TableType.ShowHand:
                                {
                                    ShowHand(Table);
                                    break;
                                }
                            case Poker.Enums.TableType.TexasHoldem:
                                {
                                    TexasHoldem(Table);
                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }
        }
    }
}
