using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core;
using GameServer.Game.MsgServer;
using GameServer.MsgInterServer.Packets;

namespace GameServer.MsgInterServer
{
   public class StaticConnexion
    {
       public class Entity
       {
           public ServerSockets.SecuritySocket SecuritySocket;

           public Entity(ServerSockets.SecuritySocket _socket)
           {
               _socket.Client = this;
               SecuritySocket = _socket;
           }
           public void Send(ServerSockets.Packet msg)
           {
               SecuritySocket.Send(msg);
           }
           public void Disconnect()
           {
               SecuritySocket.Disconnect();
           }

       }
       public static ServerSockets.ServerSocket ServerConnecxion;
       public static ThreadItem Thread = null;
       public static ServerSockets.SecuritySocket ClientConnecxion;
       public static DateTime SendInfo = new DateTime();



       public static void Create()
       {
           if (ServerConfig.IsInterServer == false)
           {

                ServerConnecxion = new ServerSockets.ServerSocket(ProcessConnect, (p, data) => { ProcesReceive(p, data); }, (p) => { (p.Client as Entity).Disconnect(); });
                Connect();
               if (Thread == null)
               {
                   Thread = new ThreadItem(1000, "InterServer", CheckConnection);
                   Thread.Open();
               }
           }
       }
       public static void Send(ServerSockets.Packet stream)
       {
           if (ClientConnecxion == null)
               return;
           if (ClientConnecxion.Alive == false)
               return;
           ClientConnecxion.Send(stream);
       }
       public static void Connect()
       {
           if (ServerConfig.IsInterServer == false)
           {
               //ServerConnecxion.Connect(Database.GroupServerList.InterServer.IPAddress, Database.GroupServerList.InterServer.Port, "InterServer");
           }
       }
       public static void ProcessConnect(ServerSockets.SecuritySocket Socket)
       {
           if (ServerConfig.IsInterServer == false)
           {
               var obj = new Entity(Socket);
               Socket.OnInterServer = true;
               ClientConnecxion = Socket;

               using (var rec = new ServerSockets.RecycledPacket())
               {
                   var stream = rec.GetStream();

                   var DBServer = Database.GroupServerList.MyServerInfo;
                   obj.Send(stream.ServerInfoCreate(1, DBServer.ID, DBServer.Name, DBServer.MapID, DBServer.X, DBServer.Y, DBServer.Group));
               }
               Socket.ConnectFull = true;
           }
       }
       public static void ProcesReceive(ServerSockets.SecuritySocket obj, ServerSockets.Packet stream)
       {
           var Game = (obj.Client as Entity);
           ushort PacketID = stream.ReadUInt16();
           try
           {
               switch (PacketID)
               {
                   case global::GameServer.Game.GamePackets.Chat:
                       {

                           var mes = new MsgMessage();
                           mes.Deserialize(stream);

                           if (mes.ChatType == MsgMessage.ChatMode.BroadcastMessage)
                           {
                               if (mes.__Message == "[Cross Elite PK Tournament] begins at 20:00. Get yourself prepared for it!")
                               {
                                    Core.IsCrossPkOpen = true;
                                    Core.JoinCrossEliteStamp = DateTime.Now.AddMinutes(60 * 5);
                                   global::GameServer.Game.MsgTournaments.MsgSchedules.SendInvitation("Cross Elite PK Tournament", "[Special Accesory/Boots,Cps and more rewards]", 293, 160, 1002, 0, 60);
                                   break;
                               }
                           }
                        
                           stream.Seek(stream.Size);
                           var x = mes.GetArray(stream);
                           foreach (var user in Pool.GamePoll.Values)
                           {
                               user.Send(x);
                           }

                           break;
                       }
                  
                   case PacketTypes.InterServer_NobilityRank:
                       {
                           int xx = stream.ReadInt32();
                           if (xx == 1)
                           {
                                NobiltyWar = true; 
                               foreach (var user in Pool.GamePoll.Values)
                               {
                                   if (user.Player.NobilityRank >= Role.Instance.Nobility.NobilityRank.Duke)
                                   {
                                       user.Player.MessageBox("NobilityCrossWar Will start in 1 minute would you like to join?",
                                           new Action<Client.GameClient>(p => p.Teleport(303, 292, 1002, 0)), null);
                                   }
                               }
                           }
                           else
                           {
                                NobiltyWar = false;
                           }
                           break;
                       }
               }
           }
           catch (Exception e)
           {
               Console.SaveException(e);
           }
           finally
           {
               ServerSockets.PacketRecycle.Reuse(stream);
           }
       }
       public static void CheckConnection()
       {
           if (ClientConnecxion == null)
               return;

           if (!ClientConnecxion.Alive || ClientConnecxion.Connection.Connected == false)
           {
               ClientConnecxion = null;
               Create();
               return;
           }
           

       }


       public static bool NobiltyWar = false;
    }
}
