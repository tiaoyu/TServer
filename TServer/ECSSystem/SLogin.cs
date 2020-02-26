using Common;
using Common.LogUtil;
using Common.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using TServer.ECSEntity;
using TServer.ECSSystem.Dungeon;

namespace TServer.ECSSystem
{
    public class SLogin : Singleton<SLogin>
    {
        private static readonly LogHelp log = LogHelp.GetLogger(typeof(SLogin));
        private int i = 0;
        public void Register(Guid guid, C2SRegister pack)
        {
            log.Debug($"Guid:{guid} --- {pack.Name} register, passwordis {pack.Password}.");
        }

        public void LoginIn(Guid guid, C2SLogin pack)
        {
            var role = new ERole();
            role.Id = ++i;
            Program.Server.dicEventArgs.TryGetValue(guid, out role.exSocket);
            Program.DicRole.Add(guid, role);

            SDungeon.Instance.EnterDungeon(role);

            log.Debug($"Guid:{guid} --- {pack.Name} login in, password is {pack.Password}.");
        }
    }
}
