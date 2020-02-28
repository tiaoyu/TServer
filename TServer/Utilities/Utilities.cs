using System;
using System.Collections.Generic;
using System.Text;

namespace TServer.Utilities
{
    public class SUtilities
    {
        private static Random _random = new Random();
        private static int i = 0;
        public static int GetIndex()
        {
            return ++i;
        }

        public static int GetRandomInt(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
}
