using Common;
using Common.LogUtil;
using Common.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using TServer.ECSEntity;
using TServer.ECSSystem.Dungeon;
using TServer.Net;

namespace TServer.ECSSystem
{
    public class SLogin : Singleton<SLogin>
    {
        private static readonly LogHelp log = LogHelp.GetLogger(typeof(SLogin));
        public void Register(Guid guid, C2SRegister pack)
        {
            log.Debug($"Guid:{guid} --- {pack.Name} register, passwordis {pack.Password}.");
        }

        public void LoginIn(Guid guid, C2SLogin pack)
        {
            var role = new ERole();
            role.Id = Utilities.SUtilities.GetIndex();
            Program.Server.DicEventArgs.TryGetValue(guid, out role.exSocket);

            GameServer.DicRole.Add(guid, role);

            SDungeon.Instance.EnterDungeon(role);

            TServer.Program.Server.StartSend(role.exSocket.SocketEventArgs, new S2CLogin { Res = 1, RoleInfo = new RoleInfo { Id = role.Id, X = role.Position.x, Y = role.Position.y } });
            log.Debug($"Guid:{guid} --- {pack.Name} login in, password is {pack.Password}.");
        }
    }
}
