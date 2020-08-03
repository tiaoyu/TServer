using System;
using System.Collections.Generic;
using System.Text;

namespace Common.NavAuto
{
    /// <summary>
    /// A*寻路
    /// 参考实现
    /// https://www.cs.auckland.ac.nz/compsci767s2c/projectReportExamples.d/astarNilsson.pdf
    /// </summary>
    public class AStar
    {
        /// <summary>
        /// 计算最优路径
        /// </summary>
        /// <param name="map">地图</param>
        /// <param name="sX">起点X</param>
        /// <param name="sY">起点Y</param>
        /// <param name="tX">终点X</param>
        /// <param name="tY">终点Y</param>
        /// <returns>结果返回所有路点</returns>
        public static Stack<Node> CalOptimalPath(MapData map, int sX, int sY, int tX, int tY)
        {
            // 目标点位置错误
            if (!map.IsValidPosition(tX, tY)) return new Stack<Node>();

            // 定义OpenList CloseList
            var openList = new HashSet<Node>();
            var closeList = new HashSet<Node>();
            var targetNode = new Node(null) { x = tX, y = tY };

            var curNode = new Node(null) { x = sX, y = sY };
            openList.Add(curNode);
            curNode.CalculateValue(targetNode);
            var count = 0;
            while (openList != null)
            {
                count++;
                // Step 1 从OpenList中挑选出F值最小的Node 称之为 CurNode 对其进行广度搜索深度为1 获得 ExpNodeList
                curNode = null;
                foreach (var node in openList)
                {
                    if (curNode == null) curNode = node;

                    if (node.FValue < curNode.FValue)
                        curNode = node;
                }
                map.GetAroundNode(curNode, out var expNodeList);
                expNodeList.ExceptWith(closeList);

                // Step 2 若 CurNode 就是目标点 则算法结束 进入回溯阶段
                if (curNode.Equals(targetNode))
                {
                    break;
                }

                // Step 3.1 将 CurNode 从OpenList中移除并加入CloseList中 
                openList.Remove(curNode);
                closeList.Add(curNode);

                // Step 3.2 遍历ExpNodeList, 若 ExpNode 已存在于OpenList中 则判断 HValue(ExpNode) + GValue(CurNode) + GValue(ExpNode,CurNode) < FValue(ExpNode)
                foreach (var node in expNodeList)
                {
                    if (openList.Contains(node))
                    {
                        if ((node.HValue + curNode.GValue + Node.GetGValue(node, curNode)) < node.FValue)
                        {
                            node.Parent = curNode;
                            node.CalculateValue(targetNode);
                        }
                    }
                    else
                    {
                        node.Parent = curNode;
                        node.CalculateValue(targetNode);
                        openList.Add(node);
                    }
                }
            }

            // Step 4 回溯生成最短路径
            var optimalPath = new Stack<Node>();
            optimalPath.Push(curNode);
            while (curNode.Parent != null)
            {
                optimalPath.Push(curNode.Parent);
                curNode = curNode.Parent;
            }
            //Console.WriteLine($"Find path {count}.");
            return optimalPath;
        }

        public static void Test(string path)
        {
            var mapData = new MapData(path);
            mapData.Init();
            var optimalPath = CalOptimalPath(mapData, 22, 22, 22, 77);
            foreach (var node in optimalPath)
            {
                mapData.map[node.x, node.y] = 2;
            }
            mapData.ConsolePrintMap();
        }
    }

    /// <summary>
    /// FValue = GValue + HValue
    /// </summary>
    public class Node
    {
        public int GValue;
        public int HValue;
        public int FValue { get { return GValue + HValue; } }
        public int x;
        public int y;
        public Node Parent;
        public Node(Node p)
        {
            Parent = p;
        }

        /// <summary>
        /// 计算当前
        /// </summary>
        /// <param name="target"></param>
        public void CalculateValue(Node target)
        {
            if (Parent == null)
            {
                GValue = 0;
                HValue = 0;
            }
            else
            {
                GValue = GetGValue(this, Parent);
                HValue = GetHValue(this, target);
            }
        }

        /// <summary>
        /// 预测值计算方法
        /// </summary>
        /// <param name="aNode"></param>
        /// <param name="bNode"></param>
        /// <returns></returns>
        public static int GetHValue(Node aNode, Node bNode)
        {
            //return (bNode.x - aNode.x) * (bNode.x - aNode.x) + (bNode.y - aNode.y) * (bNode.y - aNode.y);
            return (Math.Abs(bNode.x - aNode.x) + Math.Abs(bNode.y - aNode.y)) * 10;
        }

        public static int GetGValue(Node aNode, Node bNode)
        {
            //return (aNode.x - bNode.x) * (aNode.x - bNode.x) + (aNode.y - bNode.y) * (aNode.y - bNode.y);
            return (int)(Math.Sqrt((aNode.x - bNode.x) * (aNode.x - bNode.x) + (aNode.y - bNode.y) * (aNode.y - bNode.y)) * 10);
        }

        public override bool Equals(object obj)
        {
            if (obj is Node node)
            {
                return node.x == x && node.y == y;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + x.GetHashCode();
                hash = hash * 23 + y.GetHashCode();
                return hash;
            }
        }
    }
}
