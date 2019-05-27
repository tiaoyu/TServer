using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TClient.Simple
{
    public class SimpleSocketClient
    {
        public Socket socket;
        public byte[] recvData = new byte[4096];
        public int Port;
        public string Host;
        public SimpleSocketClient(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public void Connect()
        {
            IPAddress ip = IPAddress.Parse(Host);
            IPEndPoint ipe = new IPEndPoint(ip, Port);

            socket = new Socket(AddressFamily.InterNetwork
                , SocketType.Stream, ProtocolType.Tcp);

            // connect
            socket.BeginConnect(ipe, (ar) =>
            {
                ((Socket)ar.AsyncState).EndConnect(ar);
            }, socket);

            // receive
            socket.BeginReceive(recvData, 0, recvData.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), null);

            // send
            while (true)
            {
                var str = Console.ReadLine();
                if ("exit".Equals(str)) break;

                var sendBytes = Encoding.ASCII.GetBytes(str);
                var sendHead = BitConverter.GetBytes(sendBytes.Length);
                var sendData = new byte[sendHead.Length + sendBytes.Length];
                Array.Copy(sendHead, sendData, sendHead.Length);
                Array.Copy(sendBytes, sendData, sendBytes.Length);
                socket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, (ar) =>
                {
                    ((Socket)ar.AsyncState).EndSend(ar);
                }, socket);
            }
            socket.Close();
        }
        /// <summary>
        /// Receives the call back.
        /// </summary>
        /// <param name="ar">Ar.</param>
        public void ReceiveCallBack(IAsyncResult ar)
        {
            var endLength = socket.EndReceive(ar);
            if (endLength > 0)
            {
                var data = new byte[endLength];
                Array.Copy(recvData, 0, data, 0, endLength);
                Console.WriteLine(Encoding.ASCII.GetString(data));

                socket.BeginReceive(recvData, 0, recvData.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), null);
            }
        }
    }
}
