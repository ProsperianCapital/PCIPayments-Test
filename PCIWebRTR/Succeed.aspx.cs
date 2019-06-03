using System;
using PCIBusiness;

namespace PCIWebRTR
{
	public partial class Succeed : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Tools.LogInfo("Succeed.Page_Load/10","Loaded",199);

			if ( ! Page.IsPostBack )
			{
				lblVersion.Text = "Version " + SystemDetails.AppVersion;
				string hmac     = Request["hmac"];
				string msg      = Request["message"];

				Tools.LogInfo("Succeed.Page_Load/20","hmac="+hmac,199);
				Tools.LogInfo("Succeed.Page_Load/30","message="+msg,199);

				if ( msg.Length > 0 )
				{
					lblHmac.Text      = hmac;
					lblMessage.Text   = msg;
					string txnStatus  = Tools.JSONValue(msg,"netsTxnStatus");
					string resultMsg  = Tools.JSONValue(msg,"netsTxnMsg");
					string resultCode = Tools.JSONValue(msg,"stageRespCode");
					Tools.LogInfo("Succeed.Page_Load/40","netsTxnStatus="+txnStatus,199);
					Tools.LogInfo("Succeed.Page_Load/50","netsTxnMsg="+resultMsg,199);
					Tools.LogInfo("Succeed.Page_Load/60","stageRespCode="+resultCode,199);
				}
			}
		}
	}
}
