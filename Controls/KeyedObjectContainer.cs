using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls
{
    public abstract class KeyedObjectContainer<T> : IKeyedObjectContainer
    {
        public string AllItemText { get; set; }

        public abstract void FilterBy(string filterText);

        public abstract List<IKeyedObject> GetData();

        protected Key<T> Key;

        public abstract int GetMore();

        public abstract IKeyedObject GetSingleItem(string query);

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
