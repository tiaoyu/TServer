using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace TServer.ECSSystem
{
    /// <summary>
    /// 通知系统
    /// </summary>
    public class SNotify : Singleton<SLogin>
    {
        public Dictionary<ENotify, Notify> DicNotifies;
        public Queue<Notify> QueueNotifies;

        public void Update()
        {
            foreach (var notify in QueueNotifies)
            {
                notify.Action();
            }
        }

        public void Init()
        {
            DicNotifies = new Dictionary<ENotify, Notify>();
        }

        public void Register(ENotify notifyType, Action notifyEvent)
        {
            if (!DicNotifies.TryGetValue(notifyType, out var notify))
            {
                notify = new Notify { NotifyType = notifyType };
                DicNotifies.Add(notifyType, notify);
            }

            notify.NotifyEvent += notifyEvent;
        }
    }

    public enum ENotify
    {
        ENotify_RoleLeaveDungeon,
        ENotify_RoleEnterDungeon,
    }

    public class Notify
    {
        public ENotify NotifyType;
        public Action NotifyEvent;

        public void Action()
        {
            if (NotifyEvent != null)
                NotifyEvent();
        }
    }


    public class NotifyRoleDead : Notify
    {

    }
}
