using System;
using System.Collections.Generic;
using System.Text;
using TServer.ECSSystem.AOI;

namespace TServer.ECSEntity
{
    public class EDungeon
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

        public EDungeon()
        {
            DicRole = new Dictionary<int, ERole>();
            GridSystem = new GridSystem(5, 99.0D, 99.0D);
            GridSystem.Init();
        }
    }
}
