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
