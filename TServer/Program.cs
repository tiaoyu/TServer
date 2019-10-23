using System;
using System.Collections.Generic;
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

namespace TServer
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

            Console.WriteLine("Hello Server!");
            while (true)
            {
                log.Debug("asdfasdf");
                System.Threading.Thread.Sleep(1000);
            }

            #region Simple Socket
            //var server = new SimpleSocketServer<string>("127.0.0.1", 11000);
            //server.SetDeserializeFunc((socket, bytes) => Encoding.UTF8.GetString(bytes));
            //server.SetSerializeFunc((socket, strings) =>
            //{
            //    var sendBytes = Encoding.UTF8.GetBytes(strings);
            //    var sendHead = BitConverter.GetBytes(sendBytes.Length);
            //    var sendData = new byte[sendHead.Length + sendBytes.Length];
            //    Array.Copy(sendHead, 0, sendData, 0, sendHead.Length);
            //    Array.Copy(sendBytes, 0, sendData, sendHead.Length, sendBytes.Length);
            //    return sendData;
            //});

            //server.Start();

            //// 处理消息
            //Task.Run(() =>
            //{
            //    while (server.IsRunning)
            //    {
            //        var count = server.MessageHandler.MessageQueue.Count;
            //        for (var i = 0; i < count; ++i)
            //        {
            //            server.MessageHandler.MessageQueue.TryDequeue(out string message);
            //            Console.WriteLine(message);
            //        }
            //    }
            //});

            //// 发送消息
            //while (server.IsRunning)
            //{
            //    var str = Console.ReadLine();
            //    if ("exit".Equals(str))
            //    {
            //        server.IsRunning = false;
            //        continue;
            //    }

            //    var connectionList = new List<int>(server.DicConnection.Keys);
            //    if (connectionList.Count <= 0) continue;

            //    foreach (var connectionId in connectionList)
            //    {
            //        if (!server.DicConnection.TryGetValue(connectionId, out var connection)) continue;

            //        if (connection.Socket.Connected)
            //        {
            //            server.SendMessage(connection.Socket, str);
            //        }
            //        else
            //        {
            //            server.DicConnection.TryRemove(connectionId, out connection);
            //        }
            //    }
            //}

            //server.ServerSocket.Close();
            #endregion Simple Socket

            #region Normal Socket
            var server = new NormalServer(2, 8192);
            server.Init();
            //server.MessageHandler.SetDeserializeFunc((bytes) => Encoding.UTF8.GetString(bytes));
            server.MessageHandler.SetDeserializeFunc((bytes) =>
            {
                var protoId = BitConverter.ToInt32(bytes);
                return ProtocolParser.Instance.GetParser(protoId).ParseFrom(bytes);
            });
            server.MessageHandler.SetSerializeFunc((protocol) =>
            {
                return (protocol as ProtocolBufBase).Serialize();
            });
            server.Start("127.0.0.1", 11000);

            Task.Run(() =>
            {
                while (true)
                {
                    if (server.MessageHandler.MessageQueue.TryDequeue(out object msg))
                    {
                        log.Debug($"Get msg:{msg}");
                    }
                    System.Threading.Thread.Sleep(1000);
                }
            });
            while (true)
            {
                var str = Console.ReadLine();
                var pack = new S2CLogin
                {
                    Res = 0
                };
                foreach (var (_, v) in server.dicEventArgs)
                {
                    server.StartSend(v, pack);
                }
            }
            #endregion Normal Socket
        }
    }
}
