using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Simple;

namespace TClient
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Hello Client!");

            var client = new SimpleSocketClient<string>("127.0.0.1", 11000);
            client.SetDeserializeFunc(bytes => Encoding.UTF8.GetString(bytes));

            client.Connect();

            // 消息读取线程
            Task.Run(() =>
            {
                while (client.IsRunning)
                {
                    var count = client.MessageHandler.MessageQueue.Count;
                    for (var i = 0; i < count; ++i)
                    {
                        client.MessageHandler.MessageQueue.TryDequeue(out string message);
                        Console.WriteLine(message);
                    }
                }
            });



            // 消息发送
            while (client.IsRunning)
            {
                var str = Console.ReadLine();
                if ("exit".Equals(str))
                {
                    client.IsRunning = false;
                    continue;
                }

                var sendBytes = Encoding.UTF8.GetBytes(str);
                var sendHead = BitConverter.GetBytes(sendBytes.Length);
                var sendData = new byte[sendHead.Length + sendBytes.Length];
                Array.Copy(sendHead, 0, sendData, 0, sendHead.Length);
                Array.Copy(sendBytes, 0, sendData, sendHead.Length, sendBytes.Length);
                Console.WriteLine("Send length: " + sendData.Length);

                if (client.ClientSocket.Connected)
                {
                    client.ClientSocket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None,
                    (ar) =>
                    {
                        (ar.AsyncState as Socket)?.EndSend(ar);
                    }, client.ClientSocket);
                }
                else
                {
                    client.IsRunning = false;
                }
            }
            client.ClientSocket.Close();
        }
    }
}
