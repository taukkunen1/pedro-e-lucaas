using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet ServerInfoCreate(this ServerSockets.Packet stream, MsgServerInfo.Action Mode, uint DwParam)
        {
            stream.InitWriter();

         //   stream.Write(DateTime.Now.Value);
            stream.Write((uint)Mode);
            stream.Write(DwParam);
            stream.ZeroFill(28);

            stream.Finalize(GamePackets.ServerInfo);
            return stream;
        }

    }
   public class MsgServerInfo
    {
       public enum Action : uint
       {
           Timer = 0,
           Vigor = 2
       }
      
    }
}
