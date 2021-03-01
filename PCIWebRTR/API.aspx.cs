using System;
using System.Web.UI;
using PCIBusiness;

namespace PCIWebRTR
{
	public partial class API : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if ( Page.IsPostBack )
				return;

			string providerCode = WebTools.RequestValueString(Request,"BureauCode");
			string contractCode = WebTools.RequestValueString(Request,"ContractCode");
			string token        = WebTools.RequestValueString(Request,"Token");
			string cardNumber   = WebTools.RequestValueString(Request,"CardNumber");

			Tools.LogInfo("PageLoad/1",providerCode + " | " + contractCode
			                                        + " | " + token
			                                        + " | " + cardNumber,223,this);
		}
	}
}