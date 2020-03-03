using Common.NavAuto;
using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary> AOI </summary>
        public GridSystem GridSystem;

        /// <summary> 地图数据 </summary>
        public MapData MapData;

        public CDungeon(int tid = 1)
        {
            DicRole = new Dictionary<int, ERole>();

            MapData = new MapData(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("MapData", "1.txt")));
            MapData.Init();
            
            GridSystem = new GridSystem(5, MapData.Length, MapData.Width);
            GridSystem.Init();
        }
    }
}
