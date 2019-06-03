using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TServer.Simple
{
    public class SimpleSocketServer
    {
        public byte[] recvData = new byte[4096];
        public bool isRunning = true;
        public int Port = 11000;
        public string Host = "127.0.0.1";
        public Socket ServerSocket;
        public ConcurrentDictionary<int, SocketData> DicConnection;
        public Dictionary<int, SocketData> DicSocketData;
        public SimpleSocketServer(string host, int port)
        {
            Host = host;
            Port = port;
            DicConnection = new ConcurrentDictionary<int, SocketData>();
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
                    continue;
                }

                var connectionList = new List<int>(DicConnection.Keys);
                if (connectionList.Count <= 0) continue;

                var sendBytes = Encoding.ASCII.GetBytes(str);
                var sendHead = BitConverter.GetBytes(sendBytes.Length);
                var sendData = new byte[sendHead.Length + sendBytes.Length];
                Array.Copy(sendHead, 0, sendData, 0, sendHead.Length);
                Array.Copy(sendBytes, 0, sendData, sendHead.Length, sendBytes.Length);
                Console.WriteLine("Send length: " + sendData.Length);

                foreach (var connectionId in connectionList)
                {
                    if (DicConnection.TryGetValue(connectionId, out SocketData connection))
                    {
                        if (connection.Socket.Connected)
                        {
                            Console.WriteLine("Send to {0}, length: {1}", connectionId, sendData.Length);
                            connection.Socket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, (ar) =>
                            {
                                (ar.AsyncState as SocketData).Socket.EndSend(ar);
                            }, connection);
                        }
                        else
                        {
                            DicConnection.TryRemove(connectionId, out connection);
                        }
                    }
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

            SocketData socketData = new SocketData(connection);

            DicConnection.TryAdd(connection.GetHashCode(), socketData);

            socketData.Socket.BeginReceive(BufferMgr.Instance.ByteBufferPool, socketData.OffsetInBufferPool, BufferMgr.Instance.EachBlockBytes
                , SocketFlags.None, new AsyncCallback(ReceiveCallBack), socketData);
            Console.WriteLine("Connected end, ConnectionId : " + connection.GetHashCode());

            socket.BeginAccept(AcceptCallback, socket);
        }

        /// <summary>
        /// Receives the callback.
        /// </summary>
        /// <param name="ar">Ar.</param>
        public void ReceiveCallBack(IAsyncResult ar)
        {
            var ss = ar.AsyncState as SocketData;
            // 当前接收到的字节数
            var recvLength = ss.Socket.EndReceive(ar);
            if (recvLength > 0)
            {
                Console.WriteLine("endLength: " + recvLength);
                // 当前剩余需要处理的字节长度
                var remainProcessLength = recvLength;
                do
                {
                    remainProcessLength = BufferMgr.Instance.ReadBufferFromPool(ss, remainProcessLength);

                } while (remainProcessLength != 0);


                ss.Socket.BeginReceive(BufferMgr.Instance.ByteBufferPool, ss.OffsetInBufferPool, BufferMgr.Instance.EachBlockBytes
                    , SocketFlags.None, new AsyncCallback(ReceiveCallBack), ss);
            }
            else
            {
                Console.WriteLine("Connection closed, ConnectionId : " + ss.GetHashCode());
                ss.Socket.Close();
            }
        }
    }
}
