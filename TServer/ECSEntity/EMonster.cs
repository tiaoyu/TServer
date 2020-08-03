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
        }

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

                        foreach (var id in this.Sight.SetInSightEntity)
                        {
                            if (EEntity.IsRole(id))
                            {
                                TargetId = id;
                                AIState = EAIState.ATTACK;
                                this.Movement.IsNavAuto = false;
                            }
                        }

                        if (this.Movement.IsNavAuto)
                        {
                            NavMove();
                        }
                        else
                        {
                            SMove.Instance.OnEntityNavAuto(this, SUtilities.GetRandomInt(100, 150), SUtilities.GetRandomInt(100, 150));
                        }

                        break;
                    // 攻击
                    case EAIState.ATTACK:
                        if (this.Dungeon.DicEntity.TryGetValue(TargetId, out var entity))
                        {
                            //log.Debug($"i'm monster, i'm in attack, target is {TargetId},  my position is ({Position.x},{Position.y}), target position is ({entity.Position.x},{entity.Position.y}).");
                            if (this.Movement.IsNavAuto)
                            {
                                NavMove();
                            }
                            else
                            {
                                SMove.Instance.OnEntityNavAuto(this, entity.Position.x, entity.Position.y);
                            }
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
