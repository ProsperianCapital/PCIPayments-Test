using System;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Web.UI.WebControls;
using PCIBusiness;

namespace PCIWebRTR
{
	public partial class RTR : System.Web.UI.Page
	{
		private byte   systemStatus;
		private int    timeOut;
		private int    maxRows;
		private string provider;

		protected void Page_Load(object sender, EventArgs e)
		{
			try
			{
				systemStatus = System.Convert.ToByte(Tools.ConfigValue("SystemStatus"));
			}
			catch
			{
				systemStatus = 0;
			}
			lblSStatus.Text     = ( systemStatus == 0 ? "Active" : "Disabled" );
			btnProcess1.Enabled = ( systemStatus == 0 );
			btnProcess2.Enabled = ( systemStatus == 0 );
			lblTest.Text        = "";
			lblError.Text       = "";
			lblJS.Text          = "";

			if ( ! Page.IsPostBack )
			{
				lblVersion.Text = "Version " + SystemDetails.AppVersion;

				foreach (int bureauCode in Enum.GetValues(typeof(Constants.PaymentProvider)))
					lstProvider.Items.Add(new ListItem(Enum.GetName(typeof(Constants.PaymentProvider),bureauCode),bureauCode.ToString().PadLeft(3,'0')));

				DBConn                   conn       = null;
				ConnectionStringSettings db         = ConfigurationManager.ConnectionStrings["DBConn"];
				string[]                 connString = Tools.NullToString(db.ConnectionString).Split(';');
				int                      k;

				lblSQLServer.Text = "";
				lblSQLDB.Text     = "";
				lblSQLUser.Text   = "";
				lblSQLStatus.Text = "";
				lblSMode.Text     = Tools.ConfigValue("SystemMode");

				foreach ( string x in connString )
				{
					k = x.ToUpper().IndexOf("SERVER=");
					if ( k >= 0 )
					{
						lblSQLServer.Text = x.Substring(k+7);
						continue;
					}
					k = x.ToUpper().IndexOf("DATABASE=");
					if ( k >= 0 )
					{
						lblSQLDB.Text = x.Substring(k+9);
						continue;
					}
					k = x.ToUpper().IndexOf("UID=");
					if ( k >= 0 )
						lblSQLUser.Text = x.Substring(k+4);
				}
				if ( Tools.OpenDB(ref conn) )
					lblSQLStatus.Text = "Connected";
				else
					lblSQLStatus.Text = "<span class='Red'>Cannot connect</span>";
				Tools.CloseDB(ref conn);
				conn = null;
			}
			ProviderDetails();
		}

		private void ProviderDetails()
		{
			string   bureau      = lstProvider.SelectedValue.Trim();
			Provider provider    = new Provider();
			provider.BureauCode  = bureau;
			lblBureauCode.Text   = provider.BureauCode;
			lblBureauStatus.Text = provider.BureauStatusName;

			if ( provider.BureauStatusCode == 2 ) // Disabled
			{
				provider             = null;
				btnProcess1.Text     = "Tokens (Disabled)";
				btnProcess2.Text     = "Payments (Disabled)";
				btnProcess1.Enabled  = false;
				btnProcess2.Enabled  = false;
				lblBureauURL.Text    = "";
				lblMerchantKey.Text  = "";
				lblMerchantUser.Text = "";
				lblCards.Text        = "";
				lblPayments.Text     = "";
				return;
			}
			btnProcess1.Text    = "Get Tokens";
			btnProcess2.Text    = "Process Payments";
			btnProcess1.Enabled = true;
			btnProcess2.Enabled = true;

			if ( bureau.Length > 0 )
				using (Payments payments = new Payments())
				{
					provider             = payments.Summary(bureau);
					lblBureauURL.Text    = provider.BureauURL;
					lblMerchantKey.Text  = provider.MerchantKey;
					lblMerchantUser.Text = provider.MerchantUserID;
					lblCards.Text        = provider.CardsToBeTokenized.ToString()    + ( provider.CardsToBeTokenized    >= Constants.C_MAXPAYMENTROWS() ? "+" : "" );
					lblPayments.Text     = provider.PaymentsToBeProcessed.ToString() + ( provider.PaymentsToBeProcessed >= Constants.C_MAXPAYMENTROWS() ? "+" : "" );
					if ( provider.PaymentType == (byte)Constants.TransactionType.TokenPayment )
					{
						btnProcess1.Text            = "Get Tokens";
						btnProcess1.Enabled         = true;
						btnProcess1.CommandArgument = ((byte)Constants.TransactionType.GetToken).ToString();
						btnProcess2.CommandArgument = ((byte)Constants.TransactionType.TokenPayment).ToString();
					}
					else if ( provider.PaymentType == (byte)Constants.TransactionType.CardPayment ) // Means no tokens, card payments only
					{
						btnProcess1.Text            = "N/A";
						btnProcess1.Enabled         = false;
						btnProcess1.CommandArgument = "0";
						btnProcess2.CommandArgument = ((byte)Constants.TransactionType.CardPayment).ToString();
					}
				}
		}

		protected void btnProcess1_Click(Object sender, EventArgs e)
		{
			byte transactionType = (byte)Tools.StringToInt(btnProcess1.CommandArgument);
			ProcessCards(transactionType);
		}

		protected void btnProcess2_Click(Object sender, EventArgs e)
		{
			byte transactionType = (byte)Tools.StringToInt(btnProcess2.CommandArgument);
			ProcessCards(transactionType);
		}

		private void ProcessCards(byte transactionType)
		{
			if ( transactionType > 0 && CheckData() == 0 )
				if ( rdoWeb.Checked )
					ProcessWeb(transactionType);
				else if ( rdoAsynch.Checked )
					ProcessAsynch(transactionType);
		}

		private void ProcessAsynch(byte transactionType)
		{
			ProcessStartInfo app  = new ProcessStartInfo();

			app.Arguments      =  "Mode=" + transactionType.ToString()
			                   + " Rows=" + maxRows.ToString()
			                   + " Provider=" + provider;
			app.WindowStyle    = ProcessWindowStyle.Hidden;
		//	app.WindowStyle    = ProcessWindowStyle.Normal;
		//	app.FileName       = "PCIUnattended.exe";
			app.CreateNoWindow = false;
			app.FileName       = Tools.SystemFolder("Bin") + "PCIUnattended.exe";
		//	app.FileName       = path + ( path.EndsWith("\\") ? "" : "\\" ) + "bin\\PCIUnattended.exe";

			try
			{
				Tools.LogInfo("RTR.ProcessAsynch/2",app.FileName + " " + app.Arguments,220);
				System.Diagnostics.Process.Start(app);
				Tools.LogInfo("RTR.ProcessAsynch/3","Launched",220);

//			// Run the external process & wait for it to finish
//				using (Process proc = System.Diagnostics.Process.Start(app))
//				{
//					Tools.LogInfo("RTR.ProcessAsynch/1","");
//					proc.WaitForExit();
//				// Retrieve the app's exit code
//					exitCode = proc.ExitCode;
//				}
//				Tools.LogInfo("RTR.ProcessAsynch/2","exitCode="+exitCode.ToString());
			}
			catch (Exception ex)
			{
				Tools.LogException("RTR.ProcessAsynch/9",app.FileName + " " + app.Arguments,ex);
			}
			app = null;
		}

		private void ProcessWeb(byte transactionType)
		{
			try
			{
				Tools.LogInfo("RTR.ProcessWeb/1","Started, provider '" + provider + "'",10);

				using (Payments payments = new Payments())
				{
					int k         = payments.ProcessCards(provider,transactionType,maxRows);
					lblError.Text = (payments.CountSucceeded+payments.CountFailed).ToString() + " payment(s) completed : " + payments.CountSucceeded.ToString() + " succeeded, " + payments.CountFailed.ToString() + " failed<br />&nbsp;";
				}
				Tools.LogInfo("RTR.ProcessWeb/2","Finished",10);
			}
			catch (Exception ex)
			{
				Tools.LogException("RTR.ProcessWeb/9","",ex);
			}
		}

		private byte CheckData()
		{
			try
			{
				provider  = lstProvider.SelectedValue.Trim();
				string rw = txtRows.Text.Trim().ToUpper();
				if ( rw == "ALL" || rw.Length == 0 )
					maxRows = 0;
				else
					maxRows = Tools.StringToInt(rw);
			}
			catch
			{
				maxRows = -8;
			}
			if ( string.IsNullOrWhiteSpace(provider) )
				return 78; // Error
			if ( maxRows < 0 )
				return 79; // Error
			return 0;
		}

		private void ShowFile(string fileName)
		{
			StreamReader fHandle = null;
			DateTime     fDate   = System.DateTime.Now;
			lblJS.Text           = "<script type='text/javascript'>ShowElt('divLogs',1)</script>";

			try
			{
				if ( rdo1.Checked )
					fDate = fDate.AddDays(-1);
				else if ( rdo2.Checked )
					fDate = fDate.AddDays(-2);
				else if ( rdoX.Checked && txtDate.Text.Trim().Length == 10 )
					fDate = Tools.StringToDate(txtDate.Text,1);
				else if ( ! rdo0.Checked )
					return;

				if ( fDate <= Constants.C_NULLDATE() )
					return;

				int k    = fileName.LastIndexOf(".");
				fileName = fileName.Substring(0,k) + "-" + PCIBusiness.Tools.DateToString(fDate,7) + fileName.Substring(k);
				fHandle  = File.OpenText(fileName);
				string h = fHandle.ReadToEnd().Trim().Replace("<","&lt;").Replace(">","&gt;");
				h        = h.Replace(Environment.NewLine,"</p><p>");
				if ( ! h.EndsWith("<p>") )
					h = h + "<p>";
				lblTest.Text = "<div class='Error'>Log File : " + fileName + "</div><p>" + h + "&nbsp;</p>";
				lblJS.Text   = "";
			}
			catch
			{
				lblError.Text = "File " + fileName + " could not be found";
			}
			finally
			{
				if ( fHandle != null )
					fHandle.Close();
			}
			fHandle = null;
		}


		protected void btnJSON_Click(Object sender, EventArgs e)
		{
			string value = Tools.JSONValue(txtTest.Text,"key");
			lblTest.Text = "JSON Tag = key<br />JSON Value = " + value;
		}

		protected void btnSQL_Click(Object sender, EventArgs e)
		{
			lblTest.Text = Tools.SQLDebug(txtTest.Text) + "<p>&nbsp;</p>";
		}

		protected void btnInfo_Click(Object sender, EventArgs e)
		{
			ShowFile(Tools.ConfigValue("LogFileInfo"));
		}
		protected void btnError_Click(Object sender, EventArgs e)
		{
			ShowFile(Tools.ConfigValue("LogFileErrors"));
		}

		protected void btnConfig_Click(Object sender, EventArgs e)
		{
			try
			{
				string folder  = "<u>System Configuration</u><br />"
				               + "- Version = " + SystemDetails.AppVersion + "<br />"
				               + "- Date = " + SystemDetails.AppDate + "<br />"
				               + "- Owner = " + SystemDetails.Owner + "<br />"
				               + "- Developer = " + SystemDetails.Developer + "<hr />"
				               + "<u>Environment</u><br />"
				               + "- Machine Name = " + Environment.MachineName + "<br />"
				               + "- Processors = " + Environment.ProcessorCount.ToString() + "<br />"
				               + "- Available Memory = " + Environment.WorkingSet.ToString() + " bytes<br />"
				               + "- Operating System = " + Environment.OSVersion.ToString() + "<br />"
				               + "- Microsoft .NET Runtime = " + Environment.Version.ToString() + "<br />"
				               + "- User Name = " + Environment.UserName + "<hr />"
				               + "<u>Internal</u><br />"
				               + "- Server.MapPath = " + Server.MapPath("") + "<br />"
				               + "- Request.Url.AbsoluteUri = " + Request.Url.AbsoluteUri + "<br />"
				               + "- Request.Url.AbsolutePath = " + Request.Url.AbsolutePath + "<br />"
				               + "- Request.Url.LocalPath = " + Request.Url.LocalPath + "<br />"
				               + "- Request.Url.PathAndQuery = " + Request.Url.PathAndQuery + "<br />"
				               + "- Request.RawUrl = " + Request.RawUrl + "<br />"
				               + "- Request.PhysicalApplicationPath = " + Request.PhysicalApplicationPath + "<hr />"
				               + "<u>ECentric</u><br />"
				               + "- Certificate File = " + Tools.SystemFolder("Certificates") + Tools.ConfigValue("ECentric/CertName") + "<br />"
				               + "- Certificate Password = " + Tools.ConfigValue("ECentric/CertPassword") + "<hr />"
				               + "<u>Settings</u><br />"
				               + "- System Mode = " + Tools.ConfigValue("SystemMode") + "<br />"
				               + "- Process Mode = " + Tools.ConfigValue("ProcessMode") + "<br />"
				               + "- Page timeout = " + Server.ScriptTimeout.ToString() + " seconds<br />"
				               + "- Rows to Process per Iteration = " + PCIBusiness.Tools.ConfigValue("MaximumRows") + "<br />"
				               + "- Error Logs folder/file = " + Tools.ConfigValue("LogFileErrors") + "<br />"
				               + "- Info Logs folder/file = " + Tools.ConfigValue("LogFileInfo") + "<br />"
				               + "- System path = " + Tools.ConfigValue("SystemPath") + "<br />";
				ConnectionStringSettings db = ConfigurationManager.ConnectionStrings["DBConn"];
				folder         = folder + "- DB Connection [DBConn] = " + ( db == null ? "" : db.ConnectionString ) + "<p>&nbsp;</p>";
				lblTest.Text   = folder;
			}
			catch (Exception ex)
			{
				Tools.LogException("RTR.btnConfig_Click","",ex);
			}
		}

		public RTR() : base()
		{
			timeOut              = Server.ScriptTimeout;
			Server.ScriptTimeout = 1800; // 30 minutes
		}

		~RTR()
		{
			Server.ScriptTimeout = timeOut;
		}
	}
}
