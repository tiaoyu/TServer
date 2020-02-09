using Common;
using Common.LogUtil;
using Common.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace TServer.ECSSystem
{
    public class LoginSystem : Singleton<LoginSystem>
    {
        private static readonly LogHelp log = LogHelp.GetLogger(typeof(LoginSystem));

        public void Register(Guid guid,  C2SRegister pack)
        {
            log.Debug($"Guid:{guid} --- {pack.Name} register, passwordis {pack.Password}.");
        }

        public void LoginIn(Guid guid, C2SLogin pack)
        {
            log.Debug($"Guid:{guid} --- {pack.Name} login in, password is {pack.Password}.");
        }
    }
}
