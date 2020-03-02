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

    /// <summary>
    /// Sample:
    /// <p>
    /// var client = new SimpleSocketClient<string>("127.0.0.1", 11000);
    /// client.SetDeserializeFunc((socket, bytes) => Encoding.UTF8.GetString(bytes));
    /// client.SetSerializeFunc((socket, strings) =>
    /// {
    ///     var sendBytes = Encoding.UTF8.GetBytes(strings);
    ///     var sendHead = BitConverter.GetBytes(sendBytes.Length);
    ///     var sendData = new byte[sendHead.Length + sendBytes.Length];
    ///     Array.Copy(sendHead, 0, sendData, 0, sendHead.Length);
    ///     Array.Copy(sendBytes, 0, sendData, sendHead.Length, sendBytes.Length);
    ///     return sendData;
    /// });
    /// client.RequestHandler.Connected = () => client.SendMessage(client.ClientSocket, "hi~");
    /// client.Connect();
    /// // 消息读取线程
    /// Task.Run(() =>
    /// {
    ///     while (client.IsRunning)
    ///     {
    ///         var count = client.MessageHandler.MessageQueue.Count;
    ///         for (var i = 0; i < count; ++i)
    ///         {
    ///             client.MessageHandler.MessageQueue.TryDequeue(out var message);
    ///             Console.WriteLine(message);
    ///         }
    ///     }
    /// });
    /// // 消息发送
    /// while (client.IsRunning)
    /// {
    ///     var str = Console.ReadLine();
    ///     if ("exit".Equals(str))
    ///     {
    ///         client.IsRunning = false;
    ///         continue;
    ///     }
    ///     if (client.ClientSocket.Connected)
    ///     {
    ///         client.SendMessage(client.ClientSocket, str);
    ///     }
    ///     else
    ///     {
    ///         client.IsRunning = false;
    ///     }
    /// }
    /// client.ClientSocket.Close();
    /// </p>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete("Don't use OldMethod, use NewMethod instead", true)]
    /// <summary>
    /// 简单版客户端socket
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
                        //IsRunning = false;
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
