using Common;
using Common.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using TServer.ECSEntity;

namespace TServer.ECSSystem
{
    public class SMove : Singleton<SMove>
    {
        public void RoleMove(ERole role, C2SMove pack)
        {
            role.Dungeon.GridSystem.UpdateRolePosition(role, pack.X, pack.Y);
            SSight.Instance.RoleMove(role);
        }
    }
}
