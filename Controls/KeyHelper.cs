using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls
{
    public static class KeyHelper
    {
        public const char keySeparator = ';';

        public static bool IsKey(string keyString)
        {
            return keyString.Split(keySeparator).Length == 4;
        }
    }
}
