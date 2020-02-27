using System;
using System.Collections.Generic;
using System.Text;

namespace TServer.Utilities
{
    public class SUtilities
    {
        private static int i = 0;
        public static int GetIndex()
        {
            return ++i;
        }
    }
}
