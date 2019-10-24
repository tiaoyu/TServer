using System;
using System.Collections.Generic;
using System.Text;
using TServer.ECSSystem;

namespace Common.Protobuf
{
    public partial class C2SLogin
    {
        public override void OnProcess(Guid guid)
        {
            LoginSystem.Instance.LoginIn(guid, this);
        }
    }
}
