using System.Collections.Generic;
using TServer.ECSComponent;

namespace TServer.ECSSystem.AOI
{
    /// <summary>
    /// 格子
    /// </summary>
    public class Grid
    {
        // 格子单位长度
        public int GridUnit;

        // 格子宽度
        public int GridWidth;

        // 格子长度
        public int GridLength;
    }

    /// <summary>
    /// 地图
    /// </summary>
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

    /// <summary>
    /// 格子管理
    /// </summary>
    public class GridSystem
    {
        private Grid _grid;
        private Map _map;

        // 每个格子中的角色列表 <格子index-角色ID集合>
        private Dictionary<int, HashSet<int>> _roleMap;
        public Dictionary<int, HashSet<int>> RoleMap => _roleMap;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gridUnit">格子单元长度</param>
        /// <param name="mapWidth">地图宽度</param>
        /// <param name="mapLength">地图长度</param>
        /// <param name="mapMinX">地图最小X坐标</param>
        /// <param name="mapMinY">地图最小Y坐标</param>
        public GridSystem(int gridUnit, double mapWidth, double mapLength, double mapMinX = 0f, double mapMinY = 0f)
        {
            _map = new Map { MapMinX = mapMinX, MapMinY = mapMinY, MapWidth = mapWidth, MapLength = mapLength };
            _grid = new Grid { GridUnit = gridUnit };
        }

        /// <summary>
        /// 初始化
        /// </summary>
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
        /// 格子位置转格子下标
        /// </summary>
        /// <param name="gridPos"></param>
        /// <returns></returns>
        public int GetGridIdxFromGridPos(PositionCmt<int> gridPos)
        {
            return gridPos.x * _grid.GridWidth + gridPos.y;
        }

        /// <summary>
        /// 格子位置转格子下标
        /// </summary>
        /// <param name="gridPos"></param>
        /// <returns></returns>
        public int GetGridIdxFromGridPos(int x, int y)
        {
            return x * _grid.GridWidth + y;
        }

        /// <summary>
        /// 地图位置转格子位置 返回格子下标
        /// </summary>
        /// <param name="mapPos"></param>
        /// <param name="gridPos"></param>
        /// <return></return>
        public int GetGridPosFromMapPos(PositionCmt<double> mapPos, out PositionCmt<int> gridPos)
        {
            gridPos = new PositionCmt<int> { x = (int)(mapPos.x - _map.MapMinX) / _grid.GridUnit, y = (int)(mapPos.y - _map.MapMinY) / _grid.GridUnit };
            return GetGridIdxFromGridPos(gridPos);
        }

        /// <summary>
        /// 格子下标转格子位置
        /// </summary>
        /// <param name="gridIdx"></param>
        /// <returns></returns>
        public PositionCmt<int> GetGridPosFromGridIdx(int gridIdx)
        {
            return new PositionCmt<int> { x = gridIdx % _grid.GridWidth, y = gridIdx / _grid.GridLength };
        }

        /// <summary>
        /// 添加角色进入格子
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name=""></param>
        public int AddRoleToGrid(int roleId, PositionCmt<double> roleNewPos)
        {
            var idx = GetGridPosFromMapPos(roleNewPos, out var _);
            _roleMap[idx].Add(roleId);
            return idx;
        }

        /// <summary>
        /// 从格子中删除角色
        /// </summary>
        /// <param name="roleId"></param>
        public void DeleteRoleFromGrid(int roleId, PositionCmt<double> roleOldPos)
        {
            var idx = GetGridPosFromMapPos(roleOldPos, out var _);
            _roleMap[idx].Remove(roleId);
        }

        /// <summary>
        /// 获取周围指定深度的格子下标集合
        /// </summary>
        /// <param name="deep"></param>
        /// <param name="curPos"></param>
        /// <param name="gridIdxs"></param>
        public void GetRolesFromSight(int deep, PositionCmt<int> curPos, out HashSet<int> gridIdxs, out HashSet<int> roleIds)
        {
            gridIdxs = new HashSet<int>();
            roleIds = new HashSet<int>();
            var xFrom = curPos.x - deep < 0 ? 0 : curPos.x - deep;
            var yFrom = curPos.y - deep < 0 ? 0 : curPos.y - deep;
            var xTo = curPos.x + deep < _grid.GridWidth ? curPos.x + deep : _grid.GridWidth;
            var yTo = curPos.y + deep < _grid.GridLength ? curPos.y + deep : _grid.GridLength;

            for (var i = xFrom; i <= xTo; ++i)
            {
                for (var j = yFrom; j <= yTo; ++j)
                {
                    if (i >= 0 && i < _grid.GridWidth && j >= 0 && j < _grid.GridLength)
                    {
                        var idx = GetGridIdxFromGridPos(i, j);
                        gridIdxs.Add(idx);
                        roleIds.UnionWith(_roleMap[idx]);
                    }
                }
            }
        }
    }
}
