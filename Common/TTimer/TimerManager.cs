using System;
using System.Collections.Generic;
using System.Text;

namespace Common.TTimer
{
    /// <summary>
    /// 定时器
    /// 定义为单向链表
    /// </summary>
    public class Timer
    {
        Timer next;
        public Action<object> callback;
        public long NextTriggerTick;
        public Timer()
        {
            next = null;
        }
    }

    /// <summary>
    /// 时间轮
    /// </summary>
    public class Wheel
    {
        public int slotCount;
        public int curSlot;
        // 每个刻度对应一个链表
        public Timer[] head;

        public Wheel(int slotCount)
        {
            this.slotCount = slotCount;
            curSlot = 0;
            head = new Timer[slotCount];
        }
    }

    /// <summary>
    /// 时间轮定时器
    /// </summary>
    public class TimerManager
    {
        private const int MAX_WHEEL_COUNT = 5; // 时间轮个数
        private const int SLOT_TIME_MS = 10;   // 时间轮精度 10ms
        private int[] WheelSlotCount = new int[] { 1 << 8, 1 << 4, 1 << 4, 1 << 4, 1 << 4 };
        public Wheel[] Wheels;
        // 当前时间点 ms
        public long CurrentTick; 

        public void Init()
        {
            Wheels = new Wheel[MAX_WHEEL_COUNT];
            for (var i = 0; i < MAX_WHEEL_COUNT; ++i)
            {
                Wheels[i] = new Wheel(WheelSlotCount[i]);
            }
        }

        /// <summary>
        /// 插入定时器
        /// 每次插入时间轮都会都会将相对时间换算成实际时间来插入
        /// </summary>
        public void Insert(int delay, int interval, int count, object userData, Action<object> callback)
        {
#warning TODO
            // 构建定时器
            var t = new Timer();
            t.callback = callback;
            t.NextTriggerTick = CurrentTick + delay;
            // 计算插进时间轮中的位置
            var pos = delay / SLOT_TIME_MS;
            for (var i = 0; i < MAX_WHEEL_COUNT; ++i)
            {
            }
        }

        /// <summary>
        /// 更新定时器
        /// 每帧更新一次
        /// </summary>
        public void Update(int time)
        {
            
        }
    }
}
