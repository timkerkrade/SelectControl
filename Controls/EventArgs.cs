using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls
{
    public class EventArgs<T> : EventArgs
    {
        public EventArgs(T value)
        {
            m_value = value;
        }

        private T m_value;

        public T Value
        {
            get { return m_value; }
        }
    }
}
