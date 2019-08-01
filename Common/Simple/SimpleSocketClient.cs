using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Simple
{
    public enum ESocketStatus
    {
        Connected,
        Received,
        Closed
    }
    public class RequestHandler
    {

        public Action Received;
        public Action Connected;
        public void DoAction(int status)
        {
            switch (status)
            {
                case (int)ESocketStatus.Received:
                    Received();
                    break;
                case (int)ESocketStatus.Connected:
                    Connected();
                    break;
            }
        }
    }

    public class SimpleSocketClient<T> : SimpleSocket<T>
    {
        public Socket ClientSocket;
        public RequestHandler RequestHandler;
        public int Port;
        public string Host;
        public IPEndPoint Ipe;
        private int _index;
        public int MaxReconnectCount = 20;


        public SimpleSocketClient(string host, int port)
        {
            RequestHandler = new RequestHandler();
            Host = host;
            Port = port;
            var ip = IPAddress.Parse(Host);
            Ipe = new IPEndPoint(ip, Port);
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
            ClientSocket.BeginConnect(Ipe, ConnectCallBack, ClientSocket);

            // 断线重连线程
            Task.Run(() =>
            {
                while (IsRunning)
                {
                    if (!ClientSocket.Connected && WaitToReconnect)
                    {
                        Thread.Sleep(5000);
                        Connect();
                        WaitToReconnect = false;
                        _index = 0;
                    }
                    Thread.Sleep(1000);
                }
            });
        }

        /// <summary>
        /// Connects the callback.
        /// </summary>
        /// <param name="ar">Ar.</param>
        public void ConnectCallBack(IAsyncResult ar)
        {
            try
            {
                (ar.AsyncState as Socket)?.EndConnect(ar);
                var connection = new SocketData(ar.AsyncState as Socket);
                Console.WriteLine("Connect success!");
                RequestHandler.DoAction((int)ESocketStatus.Connected);

                // receive
                ClientSocket.BeginReceive(BufferMgr.Instance.ByteBufferPool, connection.OffsetInBufferPool, BufferMgr.Instance.EachBlockBytes
                    , SocketFlags.None, ReceiveCallBack, connection);
            }
            catch (SocketException ex)
            {
                Thread.Sleep(2000);
                Console.WriteLine("{0}. Connect failed! Try again...{1}", ex.Message, ++_index);
                if (_index < MaxReconnectCount)
                {
                    ClientSocket.BeginConnect(Ipe, ConnectCallBack, ClientSocket);
                }
                else
                {
                    ClientSocket.Close();
                    Console.WriteLine("Connect time out!");
                    IsRunning = false;
                }
            }
        }

        public void Close()
        {
            ClientSocket.Close();
            IsRunning = false;
            WaitToReconnect = true;
        }
    }
}
