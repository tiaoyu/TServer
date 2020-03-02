using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.NavAuto
{
    public class MapData
    {
        private readonly string filePath;
        private int length;
        private int width;

        public int[,] map;

        public int Width { get => width; set => width = value; }
        public int Length { get => length; set => length = value; }

        public MapData(string path)
        {
            filePath = path;
        }

        public bool Init()
        {
            var f = File.ReadAllLines(filePath);
            var lineCount = f.Length;

            if (f.Length <= 1) return false;

            // 第一行为地图长宽 逗号分隔
            var size = f[0].Split(',');

            if (size.Length < 2) return false;
            if (!int.TryParse(size[0], out var l)) return false;
            if (!int.TryParse(size[1], out var w)) return false;

            Length = l;
            Width = w;
            map = new int[l, w];

            for (var i = 1; i < lineCount; ++i)
            {
                var row = f[i].ToCharArray();
                for (var j = 0; j < row.Length; ++i)
                {
                    if (row[j] >= 48 && row[j] <= 57)
                        map[i - 1, j] = row[j] - 48;
                    else return false;
                }
            }
            return true;
        }
    }
}
