using System;

namespace GameServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static void GetTick(this ServerSockets.Packet stream, out int Time, out uint UID)
        {
            Time = stream.ReadInt32();
            UID = stream.ReadUInt32();
        }

        public static unsafe ServerSockets.Packet SendTick(this ServerSockets.Packet stream, Role.Player client)
        {
            int Tick = Environment.TickCount;
            stream.InitWriter();
            client.Owner.SendSysMesage("Tick: " + Tick);
            stream.Write(Tick);
            stream.Write(client.UID);
            stream.Finalize(GamePackets.MsgTick);
            return stream;
        }

    }
    public static unsafe class MsgTick
    {

        [PacketAttribute(GamePackets.MsgTick)]
        public unsafe static void TickHandle(Client.GameClient client, ServerSockets.Packet packet)
        {
            int Time;
            uint UID;
            packet.GetTick(out Time, out UID);
            Console.WriteLine("Time: " + Time + " UID:" + UID);
        }
    }
}
