using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls
{
    public interface IKeyedObjectContainer : IKeyedObject
    {
        string AllItemText { get; set; }
        List<IKeyedObject> GetData();
        IKeyedObject GetSingleItem(string query);
        int GetMore();
        void FilterBy(string filterText);
    }
}
