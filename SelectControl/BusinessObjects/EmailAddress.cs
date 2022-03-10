using Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SelectControl.BusinessObjects
{

    public class EmailAddress : KeyedObject<string>
    {
        public EmailAddress() { }
        public EmailAddress(string key)
        {
            Key = new Key<string>(key);
        }
        public string Name { get; set; }
        public string Email { get; set; }
        public override string ToString() => Email;
    }
}