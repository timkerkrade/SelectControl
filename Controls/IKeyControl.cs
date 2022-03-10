using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls
{
    public interface IKeyControl
    {
        string Key { get; set; }
        string Converter { get; set; }
        IKeyedObject GetConverter();
        void SetConverter(IKeyedObject converter);
        bool ConversionEnabled { get; set; }
        string ClientID { get; }
        string Text { get; set; }
    }
}
