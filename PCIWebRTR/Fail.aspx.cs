using System;

namespace PCIWebRTR
{
	public partial class Fail : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if ( ! Page.IsPostBack )
				lblVersion.Text = "Versions " + PCIWebRTR.SystemDetails.AppVersion   + " (App), "
				                              + PCIBusiness.SystemDetails.AppVersion + " (DLL)";
		}
	}
}
