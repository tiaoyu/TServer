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


            Task.Run(() =>
            {
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
                    Array.Copy(sendHead, sendData, sendHead.Length);
                    Array.Copy(sendBytes, sendData, sendBytes.Length);

                    foreach (var connectionId in connectionList)
                    {
                        if (DicConnection.TryGetValue(connectionId, out SocketData connection))
                        {
                            if (connection.Socket.Connected)
                            {
                                Console.WriteLine("Send to {0}, length: {1}", connectionId, sendData.Length);
                                connection.Socket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, (ar) =>
                                {
                                    (ar.AsyncState as Socket).EndSend(ar);
                                }, connection);
                            }
                            else
                            {
                                DicConnection.TryRemove(connectionId, out connection);
                            }
                        }
                    }
                }
            });

            while (isRunning)
            {
                Stopwatch st = new Stopwatch();
                st.Start();
                var connectionList = new List<int>(DicConnection.Keys);
                if (connectionList.Count <= 0) continue;
                foreach (var connectionId in connectionList)
                {
                    if (DicConnection.TryGetValue(connectionId, out SocketData connection))
                    {
                        if (connection.Socket.Connected)
                        {
                            byte[] buff;
                            do
                            {
                                BufferMgr.Instance.ReadBufferFromPool(out buff, connection);
                                if (buff != null)
                                {
                                    Array.Copy(buff, 0, connection.Message, connection.MessageOffet, buff.Length);
                                }
                            } while (connection._waitReadLength > 0);
                            if (connection.Message != null)
                            {

                                Console.WriteLine(Encoding.ASCII.GetString(connection.Message));
                                connection.Message = null;
                            }
                        }
                    }
                }
                var sleepTime = 30 - st.ElapsedMilliseconds;
                //Console.WriteLine("spendTime:{0}", st.ElapsedMilliseconds);
                Thread.Sleep(sleepTime > 0 ? (int)sleepTime : 0);
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

            socketData.Socket.BeginReceive(recvData, 0, recvData.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socketData);
            Console.WriteLine("Connected end, ConnectionId : " + connection.GetHashCode());

            socket.BeginAccept(AcceptCallback, socket);
        }

        /// <summary>
        /// Receives the callback.
        /// </summary>
        /// <param name="ar">Ar.</param>
        public void ReceiveCallback(IAsyncResult ar)
        {
            var ss = ar.AsyncState as SocketData;
            // 当前接收到的字节数
            var recvLength = ss.Socket.EndReceive(ar);
            if (recvLength > 0)
            {
                Console.WriteLine("endLength: " + recvLength);
                var data = new byte[recvLength];
                Array.Copy(recvData, 0, data, 0, recvLength);
                BufferMgr.Instance.WriteBufferIntoPool(recvData, recvLength, ss);
                //if (recvLength < 4)
                //{
                //    Array.Copy(recvData, 0, data, 0, recvLength);
                //}
                //else
                //{
                //    var headLength = new byte[4];
                //    Array.Copy(recvData, 0, headLength, 0, 4);
                //    Console.WriteLine("Head: " + BitConverter.ToInt32(headLength));
                //    Array.Copy(recvData, 4, data, 0, recvLength - 4);
                //}
                //Console.WriteLine(Encoding.ASCII.GetString(data));
                //Console.WriteLine("Receive length: " + data.Length);
                //Array.Clear(recvData, 0, recvData.Length);
                ss.Socket.BeginReceive(recvData, 0, recvData.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), ss);
            }
            else
            {
                Console.WriteLine("Connection closed, ConnectionId : " + ss.GetHashCode());
                ss.Socket.Close();
            }
        }
    }
}
