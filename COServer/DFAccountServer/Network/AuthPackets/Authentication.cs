using System;
using System.IO;
using System.Text;


namespace AccountServer.Network.AuthPackets
{
    public unsafe class Authentication : Interfaces.IPacket
    {
        public string Username;
        public string Password;
        public string Server;

        public Authentication()
        {
        }
        public void Deserialize(byte[] buffer)
        {
            MemoryStream MS = new(buffer);
            BinaryReader BR = new(MS);
            ushort len = BR.ReadUInt16();
            ushort id = BR.ReadUInt16();
            if (id == 1124 && len == 240)
            {
                string AccountReceived = Encoding.UTF8.GetString(BR.ReadBytes(64));
                string PasswordReceived = Encoding.UTF8.GetString(BR.ReadBytes(64));
                string Servers = Encoding.UTF8.GetString(BR.ReadBytes(32));
                Username = AccountReceived.Replace("\0", "");
                Password = PasswordReceived.Replace("\0", "");
                Server = Servers.Replace("\0", "");
            }
            BR.Close();
            MS.Close();
        }
        public byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }
}