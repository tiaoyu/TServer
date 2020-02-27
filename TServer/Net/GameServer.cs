using Common.Normal;
using System;
using System.Collections.Generic;
using TServer.ECSEntity;

namespace TServer.Net
{
    public class GameServer : NormalServer
    {
        public static Dictionary<Guid, ERole> DicRole;

        public GameServer(int numConnections, int receiveBufferSize) : base(numConnections, receiveBufferSize)
        {
            DicRole = new Dictionary<Guid, ERole>();
        }

        protected override void OnDisconnect(ExtSocket ss)
        {
            base.OnDisconnect(ss);
            DicEventArgs.Remove(ss.Guid);
            DicRole.Remove(ss.Guid);
        }

        protected override void OnClose(ExtSocket ss)
        {
            base.OnClose(ss);
            DicEventArgs.Remove(ss.Guid);
            DicRole.Remove(ss.Guid);
        }
    }
}
