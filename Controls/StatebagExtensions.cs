using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace Controls
{
    public static class StatebagExtensions
    {
        /// <summary>
        /// gets a value from statebag using the specified key
        /// </summary>
        /// <typeparam name="T">type of value to return</typeparam>
        /// <param name="vs">viewstate object</param>
        /// <param name="key">key to look for</param>
        /// <param name="defaultValue">value to return if key doesn't exist in viewstate</param>
        /// <returns></returns>
        public static T Get<T>(this StateBag vs, string key, T defaultValue)
        {
            if (vs[key] != null)
            {
                if (vs[key] is T)
                {
                    return (T)vs[key];
                }
            }
            return defaultValue;
        }
    }
}
