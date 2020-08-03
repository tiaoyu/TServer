using System;
using System.Collections.Generic;
using System.Text;

namespace TServer.ECSSystem.AI
{
    /// <summary>
    /// AI状态
    /// </summary>
    public enum EAIState
    {
        PATROL,         // 巡逻
        INVESTIGATE,    // 预警
        ATTACK,         // 攻击
        FLEE,           // 逃跑
    }
}
