using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common.LogUtil;
using Common.Normal;
using Common.Protobuf;
using Common.Simple;
using Google.Protobuf;

namespace TClient
{
    internal class Program
    {
        private static LogHelp log;
        private static void Main()
        {
            LogHelp.Init(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogConfig.log4net"));
            LogHelp.TestLog();
            log = LogHelp.GetLogger(typeof(Program));

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
            client.MessageHandler.SetDeserializeFunc((bytes, guid) =>
            {
                var protoId = BitConverter.ToInt32(bytes);
                return ProtocolParser.Instance.GetParser(protoId).ParseFrom(bytes);
            });
            client.MessageHandler.SetSerializeFunc((protocol) =>
            {
                return (protocol as ProtocolBufBase).Serialize();
            });

            client.StartConnect("127.0.0.1", 11000);

            Task.Run(() =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var t1 = stopwatch.ElapsedMilliseconds;
                var t2 = stopwatch.ElapsedMilliseconds;
                while (true)
                {
                    t1 = stopwatch.ElapsedMilliseconds;
                    var count = client.MessageHandler.MessageQueue.Count;
                    for (var i = 0; i < count; ++i)
                    {
                        if (client.MessageHandler.MessageQueue.TryDequeue(out var msg))
                        {
                            (msg as ProtocolBufBase).OnProcess();
                        }
                    }
                    t2 = stopwatch.ElapsedMilliseconds;
                    var t = (int)(t2 - t1);
                    System.Threading.Thread.Sleep(t < 30 ? 30 - t : 1);
                }
            });

            while (true)
            {
                var str = Console.ReadLine();
                var args = str.Split(' ');
                switch (args[0])
                {
                    case "register":
                        if (args.Length > 2)
                        {

                            var pack = new C2SRegister
                            {
                                Name = args[1],
                                Password = args[2]
                            };
                            client.StartSend(pack);
                        }
                        break;

                    case "login":
                        if (args.Length > 2)
                        {
                            var pack = new C2SLogin
                            {
                                Name = args[1],
                                Password = args[2]
                            };
                            client.StartSend(pack);
                        }
                        break;
                    case "move":
                        if (args.Length > 2)
                        {
                            var pack = new C2SMove
                            {
                                X = double.Parse(args[1]),
                                Y = double.Parse(args[2])
                            };
                            client.StartSend(pack);
                        }
                        break;
                    default:
                        break;
                }

            }
        }

        #endregion Normal Socket
    }
}
