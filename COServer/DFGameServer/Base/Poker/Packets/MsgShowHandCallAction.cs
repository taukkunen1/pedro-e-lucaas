using System;

namespace Poker.Packets
{
    public class MsgShowHandCallAction
    {
        private readonly byte[] _buffer;

        public MsgShowHandCallAction(byte[] buffer)
        {
            if (buffer == null)
            {
                _buffer = new byte[28];
                Packet.WriteUInt16((ushort) (_buffer.Length - 8), 0, _buffer);
                Packet.WriteUInt16(2093, 2, _buffer);
            }
            else
            {
                _buffer = buffer;
            }
        }

        public ushort Action
        {
            get { return BitConverter.ToUInt16(_buffer, 6); }
            set { Packet.WriteUInt16(value, 6, _buffer); }
        }

        public ulong RoundPot
        {
            get { return BitConverter.ToUInt64(_buffer, 8); }
            set { Packet.WriteUInt64(value, 8, _buffer); }
        }

        public ulong TotalPot
        {
            get { return BitConverter.ToUInt64(_buffer, 16); }
            set { Packet.WriteUInt64(value, 16, _buffer); }
        }

        public uint Uid
        {
            get { return BitConverter.ToUInt32(_buffer, 24); }
            set { Packet.WriteUInt32(value, 24, _buffer); }
        }

        public byte[] ToArray(ulong r, ulong t)
        {
            RoundPot = r;
            TotalPot = t;
            return _buffer;
        }
    }
}