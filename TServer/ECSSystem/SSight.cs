using Common;
using Common.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using TServer.ECSEntity;

namespace TServer.ECSSystem
{
    public class SSight : Singleton<SSight>
    {
        /// <summary>
        /// 角色进入视野时 互相加入到视野中
        /// </summary>
        /// <param name="roleA"></param>
        /// <param name="roleB"></param>
        public void EnterSight(ERole roleA, ERole roleB)
        {
            roleA.Sight.SetInSightRole.Add(roleB.Id);
            roleB.Sight.SetInSightRole.Add(roleA.Id);
            Program.Server.StartSend(roleA.exSocket.SocketEventArgs
                , new S2CSight
                {
                    RoleInfo = new RoleInfo { Id = roleB.Id, X = roleB.Position.x, Y = roleB.Position.y },
                    SightOpt = S2CSight.Types.ESightOpt.EnterSight
                });
            Program.Server.StartSend(roleB.exSocket.SocketEventArgs
                , new S2CSight
                {
                    RoleInfo = new RoleInfo { Id = roleA.Id, X = roleA.Position.x, Y = roleA.Position.y },
                    SightOpt = S2CSight.Types.ESightOpt.EnterSight
                });
        }

        /// <summary>
        /// 进入视野
        /// </summary>
        /// <param name="role"></param>
        /// <param name="roleIds"></param>
        public void EnterSight(ERole role, HashSet<int> roleIds)
        {
            foreach (var id in roleIds)
            {
                if (!role.Dungeon.DicRole.TryGetValue(id, out var other)) continue;
                EnterSight(role, other);
            }
        }

        /// <summary>
        /// 角色离开视野时 互相移除出视野
        /// </summary>
        /// <param name="roleA"></param>
        /// <param name="roleB"></param>
        public void LeaveSight(ERole roleA, ERole roleB)
        {
            roleA.Sight.SetInSightRole.Remove(roleB.Id);
            roleB.Sight.SetInSightRole.Remove(roleA.Id);
            Program.Server.StartSend(roleA.exSocket.SocketEventArgs
                , new S2CSight
                {
                    RoleInfo = new RoleInfo { Id = roleB.Id, X = roleB.Position.x, Y = roleB.Position.y },
                    SightOpt = S2CSight.Types.ESightOpt.LeaveSight
                });
            Program.Server.StartSend(roleB.exSocket.SocketEventArgs
                , new S2CSight
                {
                    RoleInfo = new RoleInfo { Id = roleA.Id, X = roleA.Position.x, Y = roleA.Position.y },
                    SightOpt = S2CSight.Types.ESightOpt.LeaveSight
                });
        }

        /// <summary>
        /// 离开视野
        /// </summary>
        /// <param name="role"></param>
        /// <param name="roleIds"></param>
        public void LeaveSight(ERole role, HashSet<int> roleIds)
        {
            foreach (var id in roleIds)
            {
                if (!role.Dungeon.DicRole.TryGetValue(id, out var other)) continue;
                LeaveSight(role, other);
            }
        }

        /// <summary>
        /// 角色移动通知视野内角色
        /// </summary>
        /// <param name="role"></param>
        public void RoleMove(ERole role)
        {
            var pack = new S2CMove();
            pack.RoleInfoList.Add(new RoleInfo { Id = role.Id, X = role.Position.x, Y = role.Position.y });
            foreach (var id in role.Sight.SetInSightRole)
            {
                if (!role.Dungeon.DicRole.TryGetValue(id, out var r)) continue;
                Program.Server.StartSend(r.exSocket.SocketEventArgs, pack);
            }
            //Program.Server.StartSend(role.exSocket.SocketEventArgs, pack);
        }
    }
}
