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

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (int.TryParse(tbUurtarief.Text, out int tarief))
            {
                var startTime = new DateTime(2022, 03, 10, 9, 0, 0);
                var ts = DateTime.Now - startTime;
                lbResult.Text = "Dit hebben we al: EUR " + ( ts.TotalMinutes * (1200 * 25 / (8 * 60))).ToString();
            }
        }
    }
}