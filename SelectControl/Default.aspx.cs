using SelectControl.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SelectControl
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void cbEmail_SelectedItemChanged(object sender, Controls.EventArgs<string> e)
        {
            // might do anything, might do nothing.. result is available in e. 
            var result = new EmailAddressRepo().GetData().Where(p => p.GetKey() == e.Value);

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            // if an item is selected, result is available by looking at the ComboBox 'Key' property
            var result = new EmailAddressRepo().GetData().Where(p => p.GetKey() == cbEmail.Key);
            Button1.Text = result.FirstOrDefault()?.ToString();
        }
    }
}