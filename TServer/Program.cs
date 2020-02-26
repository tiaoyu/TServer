using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.LogUtil;
using Common.Normal;
using Common.Protobuf;
using Common.Simple;
using Common.TTimer;
using Google.Protobuf;
using TServer.ECSEntity;
using TServer.ECSSystem.Dungeon;

namespace TServer
{
    internal class Program
    {
        private static LogHelp log;
        public static NormalServer Server;
        public static Dictionary<Guid, ERole> DicRole = new Dictionary<Guid, ERole>();
        public static TimerManager TimerManager = new TimerManager();
        private static void Main()
        {
            LogHelp.Init(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogConfig.log4net"));
            LogHelp.TestLog();
            log = LogHelp.GetLogger(typeof(Program));

            Console.SetIn(new StreamReader(Console.OpenStandardInput(8192)));

            Console.WriteLine("Hello Server!");

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
            Server = new NormalServer(128, 8192);
            Server.Init();
            //server.MessageHandler.SetDeserializeFunc((bytes) => Encoding.UTF8.GetString(bytes));
            Server.MessageHandler.SetDeserializeFunc((bytes, guid) =>
            {
                var protoId = BitConverter.ToInt32(bytes);
                return new ExtSocket { Protocol = ProtocolParser.Instance.GetParser(protoId).ParseFrom(bytes) as ProtocolBufBase, Guid = guid };
            });
            Server.MessageHandler.SetSerializeFunc((protocol) =>
            {
                return (protocol as ProtocolBufBase).Serialize();
            });
            Server.Start("127.0.0.1", 11000);

            // 主循环
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var t1 = stopwatch.ElapsedMilliseconds;
            var t2 = stopwatch.ElapsedMilliseconds;
            TimerManager.Init();
            TimerManager.Insert(6000, 6000, int.MaxValue, null, (obj) =>
            {
                foreach (var (_, role) in DicRole)
                {
                    log.Info($"role id:{role.Id}");
                }
            });

            while (true)
            {
                t1 = stopwatch.ElapsedMilliseconds;

                var count = Server.MessageHandler.MessageQueue.Count;
                while (count-- > 0)
                {
                    if (Server.MessageHandler.MessageQueue.TryDequeue(out object msg))
                    {
                        log.Debug($"Get msg:{msg}");
                        var ss = (msg as ExtSocket);
                        ss.Protocol.OnProcess(ss.Guid);
                    }
                }
                TimerManager.Update(stopwatch.ElapsedMilliseconds);

                SDungeon.Instance.Update();
                t2 = stopwatch.ElapsedMilliseconds;
                var t = (int)(t2 - t1);
                System.Threading.Thread.Sleep(t < 30 ? 30 - t : 1);
            }

            //while (true)
            //{
            //    var str = Console.ReadLine();
            //    var pack = new S2CLogin
            //    {
            //        Res = 1
            //    };
            //    foreach (var (_, v) in Server.dicEventArgs)
            //    {
            //        Server.StartSend(v.SocketEventArgs, pack);
            //    }
            //}
            #endregion Normal Socket
        }
    }
}
