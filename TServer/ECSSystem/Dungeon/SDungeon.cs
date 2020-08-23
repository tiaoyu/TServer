using Common;
using Common.NavAuto;
using System;
using System.Collections.Generic;
using System.IO;
using TServer.ECSComponent;
using TServer.ECSEntity;

namespace TServer.ECSSystem.Dungeon
{
    public class SDungeon : Singleton<SDungeon>
    {
        public Dictionary<int, CDungeon> DicDungeon = new Dictionary<int, CDungeon>();

        public void Init()
        {
            // 初始化副本
            var dInfo = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapData"));
            foreach (var filePath in dInfo.GetFiles())
            {
                var dungeon = new CDungeon(int.Parse(filePath.Name));
                DicDungeon.Add(int.Parse(filePath.Name), dungeon);
                foreach (var pos in dungeon.MapData.monsterPos)
                {
                    SDungeon.Instance.EnterDungeon(new EMonster
                    {
                        Id = EEntity.GenerateEntityId(EEntityType.MONSTER),
                        AIState = AI.EAIState.PATROL,
                        Dungeon = dungeon,
                        EntityType = EEntityType.MONSTER,
                        Position = new CPosition<double> { x = pos[0], y = pos[1], z = 0D },
                        BirthPosition = new CPosition<double> { x = pos[0], y = pos[1], z = 0D },
                        Movement = new CMovement
                        {
                            Speed = 5
                        }
                    }, dungeon);
                }
            }
        }
        public void Update()
        {
            // 副本自己的tick由timer处理 这里可以不做统一的Update
        }

        /// <summary>
        /// 角色进入指定副本
        /// </summary>
        /// <param name="role"></param>
        /// <param name="dungeon"></param>
        public void EnterDungeon(ERole role, CDungeon dungeon)
        {
            role.Dungeon = dungeon;
            dungeon.DicRole.Add(role.Id, role);
            dungeon.DicEntity.Add(role.Id, role);
            dungeon.GridSystem.AddEntityToGrid(role);
            dungeon.GridSystem.UpdateEntityPosition(role, role.Position.x, role.Position.y, true);
        }

        /// <summary>
        /// Entity进入副本
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dungeon"></param>
        public void EnterDungeon(EEntity entity, CDungeon dungeon)
        {
            dungeon.DicEntity.Add(entity.Id, entity);
            dungeon.GridSystem.AddEntityToGrid(entity);
            entity.Dungeon = dungeon;
            entity.Update();
        }

        /// <summary>
        /// 角色进入任意副本
        /// </summary>
        /// <param name="role"></param>
        public void EnterDungeon(ERole role, int tid = 101)
        {
            if (DicDungeon.Count <= 0)
            {
                var dungeon = new CDungeon();
                dungeon.Tid = tid;
                DicDungeon.Add(dungeon.Tid, dungeon);
            }

            foreach (var (id, dungeon) in DicDungeon)
            {
                if (tid == id)
                {
                    EnterDungeon(role, dungeon);
                    break;
                }
            }
        }

        /// <summary>
        /// 角色离开副本
        /// </summary>
        /// <param name="role"></param>
        public void LeaveDungeon(ERole role)
        {
            role.Dungeon?.DicRole.Remove(role.Id);
            role.Dungeon?.GridSystem.DeleteEntityFromGrid(role);
            SSight.Instance.LeaveSight(role, role.Sight.SetWatchEntity);
        }
    }
}
