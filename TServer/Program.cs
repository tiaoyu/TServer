using System;
using System.Diagnostics;
using System.IO;
using Common.LogUtil;
using Common.NavAuto;
using Common.Normal;
using Common.Protobuf;
using Common.Simple;
using Common.TTimer;
using TServer.ECSSystem;
using TServer.ECSSystem.Dungeon;
using TServer.Net;

namespace TServer
{
    internal class Program
    {
        private static LogHelp log;
        public static GameServer Server;
        public static TimerManager TimerManager = new TimerManager();
        private static void Main()
        {
            LogHelp.Init(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogConfig.log4net"));
            LogHelp.TestLog();
            log = LogHelp.GetLogger(typeof(Program));

            Console.SetIn(new StreamReader(Console.OpenStandardInput(8192)));

            log.Info("Hello Server!");

            Server = new GameServer(128, 8192);
            Server.Init();
            // 反序列化消息
            Server.MessageHandler.SetDeserializeFunc((bytes, guid) =>
            {
                var protoId = BitConverter.ToInt32(bytes);
                return new ExtSocket { Protocol = ProtocolParser.Instance.GetParser(protoId).ParseFrom(bytes) as ProtocolBufBase, Guid = guid, ESocketType = ESocketType.ESocketReceive };
            });
            // 序列化消息
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
            SDungeon.Instance.Init();
            while (true)
            {
                t1 = stopwatch.ElapsedMilliseconds;

                // 消息处理
                Server.ProcessMessage();
                // 定时器处理
                TimerManager.Update(stopwatch.ElapsedMilliseconds);

                SDungeon.Instance.Update();
                SNotify.Instance.Update();
                SMove.Instance.Update();

                t2 = stopwatch.ElapsedMilliseconds;
                var t = (int)(t2 - t1);
                System.Threading.Thread.Sleep(t < 30 ? 30 - t : 1);
                if (t > 200)
                    log.Warn($"Performance warning! One tick cost {t} ms!");
            }

        }
    }
}
