using System;
using System.Collections.Generic;
using System.Text;

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
            foreach(var roleInfo in EntityInfoList)
            {
                Console.WriteLine($"{roleInfo.Id} ({roleInfo.X}, {roleInfo.Y})");
            }
        }
    }
}
