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
        // 定时器待执行方法
        public Action<object> callback;
        public object UserData;
        // 下一次触发的时间
        public long NextTriggerTick;
        // 需要触发的次数
        public int TriggerCount;
        // 两次触发的时间间隔
        public int TriggerInterval;

        // TimerId
        public int Id;
        // Timer 所在轮
        public int Wheel;
        // Timer 所在刻度
        public int Slot;

        public Timer()
        {
        }
    }

    /// <summary>
    /// 时间轮
    /// </summary>
    public class Wheel
    {
        // 总刻度
        public int slotCount;

        // 当前指向刻度
        public int curSlot;

        // 每个刻度对应一个链表
        public LinkedList<Timer>[] head;

        public Wheel(int slotCount)
        {
            this.slotCount = slotCount;
            curSlot = 0;
            head = new LinkedList<Timer>[slotCount];
            for (var i = 0; i < slotCount; ++i)
            {
                head[i] = new LinkedList<Timer>();
            }
        }

        /// <summary>
        /// 添加定时器
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="slot"></param>
        public void Add(Timer timer, int slot)
        {
            head[slot].AddLast(timer);
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

        // TimerId 起始点
        private int Index = 0;
        // 所有轮
        public Wheel[] Wheels;
        // 当前时间点 ms
        public long CurrentTick;
        // 缓存的Timer
        public List<Timer> tempTimers;
        // 所有Timer合集
        public Dictionary<int, Timer> DicTimer;
        public HashSet<int> ReadyToRemoveTimerIds;
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            ReadyToRemoveTimerIds = new HashSet<int>();
            DicTimer = new Dictionary<int, Timer>();
            tempTimers = new List<Timer>();
            Wheels = new Wheel[MAX_WHEEL_COUNT];
            for (var i = 0; i < MAX_WHEEL_COUNT; ++i)
            {
                Wheels[i] = new Wheel(WheelSlotCount[i]);
            }
            Update(10);
        }

        /// <summary>
        /// 插入定时器
        /// 每次插入时间轮都会都会将相对时间换算成实际时间来插入
        /// </summary>
        public int Insert(int delay, int interval, int count, object userData, Action<object> callback)
        {
            // 构建定时器
            var t = new Timer
            {
                callback = callback,
                NextTriggerTick = CurrentTick + delay,
                TriggerCount = count,
                TriggerInterval = interval,
                UserData = userData,
                Id = ++Index
            };

            Insert(t);
            DicTimer.Add(t.Id, t);
            return t.Id;
        }

        /// <summary>
        /// 加入待删除的定时器ID 此方法在定时器回调中使用
        /// </summary>
        /// <param name="id"></param>
        public void AddToRemove(int id)
        {
            ReadyToRemoveTimerIds.Add(id);
        }
        /// <summary>
        /// 移除定时器
        /// </summary>
        /// <param name="id"></param>
        public void Remove(int id)
        {
            if (DicTimer.TryGetValue(id, out var timer))
            {
                DicTimer.Remove(id);
                Wheels[timer.Wheel].head[timer.Slot].Remove(timer);
            }
        }

        /// <summary>
        /// 插入定时器
        /// 定义1轮的单位时间为10ms，即一格10ms，设当前1轮指针所指在刻度 curSlot，当前时间为 CurrentTicks，要插入的定时器到期时间为 NextTriggerTick，时间统一按照时间戳表示，
        ///    则插入的位置算法为：
        /// 在1轮中的指针位置（这里的减一是因为起始数为0而不是1，0~255正好256格）
        /// pos = ((NextTriggerTick - CurrentTick) / 10  - 1 + curSlot) % 256  
        /// 若pos 大于 256 ，则将pos带入2轮进行计算，类似进位
        /// 循环：2 pos = (pos -1 + curSlot) % 64 
        /// </summary>
        /// <param name="timer"></param>
        private void Insert(Timer timer)
        {
            // 计算当前到下次触发时间所占的总刻度数
            var pos = 0L;
            // 每个轮挨个计算最终插入的位置
            for (var i = 0; i < MAX_WHEEL_COUNT; ++i)
            {
                if (i == 0)
                    pos = (Math.Max((timer.NextTriggerTick - CurrentTick) / SLOT_TIME_MS - 1, 0) + Wheels[0].curSlot);
                else
                    pos = (Math.Max(pos - 1, 0) + Wheels[i].curSlot);

                if (pos >= WheelSlotCount[i])
                {
                    pos = pos / WheelSlotCount[i];
                    continue;
                }

                timer.Wheel = i;
                timer.Slot = (int)pos;

                Wheels[i].Add(timer, (int)pos);
                break;
            }
        }

        /// <summary>
        /// 更新定时器
        /// 每帧更新一次
        /// </summary>
        public void Update(long futureTick)
        {
            //if (CurrentTick == 0)
            //{
            //    CurrentTick = futureTick;
            //    return;
            //}
            for (; CurrentTick < futureTick; CurrentTick += SLOT_TIME_MS)
            {
                // 更新
                for (var i = 0; i < MAX_WHEEL_COUNT; ++i)
                {
                    var curSlot = Wheels[i].curSlot;
                    Wheels[i].curSlot++;
                    // 执行当前Slot上的 timer
                    while (Wheels[i].head[curSlot].Count != 0)
                    {
                        var timer = Wheels[i].head[curSlot].First.Value;
                        Wheels[i].head[curSlot].RemoveFirst();

                        // 只有在最大轮上的timer才是真正需要执行的timer
                        if (i == 0)
                        {
                            timer.callback(timer.UserData);
                            --timer.TriggerCount;
                            timer.NextTriggerTick = CurrentTick + timer.TriggerInterval;
                        }

                        if (timer.TriggerCount > 0)
                        {
                            tempTimers.Add(timer);
                        }
                    }

                    foreach (var timer in tempTimers)
                    {
                        Insert(timer);
                    }
                    tempTimers.Clear();


                    Wheels[i].curSlot %= WheelSlotCount[i];

                    if (Wheels[i].curSlot == 0)
                    {
                        continue;
                    }
                    break;
                }
                foreach(var id in ReadyToRemoveTimerIds)
                {
                    Remove(id);
                }
            }
        }
    }
}
