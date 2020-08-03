using Xunit;
using TServer.ECSSystem.AOI;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using TServer.ECSComponent;
using TServer.ECSEntity;

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
            var manager = new GridSystem(5, 499.0D, 499.0D);
            manager.Init();

            int[] map = new int[250000];
            // 添加角色
            for (var i = 0; i < 250000; ++i)
            {
                var x = GetRandomNum(0, 499);
                var y = GetRandomNum(0, 499);
                map[manager.AddEntityToGrid(new EEntity { Id = i, EntityType = EEntityType.ROLE, Position = new CPosition<double> { x = x, y = y } })]++;
            }

            // 获取视野内角色
            manager.GetRolesFromSight(1, new CPosition<int> { x = 0, y = 0 }, out var set1, out var role1);
            manager.GetRolesFromSight(1, new CPosition<int> { x = 10, y = 10 }, out var set2, out var role2);
            manager.GetRolesFromSight(1, new CPosition<int> { x = 99, y = 99 }, out var set3, out var role3);
            manager.GetRolesFromSight(1, new CPosition<int> { x = 0, y = 99 }, out var set4, out var role4);
            manager.GetRolesFromSight(1, new CPosition<int> { x = 99, y = 0 }, out var set5, out var role5);
            manager.GetRolesFromSight(2, new CPosition<int> { x = 10, y = 10 }, out var set6, out var role6);

            output.WriteLine($"{set1.Count}");
            output.WriteLine($"{set2.Count}");
            output.WriteLine($"{set3.Count}");
            output.WriteLine($"{set4.Count}");
            output.WriteLine($"{set5.Count}");
            output.WriteLine($"{set6.Count}");
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
            for (var i = 0; i <= 99; ++i)
            {
                for (var j = 0; j <= 99; ++j)
                {
                    strings.Append($"{manager.EntityMap[manager.GetGridIdxFromGridPos(i, j)][(int)EEntityType.ROLE].Count} ");
                }
                output.WriteLine(strings.ToString());
                strings.Clear();
            }
            Assert.True(set6.Count == 25);
        }
    }
}