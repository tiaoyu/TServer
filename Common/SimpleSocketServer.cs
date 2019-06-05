using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.SimpleSocket
{
    public class SimpleSocketServer : SimpleSocket
    {
        public byte[] recvData = new byte[4096];
        public bool isRunning = true;
        public int Port = 11000;
        public string Host = "127.0.0.1";
        public Socket ServerSocket;

        public SimpleSocketServer(string host, int port)
        {
            Host = host;
            Port = port;
            DicConnection = new ConcurrentDictionary<int, SocketData>();
            BufferMgr.Instance.Init();
        }

        public void Start()
        {
            IPAddress ip = IPAddress.Parse(Host);
            IPEndPoint ipe = new IPEndPoint(ip, Port);

            ServerSocket = new Socket(AddressFamily.InterNetwork
                , SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(ipe);
            ServerSocket.Listen(0);

            // accept
            ServerSocket.BeginAccept(AcceptCallback, ServerSocket);
        }


    }
}
