using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;

namespace Controls
{
    internal static class KeyControlHelper
    {
        internal static string ConvertToText(IKeyControl ctl, string key)
        {
            string _text;
            if (ctl.ConversionEnabled)
            {
                var converter = ctl.GetConverter();
                if (converter != null)
                {
                    converter.SetKey(key);
                    _text = converter.ToString();
                }
                else
                {
                    _text = key;
                }
            }
            else
            {
                _text = key;
            }
            return _text;
        }

        internal static void LoadConverter(IKeyControl ctl)
        {
            Type t = BuildManager.GetType(ctl.Converter, true);
            if (t != null)
            {
                ctl.SetConverter((IKeyedObject)t.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, null, CultureInfo.InvariantCulture));
            }
            if (!string.IsNullOrEmpty(ctl.Key))
            {
                var converter = ctl.GetConverter();
                if (converter != null)
                {
                    converter.SetKey(ctl.Key);
                }
            }
        }
    }
}
