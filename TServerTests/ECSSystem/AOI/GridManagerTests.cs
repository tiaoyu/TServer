using Xunit;
using TServer.ECSSystem.AOI;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace TServer.ECSSystem.AOI.Tests
{
    public class GridManagerTests
    {
        private readonly ITestOutputHelper output;
        private readonly Random random = new Random();
        private int GetRandomNum(int min, int max)
        {
            return (int)(random.NextDouble() * (max - min)) + min;
        }
        public GridManagerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact()]
        public void GetRolesFromSightTest()
        {
            var manager = new GridManager(5, 99.0D, 99.0D);
            manager.Init();

            int[] map = new int[400];
            // 添加角色
            for (var i = 0; i < 1000; ++i)
            {
                var x = GetRandomNum(0, 100);
                var y = GetRandomNum(0, 100);
                map[manager.AddRoleToGrid(i, new Position<double> { x = x, y = y })]++;
            }

            // 获取视野内角色
            manager.GetRolesFromSight(1, new Position<int> { x = 0, y = 0 }, out var set1, out var role1);
            manager.GetRolesFromSight(1, new Position<int> { x = 10, y = 10 }, out var set2, out var role2);
            manager.GetRolesFromSight(1, new Position<int> { x = 19, y = 19 }, out var set3, out var role3);
            manager.GetRolesFromSight(1, new Position<int> { x = 0, y = 19 }, out var set4, out var role4);
            manager.GetRolesFromSight(1, new Position<int> { x = 19, y = 0 }, out var set5, out var role5);
            manager.GetRolesFromSight(2, new Position<int> { x = 10, y = 10 }, out var set6, out var role6);


            Assert.True(set1.Count == 4);
            Assert.True(set2.Count == 9);
            Assert.True(set3.Count == 4);
            Assert.True(set4.Count == 4);
            Assert.True(set5.Count == 4);

            //Assert.True(role1.Count == 7);
            //Assert.True(role2.Count == 0);
            //Assert.True(role3.Count == 0);
            //Assert.True(role4.Count == 0);
            //Assert.True(role5.Count == 0);

            var strings = new StringBuilder();
            for (var i = 0; i <= 19; ++i)
            {
                for (var j = 0; j <= 19; ++j)
                {
                    strings.Append($"{manager.RoleMap[manager.GetGridIdxFromGridPos(i, j)].Count} ");
                }
                output.WriteLine(strings.ToString());
                strings.Clear();
            }
            foreach (var id in set6)
            {
                output.WriteLine($"{id}");
            }
            Assert.True(set6.Count == 25);
        }
    }
}