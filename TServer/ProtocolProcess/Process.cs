using Common.LogUtil;
using System;
using System.Collections.Generic;
using System.Text;
using TServer;
using TServer.ECSComponent;
using TServer.ECSEntity;
using TServer.ECSSystem;
using TServer.ECSSystem.Dungeon;
using TServer.Net;

namespace Common.Protobuf
{
    public partial class C2STest
    {
        private static readonly LogHelp log = LogHelp.GetLogger(typeof(C2STest));
        public override void OnProcess(Guid guid)
        {
            if (!GameServer.DicRole.TryGetValue(guid, out var role))
                return;
            role.Position = new TServer.ECSComponent.CPosition<double> { x = X, y = Y };
            var dungeon = new CDungeon { Tid = 1 };
            dungeon.DicRole.Add(role.Id, role);
            SDungeon.Instance.DicDungeon.Add(1, dungeon);

            log.Debug($"Pos:({X}, {Y})");
        }
    }

    public partial class C2SLogin
    {
        public override void OnProcess(Guid guid)
        {
            SLogin.Instance.LoginIn(guid, this);
        }
    }

    public partial class C2SRegister
    {
        public override void OnProcess(Guid guid)
        {
            SLogin.Instance.Register(guid, this);
        }
    }

    public partial class C2SMove
    {
        public override void OnProcess(Guid guid)
        {
            if (!GameServer.DicRole.TryGetValue(guid, out var role)) return;
            SMove.Instance.OnRoleMove(role, this);
        }
    }

    public partial class C2SNavAuto
    {
        public override void OnProcess(Guid guid)
        {
            if (!GameServer.DicRole.TryGetValue(guid, out var role)) return;
            SMove.Instance.OnRoleNavAuto(role, this);
        }
    }

    public partial class C2SStopMove
    {
        public override void OnProcess(Guid guid)
        {
            if (!GameServer.DicRole.TryGetValue(guid, out var role)) return;
            SMove.Instance.OnRoleStopMove(role);
        }
    }
}
