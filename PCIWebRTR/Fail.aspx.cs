using System;
using System.Configuration;
using System.Text;
using System.Web.UI.WebControls;
using PCIBusiness;

namespace PCIWebRTR
{
	public partial class Fail : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if ( ! Page.IsPostBack )
				lblVersion.Text = "Version " + PCIBusiness.SystemDetails.AppVersion;
		}
	}
}
