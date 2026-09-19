using System;
using System.Net;
using System.Net.Sockets;

namespace AccountServer.Network.Sockets
{
    public unsafe class ServerSocket
    {
        public event Action<ClientWrapper> OnClientConnect, OnClientDisconnect;
        public event Action<byte[], int, ClientWrapper> OnClientReceive;
        private Socket _Connection;
        private ushort _LoginPort;
        private string _IPAddress;
        private bool _Enabled;
        private System.Threading.Thread _Thread;
        public ServerSocket()
        {
            this._Connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _Thread = new System.Threading.Thread(DoSyncAccept);
            _Thread.Start();
        }
        public void Enable(ushort port, string ip)
        {
            this._IPAddress = ip;
            this._LoginPort = port;
            this._Connection.Bind(new IPEndPoint(System.Net.IPAddress.Parse(_IPAddress), this._LoginPort));
            this._Connection.Listen(50);
            this._Enabled = true;
            
        }
        public bool PrintoutIPs = true;
        private void DoSyncAccept()
        {
            while (true)
            {
                if (this._Enabled)
                {
                    try
                    {
                        processSocket(this._Connection.Accept());
                    }
                    catch
                    {
                    }
                }
            }
        }
        private void DoAsyncAccept(IAsyncResult res)
        {
            try
            {
                Socket socket = this._Connection.EndAccept(res);
                processSocket(socket);
                this._Connection.BeginAccept(DoAsyncAccept, null);
            }
            catch
            {
            }
        }
        private void processSocket(Socket socket)
        {
            try
            {
                string ip = (socket.RemoteEndPoint as IPEndPoint).Address.ToString();
                ClientWrapper wrapper = new ClientWrapper();
                wrapper.Create(socket, this, OnClientReceive);
                wrapper.Alive = true;
                wrapper.IP = ip;
                if (this.OnClientConnect != null) this.OnClientConnect(wrapper);
            }
            catch
            {
            }
        }
        public void Reset()
        {
            this.Disable();
            this.Enable();
        }
        public void Disable()
        {
            this._Enabled = false;
            this._Connection.Close(1);
        }
        public void Enable()
        {
            if (!this._Enabled)
            {
                this._Connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._Connection.Bind(new IPEndPoint(IPAddress.Parse(_IPAddress), this._LoginPort));
                this._Connection.Listen((int)SocketOptionName.MaxConnections);
                this._Enabled = true;
            }
        }
        public void InvokeDisconnect(ClientWrapper Client)
        {
            if (this.OnClientDisconnect != null)
                this.OnClientDisconnect(Client);
        }
        public bool Enabled
        {
            get { return this._Enabled; }
        }
    }
}