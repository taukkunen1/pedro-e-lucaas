using System;
using HoldemHand;
using System.Net.Sockets;
namespace Poker.Structures
{
    public class PokerStructs
    {
        public class Kick
        {
             public DateTime Time;
             public System.Collections.Generic.List<uint> Accept;
             public System.Collections.Generic.List<uint> Refuse;
             public byte Total;
             public byte TotalRefuse;
             public uint Target;
             public uint Starter;
        }
        internal class SidePot
        {
            private long _Money = 0L;
            internal long Money
            {
                get { return _Money; }
                set { if (value < 0) value = 0; _Money = value; }
            }
            internal System.Collections.Generic.List<uint> Players = new System.Collections.Generic.List<uint>();
        }
        internal class PokerCard
        {
            internal Enums.CardsType Type = Enums.CardsType.Heart;
            internal byte Value = 0;

            public override string ToString()
            {
                var number = (Value + 2).ToString();
                var name = Enum.GetName(typeof(Enums.CardsType), Type);
                if (name != null)
                {
                    var suit = name.Substring(0, 1).ToLower();
                    if (number == "10")
                        number = "t";
                    if (number == "11")
                        number = "j";
                    if (number == "12")
                        number = "q";
                    if (number == "13")
                        number = "k";
                    if (number == "14")
                        number = "a";
                    return number + suit;
                }
                return "";
            }

            public static bool operator >(PokerCard pc1, PokerCard pc2)
            {
                if (pc1.Value == pc2.Value)
                    return pc1.Type > pc2.Type;
                return pc1.Value > pc2.Value;
            }

            public static bool operator <(PokerCard pc1, PokerCard pc2)
            {
                if (pc1.Value == pc2.Value)
                    return pc1.Type < pc2.Type;
                return pc1.Value < pc2.Value;
            }
        }

        public class Player 
        {
            public bool IsPlaying;
            public bool IsPotAllin;
            public bool PotinThisRound;
            public bool Fold;
            public string Name;
            public Enums.PlayerType PlayerType;
            internal PokerCard[] Pocket;
            public long _CurrentMoney;
            public long CurrentMoney
            {
                get { return _CurrentMoney;  }
                set
                {
                    if (value < 0)
                    {
                        value = 0;
                    }
                    _CurrentMoney = value;
                }
            }
          
            public long Lose;
            public ulong RoundPot;
            public byte Seat;
            public PokerTable Table;
            public ulong TotalPot;
            public uint Uid;
            public ulong TempMoney;
            public Player(string name, uint uid)
            {
                Name = name;
                Uid = uid;
            }
            public void Create(Enums.PlayerType pType, byte seat, PokerTable table, ulong money)
            {
                PotinThisRound = false;
                IsPlaying = false;
                IsPotAllin = false;
                Fold = false;
                PlayerType = pType;
                TotalPot = 0;
                RoundPot = 0;
                Seat = seat;
                Lose = 0;
                CurrentMoney = (long)money;
                TempMoney = 0;
                Table = table;
                if (Table.TableType == Enums.TableType.ShowHand)
                {
                    Pocket = new PokerCard[5];
                }
                else
                {
                    Pocket = new PokerCard[2];
                }
            }
            public void Increment(ulong money)
            {
                CurrentMoney += (long)money;
            }

            public void Decrement(ulong money)
            {
                CurrentMoney -= (long)money;
                RoundPot += money;
                TotalPot += money;
                Table.TotalPot += money;
                Table.RoundPot += money;
            }

            public override string ToString()
            {
                var pocket = "";
                foreach (var c in Pocket)
                    if (c != null)
                        if (!string.IsNullOrEmpty(pocket))
                            pocket += " " + c;
                        else
                            pocket = c.ToString();
                return pocket;
            }
        }
    }
}