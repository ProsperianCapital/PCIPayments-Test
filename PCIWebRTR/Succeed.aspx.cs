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
				lblVersion.Text  = "Versions " + PCIWebRTR.SystemDetails.AppVersion   + " (App), "
				                               + PCIBusiness.SystemDetails.AppVersion + " (DLL)";
				string transRef  = Tools.NullToString(Request["TransRef"]);
				string mode      = Tools.NullToString(Request["Mode"]);
				string hmac      = Tools.NullToString(Request["hmac"]);
				string msg       = Tools.NullToString(Request["message"]);
//				string notifyURL = Tools.NullToString(Request["NotifyUrl"]);
//				string returnURL = Tools.NullToString(Request["ReturnUrl"]);

				Tools.LogInfo("Succeed.Page_Load/20","transRef=" +transRef
				                                 + ", hmac="     +hmac
				                                 + ", msg="      +msg,199);

				lblTransRef.Text = transRef + ( mode.Length > 0 ? " (Mode=" + mode + ")" : "" );
//				lblNotify.Text   = notifyURL;
//				lblReturn.Text   = returnURL;

				if ( msg.Length > 0 )
				{
					lblHmac.Text      = hmac;
					lblMessage.Text   = msg;
					string txnStatus  = Tools.JSONValue(msg,"netsTxnStatus");
					string resultMsg  = Tools.JSONValue(msg,"netsTxnMsg");
					string resultCode = Tools.JSONValue(msg,"stageRespCode");
					Tools.LogInfo("Succeed.Page_Load/40","(eNETS) netsTxnStatus="+txnStatus,199);
					Tools.LogInfo("Succeed.Page_Load/50","(eNETS) netsTxnMsg="+resultMsg,199);
					Tools.LogInfo("Succeed.Page_Load/60","(eNETS) stageRespCode="+resultCode,199);
				}
			}
		}
	}
}
