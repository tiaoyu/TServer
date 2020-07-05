using Common.Normal;
using System;
using System.Collections.Generic;
using TServer.ECSEntity;
using TServer.ECSSystem.Dungeon;

namespace TServer.Net
{
    public class GameServer : NormalServer
    {
        public static Dictionary<Guid, ERole> DicRole;

        public GameServer(int numConnections, int receiveBufferSize) : base(numConnections, receiveBufferSize)
        {
            DicRole = new Dictionary<Guid, ERole>();
        }

        /// <summary>
        /// 客户端断开连接后做的处理
        /// </summary>
        /// <param name="ss"></param>
        protected override void OnDisconnect(ExtSocket ss)
        {
            base.OnDisconnect(ss);
            DicEventArgs.Remove(ss.Guid);
            if (DicRole.TryGetValue(ss.Guid, out var role))
            {
                DicRole.Remove(ss.Guid);
                SDungeon.Instance.LeaveDungeon(role);
            }
        }

        protected override void OnClose(ExtSocket ss)
        {
            base.OnClose(ss);
            DicEventArgs.Remove(ss.Guid);
            if (DicRole.TryGetValue(ss.Guid, out var role))
            {
                DicRole.Remove(ss.Guid);
                SDungeon.Instance.LeaveDungeon(role);
            }
        }
    }
}
