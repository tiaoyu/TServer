using System;
using System.Collections.Generic;
using System.Text;

namespace TServer.Utilities
{
    public class SUtilities
    {
        private static Random _random = new Random(Guid.NewGuid().ToString().GetHashCode());

        public static int GetRandomInt(int min, int max)
        {
            _random = new Random(Guid.NewGuid().ToString().GetHashCode());
            return _random.Next(min, max);
        }
    }
}
