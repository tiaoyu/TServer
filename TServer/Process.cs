using Common.LogUtil;
using System;
using System.Collections.Generic;
using System.Text;
using TServer;
using TServer.ECSEntity;
using TServer.ECSSystem;
using TServer.ECSSystem.Dungeon;

namespace Common.Protobuf
{
    public partial class C2STest
    {
        private static readonly LogHelp log = LogHelp.GetLogger(typeof(C2STest));
        public override void OnProcess(Guid guid)
        {
            if (!Program.DicRole.TryGetValue(guid, out var role))
                return;
            role.Position = new TServer.ECSComponent.CPosition<double> { x = X, y = Y };
            var dungeon = new EDungeon { Tid = 1 };
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
            if (!Program.DicRole.TryGetValue(guid, out var role)) return;
            SMove.Instance.RoleMove(role, this);
        }
    }
}
