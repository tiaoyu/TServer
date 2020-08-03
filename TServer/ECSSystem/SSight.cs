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
        //public void EnterSight(ERole roleA, ERole roleB)
        //{
        //    roleA.Sight.SetInSightRole.Add(roleB.Id);
        //    roleB.Sight.SetInSightRole.Add(roleA.Id);
        //    Program.Server.StartSend(roleA.exSocket.SocketEventArgs
        //        , new S2CSight
        //        {
        //            EntityInfo = new EntityInfo { Id = roleB.Id, X = roleB.Position.x, Y = roleB.Position.y },
        //            SightOpt = S2CSight.Types.ESightOpt.EnterSight
        //        });
        //    Program.Server.StartSend(roleB.exSocket.SocketEventArgs
        //        , new S2CSight
        //        {
        //            EntityInfo = new EntityInfo { Id = roleA.Id, X = roleA.Position.x, Y = roleA.Position.y },
        //            SightOpt = S2CSight.Types.ESightOpt.EnterSight
        //        });
        //}
        /// <summary>
        /// 角色进入视野时 互相加入到视野中
        /// </summary>
        /// <param name="roleA"></param>
        /// <param name="roleB"></param>
        public void EnterSight(EEntity entityA, EEntity entityB)
        {
            entityA.Sight.SetInSightEntity.Add(entityB.Id);
            entityB.Sight.SetInSightEntity.Add(entityA.Id);
            if (entityA is ERole roleA)
            {
                Program.Server.StartSend(roleA.exSocket.SocketEventArgs
                    , new S2CSight
                    {
                        EntityInfo = new EntityInfo { Id = entityB.Id, EntityType = (int)entityB.EntityType, X = entityB.Position.x, Y = entityB.Position.y },
                        SightOpt = S2CSight.Types.ESightOpt.EnterSight
                    });
            }
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
        //public void EnterSight(EEntity entityA, ERole roleB)
        //{
        //    entityA.Sight.SetInSightRole.Add(roleB.Id);
        //    roleB.Sight.SetInSightRole.Add(entityA.Id);

        //    Program.Server.StartSend(roleB.exSocket.SocketEventArgs
        //        , new S2CSight
        //        {
        //            EntityInfo = new EntityInfo { Id = entityA.Id, X = entityA.Position.x, Y = entityA.Position.y },
        //            SightOpt = S2CSight.Types.ESightOpt.EnterSight
        //        });
        //}

        /// <summary>
        /// 进入视野
        /// </summary>
        /// <param name="role"></param>
        /// <param name="roleIds"></param>
        public void EnterSight(EEntity entity, HashSet<int> entityIds)
        {
            foreach (var id in entityIds)
            {
                if (!entity.Dungeon.DicEntity.TryGetValue(id, out var other)) continue;
                EnterSight(entity, other);
            }
        }

        //public void EnterSight(EEntity entity, HashSet<int> entityIds)
        //{

        //    foreach (var id in roleIds)
        //    {
        //        if (!entity.Dungeon.DicRole.TryGetValue(id, out var other)) continue;
        //        EnterSight(entity, other);
        //    }
        //}


        //public void LeaveSight(ERole roleA, ERole roleB)
        //{
        //    roleA.Sight.SetInSightRole.Remove(roleB.Id);
        //    roleB.Sight.SetInSightRole.Remove(roleA.Id);
        //    Program.Server.StartSend(roleA.exSocket.SocketEventArgs
        //        , new S2CSight
        //        {
        //            EntityInfo = new EntityInfo { Id = roleB.Id, X = roleB.Position.x, Y = roleB.Position.y },
        //            SightOpt = S2CSight.Types.ESightOpt.LeaveSight
        //        });
        //    Program.Server.StartSend(roleB.exSocket.SocketEventArgs
        //        , new S2CSight
        //        {
        //            EntityInfo = new EntityInfo { Id = roleA.Id, X = roleA.Position.x, Y = roleA.Position.y },
        //            SightOpt = S2CSight.Types.ESightOpt.LeaveSight
        //        });
        //}

        //public void LeaveSight(EEntity entityA, ERole roleB)
        //{
        //    entityA.Sight.SetInSightRole.Remove(roleB.Id);
        //    roleB.Sight.SetInSightRole.Remove(entityA.Id);

        //    Program.Server.StartSend(roleB.exSocket.SocketEventArgs
        //        , new S2CSight
        //        {
        //            EntityInfo = new EntityInfo { Id = entityA.Id, X = entityA.Position.x, Y = entityA.Position.y },
        //            SightOpt = S2CSight.Types.ESightOpt.LeaveSight
        //        });
        //}

        /// <summary>
        /// 角色离开视野时 互相移除出视野
        /// </summary>
        /// <param name="roleA"></param>
        /// <param name="roleB"></param>
        public void LeaveSight(EEntity entityA, EEntity entityB)
        {
            entityA.Sight.SetInSightEntity.Remove(entityB.Id);
            entityB.Sight.SetInSightEntity.Remove(entityA.Id);
            if (entityA is ERole roleA)
            {
                Program.Server.StartSend(roleA.exSocket.SocketEventArgs
                    , new S2CSight
                    {
                        EntityInfo = new EntityInfo { Id = entityB.Id, EntityType = (int)entityB.EntityType, X = entityB.Position.x, Y = entityB.Position.y },
                        SightOpt = S2CSight.Types.ESightOpt.LeaveSight
                    });
            }
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

        //public void LeaveSight(ERole role, HashSet<int> roleIds)
        //{
        //    foreach (var id in roleIds)
        //    {
        //        if (!role.Dungeon.DicRole.TryGetValue(id, out var other)) continue;
        //        LeaveSight(role, other);
        //    }
        //}
        /// <summary>
        /// 离开视野
        /// </summary>
        /// <param name="role"></param>
        /// <param name="roleIds"></param>

        public void LeaveSight(EEntity entity, HashSet<int> entityIds)
        {
            foreach (var id in entityIds)
            {
                if (!entity.Dungeon.DicEntity.TryGetValue(id, out var other)) continue;
                LeaveSight(entity, other);
            }
        }


        //public void RoleMove(ERole role)
        //{
        //    var pack = new S2CMove();
        //    pack.EntityInfoList.Add(new EntityInfo { Id = role.Id, X = role.Position.x, Y = role.Position.y });
        //    foreach (var id in role.Sight.SetInSightRole)
        //    {
        //        if (!role.Dungeon.DicRole.TryGetValue(id, out var r)) continue;
        //        Program.Server.StartSend(r.exSocket.SocketEventArgs, pack);
        //    }
        //}

        /// <summary>
        /// Entity移动通知视野内角色
        /// </summary>
        /// <param name="role"></param>
        public void EntityMove(EEntity entity)
        {
            var pack = new S2CMove();
            pack.EntityInfoList.Add(new EntityInfo { Id = entity.Id, EntityType = (int)entity.EntityType, X = entity.Position.x, Y = entity.Position.y });
            foreach (var id in entity.Sight.SetInSightEntity)
            {
                if (!entity.Dungeon.DicRole.TryGetValue(id, out var r)) continue;
                Program.Server.StartSend(r.exSocket.SocketEventArgs, pack);
            }
        }
    }
}
