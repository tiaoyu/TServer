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
                for(var i = 0; i < 10; ++i)
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
            try
            {
                (ar.AsyncState as Socket).EndConnect(ar);
                Console.WriteLine("Connect success!");
            }
            catch(SocketException ex)
            {
                Thread.Sleep(2000);
                Console.WriteLine("Connect failed! Try again..." + ++index);
                ClientSocket.BeginConnect(IPE, ConnectCallBack, ClientSocket);
                return;
            }
            // receive
            ClientSocket.BeginReceive(RecvData, 0, RecvData.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), null);
        }

        /// <summary>
        /// Receives the callback.
        /// </summary>
        /// <param name="ar">Ar.</param>
        public void ReceiveCallBack(IAsyncResult ar)
        {
            var endLength = ClientSocket.EndReceive(ar);
            if (endLength > 0)
            {
                var data = new byte[endLength];
                Array.Copy(RecvData, 0, data, 0, endLength);
                Console.WriteLine(Encoding.ASCII.GetString(data));
                Console.WriteLine("Receive length: " + data.Length);
                ClientSocket.BeginReceive(RecvData, 0, RecvData.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), null);
            }
        }
    }
}
