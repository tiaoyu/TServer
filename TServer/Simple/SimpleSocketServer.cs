using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TServer.Simple
{
    public class SimpleSocketServer
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

            while (isRunning)
            {
                var str = Console.ReadLine();
                if ("exit".Equals(str))
                {
                    isRunning = false;
                }
            }
            ServerSocket.Close();
        }

        /// <summary>
        /// Accepts the callback.
        /// </summary>
        /// <param name="ar">Ar.</param>
        public void AcceptCallback(IAsyncResult ar)
        {
            var socket = ar.AsyncState as Socket;
            var connection = socket.EndAccept(ar);
            connection.BeginReceive(recvData, 0, recvData.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), connection);
            Console.WriteLine("Connected end, ConnectionId : " + connection.GetHashCode());
            socket.BeginAccept(AcceptCallback, socket);
        }

        /// <summary>
        /// Receives the callback.
        /// </summary>
        /// <param name="ar">Ar.</param>
        public void ReceiveCallback(IAsyncResult ar)
        {
            var ss = (Socket)ar.AsyncState;
            var endLength = ss.EndReceive(ar);
            if (endLength > 0)
            {
                var data = new byte[endLength];
                Array.Copy(recvData, 0, data, 0, endLength);
                Console.WriteLine(Encoding.ASCII.GetString(data));

                ss.BeginReceive(recvData, 0, recvData.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), ss);
            }
            else
            {
                Console.WriteLine("receive end, ConnectionId : " + ss.GetHashCode());
                ss.Close();
            }
        }
    }
}
