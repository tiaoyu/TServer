using System;
using System.Collections.Generic;
using System.Text;
using TServer.ECSEntity;
using TServer.ECSSystem.AOI;

namespace TServer.ECSComponent
{
    public class CDungeon
    {
        public int Tid;
        /// <summary>
        /// 副本内的所有角色
        /// </summary>
        public Dictionary<int, ERole> DicRole;

        /// <summary>
        /// AOI
        /// </summary>
        public GridSystem GridSystem;

        public CDungeon()
        {
            DicRole = new Dictionary<int, ERole>();
            GridSystem = new GridSystem(5, 99.0D, 99.0D);
            GridSystem.Init();
        }
    }
}
