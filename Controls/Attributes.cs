using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Controls
{
    [AttributeUsageAttribute(AttributeTargets.Property)]
    public sealed class OrderAttribute : Attribute
    {
        private int order = 0;

        public OrderAttribute(int order)
        {
            this.order = order;
        }

        public int Order
        {
            get { return this.order; }
        }
    }

    [AttributeUsageAttribute(AttributeTargets.Property)]
    public sealed class DisplayTextAttribute : Attribute
    {
        private string text = string.Empty;

        public DisplayTextAttribute(string text)
        {
            this.text = text;
        }

        public string Text
        {
            get { return this.text; }
        }
    }

    [AttributeUsageAttribute(AttributeTargets.Property)]
    public sealed class VisibleAttribute : Attribute
    {
        private bool visible = true;

        public VisibleAttribute(bool visible)
        {
            this.visible = visible;
        }

        public bool Visible
        {
            get { return this.visible; }
        }
    }

    [AttributeUsageAttribute(AttributeTargets.Property)]
    public sealed class WidthAttribute : Attribute
    {
        private Unit _width = new Unit();

        public WidthAttribute(string width)
        {
            _width = Unit.Parse(width, new System.Globalization.CultureInfo("nl-NL"));
        }

        public Unit Width
        {
            get { return _width; }
            set { _width = value; }
        }
    }
}
