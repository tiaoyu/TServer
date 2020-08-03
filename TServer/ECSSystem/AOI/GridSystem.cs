using System.Collections.Generic;
using TServer.ECSComponent;
using TServer.ECSEntity;

namespace TServer.ECSSystem.AOI
{
    /// <summary>
    /// 格子管理
    /// </summary>
    public class GridSystem
    {
        private Grid _grid;
        private Map _map;

        // 每个格子中的Entity列表 <格子index-<EntityType-角色ID集合>>
        public Dictionary<int, Dictionary<int, HashSet<int>>> _entityMap;
        public Dictionary<int, Dictionary<int, HashSet<int>>> EntityMap => _entityMap;

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
            _entityMap = new Dictionary<int, Dictionary<int, HashSet<int>>>();
            for (int i = 0; i < _grid.GridWidth * _grid.GridLength; ++i)
            {
                _entityMap.Add(i, new Dictionary<int, HashSet<int>>() {
                    { (int)EEntityType.ROLE,new HashSet<int>() },
                    { (int)EEntityType.MONSTER,new HashSet<int>() },
                    { (int)EEntityType.BULLET,new HashSet<int>() },
                });
            }
        }

        /// <summary>
        /// 格子位置转格子下标
        /// </summary>
        /// <param name="gridPos"></param>
        /// <returns></returns>
        public int GetGridIdxFromGridPos(CPosition<int> gridPos)
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
        public int GetGridPosFromMapPos(CPosition<double> mapPos, out CPosition<int> gridPos)
        {
            gridPos = new CPosition<int> { x = (int)(mapPos.x - _map.MapMinX) / _grid.GridUnit, y = (int)(mapPos.y - _map.MapMinY) / _grid.GridUnit };
            return GetGridIdxFromGridPos(gridPos);
        }
        public int GetGridPosFromMapPos(double x, double y, out CPosition<int> gridPos)
        {
            return GetGridPosFromMapPos(new CPosition<double>() { x = x, y = y }, out gridPos);
        }

        /// <summary>
        /// 格子下标转格子位置
        /// </summary>
        /// <param name="gridIdx"></param>
        /// <returns></returns>
        public CPosition<int> GetGridPosFromGridIdx(int gridIdx)
        {
            return new CPosition<int> { x = gridIdx % _grid.GridWidth, y = gridIdx / _grid.GridLength };
        }

        /// <summary>
        /// 添加Entity进入格子
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name=""></param>
        public int AddEntityToGrid(EEntity entity)
        {
            var idx = GetGridPosFromMapPos(entity.Position, out var _);
            _entityMap[idx][(int)entity.EntityType].Add(entity.Id);
            return idx;
        }

        /// <summary>
        /// 从格子中删除Entity
        /// </summary>
        /// <param name="roleId"></param>
        public void DeleteEntityFromGrid(EEntity entity)
        {
            var idx = GetGridPosFromMapPos(entity.Position, out var _);
            _entityMap[idx][(int)entity.EntityType].Remove(entity.Id);
        }

        /// <summary>
        /// 获取周围指定深度的格子下标集合
        /// </summary>
        /// <param name="deep"></param>
        /// <param name="curPos"></param>
        /// <param name="gridIdxs"></param>
        public void GetRolesFromSight(int deep, CPosition<int> curPos, out HashSet<int> gridIdxs, out HashSet<int> roleIds)
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
                        roleIds.UnionWith(_entityMap[idx][(int)EEntityType.ROLE]);
                    }
                }
            }
        }

        public void GetRolesFromSight(int deep, CPosition<double> curPos, out HashSet<int> gridIdxs, out HashSet<int> roleIds)
        {
            GetGridPosFromMapPos(curPos, out var gridPos);
            GetRolesFromSight(deep, gridPos, out gridIdxs, out roleIds);
        }

        public void GetEntitiesFromSight(int deep, CPosition<int> curPos, out HashSet<int> gridIdxs, out HashSet<int> roleIds)
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
                        roleIds.UnionWith(_entityMap[idx][(int)EEntityType.ROLE]);
                        roleIds.UnionWith(_entityMap[idx][(int)EEntityType.MONSTER]);
                        roleIds.UnionWith(_entityMap[idx][(int)EEntityType.BULLET]);
                    }
                }
            }
        }
        public void GetEntitiesFromSight(int deep, CPosition<double> curPos, out HashSet<int> gridIdxs, out HashSet<int> entityIds)
        {
            GetGridPosFromMapPos(curPos, out var gridPos);
            GetEntitiesFromSight(deep, gridPos, out gridIdxs, out entityIds);
        }

        /// <summary>
        /// 更新角色位置
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void UpdateEntityPosition(EEntity entity, double x, double y)
        {
            // 位置无效 
            if (!IsValidPos(x, y)) return;
            var oldPosition = new CPosition<double> { x = entity.Position.x, y = entity.Position.y };

            // 格子位置不变时说明AOI没变化 所以不必通知AOI内角色
            var oldGridId = GetGridPosFromMapPos(oldPosition, out var _);
            var newGridId = GetGridPosFromMapPos(x, y, out var _);
            if (oldGridId == newGridId)
            {
                entity.Position.x = x;
                entity.Position.y = y;
                return;
            }

            // 从旧格子中删除
            GetEntitiesFromSight(entity.SightDistance, oldPosition, out _, out var roleIdsOld);
            DeleteEntityFromGrid(entity);
            // 更新位置
            entity.Position.x = x;
            entity.Position.y = y;
            // 加入新格子
            AddEntityToGrid(entity);
            GetEntitiesFromSight(entity.SightDistance, entity.Position, out _, out var roleIdsNew);

            var bothTmp = new HashSet<int>(roleIdsOld);
            bothTmp.IntersectWith(roleIdsNew);
            roleIdsOld.ExceptWith(bothTmp);
            roleIdsNew.ExceptWith(bothTmp);

            roleIdsOld.Remove(entity.Id);
            SSight.Instance.LeaveSight(entity, roleIdsOld);
            SSight.Instance.EnterSight(entity, roleIdsNew);
        }
        //public void UpdateEntityPostion(EEntity entity, double x, double y)
        //{
        //    if (entity.EntityType == EEntityType.ROLE)
        //    {
        //        UpdateRolePosition(entity as ERole, x, y);
        //        return;
        //    }
        //    // 位置无效 
        //    if (!IsValidPos(x, y)) return;
        //    var oldPosition = new CPosition<double> { x = entity.Position.x, y = entity.Position.y };

        //    // 格子位置不变时说明AOI没变化 所以不必通知AOI内角色
        //    var oldGridId = GetGridPosFromMapPos(oldPosition, out var _);
        //    var newGridId = GetGridPosFromMapPos(x, y, out var _);
        //    if (oldGridId == newGridId)
        //    {
        //        entity.Position.x = x;
        //        entity.Position.y = y;
        //        return;
        //    }

        //    // 从旧格子中删除
        //    GetRolesFromSight(entity.SightDistance, oldPosition, out _, out var roleIdsOld);
        //    DeleteEntityFromGrid(entity);
        //    // 更新位置
        //    entity.Position.x = x;
        //    entity.Position.y = y;
        //    // 加入新格子
        //    AddEntityToGrid(entity);
        //    GetRolesFromSight(entity.SightDistance, entity.Position, out _, out var roleIdsNew);

        //    var bothTmp = new HashSet<int>(roleIdsOld);
        //    bothTmp.IntersectWith(roleIdsNew);
        //    roleIdsOld.ExceptWith(bothTmp);
        //    roleIdsNew.ExceptWith(bothTmp);

        //    roleIdsOld.Remove(entity.Id);
        //    SSight.Instance.LeaveSight(entity, roleIdsOld);
        //    SSight.Instance.EnterSight(entity, roleIdsNew);

        //}
        /// <summary>
        /// 是否在地图上的有效位置
        /// 超出地图边界为无效位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsValidPos(double x, double y)
        {
            return x >= _map.MapMinX && x <= _map.MapLength - _map.MapMinX && y >= _map.MapMinY && y <= _map.MapWidth - _map.MapMinY;
        }
    }
}
