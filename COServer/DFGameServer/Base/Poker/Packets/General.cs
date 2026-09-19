using System.Linq;
using Poker.Structures;

namespace Poker.Packets
{
    public class General
    {
        public enum HandDealtCard : ushort
        {
            TwoCardDraw = 0,
            ThreeCardDraw = 1,
            FourCardDraw = 2,
            FiveCardDraw = 3,
            OneCardDraw = 4,
            CardDown = 5,
            CardUp = 6
        }
        public static byte[] TimerTick(int time)
        {
            var buffer = new byte[16];
            Packet.WriteUInt16((ushort) (buffer.Length - 8), 0, buffer);
            Packet.WriteUInt16(3400, 2, buffer);
            Packet.WriteByte(8, 4, buffer);
            Packet.WriteByte(21, 5, buffer);
            Packet.WriteByte(16, 6, buffer);
            Packet.WriteByte((byte) time, 7, buffer);
            return buffer;
        }
        public static byte[] Kick(Poker.Structures.PokerStructs.Kick kick, byte Type, uint value, uint T)
        {
            var buffer = new byte[26];
            Packet.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
            Packet.WriteUInt16(2088, 2, buffer);
            Packet.WriteByte(Type, 4, buffer);
            Packet.WriteUInt32(kick.Starter, 5, buffer);
            Packet.WriteUInt32(kick.Target, 9, buffer);
            Packet.WriteUInt32(value, 13, buffer);
            Packet.WriteUInt32(T, 17, buffer);
            return buffer;
        }
        public static byte[] MsgShowHandDealtCard(PokerTable table, ushort counter, HandDealtCard action, uint uid = 0)
        {
            //lock (table.TableSyncRoot)
            {
                byte[] buffer;
                switch (action)
                {
                    case HandDealtCard.CardDown:
                        {
                            buffer = new byte[44 + table.Players.Values.Where(p => p.IsPlaying).ToList().Count * 8 + 8];
                            Packet.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                            Packet.WriteUInt16(2091, 2, buffer);
                            Packet.WriteUInt16((ushort)action, 6, buffer);
                            Packet.WriteUInt16((ushort)1, 8, buffer);
                            Packet.WriteUInt16((ushort)table.Players[uid].Pocket[0].Value, 10, buffer);
                            Packet.WriteUInt16((ushort)table.Players[uid].Pocket[0].Type, 20, buffer);
                            Packet.WriteUInt16((ushort)table.Players.Values.Where(p => p.IsPlaying).ToList().Count, 30, buffer);
                            Packet.WriteUInt32(table.Dealer, 32, buffer);
                            var offset = 44;
                            foreach (var x in table.Players.Values.Where(p => p.IsPlaying).OrderByDescending(p => p.Seat))
                            {
                                Packet.WriteUInt16(13, offset, buffer);
                                offset += 2;
                                Packet.WriteUInt16(4, offset, buffer);
                                offset += 2;
                                Packet.WriteUInt32(x.Uid, offset, buffer);
                                offset += 4;
                            }
                            return buffer;
                        }
                    case HandDealtCard.CardUp:
                        {
                           
                            buffer = new byte[44 + ((table.Players.Values.Where(p => p.IsPlaying).ToList().Count * 8) * counter) + 8];
                            Packet.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                            Packet.WriteUInt16(2091, 2, buffer);
                            Packet.WriteUInt16((ushort)action, 6, buffer);
                            Packet.WriteUInt16((ushort)counter, 8, buffer);
                            Packet.WriteUInt16((ushort)(table.Players.Values.Where(p => p.IsPlaying).ToList().Count * counter), 30, buffer);
                            Packet.WriteUInt32(table.Dealer, 32, buffer);
                            var offset = 44;
                            for (int i = 0; i < counter; i++)
                            {
                                table.ShowHand += 1;
                                int seat = table.Players[table.Dealer].Seat;
                                uint px = table.Dealer;
                                for (int x = 0; x < table.Players.Values.Where(p => p.IsPlaying).ToList().Count; x++)
                                {
                                    Packet.WriteUInt16(table.Players[px].Pocket[table.ShowHand].Value, offset, buffer);
                                    offset += 2;
                                    Packet.WriteUInt16((ushort)table.Players[px].Pocket[table.ShowHand].Type, offset, buffer);
                                    offset += 2;
                                    Packet.WriteUInt32(table.Players[px].Uid, offset, buffer);
                                    offset += 4;
                                    px = table.NextSeat((byte)seat);
                                    seat = table.Players[px].Seat;
                                }
                            }
                            return buffer;
                        }
                    case HandDealtCard.OneCardDraw:
                        {
                            buffer = new byte[44 + table.Players.Values.Where(p => p.IsPlaying).ToList().Count * 8 + 8];
                            Packet.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                            Packet.WriteUInt16(2091, 2, buffer);
                            Packet.WriteUInt16(counter, 4, buffer);
                            Packet.WriteUInt16((ushort)action, 6, buffer);
                            Packet.WriteUInt16((ushort)table.Players.Values.Where(p => p.IsPlaying).ToList().Count, 30, buffer);
                            Packet.WriteUInt32(table.Dealer, 32, buffer);
                            var offset = 44;
                            foreach (var x in table.Players.Values.Where(p => p.IsPlaying).OrderByDescending(p => p.Seat))
                            {
                                Packet.WriteUInt16(x.Pocket[0].Value, offset, buffer);
                                offset += 2;
                                Packet.WriteUInt16((ushort)x.Pocket[0].Type, offset, buffer);
                                offset += 2;
                                Packet.WriteUInt32(x.Uid, offset, buffer);
                                offset += 4;
                            }
                            return buffer;
                        }
                    case HandDealtCard.ThreeCardDraw:
                    case HandDealtCard.FourCardDraw:
                    case HandDealtCard.FiveCardDraw:
                        {
                               buffer = new byte[44 + 8];
                            Packet.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                            Packet.WriteUInt16(2091, 2, buffer);
                            Packet.WriteUInt16((ushort)action, 6, buffer);

                            if (action == HandDealtCard.ThreeCardDraw)
                            {
                                Packet.WriteUInt16((ushort)3, 8, buffer);
                                Packet.WriteUInt16((ushort)(table.Board[0] != null ? table.Board[0].Value : 0), 10, buffer);
                                Packet.WriteUInt16((ushort)(table.Board[1] != null ? table.Board[1].Value : 0), 12, buffer);
                                Packet.WriteUInt16((ushort)(table.Board[2] != null ? table.Board[2].Value : 0), 14, buffer);
                                Packet.WriteUInt16((ushort)(table.Board[0] != null ? table.Board[0].Type : 0), 20, buffer);
                                Packet.WriteUInt16((ushort)(table.Board[1] != null ? table.Board[1].Type : 0), 22, buffer);
                                Packet.WriteUInt16((ushort)(table.Board[2] != null ? table.Board[2].Type : 0), 24, buffer);
                            }
                            else if (action == HandDealtCard.FourCardDraw)
                            {
                                Packet.WriteUInt16((ushort)1, 8, buffer);
                                Packet.WriteUInt16((ushort)(table.Board[3] != null ? table.Board[3].Value : 0), 10, buffer);
                                Packet.WriteUInt16((ushort)(table.Board[3] != null ? table.Board[3].Type : 0), 20, buffer);
                            }
                            else if (action == HandDealtCard.FiveCardDraw)
                            {
                                Packet.WriteUInt16((ushort)1, 8, buffer);
                                Packet.WriteUInt16((ushort)(table.Board[4] != null ? table.Board[4].Value : 0), 10, buffer);
                                Packet.WriteUInt16((ushort)(table.Board[4] != null ? table.Board[4].Type : 0), 20, buffer);
                            }
                            Packet.WriteUInt16((ushort)table.Players.Values.Where(p => p.IsPlaying).ToList().Count, 30, buffer);
                            Packet.WriteUInt32(table.Dealer, 32, buffer);
                            Packet.WriteUInt32(table.SmallBlind, 36, buffer);
                            Packet.WriteUInt32(table.BigBlind, 40, buffer);
                            return buffer;
                        }
                    case HandDealtCard.TwoCardDraw:
                        {
                            buffer = new byte[44 + table.Players.Values.Where(p => p.IsPlaying).ToList().Count * 8 + 8];
                            Packet.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                            Packet.WriteUInt16(2091, 2, buffer);
                            Packet.WriteUInt16((ushort)action, 6, buffer);
                            if (table.Players.ContainsKey(uid))
                            {
                                Packet.WriteUInt16((ushort)table.Players[uid].Pocket.Where(p => p != null).ToList().Count, 8, buffer);
                                if (table.Players[uid].IsPlaying)
                                {
                                    Packet.WriteUInt16(table.Players[uid].Pocket[0].Value, 10, buffer);
                                    Packet.WriteUInt16(table.Players[uid].Pocket[1].Value, 12, buffer);
                                    Packet.WriteUInt16((ushort)table.Players[uid].Pocket[0].Type, 20, buffer);
                                    Packet.WriteUInt16((ushort)table.Players[uid].Pocket[1].Type, 22, buffer);
                                }
                            }
                            else
                            {
                                Packet.WriteUInt16(2, 8, buffer);
                                Packet.WriteUInt16(13, 10, buffer);
                                Packet.WriteUInt16(13, 12, buffer);
                                Packet.WriteUInt16((ushort)
                                4, 20, buffer);
                                Packet.WriteUInt16((ushort)
                                4, 22, buffer);
                            }
                            Packet.WriteUInt16((ushort)table.Players.Values.Where(p => p.IsPlaying).ToList().Count, 30, buffer);
                            Packet.WriteUInt32(table.Dealer, 32, buffer);
                            Packet.WriteUInt32(table.SmallBlind, 36, buffer);
                            Packet.WriteUInt32(table.BigBlind, 40, buffer);
                            int seat = table.Players[table.CurrentPlayer].Seat;
                            var offset = 44;
                            uint px = table.CurrentPlayer;
                            for (int x = 0; x < table.Players.Values.Where(p => p.IsPlaying).ToList().Count; x++)
                            {
                                Packet.WriteUInt16(13, offset, buffer);
                                offset += 2;
                                Packet.WriteUInt16(4, offset, buffer);
                                offset += 2;
                                Packet.WriteUInt32(table.Players[px].Uid, offset, buffer);
                                offset += 4;
                                px = table.NextSeat((byte)seat);
                                seat = table.Players[px].Seat;
                            }
                            return buffer;
                        }
                }
                return null;
            }
        }
        public static byte[] MsgShowHandState(PokerTable table)
        {
            //lock (table.TableSyncRoot)
            {
                byte time = 0;
                var players = table.Players.Values.Where(px => px.IsPlaying && px.Fold == false).ToList();
                byte cards = (byte)table.Board.Where(p => p != null).ToList().Count;
                byte action = 0;
                if (players.FirstOrDefault().Pocket.Where(p => p != null).ToList().Count == 1)
                    action = 4;
                if (players.FirstOrDefault().Pocket.Where(p => p != null).ToList().Count == 2)
                    action = 0;
                if (cards == 3)
                    action = 1;
                if (cards == 4)
                    action = 2;
                if (cards == 5)
                    action = 3;
                if (table.Time > System.DateTime.Now)
                {
                    time = (byte)table.Time.Subtract(System.DateTime.Now).TotalSeconds;
                }
                var buffer = new byte[53 + (11 * players.Count)];
                Packet.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                Packet.WriteUInt16(2098, 2, buffer);
                Packet.WriteByte(action, 4, buffer);
                Packet.WriteByte(cards, 5, buffer);
                Packet.WriteByte((byte)players.Count, 6, buffer);
                Packet.WriteByte(time, 7, buffer);
                Packet.WriteUInt32(table.CurrentPlayer, 19, buffer);
                Packet.WriteUInt32(table.Dealer, 23, buffer);
                Packet.WriteUInt32(table.BigBlind, 27, buffer);
                Packet.WriteUInt32(table.SmallBlind, 31, buffer);
                if (action == 1 || action == 2 || action == 3)
                {
                    Packet.WriteByte((byte)(table.Board[0] != null ? table.Board[0].Type : 0), 35, buffer);
                    Packet.WriteByte((byte)(table.Board[0] != null ? table.Board[0].Value : 0), 36, buffer);
                    Packet.WriteByte((byte)(table.Board[1] != null ? table.Board[1].Type : 0), 37, buffer);
                    Packet.WriteByte((byte)(table.Board[1] != null ? table.Board[1].Value : 0), 38, buffer);
                    Packet.WriteByte((byte)(table.Board[2] != null ? table.Board[2].Type : 0), 39, buffer);
                    Packet.WriteByte((byte)(table.Board[2] != null ? table.Board[2].Value : 0), 40, buffer);
                    Packet.WriteByte((byte)(table.Board[3] != null ? table.Board[3].Type : 0), 41, buffer);
                    Packet.WriteByte((byte)(table.Board[3] != null ? table.Board[3].Value : 0), 42, buffer);
                    Packet.WriteByte((byte)(table.Board[4] != null ? table.Board[4].Type : 0), 43, buffer);
                    Packet.WriteByte((byte)(table.Board[4] != null ? table.Board[4].Value : 0), 44, buffer);
                }
                int offset = 45;
                if (action == 4)
                {
                    foreach (var p in players)
                    {
                        Packet.WriteByte(1, offset, buffer);
                        offset++;
                        Packet.WriteByte((byte)(p.Pocket[0] != null ? p.Pocket[0].Type : 0), offset, buffer);
                        offset++;
                        Packet.WriteByte((byte)(p.Pocket[0] != null ? p.Pocket[0].Value : 0), offset, buffer);
                        offset++;
                        Packet.WriteByte(1, offset, buffer);
                        offset += 4;
                        Packet.WriteUInt32(p.Uid, offset, buffer);
                        offset += 4;
                    }
                }
                else if (action != 4 && action != 0)
                {
                    foreach (var p in players)
                    {
                        Packet.WriteByte(2, offset, buffer);
                        offset++;
                        Packet.WriteByte((byte)(p.Pocket[0] != null ? 4 : 0), offset, buffer);
                        offset++;
                        Packet.WriteByte((byte)(p.Pocket[0] != null ? 13 : 0), offset, buffer);
                        offset += 2;
                        Packet.WriteByte((byte)(p.Pocket[1] != null ? 4 : 0), offset, buffer);
                        offset++;
                        Packet.WriteByte((byte)(p.Pocket[1] != null ? 13 : 0), offset, buffer);
                        offset += 2;
                        Packet.WriteUInt32(p.Uid, offset, buffer);
                        offset += 4;
                    }
                }
                return buffer;
            }
        }
        public static byte[] MsgShowHandActivePlayer(PokerTable table, ushort counter, uint uid)
        {
            if (table.TableType == Enums.TableType.TexasHoldem)
            {
                ushort action = table.GetRequiredAction();
                var buffer = new byte[28];
                Packet.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                Packet.WriteUInt16(2092, 2, buffer);
                Packet.WriteUInt16(counter, 4, buffer);
                Packet.WriteUInt16(action, 6, buffer);
                Packet.WriteUInt64(table.MinBet, 8, buffer);
                if (!table.UnLimited)
                {
                    if (table.NumberOfRaise == 0)
                    {
                        if (table.State == Poker.Enums.TableState.Pocket || table.State == Poker.Enums.TableState.Flop)
                        {
                            Packet.WriteUInt64(table.MinBet, 8, buffer);
                        }
                        else
                        {
                            Packet.WriteUInt64(table.MinBet * 2, 8, buffer);
                        }
                    }
                    else
                    {
                        if (table.State == Poker.Enums.TableState.Pocket)
                        {
                            Packet.WriteUInt64(table.MinBet, 8, buffer);
                        }
                        else
                        {
                            Packet.WriteUInt64(table.MinBet * 2, 8, buffer);
                        }
                    }
                }
                if (table.UnLimited)
                {
                    if (table.PreviousPlayer.RoundPot != 0)
                    {
                        Packet.WriteUInt64(table.PreviousPlayer.RoundPot, 8, buffer);
                    }
                }
                if (table.CurrentPlayer != 0)
                {
                    Packet.WriteUInt64((ulong)((ulong)table.Players[table.CurrentPlayer].CurrentMoney - (ulong)table.RequiredPot), 12, buffer);
                    Packet.WriteUInt32(table.CurrentPlayer, 16, buffer);
                }
                return buffer;
            }
            else
            {
                ushort action = table.GetRequiredAction();
                var buffer = new byte[28];
                Packet.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                Packet.WriteUInt16(2092, 2, buffer);
                Packet.WriteUInt16(counter, 4, buffer);
                Packet.WriteUInt16(action, 6, buffer);
                Packet.WriteUInt64(table.MinBet, 8, buffer);
                if (table.UnLimited)
                {
                    if (table.PreviousPlayer.RoundPot != 0)
                    {
                        Packet.WriteUInt64(table.PreviousPlayer.RoundPot, 8, buffer);
                    }
                }
                if (table.CurrentPlayer != 0)
                {
                    if (table.Showhand == false)
                    {
                        Packet.WriteUInt64((ulong)(table.LowestMoney) - table.RequiredPot, 12, buffer);
                        Packet.WriteUInt32(table.CurrentPlayer, 16, buffer);
                    }
                    else
                    {
                        Packet.WriteUInt64((ulong)table.RequiredPot, 12, buffer);
                        Packet.WriteUInt32(table.CurrentPlayer, 16, buffer);
                    }
                }
                return buffer;
            }
        }

        public static byte[] MsgShowHandLayCard(PokerTable table)
        {
            if (table.TableType == Enums.TableType.TexasHoldem)
            {
                var list = table.Players.Values.Where(p => p.IsPlaying && p.Fold == false).ToList();
                var buffer = new byte[9 + 8 + (list.Count * 12)];
                Packet.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                Packet.WriteUInt16(2094, 2, buffer);
                Packet.WriteUInt16((ushort)list.Count, 6, buffer);
                int offset = 8;
                foreach (var p in list)
                {

                    Packet.WriteUInt16(p.Pocket[0] == null ? (ushort)0 : p.Pocket[0].Value, offset, buffer); offset += 2;
                    Packet.WriteUInt16(p.Pocket[1] == null ? (ushort)0 : p.Pocket[1].Value, offset, buffer); offset += 2;
                    Packet.WriteUInt16(p.Pocket[0] == null ? (ushort)0 : (ushort)p.Pocket[0].Type, offset, buffer); offset += 2;
                    Packet.WriteUInt16(p.Pocket[1] == null ? (ushort)0 : (ushort)p.Pocket[1].Type, offset, buffer); offset += 2;
                    Packet.WriteUInt32(p.Uid, offset, buffer); offset += 4;
                }
                return buffer;
            }
            else
            {
                var list = table.Players.Values.Where(p => p.IsPlaying && p.Fold == false).ToList();
                var buffer = new byte[9 + 8 + (list.Count * 12)];
                Packet.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                Packet.WriteUInt16(2094, 2, buffer);
                Packet.WriteUInt16((ushort)list.Count, 6, buffer);
                int offset = 8;
                foreach (var p in list)
                {

                    Packet.WriteUInt16(p.Pocket[0] == null ? (ushort)0 : p.Pocket[0].Value, offset, buffer); offset += 4;
                    Packet.WriteUInt16(p.Pocket[0] == null ? (ushort)0 : (ushort)p.Pocket[0].Type, offset, buffer); offset += 4;
                    Packet.WriteUInt32(p.Uid, offset, buffer); offset += 4;
                }
                return buffer;
            }
        }

        public static byte[] MsgShowHandGameResult(PokerTable table)
        {
           lock (table.TableSyncRoot)
            {
                int count = table.Players.Values.Where(p => p.IsPlaying).ToList().Count;
                var buffer = new byte[9 + 8 + (count * 15)];
                Packet.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                Packet.WriteUInt16(2095, 2, buffer);
                Packet.WriteUInt16((ushort)10, 4, buffer);
                Packet.WriteUInt16((ushort)count, 6, buffer);
                int offset = 8;
                foreach (var p in table.Players.Values.Where(p => p.IsPlaying))
                {
                    if (p.CurrentMoney < table.MinBet * 10)
                    {
                        Packet.WriteByte(1, offset, buffer); offset += 1;
                    }
                    else
                    {
                        Packet.WriteByte(0, offset, buffer); offset += 1;
                    }

                    if (p.Fold)
                    {
                        Packet.WriteUInt16(3, offset, buffer); offset += 2;
                    }
                    else if (p.Lose < 0)
                    {
                        Packet.WriteUInt16(255, offset, buffer); offset += 2;
                    }
                    else if (p.Lose > 0)
                    {
                        Packet.WriteUInt16(0, offset, buffer); offset += 2;
                    }

                    Packet.WriteUInt32(p.Uid, offset, buffer); offset += 4;
                    Packet.WriteInt64(p.Lose, offset, buffer); offset += 8;
                }
                return buffer;
            }
        }

        public struct ActivePlayerTypes
        {
            public static byte Bet = 1;
            public static byte Call = 2;
            public static byte Fold = 4;
            public static byte Check = 8;
            public static byte Raise = 16;
            public static byte Allin = 32;
        }

      
    }
}