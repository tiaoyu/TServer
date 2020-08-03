using Common.NavAuto;
using System;
using System.Collections.Generic;
using System.Text;

namespace TServer.ECSComponent
{
    public class CMovement
    {
        public bool IsNavAuto; // 是否正在自动寻路中
        public Stack<Node> StackNavPath; // 自动寻路的路径
        /// <summary>
        /// 速度
        /// </summary>
        public double Speed { get; set; }
        /// <summary>
        /// 朝向
        /// </summary>
        public Vector Orientation { get; set; }
        public int MoveTimerId { get; set; }
        public CMovement()
        {
            IsNavAuto = false;
            StackNavPath = new Stack<Node>();
            Orientation = new Vector();
        }
    }

    /// <summary>
    /// 向量
    /// </summary>
    public class Vector
    {
        public double X;
        public double Y;
        public double Z;

        public Vector()
        {
            X = 0D;
            Y = 0D;
            Z = 0D;
        }
    }
}
