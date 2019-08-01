using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common.Simple;

namespace TClient
{
    internal class Program
    {
        private static void Main()
        {
            Console.SetIn(new StreamReader(Console.OpenStandardInput(8192)));
            Console.WriteLine("Hello Client!");

            var client = new SimpleSocketClient<string>("127.0.0.1", 11000);
            client.SetDeserializeFunc((socket, bytes) => Encoding.UTF8.GetString(bytes));
            client.SetSerializeFunc((socket, strings) =>
            {
                var sendBytes = Encoding.UTF8.GetBytes(strings);
                var sendHead = BitConverter.GetBytes(sendBytes.Length);
                var sendData = new byte[sendHead.Length + sendBytes.Length];
                Array.Copy(sendHead, 0, sendData, 0, sendHead.Length);
                Array.Copy(sendBytes, 0, sendData, sendHead.Length, sendBytes.Length);
                return sendData;
            });
            client.RequestHandler.Connected = () => client.SendMessage(client.ClientSocket, "hi~");

            client.Connect();

            // 消息读取线程
            Task.Run(() =>
            {
                while (client.IsRunning)
                {
                    var count = client.MessageHandler.MessageQueue.Count;
                    for (var i = 0; i < count; ++i)
                    {
                        client.MessageHandler.MessageQueue.TryDequeue(out var message);
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

                if (client.ClientSocket.Connected)
                {
                    client.SendMessage(client.ClientSocket, str);
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
