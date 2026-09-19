using AccountServer.Interfaces;

namespace AccountServer.Network.AuthPackets
{
    public unsafe class PasswordCryptographySeed : IPacket
    {
        private byte[] Buffer;
        public PasswordCryptographySeed()
        {
            Buffer = new byte[8];
            Writer.WriteUInt16(8, 0, Buffer);
            Writer.WriteUInt16(1059, 2, Buffer);
        }
        public int Seed
        {
            get { return BitConverter.ToInt32(Buffer, 4); }
            set { Writer.WriteInt32(value, 4, Buffer); }
        }
        public void Deserialize(byte[] buffer)
        {
        }
        public byte[] ToArray()
        {
            return Buffer;
        }
    }
}