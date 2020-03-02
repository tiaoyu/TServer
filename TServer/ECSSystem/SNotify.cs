using Common;
using System;
using System.Collections.Generic;
using System.Text;
using TServer.ECSEntity;

namespace TServer.ECSSystem
{
    /// <summary>
    /// 通知系统
    /// </summary>
    public class SNotify : Singleton<SNotify>
    {
        public Dictionary<ENotify, Action<Notify>> DicNotifies;
        public Queue<Notify> QueueNotifies;

        public void Update()
        {
            foreach (var notify in QueueNotifies)
            {
                if (DicNotifies.TryGetValue(notify.NotifyType, out var callback))
                {
                    callback(notify);
                }
            }
        }

        public SNotify()
        {
            DicNotifies = new Dictionary<ENotify, Action<Notify>>();
            QueueNotifies = new Queue<Notify>();
        }

        public void Register(ENotify notifyType, Action<Notify> notifyEvent)
        {
            if (!DicNotifies.TryGetValue(notifyType, out var notify))
            {
                notify += notifyEvent;
                DicNotifies.Add(notifyType, notify);
            }
            else
            {
                notify += notifyEvent;
            }

        }

        public void Push(Notify notify)
        {
            QueueNotifies.Enqueue(notify);
        }
    }

    public enum ENotify
    {
        ENotify_RoleLeaveDungeon,
        ENotify_RoleEnterDungeon,
        ENotify_RoleDead,
    }

    public class Notify
    {
        public ENotify NotifyType;
    }

    public class NotifyRoleDead : Notify
    {
        public ERole Role;

        public static void Push(ERole role)
        {
            SNotify.Instance.Push(new NotifyRoleDead { NotifyType = ENotify.ENotify_RoleDead, Role = role });
        }
    }
}
