using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Protobuf
{
    public partial class S2CLogin
    {
        public override void OnProcess()
        {
            Console.WriteLine($"Res:{this.Res}");
        }
    }
}
