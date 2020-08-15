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
        /// Entity进入视野时 
        ///     根据相互的视野范围来决定是否要互相加入到视野中
        /// </summary>
        /// <param name="roleA"></param>
        /// <param name="roleB"></param>
        public void EnterSight(EEntity entityA, EEntity entityB)
        {
            var dis = entityA.Dungeon.GridSystem.GetDistanceFromTwoPosition(entityA.Position, entityB.Position);

            if (entityA.SightDistance >= dis)
            {
                // 自己的视野 大于 两者间距 则 一定能进入视野
                //entityA.Sight.SetInSightEntityIds.Add(entityB.Id);
                entityA.Sight.SetInSightEntity.Add(entityB);
                entityB.Sight.SetWatchEntity.Add(entityA);

                if (entityA is ERole roleA)
                {
                    Program.Server.StartSend(roleA.exSocket.SocketEventArgs
                        , new S2CSight
                        {
                            EntityInfo = new EntityInfo { Id = entityB.Id, EntityType = (int)entityB.EntityType, X = entityB.Position.x, Y = entityB.Position.y },
                            SightOpt = S2CSight.Types.ESightOpt.EnterSight
                        });
                }
            }

            if (entityB.SightDistance >= dis)
            {
                //entityB.Sight.SetInSightEntityIds.Add(entityA.Id);
                entityB.Sight.SetInSightEntity.Add(entityA);
                entityA.Sight.SetWatchEntity.Add(entityB);
                if (entityB is ERole roleB)
                {
                    Program.Server.StartSend(roleB.exSocket.SocketEventArgs
                        , new S2CSight
                        {
                            EntityInfo = new EntityInfo { Id = entityA.Id, EntityType = (int)entityA.EntityType, X = entityA.Position.x, Y = entityA.Position.y },
                            SightOpt = S2CSight.Types.ESightOpt.EnterSight
                        });
                }
            }
        }

        public void EnterSight(EEntity self, HashSet<EEntity> entities)
        {
            foreach (var entity in entities)
            {
                EnterSight(self, entity);
            }
        }

        /// <summary>
        /// Entity离开视野时 
        ///     根据相互的视野范围来决定是否互相移除出视野
        /// </summary>
        /// <param name="roleA"></param>
        /// <param name="roleB"></param>
        public void LeaveSight(EEntity entityA, EEntity entityB)
        {
            var dis = entityA.Dungeon.GridSystem.GetDistanceFromTwoPosition(entityA.Position, entityB.Position);
            if (entityA.SightDistance < dis)
            {
                entityA.Sight.SetInSightEntity.Remove(entityB);
                entityB.Sight.SetWatchEntity.Remove(entityA);
                if (entityA is ERole roleA)
                {
                    Program.Server.StartSend(roleA.exSocket.SocketEventArgs
                        , new S2CSight
                        {
                            EntityInfo = new EntityInfo { Id = entityB.Id, EntityType = (int)entityB.EntityType, X = entityB.Position.x, Y = entityB.Position.y },
                            SightOpt = S2CSight.Types.ESightOpt.LeaveSight
                        });
                }
            }
            if (entityB.SightDistance < dis)
            {
                //entityB.Sight.SetInSightEntityIds.Remove(entityA.Id);
                entityA.Sight.SetWatchEntity.Remove(entityB);
                if (entityB is ERole roleB)
                {
                    Program.Server.StartSend(roleB.exSocket.SocketEventArgs
                        , new S2CSight
                        {
                            EntityInfo = new EntityInfo { Id = entityA.Id, EntityType = (int)entityA.EntityType, X = entityA.Position.x, Y = entityA.Position.y },
                            SightOpt = S2CSight.Types.ESightOpt.LeaveSight
                        });
                }
            }
        }

        public void LeaveSight(EEntity self, HashSet<EEntity> entities)
        {
            foreach (var entity in entities)
            {
                LeaveSight(self, entity);
            }
        }

        /// <summary>
        /// Entity移动通知视野内Entity
        /// </summary>
        /// <param name="role"></param>
        public void EntityMove(EEntity self)
        {
            var pack = new S2CMove();
            pack.EntityInfoList.Add(new EntityInfo { Id = self.Id, EntityType = (int)self.EntityType, X = self.Position.x, Y = self.Position.y });
            foreach (var entity in self.Sight.SetWatchEntity)
            {
                if (!self.Dungeon.DicRole.ContainsKey(entity.Id)) continue;
                Program.Server.StartSend((entity as ERole).exSocket.SocketEventArgs, pack);
            }
        }
    }
}
