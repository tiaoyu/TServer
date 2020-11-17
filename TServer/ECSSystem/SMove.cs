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
                        EntityMove(role, node.x, node.y);
                    }
                }
            }
        }

        /// <summary>
        /// 自动寻路
        /// </summary>
        /// <param name="role"></param>
        /// <param name="pack"></param>
        public void OnRoleNavAuto(ERole role, C2SNavAuto pack)
        {
            role.Movement.StackNavPath = Common.NavAuto.AStar.CalOptimalPath(role.Dungeon.MapData, (int)role.Position.x, (int)role.Position.y, (int)pack.X, (int)pack.Y);
            role.Movement.IsNavAuto = true;
        }

        public void OnEntityNavAuto(EEntity entity, double x, double y)
        {
            entity.Movement.StackNavPath = Common.NavAuto.AStar.CalOptimalPath(entity.Dungeon.MapData, (int)entity.Position.x, (int)entity.Position.y, (int)x, (int)y);
            entity.Movement.IsNavAuto = entity.Movement.StackNavPath.Count > 0;
        }

        public void OnRoleMove(ERole role, C2SMove pack)
        {
            role.Movement.IsNavAuto = false;
            if (pack.IsUsePosition)
            {
                // new
                EntityMove(role, pack.Position.X, pack.Position.Y);

                // old
                //var speed = pack.Speed;
                //var xt = pack.X - role.Position.x;
                //var yt = pack.Y - role.Position.y;

                //var tmp = Math.Max(Math.Abs(xt), Math.Abs(yt));

                //var x = xt / tmp * speed;
                //var y = yt / tmp * speed;

                //EntityMove(role, role.Position.x + x, role.Position.y + y);
            }
            else
            {
                // 朝着当前朝向前进（使用timer处理）
                if (role.Movement.MoveTimerId > 0)
                {
                    Program.TimerManager.Remove(role.Movement.MoveTimerId);
                    role.Movement.MoveTimerId = 0;
                }
                role.Movement.Orientation.X = pack.Orientation.X;
                role.Movement.Orientation.Y = pack.Orientation.Y;
                role.Movement.Orientation.Z = pack.Orientation.Z;

                role.Movement.MoveTimerId = Program.TimerManager.Insert(50, 50, int.MaxValue, null, obj =>
                {
                    var x = pack.Orientation.X;
                    var y = pack.Orientation.Y;
                    var l = Math.Sqrt(x * x + y * y);

                    EntityMove(role, role.Position.x + pack.Speed * 50 / 1000 * x / l, role.Position.y + pack.Speed * 50 / 1000 * y / l);
                });
            }
        }

        public void OnRoleStopMove(ERole role)
        {
            Program.TimerManager.Remove(role.Movement.MoveTimerId);
            role.Movement.MoveTimerId = 0;
        }

        //public void RoleMove(ERole role, double x, double y)
        //{
        //    if (!role.Dungeon.MapData.IsValidPosition((int)x, (int)y)) return;
        //    role.Dungeon.GridSystem.UpdateRolePosition(role, x, y);
        //    SSight.Instance.EntityMove(role);
        //}

        public void EntityMove(EEntity entity, double x, double y)
        {
            if (!entity.Dungeon.MapData.IsValidPosition((int)x, (int)y)) return;
            entity.Dungeon.GridSystem.UpdateEntityPosition(entity, x, y);
            SSight.Instance.EntityMove(entity);
        }
    }
}
