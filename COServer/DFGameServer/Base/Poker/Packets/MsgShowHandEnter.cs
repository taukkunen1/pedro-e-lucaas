using System;
using Poker.Structures;

namespace Poker.Packets
{
    public class MsgShowHandEnter
    {
        private readonly byte[] _buffer;

        public MsgShowHandEnter(byte[] buffer)
        {
            if (buffer == null)
            {
                _buffer = new byte[28];
                Packet.WriteUInt16((ushort) (_buffer.Length - 8), 0, _buffer);
                Packet.WriteUInt16(2090, 2, _buffer);
            }
            else
            {
                _buffer = buffer;
            }
        }

        public byte Action
        {
            get { return _buffer[4]; }
            set { _buffer[4] = value; }
        }

        public Enums.PlayerType PlayerType
        {
            get { return (Enums.PlayerType) _buffer[6]; }
            set { _buffer[6] = (byte) value; }
        }

        public ushort Seat
        {
            get { return BitConverter.ToUInt16(_buffer, 8); }
            set { Packet.WriteUInt16(value, 8, _buffer); }
        }

        public uint TableNumber
        {
            get { return BitConverter.ToUInt32(_buffer, 12); }
            set { Packet.WriteUInt32(value, 12, _buffer); }
        }

        public uint PlayerUid
        {
            get { return BitConverter.ToUInt32(_buffer, 16); }
            set { Packet.WriteUInt32(value, 16, _buffer); }
        }
        public byte[] ToArray()
        {
            return _buffer;
        }

        public byte[] ToArray(byte action, PokerStructs.Player player)
        {
            Action = action;
            PlayerType = player.PlayerType;
            Seat = player.Seat;
            TableNumber = player.Table.Number;
            PlayerUid = player.Uid;
            return _buffer;
        }
    }
}