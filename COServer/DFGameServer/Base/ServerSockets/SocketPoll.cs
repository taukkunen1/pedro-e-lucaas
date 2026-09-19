using Core;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace GameServer.ServerSockets
{
    public class SocketPoll
    {
        const int SOCKET_PROCESS_INTERVAL = 20, FD_SETSIZE = 2048;

        public static MyList<SecuritySocket> ConnectionPoll = new MyList<SecuritySocket>();

        private static ServerSocket[] Sockets;
        public SocketPoll(string GroupName, params ServerSocket[] _Sockets)
        {
            Sockets = _Sockets;
            var ThreadItem = new ThreadItem(SOCKET_PROCESS_INTERVAL, GroupName, CheckUp);
            ThreadItem.Open();
        }
        public static void CheckUp()
        {
            try
            {
                List<Socket> RecSockets = new List<Socket>();
                if (ConnectionPoll.Count > 0 || Sockets.Length > 0)
                {
                    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                    timer.Start();
                    foreach (var socket in Sockets)
                    {
                        if (socket == null || socket.IsAlive == false)
                            continue;
                        RecSockets.Add(socket.GetConnection);
                    }
                    foreach (var socket in ConnectionPoll.GetValues())
                    {
                        if (socket.Alive && socket.Connection.Connected)
                            RecSockets.Add(socket.Connection);
                        else
                            socket.Disconnect();
                    }
                    foreach (var socket in ConnectionPoll.GetValues())
                    {
                        try
                        {
                            socket.ReceiveBuffer();
                            socket.HandlerBuffer();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            continue;
                        }
                        try
                        {
                            while (SecuritySocket.TrySend(socket)) ;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                    foreach (var socket in Sockets)
                    {
                        if (socket == null)
                            continue;
                        socket.Accept();
                    }
                    timer.Stop();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
