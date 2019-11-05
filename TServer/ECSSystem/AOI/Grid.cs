using System;
using System.Collections.Generic;
using System.Text;

namespace TServer.ECSSystem.AOI
{
    /// <summary>
    /// 位置
    /// </summary>
    class Position<T>
    {
        public T x;
        public T y;
    }

    /// <summary>
    /// 格子
    /// </summary>
    class Grid
    {
        // 格子单位长度
        public int GridUnit;

        // 格子宽度
        public int GridWidth;

        // 格子长度
        public int GridLength;
    }

    public class Map
    {
        // 地图起始X位置
        public double MapMinX { get; set; }

        // 地图起始Y位置
        public double MapMinY { get; set; }

        // 地图宽度
        public double MapWidth { get; set; }

        // 地图长度
        public double MapLength { get; set; }
    }

    class GridManager
    {
        private Grid _grid;
        private Map _map;

        // 每个格子中的角色列表 <格子index-角色ID集合>
        private Dictionary<int, HashSet<int>> _roleMap;

        public GridManager(int gridUnit, double mapMinX, double mapMinY, double mapWidth, double mapLength)
        {
            _map = new Map { MapMinX = mapMinX, MapMinY = mapMinY, MapWidth = mapWidth, MapLength = mapLength };
            _grid = new Grid { GridUnit = gridUnit };
        }

        public void Init()
        {
            _grid.GridWidth = (int)_map.MapWidth / _grid.GridUnit + 1;
            _grid.GridLength = (int)_map.MapLength / _grid.GridUnit + 1;

            _roleMap = new Dictionary<int, HashSet<int>>();
            for (int i = 0; i < _grid.GridWidth * _grid.GridLength; ++i)
            {
                _roleMap.Add(i, new HashSet<int>(5));
            }
        }

        /// <summary>
        /// 地图位置转格子位置 返回格子的index
        /// </summary>
        /// <param name="mapPos"></param>
        /// <param name="gridPos"></param>
        /// <return></return>
        public int GetGridPosFromMapPos(Position<double> mapPos, out Position<int> gridPos)
        {
            gridPos = new Position<int> { x = (int)mapPos.x / _grid.GridUnit, y = (int)mapPos.y / _grid.GridUnit };
            return gridPos.x * _grid.GridWidth + gridPos.y;
        }

        /// <summary>
        /// 添加角色进入格子
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name=""></param>
        public void AddRoleToGrid(int roleId, Position<double> roleNewPos)
        {
            var idx = GetGridPosFromMapPos(roleNewPos, out var _);
            _roleMap[idx].Add(roleId);
        }

        /// <summary>
        /// 从格子中删除角色
        /// </summary>
        /// <param name="roleId"></param>
        public void DeleteRoleFromGrid(int roleId, Position<double> roleOldPos)
        {
            var idx = GetGridPosFromMapPos(roleOldPos, out var _);
            _roleMap[idx].Remove(roleId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deep"></param>
        /// <returns></returns>
        public HashSet<int> GetRolesFromSight(int gridIdx, int deep)
        {
            var idxs = new HashSet<int>();

            idxs.Add(gridIdx);
            idxs.Add(gridIdx);
            idxs.Add(gridIdx);
            idxs.Add(gridIdx);
            idxs.Add(gridIdx);
            idxs.Add(gridIdx);
            idxs.Add(gridIdx);
            idxs.Add(gridIdx);
            idxs.Add(gridIdx);

            // 左边界
            if (gridIdx % _grid.GridWidth == 0)
            {

            }
            // 右边界
            if ((gridIdx + 1) % _grid.GridWidth == 0)
            {

            }
            // 上边界
            if (gridIdx / _grid.GridWidth < 1)
            {

            }
            // 下边界
            if ((gridIdx / _grid.GridWidth + 1) == _grid.GridLength)
            {

            }

            return idxs;
        }
    }
}
