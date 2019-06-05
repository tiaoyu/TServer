using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Common.SimpleSocket
{
    public class SimpleSocketClient:SimpleSocket
    {
        public Socket ClientSocket;
        public int Port;
        public string Host;
        public IPEndPoint IPE;
        public int index = 0;
        public int MaxReconnectCount = 20;
        public bool IsRunning = true;

        public SimpleSocketClient(string host, int port)
        {
            Host = host;
            Port = port;
            IPAddress ip = IPAddress.Parse(Host);
            IPE = new IPEndPoint(ip, Port);
            BufferMgr.Instance.Init();
        }

        /// <summary>
        /// Connect this instance.
        /// </summary>
        public void Connect()
        {
            ClientSocket = new Socket(AddressFamily.InterNetwork
                , SocketType.Stream, ProtocolType.Tcp);
            // connect
            ClientSocket.BeginConnect(IPE, ConnectCallBack, ClientSocket);
        }

        /// <summary>
        /// Connects the callback.
        /// </summary>
        /// <param name="ar">Ar.</param>
        public void ConnectCallBack(IAsyncResult ar)
        {
            var connection = new SocketData(ar.AsyncState as Socket);
            try
            {
                connection.Socket.EndConnect(ar);
                Console.WriteLine("Connect success!");
            }
            catch (SocketException ex)
            {
                Thread.Sleep(2000);
                Console.WriteLine("{0}. Connect failed! Try again...{1}", ex.Message, ++index);
                if (index < MaxReconnectCount)
                {
                    ClientSocket.BeginConnect(IPE, ConnectCallBack, ClientSocket);
                    return;
                }
                else 
                {
                    ClientSocket.Close();
                    Console.WriteLine("Connect time out!");
                    IsRunning = false;
                    return;
                }
            }

            // receive
            ClientSocket.BeginReceive(BufferMgr.Instance.ByteBufferPool, connection.OffsetInBufferPool, BufferMgr.Instance.EachBlockBytes
                    , SocketFlags.None, new AsyncCallback(ReceiveCallBack), connection);
        }
    }
}
