using System;
using Poker.Structures;

namespace Poker.Packets
{
    public class MsgShowHandExit
    {
        private readonly byte[] _buffer;

        public MsgShowHandExit(byte[] buffer)
        {
            if (buffer == null)
            {
                _buffer = new byte[24];
                Packet.WriteUInt16((ushort) (_buffer.Length - 8), 0, _buffer);
                Packet.WriteUInt16(2096, 2, _buffer);
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

        public uint TableNumber
        {
            get { return BitConverter.ToUInt32(_buffer, 8); }
            set { Packet.WriteUInt32(value, 8, _buffer); }
        }

        public uint PlayerUid
        {
            get { return BitConverter.ToUInt32(_buffer, 12); }
            set { Packet.WriteUInt32(value, 12, _buffer); }
        }

        public byte[] ToArray()
        {
            return _buffer;
        }

        public byte[] ToArray(byte action, PokerStructs.Player player)
        {
            Action = action;
            TableNumber = player.Table.Number;
            PlayerUid = player.Uid;
            return _buffer;
        }
    }
}