using Common.LogUtil;
using System;
using System.Collections.Generic;
using System.Text;
using TServer.ECSSystem;
using TServer.ECSSystem.AI;
using TServer.Utilities;

namespace TServer.ECSEntity
{
    /// <summary>
    /// 怪物
    /// </summary>
    public class EMonster : EEntity
    {
        private static readonly LogHelp log = LogHelp.GetLogger(typeof(EMonster));

        public EAIState AIState;
        public int TargetId;
        public EMonster() : base()
        {
            AIState = EAIState.PATROL;
            SightDistance = 150;
            CanBeSeeDistance = 150;
            AutoAttackDistance = 5;
        }

        /// <summary>
        /// Entity更新逻辑
        /// 用于做一些定时操作
        /// </summary>
        public override void Update()
        {
            Program.TimerManager.Insert(50, 50, int.MaxValue, null, (obj) =>
            {
                // 根据当前AI状态进行不同逻辑
                switch (AIState)
                {
                    // 巡逻 在出生点一定范围内随机一个位置移动
                    case EAIState.PATROL:
                        log.Debug($"i'm monster, i'm in partrol, my position is ({Position.x},{Position.y}).");

                        //foreach (var id in this.Sight.SetInSightEntityIds)
                        foreach (var target in this.Sight.SetInSightEntity)
                        {
                            if (this.Dungeon.GridSystem.GetDistanceFromTwoPosition(target.Position, this.Position) > AutoAttackDistance)
                                continue;
                            if (EEntity.IsRole(target.Id))
                            {
                                TargetId = target.Id;
                                AIState = EAIState.ATTACK;
                                this.Movement.IsNavAuto = false;
                                break;
                            }
                        }

                        if (TargetId == 0)
                        {
                            if (this.Movement.IsNavAuto)
                            {
                                NavMove();
                            }
                            else
                            {
                                // 巡逻时在自己出生点一定距离内随机一个位置寻路
                                SMove.Instance.OnEntityNavAuto(this, SUtilities.GetRandomInt((int)BirthPosition.x - 25, (int)BirthPosition.x + 25), SUtilities.GetRandomInt((int)BirthPosition.y - 25, (int)BirthPosition.y + 25));
                            }
                        }

                        break;
                    // 攻击
                    case EAIState.ATTACK:
                        if (this.Dungeon.DicEntity.TryGetValue(TargetId, out var entity))
                        {
                            log.Debug($"i'm monster, i'm in attack, target is {TargetId},  my position is ({Position.x},{Position.y}), target position is ({entity.Position.x},{entity.Position.y}).");
                            if (this.Movement.IsNavAuto)
                            {
                                NavMove();
                            }
                            else
                            {
                                SMove.Instance.OnEntityNavAuto(this, entity.Position.x, entity.Position.y);
                            }
                        }
                        else
                        {
                            TargetId = 0;
                            AIState = EAIState.PATROL;
                        }
                        break;
                    // 逃跑
                    case EAIState.FLEE:
                        log.Debug($"i'm monster, i'm in flee.");
                        break;
                    // 预警
                    case EAIState.INVESTIGATE:
                        log.Debug($"i'm monster, i'm in investigate.");
                        break;
                }
            });
        }

        private void NavMove()
        {
            if (!this.Movement.StackNavPath.TryPop(out var node))
            {
                this.Movement.IsNavAuto = false;
            }
            else
            {
                SMove.Instance.EntityMove(this, node.x, node.y);
            }
        }
    }
}
