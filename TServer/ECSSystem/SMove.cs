using Common;
using Common.Protobuf;
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
            RoleMove(role, pack.X, pack.Y);
        }

        public void RoleMove(ERole role, double x, double y)
        {
            role.Dungeon.GridSystem.UpdateRolePosition(role, x, y);
            SSight.Instance.RoleMove(role);
        }
    }
}
