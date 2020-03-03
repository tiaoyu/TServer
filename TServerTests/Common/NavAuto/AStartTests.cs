using Xunit;
using Xunit.Abstractions;

namespace Common.NavAuto.Tests
{
    public class AStartTests
    {
        private readonly ITestOutputHelper output;

        public AStartTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact()]
        public void CalOptimalPathTest()
        {
            var mapData = new MapData(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.IO.Path.Combine("MapData", "1.txt")));
            mapData.Init();
            var optimalPath = AStar.CalOptimalPath(mapData, 22, 22, 22, 77);
            foreach (var node in optimalPath)
            {
                output.WriteLine($"({node.x}, {node.y})");
            }
            Assert.True(optimalPath.Count > 1);
        }
    }
}