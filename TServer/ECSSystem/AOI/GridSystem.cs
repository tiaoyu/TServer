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

        // 每个格子中的Entity列表 <格子index-<EntityType-EntityID集合>>
        private Dictionary<int, Dictionary<int, HashSet<int>>> _entityIdMap;
        public Dictionary<int, Dictionary<int, HashSet<int>>> EntityIdMap => _entityIdMap;

        private Dictionary<int, Dictionary<int, HashSet<EEntity>>> _entityMap;
        public Dictionary<int, Dictionary<int, HashSet<EEntity>>> EntityMap => _entityMap;

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
            _entityIdMap = new Dictionary<int, Dictionary<int, HashSet<int>>>();
            _entityMap = new Dictionary<int, Dictionary<int, HashSet<EEntity>>>();

            for (int i = 0; i < _grid.GridWidth * _grid.GridLength; ++i)
            {
                _entityIdMap.Add(i, new Dictionary<int, HashSet<int>>() {
                    { (int)EEntityType.ROLE,new HashSet<int>() },
                    { (int)EEntityType.MONSTER,new HashSet<int>() },
                    { (int)EEntityType.BULLET,new HashSet<int>() },
                });
                _entityMap.Add(i, new Dictionary<int, HashSet<EEntity>>()
                {
                    { (int)EEntityType.ROLE,new HashSet<EEntity>() },
                    { (int)EEntityType.MONSTER,new HashSet<EEntity>() },
                    { (int)EEntityType.BULLET,new HashSet<EEntity>() },
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
            //_entityIdMap[idx][(int)entity.EntityType].Add(entity.Id);
            _entityMap[idx][(int)entity.EntityType].Add(entity);
            return idx;
        }

        /// <summary>
        /// 从格子中删除Entity
        /// </summary>
        /// <param name="roleId"></param>
        public void DeleteEntityFromGrid(EEntity entity)
        {
            var idx = GetGridPosFromMapPos(entity.Position, out var _);
            _entityIdMap[idx][(int)entity.EntityType].Remove(entity.Id);
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
                        roleIds.UnionWith(_entityIdMap[idx][(int)EEntityType.ROLE]);
                    }
                }
            }
        }

        /// <summary>
        /// 一定范围内的Entity集合
        /// </summary>
        /// <param name="deep"></param>
        /// <param name="gridPos"></param>
        /// <param name="gridIdxs"></param>
        /// <param name="entities"></param>
        public void GetEntitiesFromSight(int deep, CPosition<int> gridPos, out HashSet<int> gridIdxs, out HashSet<EEntity> roleIds)
        {
            gridIdxs = new HashSet<int>();
            roleIds = new HashSet<EEntity>();
            var xFrom = gridPos.x - deep < 0 ? 0 : gridPos.x - deep;
            var yFrom = gridPos.y - deep < 0 ? 0 : gridPos.y - deep;
            var xTo = gridPos.x + deep < _grid.GridWidth ? gridPos.x + deep : _grid.GridWidth;
            var yTo = gridPos.y + deep < _grid.GridLength ? gridPos.y + deep : _grid.GridLength;

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

        /// <summary>
        /// 找到所有能看到自己的Entity集合
        /// </summary>
        /// <param name="self"></param>
        /// <param name="deep"></param>
        /// <param name="curPos"></param>
        /// <param name="gridIdxs"></param>
        /// <param name="entities"></param>
        public void GetEntitiesFromSight(EEntity self, CPosition<double> curPos, out HashSet<int> gridIdxs, out HashSet<EEntity> entities)
        {
            GetGridPosFromMapPos(curPos, out var gridPos);

            GetEntitiesFromSight(self.SightDistance, gridPos, out gridIdxs, out entities);
        }

        /// <summary>
        /// 获得两个位置的相对格子距离
        /// </summary>
        /// <param name="aPos"></param>
        /// <param name="bPos"></param>
        /// <returns></returns>
        public int GetDistanceFromTwoPosition(CPosition<double> aPos, CPosition<double> bPos)
        {
            GetGridPosFromMapPos(aPos, out var aGridPos);
            GetGridPosFromMapPos(bPos, out var bGridPos);

            var res = System.Math.Max(System.Math.Abs(aGridPos.x - bGridPos.x), System.Math.Abs(aGridPos.y - bGridPos.y));
            return res;
        }

        /// <summary>
        /// 更新Entity位置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void UpdateEntityPosition(EEntity self, double x, double y, bool isFirst = false)
        {
            // 位置无效 
            if (!IsValidPos(x, y)) return;
            var oldPosition = new CPosition<double> { x = self.Position.x, y = self.Position.y };

            // 格子位置不变时说明AOI没变化 所以不必通知AOI内角色
            var oldGridId = GetGridPosFromMapPos(oldPosition, out var _);
            var newGridId = GetGridPosFromMapPos(x, y, out var _);
            if (oldGridId == newGridId && !isFirst)
            {
                self.Position.x = x;
                self.Position.y = y;
                return;
            }

            GetEntitiesFromSight(self, oldPosition, out _, out var oldEntities);

            // 从旧格子中删除
            DeleteEntityFromGrid(self);
            // 更新位置
            self.Position.x = x;
            self.Position.y = y;
            // 加入新格子
            AddEntityToGrid(self);

            GetEntitiesFromSight(self, self.Position, out _, out var newEntities);

            if (!isFirst)
            {// 首次进入AOI不需要排除
                var bothTmp = new HashSet<EEntity>(oldEntities);
                bothTmp.IntersectWith(newEntities);
                oldEntities.ExceptWith(bothTmp);
                newEntities.ExceptWith(bothTmp);
            }

            oldEntities.Remove(self);
            SSight.Instance.LeaveSight(self, oldEntities);
            SSight.Instance.EnterSight(self, newEntities);
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
