using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using TClientUI;

namespace Common.Protobuf
{
    public partial class S2CTest
    {
        public override void OnProcess()
        {
            foreach (var role in RoleList)
            {
                Console.WriteLine($"Role{role.Id} pos ({role.X}, {role.Y}).");
            }
        }
    }
    public partial class S2CLogin
    {
        public override void OnProcess()
        {
            TClient.UpdateRole(RoleInfo);
            Console.WriteLine($"Res:{this.Res}");
        }
    }

    public partial class S2CRegister
    {
        public override void OnProcess()
        {
            Console.WriteLine($"Res:{this.Res}");
        }
    }

    public partial class S2CMove
    {
        public override void OnProcess()
        {
            TClient.UpdateRoleList(new List<RoleInfo>(RoleInfoList));
        }
    }

    public partial class S2CSight
    {
        public override void OnProcess()
        {
            TClient.UpdateRoleSight(RoleInfo, SightOpt);
        }
    }
}
