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
            role.Position.x = pack.X;
            role.Position.y = pack.Y;

            role.Dungeon.GridSystem.UpdateRolePosition(role);
        }
    }
}
