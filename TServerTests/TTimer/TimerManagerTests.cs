using Xunit;
using Common.TTimer;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace Common.TTimer.Tests
{
    public class TimerManagerTests
    {
        private readonly ITestOutputHelper output;
        private TimerManager tmgr;
        public TimerManagerTests(ITestOutputHelper output)
        {
            this.output = output;
            this.tmgr = new TimerManager();
            tmgr.Init();
            //tmgr.Insert(10, 10, 1, null, (obj) =>
            //{
            //    output.WriteLine($"{tmgr.CurrentTick}: hi!");
            //});
            //tmgr.Insert(20, 10, 1, null, (obj) =>
            //{
            //    output.WriteLine($"{tmgr.CurrentTick}: hi!");
            //});
            //tmgr.Insert(30, 10, 1, null, (obj) =>
            //{
            //    output.WriteLine($"{tmgr.CurrentTick}: hi!");
            //});
        }

        [Fact()]
        public void InsertTest()
        {
            tmgr.Insert(40, 0, 1, null, (obj) =>
            {
                output.WriteLine("hi!");
            });
            Assert.True(true);
        }

        [Fact()]
        public void UpdateTest()
        {
            tmgr.Update(1000);
            tmgr.Insert(0, 10, 10, null, (obj) =>
            {
                output.WriteLine($"{tmgr.CurrentTick}: one_hi!");
            });
            tmgr.Insert(10, 10, 10, null, (obj) =>
            {
                output.WriteLine($"{tmgr.CurrentTick}: two_hi!");
            });
            tmgr.Insert(20, 10, 10, null, (obj) =>
            {
                output.WriteLine($"{tmgr.CurrentTick}: three_hi!");
            });
            tmgr.Insert(3000, 3000, 10, null, (obj) =>
            {
                output.WriteLine($"{tmgr.CurrentTick}: four_hi!");
            });
            tmgr.Update(300000);
            Assert.True(true);
        }

        [Fact()]
        public void RemoveTest()
        {
            int t1;
            t1 = tmgr.Insert(0, 100, 10, null, obj =>
            {
                output.WriteLine($"once is over~");
            });

            tmgr.Update(200);
            Assert.True(tmgr.DicTimer.Count == 1);

            tmgr.Remove(t1);
            Assert.True(tmgr.DicTimer.Count == 0);

            tmgr.Update(200);

        }
    }
}