using Common;
using Common.Protobuf;
using System;
using TServer.ECSEntity;
using TServer.Net;

namespace TServer.ECSSystem
{
    public class SMove : Singleton<SMove>
    {
        public void Update()
        {
            foreach (var (_, role) in GameServer.DicRole)
            {
                if (role.Movement.IsNavAuto)
                {
                    if (!role.Movement.StackNavPath.TryPop(out var node))
                    {
                        role.Movement.IsNavAuto = false;
                    }
                    else
                    {
                        RoleMove(role, node.x, node.y);
                    }
                }
            }
        }

        public void OnRoleNavAuto(ERole role, C2SNavAuto pack)
        {
            role.Movement.StackNavPath = Common.NavAuto.AStar.CalOptimalPath(role.Dungeon.MapData, (int)role.Position.x, (int)role.Position.y, (int)pack.X, (int)pack.Y);
            role.Movement.IsNavAuto = true;
        }

        public void OnRoleMove(ERole role, C2SMove pack)
        {
            role.Movement.IsNavAuto = false;
            var speed = pack.Speed;
            var xt = pack.X - role.Position.x;
            var yt = pack.Y - role.Position.y;

            var tmp = Math.Max(Math.Abs(xt), Math.Abs(yt));

            var x = xt / tmp * speed;
            var y = yt / tmp * speed;

            RoleMove(role, role.Position.x + x, role.Position.y + y);
        }

        public void RoleMove(ERole role, double x, double y)
        {

            if (!role.Dungeon.MapData.IsValidPosition((int)x, (int)y)) return;
            role.Dungeon.GridSystem.UpdateRolePosition(role, x, y);
            SSight.Instance.RoleMove(role);
        }
    }
}
