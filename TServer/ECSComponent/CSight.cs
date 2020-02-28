using System;
using System.Collections.Generic;
using System.Text;

namespace TServer.ECSComponent
{
    public class CSight
    {
        /// <summary>
        /// 视野内玩具ID
        /// </summary>
        public HashSet<int> SetInSightRole { get; set; }

        public CSight()
        {
            SetInSightRole = new HashSet<int>();
        }
    }
}
