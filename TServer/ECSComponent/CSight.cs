using System;
using System.Collections.Generic;
using System.Text;
using TServer.ECSEntity;

namespace TServer.ECSComponent
{
    public class CSight
    {
        /// <summary>
        /// 视野内Entity ID
        /// </summary>
        //public HashSet<int> SetInSightEntityIds { get; set; }

        /// <summary>
        /// 自己能看到的Entity
        /// </summary>
        public HashSet<EEntity> SetInSightEntity { get; set; }

        /// <summary>
        /// 能看到自己的Entity
        /// </summary>
        public HashSet<EEntity> SetWatchEntity { get; set; }

        public CSight()
        {
            //SetInSightEntityIds = new HashSet<int>();
            SetInSightEntity = new HashSet<EEntity>();
            SetWatchEntity = new HashSet<EEntity>();
        }
    }
}
