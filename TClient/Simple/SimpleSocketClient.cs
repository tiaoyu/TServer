using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TClient.Simple
{
    public class SimpleSocketClient
    {

        public Socket ClientSocket;
        public byte[] RecvData = new byte[4096];
        public int Port;
        public string Host;
        public IPEndPoint IPE;
        public int index = 0;
        public SimpleSocketClient(string host, int port)
        {
            Host = host;
            Port = port;
            IPAddress ip = IPAddress.Parse(Host);
            IPE = new IPEndPoint(ip, Port);
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

            // send
            while (true)
            {
                var str = Console.ReadLine();
                if ("exit".Equals(str)) break;
                for (var i = 0; i < 10; ++i)
                {
                    var sendBytes = Encoding.ASCII.GetBytes(str);
                    var sendHead = BitConverter.GetBytes(sendBytes.Length);
                    var sendData = new byte[sendHead.Length + sendBytes.Length];
                    Array.Copy(sendHead, 0, sendData, 0, sendHead.Length);
                    Array.Copy(sendBytes, 0, sendData, sendHead.Length, sendBytes.Length);
                    Console.WriteLine("Send length: " + sendData.Length);
                    ClientSocket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, (ar) =>
                    {
                        ((Socket)ar.AsyncState).EndSend(ar);
                    }, ClientSocket);
                }
            }
            ClientSocket.Close();
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
                Console.WriteLine("Connect failed! Try again..." + ++index);
                ClientSocket.BeginConnect(IPE, ConnectCallBack, ClientSocket);
                return;
            }
            // receive
            ClientSocket.BeginReceive(BufferMgr.Instance.ByteBufferPool, connection.OffsetInBufferPool, BufferMgr.Instance.EachBlockBytes
                    , SocketFlags.None, new AsyncCallback(ReceiveCallBack), connection);
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
