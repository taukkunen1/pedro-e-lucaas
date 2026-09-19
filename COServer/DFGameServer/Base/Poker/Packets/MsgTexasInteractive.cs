using System;
using Poker.Structures;

namespace Poker.Packets
{
    public class MsgTexasInteractive
    {
        private readonly byte[] _buffer;

        public MsgTexasInteractive(byte[] buffer)
        {
            if (buffer == null)
            {
                _buffer = new byte[28];
                Packet.WriteUInt16((ushort) (_buffer.Length - 8), 0, _buffer);
                Packet.WriteUInt16(2171, 2, _buffer);
            }
            else
            {
                _buffer = buffer;
            }
        }

        public Enums.TableInteractiveType InteractiveType
        {
            get { return (Enums.TableInteractiveType) _buffer[4]; }
            set { _buffer[4] = (byte) value; }
        }

        public uint TableId
        {
            get { return BitConverter.ToUInt32(_buffer, 8); }
            set { Packet.WriteUInt32(value, 8, _buffer); }
        }

        public uint PlayerUid
        {
            get { return BitConverter.ToUInt32(_buffer, 12); }
            set { Packet.WriteUInt32(value, 12, _buffer); }
        }

        public byte Seat
        {
            get { return _buffer[16]; }
            set { _buffer[16] = value; }
        }

        public byte[] ToArray()
        {
            return _buffer;
        }

        public byte[] ToArray(Enums.TableInteractiveType interactivetype, PokerStructs.Player player)
        {
            InteractiveType = interactivetype;
            TableId = player.Table.Id;
            PlayerUid = player.Uid;
            Seat = player.Seat;
            return _buffer;
        }
    }
}