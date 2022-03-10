using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls
{
    /// <summary>
    /// Indicates this object can be constructed from a 'keyString'
    /// and can display its value by calling 'ToString()'
    /// </summary>
    public interface IKeyedObject
    {
        // constructs an object from a keystring
        void SetKey(string keyString);
        string GetKey();
        KeyInfo GetKeyInfo();
        // displays value of this object
        string ToString();
    }
}
