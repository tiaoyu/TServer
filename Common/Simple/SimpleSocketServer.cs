using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Common.Simple
{
    public class SimpleSocketServer<T> : SimpleSocket<T>
    {
        public int Port;
        public string Host;
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

        public void SendMessage()
        {

        }

    }
}
