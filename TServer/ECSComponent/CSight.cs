using System;
using System.Collections.Generic;
using System.Text;

namespace TServer.ECSComponent
{
    public class CSight
    {
        /// <summary>
        /// 视野内Entity ID
        /// </summary>
        public HashSet<int> SetInSightEntity { get; set; }
        public CSight()
        {
            SetInSightEntity = new HashSet<int>();
        }
    }
}
