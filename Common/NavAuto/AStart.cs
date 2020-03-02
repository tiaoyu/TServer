using System;
using System.Collections.Generic;
using System.Text;

namespace Common.NavAuto
{
    public class AStart
    {
        public void SetSourcePos(MapData map, int x, int y) { }

        public void SetTargetPos(MapData map, int x, int y) { }

        public int GValue(MapData map, int x, int y) { return 0; }

        public int HValue(MapData map, int x, int y) { return 0; }

        public int FValue(MapData map, int x, int y) { return 0; }

        public void CalOptimalPath(MapData map, int sX, int sY, int tX, int tY)
        {
            // 定义OpenList CloseList
            var openList = new List<Node>();
            var closeList = new List<Node>();
            openList.Add(new Node { x = sX, y = sY });


            // Step 1 从OpenList中挑选出F值最小的Node 称之为 CurNode 对其进行广度搜索深度为1 获得 ExpNodeList
            
            // Step 2 若 CurNode 就是目标点 则算法结束 进入End 阶段

            // Step 3.1 将 CurNode 从OpenList中移除并加入CloseList中 
            // Step 3.2 遍历ExpNodeList, 若 ExpNode 已存在于OpenList中 则判断 FValue(CurNode) + FValue(ExpNode,CurNode) < FValue(ExpNode)

            // Step Generate Optimal Path 回溯生成最短路径
        }
    }

    public class Node
    {
        public int GValue;
        public int HValue;
        public int FValue;
        public int x;
        public int y;
        public Node Parent;
    }
}
