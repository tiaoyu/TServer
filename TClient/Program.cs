using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common.SimpleSocket;

namespace TClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Client!");

            SimpleSocketClient client = new SimpleSocketClient("127.0.0.1", 11000);
            client.Connect();

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

            // 发送消息
            while (client.IsRunning)
            {
                var str = Console.ReadLine();
                if ("exit".Equals(str))
                {
                    client.IsRunning = false;
                    continue;
                }

                var sendBytes = Encoding.ASCII.GetBytes(str);
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
                        (ar.AsyncState as Socket).EndSend(ar);
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
