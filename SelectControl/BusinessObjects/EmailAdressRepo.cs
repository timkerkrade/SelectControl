using Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SelectControl.BusinessObjects
{
    public class EmailAddressRepo : KeyedObjectContainer<string>
    {
        private List<EmailAddress> testData = new List<EmailAddress>()
        {
            new EmailAddress("jan@test.nl") { Email="jan@test.nl", Name="Jan (Test)" },
            new EmailAddress("piet@test.nl") { Email="piet@test.nl", Name="Piet (Test)" },
            new EmailAddress("somebody@test.nl") { Email="somebody@test.nl", Name="Iemand" },
            new EmailAddress("info@google.com") { Email="info@google.com", Name="Google Info" },
            new EmailAddress("pete@google.com") { Email="pete@google.com", Name="Google Info - Pete" },
            new EmailAddress("john@google.com") { Email="john@google.com", Name="Google Info - Pete" },
        };

        private string _filter = string.Empty;

        public override void FilterBy(string filterText)
        {
            _filter = filterText;
        }

        public override List<IKeyedObject> GetData()
        {
            // implement your own filtering algorithm
            if (string.IsNullOrWhiteSpace(_filter)) return testData.Select(p=> (IKeyedObject)p).ToList();
            return testData.Where(p => p.Email.Contains(_filter) || p.Name.Contains(_filter)).Select(p => (IKeyedObject)p).ToList();
        }

        public override int GetMore()
        {
            // only if 'more construct' is used
            throw new NotImplementedException();
        }

        public override IKeyedObject GetSingleItem(string query)
        {
            return testData.FirstOrDefault(p => p.Email == query);
        }
    }
}