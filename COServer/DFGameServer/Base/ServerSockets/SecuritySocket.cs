using GameServer.Role;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace GameServer.ServerSockets
{
    public unsafe class SecuritySocket
    {
        public ReceiveBuffer ReceiveBuff;
        public bool IsCOServer
        {
            get
            {
                return Crypto != null;
            }
        }

        public bool SetDHKey = false;
        public object SendRoot;
        public Action<SecuritySocket> OnDisconnect;
        private Action<SecuritySocket, Packet> _OnReceiveHandler;
        public Socket Connection;
        public object Client;
        private Queue<byte[]> _OnSend;
        private IDisposable[] _TimerSubscriptions = null;
        public bool Alive = false;
        public Cryptography.TQCast5 Crypto;
        public bool OnInterServer = false;
        public string RemoteIp
        {
            get
            {
                try
                {
                    if (Connection == null)
                        return "NONE";
                    return (Connection.RemoteEndPoint as IPEndPoint).Address.ToString();
                }
                catch
                {
                    return "NONE";
                }
            }
        }
        public DateTime LastReceive;
        public Client.GameClient Game;
        public ServerSocket Server;

        public bool ConnectFull = false;

        public SecuritySocket(ServerSocket serversocket, Action<SecuritySocket> _OnDisconnect, Action<SecuritySocket, Packet> _OnReceiveHandler)
        {
            Server = serversocket;
            this._OnReceiveHandler = _OnReceiveHandler;
            OnDisconnect = _OnDisconnect;


        }
        public SecuritySocket(Action<SecuritySocket> _OnDisconnect, Action<SecuritySocket, Packet> _OnReceiveHandler)
        {
            this._OnReceiveHandler = _OnReceiveHandler;
            OnDisconnect = _OnDisconnect;
        }
        public bool Connect(string IPAddres, ushort port, out Socket _socket)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult asyncResult = _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(IPAddres), port), null, null);
            uint count = 0;
            while (!asyncResult.IsCompleted && count < 10)
            {
                count++;
                System.Threading.Thread.Sleep(100);
            }
            if (asyncResult.IsCompleted)
            {
                _socket.Blocking = false;
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                } else
                {
                    LingerOption lingerOption = new LingerOption(false, 0);
                    _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
                }
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                _socket.ReceiveBufferSize = _socket.SendBufferSize = ServerSockets.ReceiveBuffer.RECV_BUFFER_SIZE;
                SocketPoll.ConnectionPoll.Add(this);
            }
            return asyncResult.IsCompleted;
        }
        public void Create(Socket _socket)
        {
            try
            {
                ReceiveBuff = new ReceiveBuffer(ServerConfig.Port_ReceiveSize);
                Connection = _socket;
                SetDHKey = false;
                Alive = true;
                LastReceive = DateTime.Now;
                SendRoot = new object();
                _OnSend = new Queue<byte[]>();
                Client = Crypto = null;
                _TimerSubscriptions = new IDisposable[]
      {
      };
                SocketPoll.ConnectionPoll.Add(this);
            }
            catch (Exception e)
            {
                Console.SaveException(e);
            }
        }
        public void SetCrypto(Cryptography.TQCast5 Crypt)
        {
            if (!ServerConfig.IsInterServer)
            {
                Crypto = Crypt;
            }
        }

        public unsafe void Send(Packet msg)
        {
            try
            {
                if (Alive)
                {
                    lock (SendRoot)
                    {

                        byte[] _buffer = new byte[msg.Size];
                        fixed (byte* ptr = _buffer)
                            msg.memcpy(ptr, msg.Memory, msg.Size);
                        if (Crypto != null)
                            Crypto.Encrypt(msg.Memory, 0, _buffer, 0, msg.Size);
                        else
                            fixed (byte* ptr = _buffer)
                                msg.memcpy(ptr, msg.Memory, msg.Size);

                        _OnSend.Enqueue(_buffer);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public ReceiveBuffer DHKeyBuffer = new ReceiveBuffer(1024, true);
        public ReceiveBuffer EncryptedDHKeyBuffer = new ReceiveBuffer(1024, true);
        public bool IsCompleteDHKey(out int type)
        {

            type = 0;
            try
            {
                if (DHKeyBuffer.Length() < 8)
                    return false;

                byte[] buffer = new byte[Packet.SealSize];
                for (int x = 0; x < buffer.Length; x++)
                    buffer[x] = DHKeyBuffer.buffer[x + (DHKeyBuffer.Length() - Packet.SealSize)];

                string Text = Encoding.UTF8.GetString(DHKeyBuffer.buffer);

                bool accept = Text.Contains("TQClient");
                if (!Text.EndsWith("TQClient"))
                    type = 1;
                return accept;
            }
            catch (Exception e)
            {
                Console.SaveException(e);
                Disconnect();
                return false;
            }
        }
        public unsafe bool ReceiveDHKey()
        {
            try
            {
                if (Alive)
                {
                    int rec_type = 0;
                    if (!SetDHKey && Alive)
                    {
                        SocketError Socket_Error = SocketError.IsConnected;
                        int length = DHKeyBuffer.MaxLength() - DHKeyBuffer.Length();
                        if (length <= 0)
                        {
                            Disconnect();
                            return false;
                        }
                        int ret = Connection.Receive(DHKeyBuffer.buffer, DHKeyBuffer.Length(), length, SocketFlags.None, out Socket_Error);
                        if (ret > 0)
                        {
                            System.Buffer.BlockCopy(DHKeyBuffer.buffer, DHKeyBuffer.Length(), EncryptedDHKeyBuffer.buffer, EncryptedDHKeyBuffer.Length(), ret);
                            EncryptedDHKeyBuffer.AddLength(ret);
                            if (Crypto != null)
                            {
                                fixed (byte* ptr = DHKeyBuffer.buffer)
                                    Crypto.Decrypt(DHKeyBuffer.buffer, DHKeyBuffer.Length(), ptr, DHKeyBuffer.Length(), ret);
                            }
                            DHKeyBuffer.AddLength(ret);
                            if (IsCompleteDHKey(out rec_type))
                            {

                                using (var rec = new RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    stream.Seek(0);
                                    fixed (byte* ptr = DHKeyBuffer.buffer)
                                        stream.memcpy(stream.Memory, ptr, DHKeyBuffer.Length());

                                    stream.Size = DHKeyBuffer.Length();

                                    if (_OnReceiveHandler != null)
                                        _OnReceiveHandler.Invoke(this, stream);
                                }
                            }
                        }
                        else if (ret == 0)//<= 0 || Socket_Error == SocketError.ConnectionAborted)
                        {
                            if (Socket_Error != SocketError.WouldBlock)
                                Disconnect();
                            return false;
                        }
                        else//ret < 0
                        {
                            if (Socket_Error != SocketError.WouldBlock)
                                Disconnect();
                        }


                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Disconnect();
            }
            return false;
        }
        public bool ReceiveBuffer()
        {
            if (Alive)
            {
                {
                    try
                    {
                        if (!ConnectFull)
                            return false;
                        if (!SetDHKey && Crypto != null)
                        {
                            ReceiveDHKey();
                        }
                        else
                        {
                            try
                            {
                                if (!Alive)
                                    return false;
                                if (!Receive())
                                {
                                    return false;
                                }

                            }
                            catch (Exception e)
                            {
                                Console.SaveException(e);
                                Disconnect();
                            }
                            return true;
                        }


                    }
                    catch (Exception e) { Disconnect(); Console.WriteLine(e.ToString()); }
                }
            }
            return false;
        }

        public unsafe bool HandlerBuffer()
        {
            int counts = 30;
            while (true && counts > 0)
            {
                counts--;
                if (!Alive)
                    return false;
                try
                {

                    if (!ConnectFull)
                        return false;
                    if (ReceiveBuff.Length() == 0)
                        return false;
                    int Length = (int)(ReceiveBuff.ReadHead() + (IsCOServer ? 8 : 0));
                    if (ServerConfig.IsInterServer || OnInterServer)
                        Length += 8;

                    if (Length < 2)
                        return false;
                    if (Length > ServerSockets.ReceiveBuffer.HeadSize)
                    {
                        Disconnect();
                        return false;
                    }
                    if (Length > ReceiveBuff.Length())
                        return false;

                    LastReceive = DateTime.Now;
                    Packet Stream = PacketRecycle.Take();

                    Stream.Seek(0);

                    fixed (byte* ptr = ReceiveBuff.buffer)
                    {
                        Stream.memcpy(Stream.stream, ptr, Length);
                        if (Length < ReceiveBuff.Length())
                        {
                            fixed (void* next_buffer = &ReceiveBuff.buffer[Length])
                            {
                                Stream.memcpy(ptr, next_buffer, ReceiveBuff.Length() - Length);
                            }
                        }
                        Stream.Size = Length;

                        ReceiveBuff.DelLength(Length);
                    }

                    Stream.SeekForward(2);

                    if (_OnReceiveHandler != null)
                        _OnReceiveHandler.Invoke(this, Stream);

                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
            return false;
        }

        public bool Receive()
        {
            if (Alive)
            {
                SocketError Socket_Error = SocketError.IsConnected;
                try
                {
                    int length = ReceiveBuff.MaxLength() - ReceiveBuff.Length();
                    int ret = Connection.Receive(ReceiveBuff.buffer, ReceiveBuff.Length(), length, SocketFlags.None, out Socket_Error);
                    if (ret > 0)
                    {

                        if (Crypto != null)
                        {

                            fixed (byte* ptr = ReceiveBuff.buffer)
                                Crypto.Decrypt(ReceiveBuff.buffer, ReceiveBuff.Length(), ptr, ReceiveBuff.Length(), ret);
                        }
                        ReceiveBuff.AddLength(ret);

                        return true;
                    }
                    else if (ret == 0)
                    {
                        if (Socket_Error != SocketError.WouldBlock)
                            Disconnect();
                        return false;
                    }
                    else
                    {
                        if (Socket_Error != SocketError.WouldBlock)
                            Disconnect();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine(Socket_Error.ToString());

                }
            }
            return false;
        }

        public bool CanDequeue(out byte[] data)
        {
            data = null;


            if (_OnSend.Count > 0)
            {
                lock (SendRoot)
                    data = _OnSend.Dequeue();
            }
            return data != null;
        }

        public int nPutBytes = 0;
        public byte[] packet = null;
        public static bool TrySend(SecuritySocket _socket)
        {

            if (_socket.Alive)
            {
                if (!_socket.ConnectFull)
                    return false;
#if ! TEST
                if (_socket._OnSend.Count > 2500)
                {
                    if (_socket.packet != null)
                        Console.WriteLine("the sync is " + _socket._OnSend.Count + " PACKET ID: " + _socket.packet);
                    else
                        Console.WriteLine("the sync is " + _socket._OnSend.Count);
                    _socket.Disconnect();
                    return false;
                }
#endif
                SocketError sError = SocketError.Success;
                int ret = 0;
                if (_socket.packet != null)
                {
                    //   if (WindowsAPI.ws2_32.CanWrite(_socket))
                    {
                        try
                        {
                            int nLen = _socket.packet.Length;
                            ret = _socket.Connection.Send(_socket.packet, _socket.nPutBytes, nLen - _socket.nPutBytes, SocketFlags.None, out sError);
                            if (ret > 0)
                            {
                                _socket.nPutBytes += ret;//Connection are NonBlocking = true
                                if (_socket.nPutBytes >= nLen)
                                {
                                    _socket.nPutBytes = 0;
                                    _socket.packet = null;
                                }
                                else
                                {
                                    if (sError != SocketError.WouldBlock)
                                    {
                                        _socket.Disconnect();
                                        return false;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            else if (ret <= 0)
                            {
                                if (sError != SocketError.WouldBlock)
                                {
                                    _socket.Disconnect();
                                    return false;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.SaveException(e);
                            _socket.Disconnect();
                            return false;
                        }
                    }
                }

                // int counts = 50;
                //    while (counts > 0 /*&& WindowsAPI.ws2_32.CanWrite(_socket)*/ && _socket.CanDequeue(out _socket.packet))
                if (_socket.CanDequeue(out _socket.packet))
                {
                    try
                    {

                        if (!_socket.Alive || _socket.packet == null)
                            return false;
                        int nLen = _socket.packet.Length;
                        ret = _socket.Connection.Send(_socket.packet, _socket.nPutBytes, nLen - _socket.nPutBytes, SocketFlags.None, out sError);
                        if (ret > 0)
                        {
                            _socket.nPutBytes += ret;//Connection are NonBlocking = true? yes
                            if (_socket.nPutBytes < _socket.packet.Length)
                            {
                                if (sError != SocketError.WouldBlock)
                                {
                                    _socket.Disconnect();
                                    return false;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                _socket.packet = null;
                                _socket.nPutBytes = 0;
                                return true;
                            }
                        }
                        else
                        {
                            if (sError != SocketError.WouldBlock)
                            {
                                _socket.Disconnect();
                                return false;
                            }
                            else
                            {
                                return false;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteException(e);
                        _socket.Disconnect();
                        return false;
                    }
                }

            }

            return false;
        }

        public object SynRoot = new object();
        public void Disconnect()
        {
            lock (SynRoot)
            {
                if (Alive)
                {
                    SocketPoll.ConnectionPoll.Remove(this);
                    _OnSend.Clear();
                    if (Server != null)
                        Server.Clients.Remove(this);

                    Alive = false;
                    if (_TimerSubscriptions != null)
                    {
                        for (int i = 0; i < _TimerSubscriptions.Length; i++)
                            if (_TimerSubscriptions[i] != null)
                                _TimerSubscriptions[i].Dispose();
                    }
                    try
                    {
                        Game.DisconectStopFunctions();
                        Connection.Shutdown(SocketShutdown.Both);
                        Connection.Close();
                        Connection.Dispose();
                        GC.SuppressFinalize(Connection);
                    }
                    catch (Exception e)
                    {
                        Console.SaveException(e);
                    }
                    finally
                    {
                        if (OnDisconnect != null)
                            OnDisconnect.Invoke(this);
                    }
                }
            }
        }
    }
}
