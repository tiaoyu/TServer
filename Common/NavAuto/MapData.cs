using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Common.NavAuto
{
    public enum EEarthType
    {
        NORMAL = 0,
        DISABLE = 1,
    }

    public class MapData
    {
        private readonly string filePath;
        private int length;
        private int width;

        public int[,] map;
        public List<int[]> monsterPos;
        public int Width { get => width; set => width = value; }
        public int Length { get => length; set => length = value; }

        public MapData(string path)
        {
            filePath = path;
            monsterPos = new List<int[]>();
        }

        public bool Init()
        {
            var f = File.ReadAllLines(filePath);
            var inx = 0;
            var lineCount = f.Length;

            if (f.Length <= 1) return false;

            // 第一行为地图长宽 逗号分隔
            var size = f[inx++].Split(',');

            if (size.Length < 2) return false;
            if (!int.TryParse(size[0], out var l)) return false;
            if (!int.TryParse(size[1], out var w)) return false;

            Length = l;
            Width = w;
            map = new int[l, w];
            for (var i = inx; i < l + inx; ++i)
            {
                var row = f[i].ToCharArray();
                for (var j = 0; j < row.Length; ++j)
                {
                    if (row[j] >= 48 && row[j] <= 57)
                        map[i - 1, j] = row[j] - 48;
                    else return false;
                }
            }
            inx += l;
            // 初始化的怪物位置
            if (!int.TryParse(f[inx++], out var monsterCount)) return false;
            for (var i = inx; i < monsterCount + inx; ++i)
            {
                var pos = f[i].Split(",").Select(int.Parse).ToArray();
                monsterPos.Add(pos);
            }
            return true;
        }

        public void GetAroundNode(Node curNode, out HashSet<Node> expandNodes)
        {
            expandNodes = new HashSet<Node>();
            if (curNode == null) return;

            if (curNode.x > 0 && curNode.y > 0)
            {
                if (map[curNode.x - 1, curNode.y - 1] == (int)EEarthType.NORMAL)
                    expandNodes.Add(new Node(null) { x = curNode.x - 1, y = curNode.y - 1 });
                if (map[curNode.x - 1, curNode.y] == (int)EEarthType.NORMAL)
                    expandNodes.Add(new Node(null) { x = curNode.x - 1, y = curNode.y });
                if (map[curNode.x, curNode.y - 1] == (int)EEarthType.NORMAL)
                    expandNodes.Add(new Node(null) { x = curNode.x, y = curNode.y - 1 });
            }

            if (curNode.x + 1 < Length && curNode.y + 1 < Width)
            {
                if (map[curNode.x + 1, curNode.y + 1] == (int)EEarthType.NORMAL)
                    expandNodes.Add(new Node(null) { x = curNode.x + 1, y = curNode.y + 1 });
                if (map[curNode.x, curNode.y + 1] == (int)EEarthType.NORMAL)
                    expandNodes.Add(new Node(null) { x = curNode.x, y = curNode.y + 1 });
                if (map[curNode.x + 1, curNode.y] == (int)EEarthType.NORMAL)
                    expandNodes.Add(new Node(null) { x = curNode.x + 1, y = curNode.y });
            }

            if (curNode.x > 0 && curNode.y + 1 < Width)
            {
                if (map[curNode.x - 1, curNode.y + 1] == (int)EEarthType.NORMAL)
                    expandNodes.Add(new Node(null) { x = curNode.x - 1, y = curNode.y + 1 });

            }

            if (curNode.y > 0 && curNode.x + 1 < Length)
            {
                if (map[curNode.x + 1, curNode.y - 1] == (int)EEarthType.NORMAL)
                    expandNodes.Add(new Node(null) { x = curNode.x + 1, y = curNode.y - 1 });
            }
        }

        public bool IsValidPosition(int x, int y)
        {

            return x >= 0 && x < Length && y < Width && y >= 0 && map[x, y] == 0;
        }

        public void ConsolePrintMap()
        {
            for (var i = 0; i < Length; ++i)
            {
                for (var j = 0; j < Width; ++j)
                {
                    Console.Write(map[i, j]);
                }
                Console.WriteLine();
            }
        }
    }
}
