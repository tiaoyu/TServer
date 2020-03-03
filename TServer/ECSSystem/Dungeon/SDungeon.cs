using Common;
using Common.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using TServer.ECSComponent;
using TServer.ECSEntity;

namespace TServer.ECSSystem.Dungeon
{
    public class SDungeon : Singleton<SDungeon>
    {
        public Dictionary<int, CDungeon> DicDungeon = new Dictionary<int, CDungeon>();

        public void Update()
        {
            // 同步视野内角色数据由: 角色移动、角色进入、角色退出来触发
            foreach (var (_, dungeon) in DicDungeon)
            {
            }
        }

        /// <summary>
        /// 角色进入指定副本
        /// </summary>
        /// <param name="role"></param>
        /// <param name="dungeon"></param>
        public void EnterDungeon(ERole role, CDungeon dungeon)
        {
            dungeon.DicRole.Add(role.Id, role);
            dungeon.GridSystem.AddRoleToGrid(role.Id, role.Position);
            role.Dungeon = dungeon;
        }

        /// <summary>
        /// 角色进入任意副本
        /// </summary>
        /// <param name="role"></param>
        public void EnterDungeon(ERole role)
        {
            if (DicDungeon.Count <= 0)
            {
                var dungeon = new CDungeon();
                dungeon.Tid = 101;
                DicDungeon.Add(dungeon.Tid, dungeon);
            }

            foreach (var (_, dungeon) in DicDungeon)
            {
                EnterDungeon(role, dungeon);
                break;
            }
            role.Dungeon.GridSystem.GetRolesFromSight(role.SightDistance, role.Position, out _, out var roleIds);
            role.Sight.SetInSightRole = roleIds;
            SSight.Instance.EnterSight(role, roleIds);
        }

        /// <summary>
        /// 角色离开副本
        /// </summary>
        /// <param name="role"></param>
        public void LeaveDungeon(ERole role)
        {
            var roleIds = new HashSet<int>();
            role.Dungeon?.GridSystem.GetRolesFromSight(role.SightDistance, role.Position, out _, out roleIds);

            role.Dungeon?.DicRole.Remove(role.Id);
            role.Dungeon?.GridSystem.DeleteRoleFromGrid(role.Id, role.Position);

            SSight.Instance.LeaveSight(role, roleIds);
        }
    }
}
