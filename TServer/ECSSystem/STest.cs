using Common;
using Common.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using TServer.Net;

namespace TServer.ECSSystem
{
    public class STest : Singleton<STest>
    {
        public void Test(Guid guid, C2STest pack)
        {
            if (!GameServer.DicRole.TryGetValue(guid, out var role))
                return;
            role.Position = new ECSComponent.CPosition<double> { x = pack.X, y = pack.Y };
        }
    }
}
