using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common.Simple;

namespace TServer
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Hello Server!");

            var server = new SimpleSocketServer<string>("127.0.0.1", 11000);
            server.SetDeserializeFunc(bytes => Encoding.UTF8.GetString(bytes));

            server.Start();

            // 处理消息
            Task.Run(() =>
            {
                while (server.IsRunning)
                {
                    var count = server.MessageHandler.MessageQueue.Count;
                    for (var i = 0; i < count; ++i)
                    {
                        server.MessageHandler.MessageQueue.TryDequeue(out string message);
                        Console.WriteLine(message);
                    }
                }
            });

            // 发送消息
            while (server.IsRunning)
            {
                var str = Console.ReadLine();
                if ("exit".Equals(str))
                {
                    server.IsRunning = false;
                    continue;
                }

                var connectionList = new List<int>(server.DicConnection.Keys);
                if (connectionList.Count <= 0) continue;

                var sendBytes = Encoding.UTF8.GetBytes(str);
                var sendHead = BitConverter.GetBytes(sendBytes.Length);
                var sendData = new byte[sendHead.Length + sendBytes.Length];
                Array.Copy(sendHead, 0, sendData, 0, sendHead.Length);
                Array.Copy(sendBytes, 0, sendData, sendHead.Length, sendBytes.Length);
                Console.WriteLine("Send length: " + sendData.Length);

                foreach (var connectionId in connectionList)
                {
                    if (server.DicConnection.TryGetValue(connectionId, out SocketData connection))
                    {
                        if (connection.Socket.Connected)
                        {
                            Console.WriteLine("Send to {0}, length: {1}", connectionId, sendData.Length);
                            connection.Socket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, (ar) =>
                            {
                                (ar.AsyncState as SocketData)?.Socket.EndSend(ar);
                            }, connection);
                        }
                        else
                        {
                            server.DicConnection.TryRemove(connectionId, out connection);
                        }
                    }
                }
            }

            server.ServerSocket.Close();
        }


    }
}
