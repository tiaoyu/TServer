using Common;
using Common.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using TServer.ECSEntity;

namespace TServer.ECSSystem.Dungeon
{
    public class SDungeon : Singleton<SDungeon>
    {
        public Dictionary<int, EDungeon> DicDungeon = new Dictionary<int, EDungeon>();
        public void Update()
        {
            foreach (var (_, dungeon) in DicDungeon)
            {
                var pack = new S2CMove();

                foreach (var (_, role) in dungeon.DicRole)
                {
                    dungeon.GridSystem.GetRolesFromSight(1, role.Position, out var girdIdxs, out var roleIds);
                    foreach (var id in roleIds)
                    {
                        pack.RoleInfoList.Add(new RoleInfo { Id = id, X = dungeon.DicRole[id].Position.x, Y = dungeon.DicRole[id].Position.y });
                    }

                    TServer.Program.Server.StartSend(role.exSocket.SocketEventArgs, pack);
                    pack.RoleInfoList.Clear();
                }
            }
        }

        /// <summary>
        /// 角色进入指定副本
        /// </summary>
        /// <param name="role"></param>
        /// <param name="dungeon"></param>
        public void EnterDungeon(ERole role, EDungeon dungeon)
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
                var dungeon = new EDungeon();
                dungeon.Tid = 101;
                DicDungeon.Add(dungeon.Tid, dungeon);
            }

            foreach (var (_, dungeon) in DicDungeon)
            {
                dungeon.DicRole.Add(role.Id, role);
                dungeon.GridSystem.AddRoleToGrid(role.Id, role.Position);
                role.Dungeon = dungeon;
                return;
            }
        }
    }
}
