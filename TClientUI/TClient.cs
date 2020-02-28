using Common.LogUtil;
using Common.Normal;
using Common.Protobuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TClientUI
{
    public class TClient
    {
        private static MainWindow _mainWindow;
        public static NormalClient _client;
        public TClient(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            LogHelp.Init(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogConfig.log4net"));
        }

        public static void UpdateRole(RoleInfo roleInfo, bool isSelf = false)
        {
            if (roleInfo.Id == _mainWindow.SelfRole.Id || isSelf)
            {
                _mainWindow.SelfRole.Id = roleInfo.Id;
                _mainWindow.SelfRole.X = roleInfo.X;
                _mainWindow.SelfRole.Y = roleInfo.Y;
            }

            if (_mainWindow.DicRole.TryGetValue(roleInfo.Id, out var e))
            {
                _mainWindow.UpdateCanvas(e, roleInfo.X, roleInfo.Y);
            }
            else
            {
                _mainWindow.AddToCanvas(roleInfo.Id, roleInfo.X, roleInfo.Y);
            }
        }

        public static void UpdateRoleList(List<RoleInfo> roleInfoList)
        {
            _mainWindow.UpdateRoleListOnCanvas(roleInfoList);
        }

        public static void UpdateRoleSight(RoleInfo roleInfo, S2CSight.Types.ESightOpt opt)
        {
            _mainWindow.UpdateRoleSight(roleInfo, opt);
        }

        public void StartConnect()
        {
            _client = new NormalClient(20, 8192);
            _client.Init();
            _client.MessageHandler.SetDeserializeFunc((bytes, guid) =>
            {
                var protoId = BitConverter.ToInt32(bytes);
                return ProtocolParser.Instance.GetParser(protoId).ParseFrom(bytes);
            });
            _client.MessageHandler.SetSerializeFunc((protocol) =>
            {
                return (protocol as ProtocolBufBase).Serialize();
            });

            _client.StartConnect("127.0.0.1", 11000);

            Task.Run(() =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var t1 = stopwatch.ElapsedMilliseconds;
                var t2 = stopwatch.ElapsedMilliseconds;
                while (true)
                {
                    t1 = stopwatch.ElapsedMilliseconds;
                    var count = _client.MessageHandler.MessageQueue.Count;
                    for (var i = 0; i < count; ++i)
                    {
                        if (_client.MessageHandler.MessageQueue.TryDequeue(out var msg))
                        {
                            (msg as ProtocolBufBase).OnProcess();
                        }
                    }
                    t2 = stopwatch.ElapsedMilliseconds;
                    var t = (int)(t2 - t1);
                    System.Threading.Thread.Sleep(t < 30 ? 30 - t : 1);
                }
            });
        }
    }
}
