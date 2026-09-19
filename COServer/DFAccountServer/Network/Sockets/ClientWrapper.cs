using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace AccountServer.Network.Sockets
{
    public unsafe class ClientWrapper
    {
        public enum ShutDownFlags : int
        {
            SD_RECEIVE = 0,
            SD_SEND = 1,
            SD_BOTH = 2
        }
        public int BufferSize;
        public byte[] Buffer;
        public Socket Socket;
        public object Connector;
        public ServerSocket Server;
        public string IP;
        public bool Alive;
        public bool OverrideTiming;
        private IDisposable[] _TimerSubscriptions;
        private Queue<byte[]> _SendQueue;
        private object _SendSyncRoot;
        public Action<byte[], int, ClientWrapper> Callback;
        public void Create(Socket socket, ServerSocket server, Action<byte[], int, ClientWrapper> callBack)
        {
            Callback = callBack;
            BufferSize = 2047;
            Socket = socket;
            Server = server;
            Buffer = new byte[BufferSize];
            LastReceive = Time32.Now;
            OverrideTiming = false;
            _SendQueue = new Queue<byte[]>();
            _SendSyncRoot = new object();
            _TimerSubscriptions = new[]
            {
                World.Subscribe<ClientWrapper>(Program.World.ConnectionReview, this, World.SendPool),
                World.Subscribe<ClientWrapper>(Program.World.ConnectionReceive, this, World.ReceivePool),
                World.Subscribe<ClientWrapper>(Program.World.ConnectionSend, this, World.SendPool)
            };

        }
        public void Send(byte[] data)
        {
            lock (_SendSyncRoot)
                _SendQueue.Enqueue(data);
        }
        public Time32 LastReceive;
        public Time32 LastReceiveCall;
        public void Disconnect()
        {
            lock (Socket)
            {
                int K = 1000;
                while (_SendQueue.Count > 0 && Alive && (K--) > 0)
                    if (!Alive) return;
                Alive = false;
                for (int i = 0; i < _TimerSubscriptions.Length; i++)
                    _TimerSubscriptions[i].Dispose();
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
                Socket.Dispose();
            }
        }
        public static void TryReview(ClientWrapper wrapper)
        {
            if (wrapper.Alive)
            {
                if (wrapper.OverrideTiming)
                {
                    if (Time32.Now > wrapper.LastReceive.AddMilliseconds(180000))
                        wrapper.Server.InvokeDisconnect(wrapper);
                }
                else
                {
                    if (Time32.Now < wrapper.LastReceiveCall.AddMilliseconds(2000))
                        if (Time32.Now > wrapper.LastReceive.AddMilliseconds(6000))
                            wrapper.Server.InvokeDisconnect(wrapper);
                }
            }
        }
        private bool isValid()
        {
            if (!Alive)
            {
                for (int i = 0; i < _TimerSubscriptions.Length; i++)
                    _TimerSubscriptions[i].Dispose();
                return false;
            }
            return true;
        }
       
        private void doReceive(int available)
        {
            LastReceive = Time32.Now;
            try
            {
                if (available > Buffer.Length) available = Buffer.Length;
                int size = Socket.Receive(Buffer, available, SocketFlags.None);
                if (size != 0)
                {
                    if (Callback != null)
                    {
                        Callback(Buffer, size, this);
                    }
                }
                else
                {
                    Server.InvokeDisconnect(this);
                }
            }
            catch (SocketException)
            {
                Server.InvokeDisconnect(this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public static void TryReceive(ClientWrapper wrapper)
        {
            wrapper.LastReceiveCall = Time32.Now;
            if (!wrapper.isValid()) return;
            try
            {
                bool poll = wrapper.Socket.Poll(0, SelectMode.SelectRead);
                int available = wrapper.Socket.Available;
                if (available > 0)
                    wrapper.doReceive(available);
                else if (poll)
                    wrapper.Server.InvokeDisconnect(wrapper);
            }
            catch (SocketException)
            {
                wrapper.Server.InvokeDisconnect(wrapper);
            }
        }
        private bool TryDequeueSend(out byte[] buffer)
        {
            buffer = null;
            lock (_SendSyncRoot)
                if (_SendQueue.Count != 0)
                    buffer = _SendQueue.Dequeue();
            return buffer != null;
        }
        public static void TrySend(ClientWrapper wrapper)
        {
            if (!wrapper.isValid()) return;
            byte[] buffer;

            while (wrapper.TryDequeueSend(out buffer))
            {
                try
                {
                    wrapper.Socket.Send(buffer);
                }
                catch
                {
                    wrapper.Server.InvokeDisconnect(wrapper);
                }
            }
        }
        private static void endSend(IAsyncResult ar)
        {
            var wrapper = ar.AsyncState as ClientWrapper;
            try
            {
                wrapper.Socket.EndSend(ar);
            }
            catch
            {
                wrapper.Server.InvokeDisconnect(wrapper);
            }
        }
    }
}