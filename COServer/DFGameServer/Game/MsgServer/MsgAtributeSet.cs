using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {

        public static unsafe void GetAtributeSet(this ServerSockets.Packet stream, out uint Str, out uint Agi, out uint vit, out uint spi)
        {
            //   uint timerstamp = stream.ReadUInt32();
            uint UID = stream.ReadUInt32();
            Str = (uint)stream.ReadUInt32();
            Agi = (uint)stream.ReadUInt32();
            vit = (uint)stream.ReadUInt32();
            spi = (uint)stream.ReadUInt32();
        }
        public static unsafe ServerSockets.Packet AtributeSetCreate(this ServerSockets.Packet stream, uint Str, uint Agi, uint vit, uint spi)
        {
            stream.InitWriter();

            //   stream.Write(DateTime.Now.Value);
            stream.Write((uint)0);//unknow
            stream.Write(Str);
            stream.Write(Agi);
            stream.Write(vit);
            stream.Write(spi);

            stream.Finalize(GamePackets.AtributeSet);
            return stream;
        }
    }

    public unsafe struct MsgAtributeSet
    {
        [PacketAttribute(GamePackets.AtributeSet)]
        private static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {

            uint Str;
            uint Agi;
            uint Vit;
            uint Spi;

            stream.GetAtributeSet(out Str, out Agi, out Vit, out Spi);

            if (user.Player.Atributes == 0)
                return;

            uint TotalStatPoints = Str + Agi + Vit + Spi;

            if (user.Player.Atributes >= TotalStatPoints)
            {
                user.Player.Strength += (ushort)Str;
                user.Player.Vitality += (ushort)Vit;
                user.Player.Spirit += (ushort)Spi;
                user.Player.Agility += (ushort)Agi;
                user.Player.Atributes -= (ushort)TotalStatPoints;

                user.Send(stream.AtributeSetCreate(Str, Agi, Vit, Spi));

                user.Equipment.QueryEquipment(user.Equipment.Alternante, false);
            }
        }
    }
}
