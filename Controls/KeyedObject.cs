using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls
{
    public class KeyedObject<T> : IKeyedObject
    {
        protected Key<T> Key;

        public string GetKey() => Key.ToString();
        public KeyInfo GetKeyInfo()
        {
            if (Key != null)
            {
                return new KeyInfo
                {
                    Level = Key.KeyLevel,
                    More = Key.Option == KeyOption.more,
                    Other = Key.Option == KeyOption.other,
                    New = Key.Option == KeyOption.newitem,
                    Text = Key.Text
                };
            }

            return new KeyInfo();
        }

        public void SetKey(string keyString)
        {
            Key = Key<T>.CreateKey(keyString);
        }
    }
}
