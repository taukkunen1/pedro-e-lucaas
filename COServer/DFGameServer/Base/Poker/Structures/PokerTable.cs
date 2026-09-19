using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using HoldemHand;
using Poker.Packets;

namespace Poker.Structures
{
    public class PokerTable
    {
        public PokerStructs.Kick Kick;
        private byte[] _buffer;
        public List<uint> ToSend = new List<uint>();
        public uint BigBlind;
        internal PokerStructs.PokerCard[] Board = new PokerStructs.PokerCard[5];
        internal List<PokerStructs.PokerCard> Cards;
        public uint CurrentPlayer;
        public uint Dealer;
        public object dr;
        public ushort MapId = 1858;
        public System.Collections.Concurrent.ConcurrentDictionary<uint, uint> OnScreen;
        public System.Collections.Concurrent.ConcurrentDictionary<uint, PokerStructs.Player> Players;
        private System.Collections.Generic.Dictionary<uint, ulong> TempPlayers;
        public System.Collections.Concurrent.ConcurrentDictionary<uint, PokerStructs.Player> Watchers;
        public PokerStructs.Player PreviousPlayer;
        public ulong RequiredPot;
        public ulong RoundPot;
        public uint SmallBlind;
        public bool TableIsChange;
        public object TableSyncRoot;
        public Enums.TableType TableType;
        public DateTime ThreadTime;
        public DateTime Time;
        public byte RoundState;
        public Enums.TableState PreviousState;
        public byte NumberOfRaise = 0;
        public PokerTable(uint id)
        {
            TableSyncRoot = new object();
            _buffer = new byte[60];
            Packet.WriteUInt16((ushort)(_buffer.Length - 8), 0, _buffer);
            Packet.WriteUInt16(2172, 2, _buffer);
            Id = id;
            Players = new System.Collections.Concurrent.ConcurrentDictionary<uint, PokerStructs.Player>();
            Watchers = new System.Collections.Concurrent.ConcurrentDictionary<uint, PokerStructs.Player>();
            TempPlayers = new Dictionary<uint, ulong>();
            RoundState = 0;
            Kick = null;
            PreviousState = 0;
            RequiredPot = 0;
            NumberOfRaise = 0;
            TotalPot = 0;
            RoundPot = 0;
            CurrentPlayer = 0;
            PreviousPlayer = null;
            Board = new PokerStructs.PokerCard[5];
            ReloadCards();
            dr = new object();
        }
        public bool CanInterface(uint UID)
        {

            {
                if (this.OnScreen.ContainsKey(UID))
                {
                    return true;
                }
                return false;
            }
        }
        public uint Id
        {
            get { return BitConverter.ToUInt32(_buffer, 4); }
            set { Packet.WriteUInt32(value, 4, _buffer); }
        }

        public ushort X
        {
            get { return BitConverter.ToUInt16(_buffer, 16); }
            set { Packet.WriteUInt16(value, 16, _buffer); }
        }

        public ushort Y
        {
            get { return BitConverter.ToUInt16(_buffer, 18); }
            set { Packet.WriteUInt16(value, 18, _buffer); }
        }

        internal uint Mesh
        {
            get { return BitConverter.ToUInt32(_buffer, 20); }
            set { Packet.WriteUInt32(value, 20, _buffer); }
        }

        public uint Number
        {
            get { return BitConverter.ToUInt32(_buffer, 26); }
            set { Packet.WriteUInt32(value, 26, _buffer); }
        }

        public bool UnLimited
        {
            get { return BitConverter.ToBoolean(_buffer, 30); }
            set { Packet.WriteBoolean(value, 30, _buffer); }
        }

        public bool IsCPs
        {
            get { return BitConverter.ToBoolean(_buffer, 34); }
            set { Packet.WriteBoolean(value, 34, _buffer); }
        }

        public uint MinBet
        {
            get { return BitConverter.ToUInt32(_buffer, 38); }
            set { Packet.WriteUInt32(value, 38, _buffer); }
        }

        public Enums.TableState State
        {
            get { return (Enums.TableState)_buffer[42]; }
            set { Packet.WriteByte((byte)value, 42, _buffer); }
        }

        public ulong TotalPot
        {
            get { return BitConverter.ToUInt64(_buffer, 43); }
            set { Packet.WriteUInt64(value, 43, _buffer); }
        }

        public byte PlayerCount
        {
            get { return _buffer[51]; }
            set
            {
                Packet.WriteByte(value, 51, _buffer);
                Array.Resize(ref _buffer, 60 + 6 * value);
                Packet.WriteUInt16((ushort)(_buffer.Length - 8), 0, _buffer);
            }
        }



        public bool IsSeatAvailable(byte seat)
        {

            {
               // if (!MD5Hash.Same(Database.IP))
                 //   return false;
                try
                {
                    foreach (var p in Players.Values)
                        if (seat == p.Seat)
                            return false;
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
        }

        public bool AddWatcher(PokerStructs.Player player)
        {

            {
              //  if (!MD5Hash.Same(Database.IP))
               //     return false;
                try
                {
                    if (player.PlayerType == Enums.PlayerType.Watcher)
                        if (!Watchers.ContainsKey(player.Uid))
                        {
                            Watchers.TryAdd(player.Uid, player);
                            return true;
                        }
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
        }
        public bool InGame()
        {

            {
                //if (!MD5Hash.Same(Database.IP))
                //    return false;

                if (State == Enums.TableState.Unopened)
                    return false;
                if (State == Enums.TableState.ShowDown)
                    return false;
                return true;
            }
        }
        public bool AddPlayer(PokerStructs.Player player)
        {

            {
               // if (!MD5Hash.Same(Database.IP))
                  //  return false;
                try
                {
                    if (player.CurrentMoney < MinBet * 10)
                        return false;
                    WatcherLeave(player);
                    if (player.PlayerType == Enums.PlayerType.Player)
                    {
                        if (IsSeatAvailable(player.Seat))
                        {
                            if (!Players.ContainsKey(player.Uid))
                            {
                                if (TableType == Enums.TableType.TexasHoldem)
                                {
                                    if (PlayerCount < 9)
                                    {
                                        Players.TryAdd(player.Uid, player);
                                        PlayerCount += 1;
                                        var offset = 52;
                                        foreach (var p in Players.Values)
                                        {
                                            Packet.WriteUInt32(p.Uid, offset, _buffer);
                                            offset += 4;
                                            Packet.WriteByte(p.Seat, offset, _buffer);
                                            offset += 1;
                                            Packet.WriteBoolean(true, offset, _buffer);
                                            offset += 1;
                                        }
                                        TableIsChange = true;
                                        return true;
                                    }
                                }
                                else
                                {
                                    if (PlayerCount < 5)
                                    {
                                        Players.TryAdd(player.Uid, player);
                                        PlayerCount += 1;
                                        var offset = 52;
                                        foreach (var p in Players.Values)
                                        {
                                            Packet.WriteUInt32(p.Uid, offset, _buffer);
                                            offset += 4;
                                            Packet.WriteByte(p.Seat, offset, _buffer);
                                            offset += 1;
                                            Packet.WriteBoolean(true, offset, _buffer);
                                            offset += 1;
                                        }
                                        TableIsChange = true;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
        }
        public bool PlayerLeave(PokerStructs.Player player)
        {

            {
               // if (!MD5Hash.Same(Database.IP))
                 //   return false;
                try
                {
                    if (!IsSeatAvailable(player.Seat))
                    {
                        if (Players.ContainsKey(player.Uid))
                        {
                            if (PlayerCount > 0)
                            {
                                if (InGame())
                                {
                                    if (player.IsPlaying)
                                    {
                                        TempPlayers.Add(player.Uid, player.TotalPot);
                                    }
                                }
                                ((IDictionary<uint, PokerStructs.Player>)Players).Remove(player.Uid);
                                PlayerCount -= 1;
                                var offset = 52;
                                foreach (var p in Players.Values)
                                {
                                    Packet.WriteUInt32(p.Uid, offset, _buffer);
                                    offset += 4;
                                    Packet.WriteByte(p.Seat, offset, _buffer);
                                    offset += 1;
                                    Packet.WriteBoolean(true, offset, _buffer);
                                    offset += 1;
                                }
                                TableIsChange = true;
                                return true;
                            }
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
        }
        public bool WatcherLeave(PokerStructs.Player player)
        {

            {
               // if (!MD5Hash.Same(Database.IP))
                //    return false;
                try
                {
                    if (Watchers.ContainsKey(player.Uid))
                    {
                        ((IDictionary<uint, PokerStructs.Player>)Watchers).Remove(player.Uid);
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
        }

        public byte[] Update(Enums.TableUpdate type, ulong value = 0)
        {

            {
                var buff = new byte[42];
                Packet.WriteUInt16(34, 0, buff);
                Packet.WriteUInt16(10010, 2, buff);
                //Packet.WriteUInt32((uint)DateTime.Now.GetHashCode(), 4, buff);
                switch (type)
                {
                    case Enums.TableUpdate.PlayerCount:
                        {
                            Packet.WriteUInt32(Id, 4, buff);
                            Packet.WriteUInt64(value != 0 ? value : PlayerCount, 8, buff);
                            Packet.WriteUInt32((uint)DateTime.Now.GetHashCode(), 16, buff);
                            Packet.WriteUInt32((uint)type, 20, buff);
                            return buff;
                        }
                    case Enums.TableUpdate.Statue:
                        {
                            Packet.WriteUInt32(Id, 4, buff);
                            Packet.WriteUInt64(value != 0 ? value : (ulong)State, 8, buff);
                            Packet.WriteUInt32((uint)DateTime.Now.GetHashCode(), 16, buff);
                            Packet.WriteUInt32((uint)type, 20, buff);
                            return buff;
                        }
                    case Enums.TableUpdate.Chips:
                        {
                            Packet.WriteUInt32(Id, 4, buff);
                            Packet.WriteUInt64(value != 0 ? value : TotalPot, 8, buff);
                            Packet.WriteUInt32((uint)DateTime.Now.GetHashCode(), 16, buff);
                            Packet.WriteUInt32((uint)type, 20, buff);
                            return buff;
                        }
                }
                return buff;
            }
        }

        private void ReloadCards()
        {
        lablex:
            Cards = new List<PokerStructs.PokerCard>();
            byte S = 0;
            if (this.Mesh == 7247567 || this.Mesh == 7255787 || this.TableType == Enums.TableType.ShowHand)
            {
                S = 5;
            }
            for (byte y = 0; y < 4; y++)
            {
                var T = Enums.CardsType.Heart;
                if (y == 1)
                    T = Enums.CardsType.Spade;
                if (y == 2)
                    T = Enums.CardsType.Club;
                if (y == 3)
                    T = Enums.CardsType.Diamond;
                for (byte x = S; x < 13; x++)
                {
                    var c = new PokerStructs.PokerCard
                    {
                        Type = T,
                        Value = x
                    };
                    Cards.Add(c);
                }
            }
            if (S == 5)
            {
                if (Cards.Count < 32)
                {
                    goto lablex;
                }
            }
            else
            {
                if (Cards.Count < 52)
                {
                    goto lablex;
                }
            }
        }
        private PokerStructs.PokerCard Draw()
        {

            PokerStructs.PokerCard card = null;
            lock (dr)
            {
                var rand2 = new Random(Guid.NewGuid().GetHashCode());
                var rand = new Random(Guid.NewGuid().GetHashCode() * rand2.Next(1, 100));
                var I = rand.Next(0, Cards.Count);
                if (Cards.Count > 0)
                {
                    if (Cards.Count > I)
                    {
                        if (Cards[I] != null)
                        {
                            card = Cards[I];
                            Cards.RemoveAt(I);
                            return card;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error In Card Draw");
                }
                return null;
            }
        }

        private void AddCardsToPlayers(int count)
        {
            if (TableType == Enums.TableType.ShowHand)
            {
                for (var i = 0; i < count; i++)
                {
                    foreach (var p in Players.Values.Where(p => p.IsPlaying))
                    {
                        if (p.Pocket[i] == null)
                        {
                            int loob = 0;
                        lablex:
                            p.Pocket[i] = Draw();
                            if (p.Pocket[i] == null && Cards.Count > 1 && loob < 30)
                            {
                                loob += 1;
                                goto lablex;
                            }
                        }
                    }
                }
            }
            else
            {

                for (var i = 0; i < count; i++)
                {
                    foreach (var p in Players.Values.Where(p => p.IsPlaying))
                    {
                        int loob = 0;
                    lablex:
                        p.Pocket[i] = Draw();
                        if (p.Pocket[i] == null && Cards.Count > 1 && loob < 30)
                        {
                            loob += 1;
                            goto lablex;
                        }
                    }
                }
            }
        }
        public void AddCardsToBoard(int count)
        {
            for (byte i = 0; i < count; i++)
            {
                if (Board[i] == null)
                {
                    int loob = 0;
                lablex:
                    Board[i] = Draw();
                    if (Board[i] == null && Cards.Count > 1 && loob < 30)
                    {
                        loob += 1;
                        goto lablex;
                    }
                }
            }
        }
        public void Clear()
        {
            //if (!MD5Hash.Same(Database.IP))
            //    return;
            Showhand = false;
            ShowhandTotalPot = 0;
            ShowHand = 0;
            TempPlayers.Clear();
            RoundState = 0;
            Kick = null;
            PreviousState = 0;
            RequiredPot = 0;
            NumberOfRaise = 0;
            TotalPot = 0;
            RoundPot = 0;
            CurrentPlayer = 0;
            PreviousPlayer = null;
            Board = new PokerStructs.PokerCard[5];
            foreach (var p in Players.Values)
            {
                p.Create(p.PlayerType, p.Seat, this, (ulong)p.CurrentMoney);
            }
            ReloadCards();

        }
        public void StartNewRound()
        {
            //if (!MD5Hash.Same(Database.IP))
            //    return;
            if (this.TableType == Enums.TableType.TexasHoldem)
            {
                ShowHand = 0;
                ReloadCards();
                foreach (var p in Players.Values)
                {
                    p.IsPlaying = true;
                }
                if (TableIsChange)
                {
                    AddCardsToPlayers(1);
                    ReloadCards();
                }
                GetDealer();
                TotalPot = (ulong)(Players.Values.Where(p => p.IsPlaying).ToList().Count * (MinBet / 2));
                foreach (var p in Players.Values.Where(p => p.IsPlaying))
                {
                    p.CurrentMoney -= MinBet / 2;
                    p.TotalPot = MinBet / 2;
                }
                Players[SmallBlind].CurrentMoney -= MinBet / 2;
                Players[SmallBlind].RoundPot += MinBet / 2;
                Players[SmallBlind].TotalPot += MinBet / 2;
                TotalPot += MinBet / 2;
                RoundPot += MinBet / 2;

                Players[BigBlind].CurrentMoney -= MinBet;
                Players[BigBlind].RoundPot += MinBet;
                Players[BigBlind].TotalPot += MinBet;
                TotalPot += MinBet;
                RoundPot += MinBet;
                State = Enums.TableState.Pocket;
            }
            else
            {
                ReloadCards();
                foreach (var p in Players.Values)
                {
                    p.IsPlaying = true;
                }
                AddCardsToPlayers(2);
                GetDealer();
                PreviousPlayer = Players[PreviousSeat(Players[Dealer].Seat)];
                TotalPot = (ulong)(Players.Values.Where(p => p.IsPlaying).ToList().Count * (MinBet / 2));
                foreach (var p in Players.Values.Where(p => p.IsPlaying))
                {
                    p.CurrentMoney -= MinBet / 2;
                    p.TotalPot = MinBet / 2;
                }
                State = Enums.TableState.Pocket;
            }
        }
        public void StartPocket()
        {
           // if (!MD5Hash.Same(Database.IP))
             //   return;
            if (this.TableType == Enums.TableType.TexasHoldem)
            {
                CurrentPlayer = NextSeat(Players[BigBlind].Seat);
                AddCardsToPlayers(2);
                GetRequiredBet();
            }
            else
            {
                CurrentPlayer = Dealer;
                GetRequiredBet();
            }
        }
        private void GetRequiredBet()
        {
           // if (!MD5Hash.Same(Database.IP))
               // return;
            if (PreviousPlayer != null && CurrentPlayer != 0)
            {
                RequiredPot = PreviousPlayer.RoundPot - Players[CurrentPlayer].RoundPot;

            }
            else
            {
                RequiredPot = 0;
            }
        }
        public long LowestMoney
        {
            get
            {

                long M = 0;
                if (Showhand == false)
                {
                    foreach (var P in Players.Values)
                    {
                        if (P.IsPlaying)
                        {
                            if (!P.Fold)
                            {
                                if (M == 0)
                                {
                                    M = P.CurrentMoney;
                                }
                                else
                                {
                                    if (P.CurrentMoney < M)
                                    {
                                        M = P.CurrentMoney;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    M = ShowhandTotalPot;
                }
                return M;
            }

        }
        public bool Showhand = false;
        public long ShowhandTotalPot = 0;
        internal ushort GetRequiredAction()
        {
            if (TableType == Enums.TableType.TexasHoldem)
            {
                if (!Players.ContainsKey(CurrentPlayer))
                    return 0;
                if (!UnLimited)
                {
                    if ((RequiredPot > 0) && ((ulong)Players[CurrentPlayer].CurrentMoney > RequiredPot + MinBet) && State == Enums.TableState.Pocket && NumberOfRaise < 3)
                        return (ushort)
                        (General.ActivePlayerTypes.Raise +
                         General.ActivePlayerTypes.Call +
                         General.ActivePlayerTypes.Fold);
                    if ((RequiredPot > 0) && ((ulong)Players[CurrentPlayer].CurrentMoney > RequiredPot + MinBet) && State == Enums.TableState.Pocket && NumberOfRaise >= 3)
                        return (ushort)
                        (General.ActivePlayerTypes.Call +
                         General.ActivePlayerTypes.Fold);
                    if ((RequiredPot > 0) && ((ulong)Players[CurrentPlayer].CurrentMoney > RequiredPot + MinBet) && State != Enums.TableState.Pocket)
                        return (ushort)
                        (General.ActivePlayerTypes.Call +
                         General.ActivePlayerTypes.Fold);
                    if ((RequiredPot > 0) && ((ulong)Players[CurrentPlayer].CurrentMoney <= RequiredPot + 10) &&
                        ((ulong)Players[CurrentPlayer].CurrentMoney > RequiredPot))
                        return (ushort)
                        (General.ActivePlayerTypes.Call +
                         General.ActivePlayerTypes.Fold);
                    if ((RequiredPot > 0) && ((ulong)Players[CurrentPlayer].CurrentMoney <= RequiredPot))
                        return (ushort)
                        (General.ActivePlayerTypes.Allin +
                         General.ActivePlayerTypes.Fold);
                    if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney > MinBet * 2) && (RoundPot > 0))
                        return (ushort)
                        (General.ActivePlayerTypes.Raise +
                         General.ActivePlayerTypes.Check +
                         General.ActivePlayerTypes.Fold);
                    if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney <= MinBet * 2) && (RoundPot > 0))
                        return (ushort)
                        (General.ActivePlayerTypes.Allin +
                         General.ActivePlayerTypes.Check +
                         General.ActivePlayerTypes.Fold);
                    if (NumberOfRaise == 0)
                    {
                        if (State == Poker.Enums.TableState.Pocket || State == Poker.Enums.TableState.Flop)
                        {
                            if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney > MinBet) && (RoundPot == 0))
                                return (ushort)
                                (General.ActivePlayerTypes.Bet +
                                 General.ActivePlayerTypes.Check +
                                 General.ActivePlayerTypes.Fold);
                            if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney <= MinBet) && (RoundPot == 0))
                                return (ushort)
                                (General.ActivePlayerTypes.Allin +
                                 General.ActivePlayerTypes.Check +
                                 General.ActivePlayerTypes.Fold);
                        }
                        else
                        {
                            if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney > MinBet * 2) && (RoundPot == 0))
                                return (ushort)
                                (General.ActivePlayerTypes.Bet +
                                 General.ActivePlayerTypes.Check +
                                 General.ActivePlayerTypes.Fold);
                            if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney <= MinBet * 2) && (RoundPot == 0))
                                return (ushort)
                                (General.ActivePlayerTypes.Allin +
                                 General.ActivePlayerTypes.Check +
                                 General.ActivePlayerTypes.Fold);
                        }
                    }
                    else if (NumberOfRaise > 0)
                    {
                        if (State == Poker.Enums.TableState.Pocket)
                        {
                            if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney > MinBet) && (RoundPot == 0))
                                return (ushort)
                                (General.ActivePlayerTypes.Bet +
                                 General.ActivePlayerTypes.Check +
                                 General.ActivePlayerTypes.Fold);
                            if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney <= MinBet) && (RoundPot == 0))
                                return (ushort)
                                (General.ActivePlayerTypes.Allin +
                                 General.ActivePlayerTypes.Check +
                                 General.ActivePlayerTypes.Fold);
                        }
                        else
                        {
                            if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney > MinBet * 2) && (RoundPot == 0))
                                return (ushort)
                                (General.ActivePlayerTypes.Bet +
                                 General.ActivePlayerTypes.Check +
                                 General.ActivePlayerTypes.Fold);
                            if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney <= MinBet * 2) && (RoundPot == 0))
                                return (ushort)
                                (General.ActivePlayerTypes.Allin +
                                 General.ActivePlayerTypes.Check +
                                 General.ActivePlayerTypes.Fold);
                        }
                    }
                    Console.WriteLine("Unhandle RequiredBet: " + RequiredPot);
                    return 0;
                }
                else
                {
                    if ((RequiredPot > 0) && ((ulong)Players[CurrentPlayer].CurrentMoney > PreviousPlayer.RoundPot + RequiredPot))
                        return (ushort)
                        (General.ActivePlayerTypes.Raise +
                         General.ActivePlayerTypes.Allin +
                         General.ActivePlayerTypes.Call +
                         General.ActivePlayerTypes.Fold);
                    if ((RequiredPot > 0) && ((ulong)Players[CurrentPlayer].CurrentMoney <= PreviousPlayer.RoundPot + RequiredPot) &&
                        ((ulong)Players[CurrentPlayer].CurrentMoney > RequiredPot))
                        return (ushort)
                        (General.ActivePlayerTypes.Allin +
                         General.ActivePlayerTypes.Call +
                         General.ActivePlayerTypes.Fold);
                    if ((RequiredPot > 0) && ((ulong)Players[CurrentPlayer].CurrentMoney <= RequiredPot))
                        return (ushort)
                        (General.ActivePlayerTypes.Allin +
                         General.ActivePlayerTypes.Fold);
                    if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney > MinBet) && (RoundPot > 0))
                        return (ushort)
                        (General.ActivePlayerTypes.Raise +
                         General.ActivePlayerTypes.Allin +
                         General.ActivePlayerTypes.Check +
                         General.ActivePlayerTypes.Fold);
                    if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney <= MinBet) && (RoundPot > 0))
                        return (ushort)
                        (General.ActivePlayerTypes.Allin +
                         General.ActivePlayerTypes.Check +
                         General.ActivePlayerTypes.Fold);
                    if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney > MinBet) && (RoundPot == 0))
                        return (ushort)
                        (General.ActivePlayerTypes.Bet +
                         General.ActivePlayerTypes.Allin +
                         General.ActivePlayerTypes.Check +
                         General.ActivePlayerTypes.Fold);
                    if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney <= MinBet) && (RoundPot == 0))
                        return (ushort)
                        (General.ActivePlayerTypes.Allin +
                         General.ActivePlayerTypes.Check +
                         General.ActivePlayerTypes.Fold);
                    Console.WriteLine("Unhandle RequiredBet: " + RequiredPot);

                }
            }
            else
            {
                if ((RequiredPot > 0) && ((ulong)Players[CurrentPlayer].CurrentMoney > PreviousPlayer.RoundPot + RequiredPot) && Showhand == false)
                    return (ushort)
                    (General.ActivePlayerTypes.Raise +
                     General.ActivePlayerTypes.Allin +
                     General.ActivePlayerTypes.Call +
                     General.ActivePlayerTypes.Fold);
                if ((RequiredPot > 0) && ((ulong)Players[CurrentPlayer].CurrentMoney <= PreviousPlayer.RoundPot + RequiredPot) &&
                    ((ulong)Players[CurrentPlayer].CurrentMoney > RequiredPot) && Showhand == false)
                    return (ushort)
                    (General.ActivePlayerTypes.Allin +
                     General.ActivePlayerTypes.Call +
                     General.ActivePlayerTypes.Fold);
                if ((RequiredPot > 0) && ((ulong)Players[CurrentPlayer].CurrentMoney <= RequiredPot) && Showhand == false)
                    return (ushort)
                    (General.ActivePlayerTypes.Allin +
                     General.ActivePlayerTypes.Fold);
                if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney > MinBet) && (RoundPot > 0) && Showhand == false)
                    return (ushort)
                    (General.ActivePlayerTypes.Raise +
                     General.ActivePlayerTypes.Allin +
                     General.ActivePlayerTypes.Check +
                     General.ActivePlayerTypes.Fold);
                if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney <= MinBet) && (RoundPot > 0) && Showhand == false)
                    return (ushort)
                    (General.ActivePlayerTypes.Allin +
                     General.ActivePlayerTypes.Check +
                     General.ActivePlayerTypes.Fold);
                if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney > MinBet) && (RoundPot == 0) && Showhand == false)
                    return (ushort)
                    (General.ActivePlayerTypes.Bet +
                     General.ActivePlayerTypes.Allin +
                     General.ActivePlayerTypes.Check +
                     General.ActivePlayerTypes.Fold);
                if ((RequiredPot == 0) && (Players[CurrentPlayer].CurrentMoney <= MinBet) && (RoundPot == 0) && Showhand == false)
                    return (ushort)
                    (General.ActivePlayerTypes.Allin +
                     General.ActivePlayerTypes.Check +
                     General.ActivePlayerTypes.Fold);
                if (Showhand == true)
                    return (ushort)
                    (General.ActivePlayerTypes.Allin +
                     General.ActivePlayerTypes.Fold);
                Console.WriteLine("Unhandle RequiredBet: " + RequiredPot);
            }
            return 0;
        }
        private byte CheckRound()
        {

            {
               // if (!MD5Hash.Same(Database.IP))
                  //  return 255;
                ulong hight = 0;
                var equil = true;
                var allPot = true;
                foreach (var p in Players.Values.Where(p => p.IsPlaying && p.IsPotAllin == false && p.Fold == false))
                {
                    if (hight == 0)
                        hight = p.RoundPot;
                    if (hight != p.RoundPot)
                    {
                        equil = false;
                        break;
                    }
                    if (p.PotinThisRound == false)
                    {
                        allPot = false;
                        break;
                    }
                }
                if (Players.Values.Where(p => p.IsPlaying && p.Fold == false).ToList().Count == 1)
                    return 3;//End AllFold
                if (allPot && equil && Players.Values.Where(p => p.IsPlaying && p.IsPotAllin == false && p.Fold == false).ToList().Count == 0)
                    return 1; //End Allin
                if (allPot && equil && Players.Values.Where(p => p.IsPlaying && p.IsPotAllin == false && p.Fold == false).ToList().Count == 1 && RequiredPot == 0)
                    return 1; //End Allin
                if (allPot && equil && Players.Values.Where(p => p.IsPlaying && p.IsPotAllin == false && p.Fold == false).ToList().Count > 1 && RequiredPot == 0)
                    return 2; //NextRound
                return 0;
            }
        }
        public bool Next(bool c = false)
        {
          //  if (!MD5Hash.Same(Database.IP))
           //     return false;
            if (TableType == Enums.TableType.TexasHoldem)
            {
                if (c)
                {
                    PreviousPlayer = Players[CurrentPlayer];
                }
                if (PreviousPlayer == null)
                {
                    CurrentPlayer = NextSeat(Players[SmallBlind].Seat);
                }
                else
                {
                    CurrentPlayer = NextSeat(PreviousPlayer.Seat);
                }
                GetRequiredBet();
                NextRound();
                if (State == Enums.TableState.ShowDown)
                {
                    return false;
                }
                return true;
            }
            else
            {
                if (c)
                {
                    PreviousPlayer = Players[CurrentPlayer];
                }
                if (PreviousPlayer == null)
                {
                    CurrentPlayer = NextSeat(Players[Dealer].Seat);
                }
                else
                {
                    CurrentPlayer = NextSeat(PreviousPlayer.Seat);
                }
                GetRequiredBet();
                NextRound();
                if (State == Enums.TableState.ShowDown)
                {
                    return false;
                }
                return true;
            }
        }
        public void NextRound()
        {
           // if (!MD5Hash.Same(Database.IP))
               // return;
            if (TableType == Enums.TableType.TexasHoldem)
            {

                var check = CheckRound();
                if (check == 1 || check == 3)
                {
                    AddCardsToBoard(5);
                    if (check == 1)
                    {
                        PreviousState = State;
                    }
                    RoundPot = 0;
                    CurrentPlayer = 0;
                    PreviousPlayer = null;
                    RoundState = 0;
                    State = Enums.TableState.ShowDown;

                }
                else if (check == 2)
                {
                    if (State + 1 == Enums.TableState.Flop)
                        AddCardsToBoard(3);
                    if (State + 1 == Enums.TableState.Turn)
                        AddCardsToBoard(4);
                    if (State + 1 == Enums.TableState.River)
                        AddCardsToBoard(5);
                    foreach (var p in Players.Values.Where(p => p.IsPlaying))
                    {
                        p.PotinThisRound = false;
                        p.RoundPot = 0;
                    }
                    CurrentPlayer = SmallBlind;
                    RoundPot = 0;
                    RoundState = 0;
                    State += 1;
                    if (State == Enums.TableState.ShowDown)
                    {
                        CurrentPlayer = 0;
                        PreviousPlayer = null;
                    }
                }
            }
            else
            {

                var check = CheckRound();
                if (check == 1 || check == 3)
                {
                    AddCardsToPlayers(5);
                    if (check == 1)
                    {
                        PreviousState = State;
                    }
                    RoundPot = 0;
                    CurrentPlayer = 0;
                    PreviousPlayer = null;
                    RoundState = 0;
                    GetDealer();
                    State = Enums.TableState.ShowDown;

                }
                else if (check == 2)
                {
                    if (State + 1 == Enums.TableState.Flop)
                        AddCardsToPlayers(3);
                    if (State + 1 == Enums.TableState.Turn)
                        AddCardsToPlayers(4);
                    if (State + 1 == Enums.TableState.River)
                        AddCardsToPlayers(5);
                    foreach (var p in Players.Values.Where(p => p.IsPlaying))
                    {
                        p.PotinThisRound = false;
                        p.RoundPot = 0;
                    }
                    GetDealer();
                    CurrentPlayer = Dealer;
                    RoundPot = 0;
                    RoundState = 0;
                    State += 1;
                    if (State == Enums.TableState.ShowDown)
                    {
                        CurrentPlayer = 0;
                        PreviousPlayer = null;
                    }
                }
            }
        }
        internal void GetDealer()
        {
            //if (!MD5Hash.Same(Database.IP))
           //     return;
            if (TableType == Enums.TableType.TexasHoldem)
            {

                if (TableIsChange)
                    Dealer = 0;
                if (Dealer == 0)
                {
                    foreach (var p in Players.Values.Where(p => p.IsPlaying))
                    {
                        if (Dealer == 0)
                        {
                            Dealer = p.Uid;
                        }
                        else
                        {
                            if (p.Pocket[0] > Players[Dealer].Pocket[0])
                                Dealer = p.Uid;
                        }
                    }
                }
                else
                {
                    Dealer = NextSeat(Players[Dealer].Seat);
                }
                SmallBlind = NextSeat(Players[Dealer].Seat);
                BigBlind = NextSeat(Players[SmallBlind].Seat);
                PreviousPlayer = Players[BigBlind];
                CurrentPlayer = 0;
            }
            else
            {
                Dealer = 0;
                if (Dealer == 0)
                {
                    foreach (var p in Players.Values.Where(p => p.IsPlaying))
                    {
                        if (Dealer == 0)
                        {
                            Dealer = p.Uid;
                        }
                        else
                        {
                            int count1 = 0;
                            int count2 = 0;
                            string hand1 = "";
                            string hand2 = "";
                            foreach(var Card in p.Pocket)
                            {
                                if (p.Pocket[0] != null)
                                {
                                    if (p.Pocket[0] != Card)
                                    {
                                        if (Card != null)
                                        {
                                            hand1 += " " + Card.ToString();
                                            count1 += 1;
                                        }
                                    }
                                }
                            }
                            foreach (var Card in Players[Dealer].Pocket)
                            {
                                if (Players[Dealer].Pocket[0] != null)
                                {
                                    if (Players[Dealer].Pocket[0] != Card)
                                    {
                                        if (Card != null)
                                        {
                                            hand2 += " " + Card.ToString();
                                            count2 += 1;
                                        }
                                    }
                                }
                            }
                            ulong playerMask1 = HoldemHand.Hand.ParseHand(hand1);
                            ulong playerMask2 = HoldemHand.Hand.ParseHand(hand2);
                            var CompirsHand1 = HoldemHand.Hand.Evaluate(playerMask1, count1);
                            var CompirsHand2 = HoldemHand.Hand.Evaluate(playerMask2, count2);
                            var X1 = HoldemHand.Hand.DescriptionFromHandValueInternal(CompirsHand1);
                            var X2 = HoldemHand.Hand.DescriptionFromHandValueInternal(CompirsHand2);
                            if (CompirsHand1 > CompirsHand2)
                                Dealer = p.Uid;
                        }
                    }
                }
               
                CurrentPlayer = 0;
            }
        }
        public uint PreviousSeat(int seat)
        {

            var List = Players.Values.Where(p => p.IsPlaying && p.IsPotAllin == false && p.Fold == false).ToList();
            if (List.Count == 0)
                return 0;
            if (List.Count == 1)
                return List.FirstOrDefault().Uid;
            uint uid = 0;
            int I = 0;
            while (uid == 0 && I < 20)
            {
                I++;
                seat -= 1;
                if (seat < 0)
                    seat = 4;
                var p = List.Where(x => x.Seat == seat).FirstOrDefault();
                if (p != null)
                    uid = p.Uid;
            }
            return uid;
        }
        public uint NextSeat(byte seat)
        {
            if (TableType == Enums.TableType.TexasHoldem)
            {
                var List = Players.Values.Where(p => p.IsPlaying && p.IsPotAllin == false && p.Fold == false).ToList();
                if (List.Count == 0)
                    return 0;
                if (List.Count == 1)
                    return List.FirstOrDefault().Uid;
                uint uid = 0;
                int I = 0;
                while (uid == 0 && I < 20)
                {
                    I++;
                    seat += 1;
                    if (seat > 8)
                        seat = 0;
                    var p = List.Where(x => x.Seat == seat).FirstOrDefault();
                    if (p != null)
                        uid = p.Uid;
                }
                return uid;
            }
            else
            {
                var List = Players.Values.Where(p => p.IsPlaying && p.IsPotAllin == false && p.Fold == false).ToList();
                if (State == Enums.TableState.ShowDown)
                {
                    List = Players.Values.Where(p => p.IsPlaying && p.Fold == false).ToList();
                }
                if (List.Count == 0)
                    return 0;
                if (List.Count == 1)
                    return List.FirstOrDefault().Uid;
                uint uid = 0;
                int I = 0;
                while (uid == 0 && I < 20)
                {
                    I++;
                    seat += 1;
                    if (seat > 5)
                        seat = 0;
                    var p = List.Where(x => x.Seat == seat).FirstOrDefault();
                    if (p != null)
                        uid = p.Uid;
                }
                return uid;
            }
        }
        public bool HighestBet(uint uid, ulong bet)
        {

            {
               // if (!MD5Hash.Same(Database.IP))
                //    return false;
                var high = true;
                foreach (var p in Players.Values.Where(p => p.IsPlaying))
                {
                    if (p.Uid != uid)
                    {
                        if (p.RoundPot > bet)
                        {
                            high = false;
                            break;
                        }
                    }
                }
                foreach (var p in TempPlayers)
                {
                    if (p.Value > Players[uid].TotalPot)
                    {
                        high = false;
                        break;
                    }
                }
                return high;
            }
        }
        public IEnumerable<PokerStructs.Player> PlayersOnTable()
        {

            {
               // if (!MD5Hash.Same(Database.IP))
                 //   return null;
                var list = new List<PokerStructs.Player>();
                list.AddRange(Players.Values);
                list.AddRange(Watchers.Values);
                return list;
            }
        }
        public void GetWinners()
        {
            int loob1 = 0;
            int loob2 = 0;
            int loob3 = 0;
            int loob4 = 0;
            try
            {
                if (TableType == Enums.TableType.TexasHoldem)
                {

                    TableBusy = true;
                    //if (!MD5Hash.Same(Database.IP))
                      //  return;
                    var xPlayer = Players.Values.Where(p => p.IsPlaying);
                    var Hands = new Dictionary<uint, Hand>();
                    var Winners = new Dictionary<uint, Hand>();
                    foreach (var p in xPlayer)
                    {
                        if (TempPlayers.ContainsKey(p.Uid))
                        {
                            TempPlayers[p.Uid] = p.TotalPot;
                        }
                        else
                        {
                            TempPlayers.Add(p.Uid, p.TotalPot);
                        }
                        if (p.Fold)
                        {
                            p.Lose -= (long)p.TotalPot;
                        }
                        if (!p.Fold)
                        {
                                Hand hand = new Hand(p.ToString(), this.ToString());
                                Winners.Add(p.Uid, hand);
                                Hands.Add(p.Uid, hand);
                        }
                    }
                    var Pots = CalcaluteSidePots();
                    var RemovesPot = new List<uint>();
                    foreach (var pot in Pots)
                    {
                        foreach (var p in Hands)
                        {
                            if (pot.Value.Players.Count == 1)
                            {
                                if (pot.Value.Players.Contains(p.Key))
                                {
                                    Players[p.Key].CurrentMoney += (long)pot.Value.Money;
                                    Players[p.Key].TotalPot -= (ulong)pot.Value.Money;
                                    if (!RemovesPot.Contains(pot.Key))
                                    {
                                        RemovesPot.Add(pot.Key);
                                    }
                                }
                            }
                        }
                    }
                    foreach (var I in RemovesPot)
                    {
                        if (Pots.ContainsKey(I))
                        {
                            ((IDictionary<uint, PokerStructs.SidePot>)Pots).Remove(I);
                        }
                    }
                    RemovesPot.Clear();
                Lable_X:
                    var X = CalcaluteBestHands(Hands);
                    bool Exp = false;
                Lable_X1:
                    var RemovesWinner = new List<uint>();
                    foreach (var p in X)
                    {
                        var player = xPlayer.Where(x => x.Uid == p.Key).FirstOrDefault();
                        if (player != null)
                        {
                            foreach (var pot in Pots)
                            {
                                long div = 0;
                                foreach (var W in pot.Value.Players)
                                {
                                    if (X.ContainsKey(W))
                                    {
                                        div += 1;
                                    }
                                }
                                if (pot.Value.Players.Contains(player.Uid) || Exp)
                                {
                                    if (Exp == false)
                                    {
                                        player.Lose += (long)pot.Value.Money / div;
                                    }
                                    else
                                    {
                                        player.Lose += (long)pot.Value.Money / X.Count;
                                    }
                                    if (!RemovesPot.Contains(pot.Key))
                                    {
                                        RemovesPot.Add(pot.Key);
                                    }
                                }
                            }
                        }
                        if (!RemovesWinner.Contains(p.Key))
                        {
                            RemovesWinner.Add(p.Key);
                        }

                    }
                    foreach (var I in RemovesWinner)
                    {
                        if (Hands.ContainsKey(I))
                        {
                            Hands.Remove(I);
                        }
                    }
                    foreach (var I in RemovesPot)
                    {
                        if (Pots.ContainsKey(I))
                        {
                            ((IDictionary<uint, PokerStructs.SidePot>)Pots).Remove(I);
                        }
                    }
                    if (Pots.Count > 0)
                    {
                        foreach (var pot in Pots.Values)
                        {
                            foreach (var p in Hands)
                            {
                                if (pot.Players.Contains(p.Key))
                                {
                                    if (Players.ContainsKey(p.Key))
                                    {
                                        if (Players[p.Key].Fold == false)
                                        {
                                            if (loob1 < 30)
                                            {
                                                loob1++;
                                                goto Lable_X;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (Pots.Count > 0)
                    {
                        foreach (var pot in Pots.Values)
                        {
                            X = CalcaluteBestHands(Winners);
                            Exp = true;
                            if (X.Count >= 1)
                            {
                                bool xxxx = false;
                                foreach (var xx in X)
                                {
                                    if (!Players.ContainsKey(xx.Key))
                                    {
                                        xxxx = true;
                                    }
                                }
                                if (xxxx == false)
                                {
                                    if (loob2 < 30)
                                    {
                                        loob2++;
                                        goto Lable_X1;
                                    }
                                }
                            }
                        }
                    }
                    foreach (var p in Winners)
                    {
                        Players[p.Key].CurrentMoney += (long)Players[p.Key].Lose;
                        if (Players[p.Key].CurrentMoney < 0)
                        {
                            Players[p.Key].CurrentMoney = 0; 
                        }
                        if (Players[p.Key].Lose >= (long)Players[p.Key].TotalPot)
                        {
                            Players[p.Key].Lose = (long)Players[p.Key].Lose - (long)((Players[p.Key].TotalPot * 99) / 100);
                        }
                        else
                        {
                            Players[p.Key].Lose = (long)Players[p.Key].Lose - (long)(Players[p.Key].TotalPot);
                        }
                    }
                }
                else
                {
                    TableBusy = true;
                    var xPlayer = Players.Values.Where(p => p.IsPlaying);
                    var Hands = new Dictionary<uint, ulong>();
                    var Winners = new Dictionary<uint, ulong>();
                    foreach (var p in xPlayer)
                    {
                        if (TempPlayers.ContainsKey(p.Uid))
                        {
                            TempPlayers[p.Uid] = p.TotalPot;
                        }
                        else
                        {
                            TempPlayers.Add(p.Uid, p.TotalPot);
                        }
                        if (p.Fold)
                        {
                            p.Lose -= (long)p.TotalPot;
                        }
                        if (!p.Fold)
                        {
                            int count1 = 0;
                            string hand1 = "";
                            foreach (var Card in p.Pocket)
                            {
                                if (p.Pocket[0] != null)
                                {
                                    if (p.Pocket[0] != Card)
                                    {
                                        if (Card != null)
                                        {
                                            hand1 += " " + Card.ToString();
                                            count1 += 1;
                                        }
                                    }
                                }
                            }
                            ulong playerMask1 = HoldemHand.Hand.ParseHand(hand1);
                            var CompirsHand1 = HoldemHand.Hand.Evaluate(playerMask1, count1);
                            Winners.Add(p.Uid, CompirsHand1);
                            Hands.Add(p.Uid, CompirsHand1);
                        }

                    }
                    var Pots = CalcaluteSidePots();
                    var RemovesPot = new List<uint>();
                    foreach (var pot in Pots)
                    {
                        foreach (var p in Hands)
                        {
                            if (pot.Value.Players.Count == 1)
                            {
                                if (pot.Value.Players.Contains(p.Key))
                                {
                                    Players[p.Key].CurrentMoney += (long)pot.Value.Money;
                                    Players[p.Key].TotalPot -= (ulong)pot.Value.Money;
                                    if (!RemovesPot.Contains(pot.Key))
                                    {
                                        RemovesPot.Add(pot.Key);
                                    }
                                }
                            }
                        }
                    }
                    foreach (var I in RemovesPot)
                    {
                        if (Pots.ContainsKey(I))
                        {
                            ((IDictionary<uint, PokerStructs.SidePot>)Pots).Remove(I);
                        }
                    }
                    RemovesPot.Clear();
                Lable_X:
                    var X = CalcaluteBestHands(Hands);
                    bool Exp = false;
                Lable_X1:
                    var RemovesWinner = new List<uint>();
                    foreach (var p in X)
                    {
                        var player = xPlayer.Where(x => x.Uid == p.Key).FirstOrDefault();
                        if (player != null)
                        {
                            foreach (var pot in Pots)
                            {
                                long div = 0;
                                foreach (var W in pot.Value.Players)
                                {
                                    if (X.ContainsKey(W))
                                    {
                                        div += 1;
                                    }
                                }
                                if (pot.Value.Players.Contains(player.Uid) || Exp)
                                {
                                    if (Exp == false)
                                    {
                                        player.Lose += (long)pot.Value.Money / div;
                                    }
                                    else
                                    {
                                        player.Lose += (long)pot.Value.Money / X.Count;
                                    }
                                    if (!RemovesPot.Contains(pot.Key))
                                    {
                                        RemovesPot.Add(pot.Key);
                                    }
                                }
                            }
                        }
                        if (!RemovesWinner.Contains(p.Key))
                        {
                            RemovesWinner.Add(p.Key);
                        }

                    }
                    foreach (var I in RemovesWinner)
                    {
                        if (Hands.ContainsKey(I))
                        {
                            Hands.Remove(I);
                        }
                    }
                    foreach (var I in RemovesPot)
                    {
                        if (Pots.ContainsKey(I))
                        {
                            ((IDictionary<uint, PokerStructs.SidePot>)Pots).Remove(I);
                        }
                    }
                    if (Pots.Count > 0)
                    {
                        foreach (var pot in Pots.Values)
                        {
                            foreach (var p in Hands)
                            {
                                if (pot.Players.Contains(p.Key))
                                {
                                    if (Players.ContainsKey(p.Key))
                                    {
                                        if (Players[p.Key].Fold == false)
                                        {
                                            if (loob3 < 30)
                                            {
                                                loob3++;
                                                goto Lable_X;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (Pots.Count > 0)
                    {
                        foreach (var pot in Pots.Values)
                        {
                            X = CalcaluteBestHands(Winners);
                            Exp = true;
                            if (X.Count >= 1)
                            {
                                bool xxxx = false;
                                foreach (var xx in X)
                                {
                                    if (!Players.ContainsKey(xx.Key))
                                    {
                                        xxxx = true;
                                    }
                                }
                                if (xxxx == false)
                                {
                                    if (loob4 < 30)
                                    {
                                        loob4++;
                                        goto Lable_X1;
                                    }
                                }
                            }
                        }
                    }
                    foreach (var p in Winners)
                    {
                        Players[p.Key].CurrentMoney += (long)Players[p.Key].Lose;
                        if (Players[p.Key].Lose >= (long)Players[p.Key].TotalPot)
                        {
                            Players[p.Key].Lose = (long)Players[p.Key].Lose - (long)((Players[p.Key].TotalPot * 99) / 100);
                        }
                        else
                        {
                            Players[p.Key].Lose = (long)Players[p.Key].Lose - (long)(Players[p.Key].TotalPot);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                TableBusy = false;
            }
            finally
            {
                TableBusy = false;
            }
        }
        internal System.Collections.Concurrent.ConcurrentDictionary<uint, PokerStructs.SidePot> CalcaluteSidePots()
        {

            {
                uint count = 0;
                var dic = new System.Collections.Concurrent.ConcurrentDictionary<uint, PokerStructs.SidePot>();
                var s2 = TempPlayers.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                int I = 0;
                while (s2.Count > 0 && I < 30)
                {
                    I++;
                    count += 1;
                    var SidePot = new PokerStructs.SidePot();
                    var Lowest = s2.FirstOrDefault();
                    SidePot.Money += ((long)Lowest.Value * (long)s2.Count);
                    foreach (var p in s2)
                    {
                        TempPlayers[p.Key] -= Lowest.Value;
                        if (TempPlayers[p.Key] == 0)
                        {
                            TempPlayers.Remove(p.Key);
                        }
                        SidePot.Players.Add(p.Key);
                    }
                    if (SidePot.Players.Count > 1)
                    {
                        SidePot.Money = SidePot.Money * 99 / 100;
                    }
                    dic.TryAdd(count, SidePot);
                    s2 = new Dictionary<uint, ulong>(TempPlayers.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value));
                }
                TempPlayers.Clear();
                return dic;
            }
        }
        internal Dictionary<uint, ulong> CalcaluteBestHands(Dictionary<uint, ulong> Hands)
        {

            var dic = new Dictionary<uint, ulong>();
            foreach (var p in Hands)
            {
                if (dic.Count == 0)
                {
                    dic.Add(p.Key, p.Value);
                }
                else if (p.Value > dic.FirstOrDefault().Value)
                {
                    dic.Clear();
                    dic.Add(p.Key, p.Value);
                }
                else if (p.Value == dic.FirstOrDefault().Value)
                {

                    dic.Add(p.Key, p.Value);

                }
            }
            return dic;
        }
        internal Dictionary<uint, Hand> CalcaluteBestHands(Dictionary<uint, Hand> Hands)
        {

            var dic = new Dictionary<uint, Hand>();
            foreach (var p in Hands)
            {
                if (dic.Count == 0)
                {
                    dic.Add(p.Key, p.Value);
                }
                else if (p.Value > dic.FirstOrDefault().Value)
                {
                    dic.Clear();
                    dic.Add(p.Key, p.Value);
                }
                else if (p.Value == dic.FirstOrDefault().Value)
                {

                    dic.Add(p.Key, p.Value);

                }
            }
            return dic;
        }
        public override string ToString()
        {

            {
                var board = "";
                foreach (var c in Board)
                    if (c != null)
                        if (!string.IsNullOrEmpty(board))
                            board += " " + c;
                        else
                            board = c.ToString();
                return board;
            }
        }
        public byte[] ToArray()
        {
            return _buffer;
        }

        public bool TableBusy = false;

        public int ShowHand { get; set; }
    }
}