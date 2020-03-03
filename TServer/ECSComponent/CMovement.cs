using Common.NavAuto;
using System;
using System.Collections.Generic;
using System.Text;

namespace TServer.ECSComponent
{
    public class CMovement
    {
        public bool IsNavAuto; // 是否正在自动寻路中
        public Stack<Node> StackNavPath; // 寻路路径

        public CMovement()
        {
            IsNavAuto = false;
            StackNavPath = new Stack<Node>();
        }
    }
}
