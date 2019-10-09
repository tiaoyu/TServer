using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common.LogUtil;
using Common.Normal;
using Common.Simple;

namespace TClient
{
    internal class Program
    {
        private static void Main()
        {
            LogHelp.Init(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogConfig.log4net"));
            LogHelp.TestLog();

            Console.SetIn(new StreamReader(Console.OpenStandardInput(8192)));
            Console.WriteLine("Hello Client!");

            #region Simple Socket
            //var client = new SimpleSocketClient<string>("127.0.0.1", 11000);
            //client.SetDeserializeFunc((socket, bytes) => Encoding.UTF8.GetString(bytes));
            //client.SetSerializeFunc((socket, strings) =>
            //{
            //    var sendBytes = Encoding.UTF8.GetBytes(strings);
            //    var sendHead = BitConverter.GetBytes(sendBytes.Length);
            //    var sendData = new byte[sendHead.Length + sendBytes.Length];
            //    Array.Copy(sendHead, 0, sendData, 0, sendHead.Length);
            //    Array.Copy(sendBytes, 0, sendData, sendHead.Length, sendBytes.Length);
            //    return sendData;
            //});
            //client.RequestHandler.Connected = () => client.SendMessage(client.ClientSocket, "hi~");

            //client.Connect();

            //// 消息读取线程
            //Task.Run(() =>
            //{
            //    while (client.IsRunning)
            //    {
            //        var count = client.MessageHandler.MessageQueue.Count;
            //        for (var i = 0; i < count; ++i)
            //        {
            //            client.MessageHandler.MessageQueue.TryDequeue(out var message);
            //            Console.WriteLine(message);
            //        }
            //    }
            //});



            //// 消息发送
            //while (client.IsRunning)
            //{
            //    var str = Console.ReadLine();
            //    if ("exit".Equals(str))
            //    {
            //        client.IsRunning = false;
            //        continue;
            //    }

            //    if (client.ClientSocket.Connected)
            //    {
            //        client.SendMessage(client.ClientSocket, str);
            //    }
            //    else
            //    {
            //        client.IsRunning = false;
            //    }
            //}
            //client.ClientSocket.Close();

            #endregion Simple Socket

            #region Normal Socket
            var client = new NormalClient(20, 8192);
            client.Init();
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            var listenSocket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            var e = client.CreateNewSocketAsyncEventArgsForConnect();
            e.RemoteEndPoint = remoteEndPoint;

            listenSocket.ConnectAsync(e);
            while (true)
            {
                var str = Console.ReadLine();
                client.StartSend(Encoding.UTF8.GetBytes(str));
            }
            #endregion Normal Socket
        }
    }
}
