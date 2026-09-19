namespace Poker
{
    public class Enums
    {
        public enum CardsType : byte
        {
            Heart = 0,
            Spade = 1,
            Club = 2,
            Diamond = 3
        }

        public enum PlayerType : byte
        {
            Player = 1,
            Watcher = 2
        }

        public enum TableInteractiveType : byte
        {
            Join = 0,
            Leave = 1,
            Watch = 4
        }

        public enum TableState : byte
        {
            Unopened = 0,
            Pocket = 1,
            Flop = 2,
            Turn = 3,
            River = 4,
            ShowDown = 5
        }

        public enum TableType : byte
        {
            TexasHoldem = 1,
            ShowHand = 2
        }

        public enum TableUpdate : byte
        {
            Statue = 233,
            Chips = 234,
            PlayerCount = 235
        }
    }
}