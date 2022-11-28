using System;
using System.IO;
using System.Configuration;
using System.Diagnostics;
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
		private string userCode;

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

			lblSStatus.Text = ( systemStatus == 0 ? "Active" : "Disabled" );
			lblTest.Text    = "";
			lblError.Text   = "";
			lblError2.Text  = "";
			lblJS.Text      = "";
			EnableButtons(systemStatus == 0);

			if ( Page.IsPostBack )
				try
				{
					userCode = Tools.ObjectToString(ViewState["UserCode"]);
					if ( lblBureauCode.Text.ToUpper() != lstProvider.SelectedValue.ToUpper() )
						ProviderDetails();
				}
				catch
				{
					userCode = "";
				}

			else
			{
				lblVersion.Text = "Versions " + SystemDetails.AppVersion + " (app), " + PCIBusiness.SystemDetails.AppVersion + " (DLL)";
				lblVerApp.Text  = SystemDetails.AppVersion + " (" + SystemDetails.AppDate + ")";
				lblVerDLL.Text  = PCIBusiness.SystemDetails.AppVersion + " (" + PCIBusiness.SystemDetails.AppDate + ")";
				userCode        = Tools.ObjectToString(Request["UserCode"]);
				string ref1     = Tools.ObjectToString(Request.UrlReferrer);
				string ref2     = Tools.ObjectToString(Request.Headers["Referer"]); // Yes, this is spelt CORRECTLY! Do not change

//	Dev mode
				if ( Tools.ConfigValue("Access/BackDoor")  == ((int)Constants.SystemPassword.BackDoor).ToString() ||
				   Tools.NullToString(Request["BackDoor"]) == ((int)Constants.SystemPassword.BackDoor).ToString() )
					userCode = ( userCode.Length == 0 ? "013" : userCode );
				else
				{
//	User Check
					if ( Tools.ConfigValue("Access/UserCode").Length > 0 &&
						! (","+Tools.ConfigValue("Access/UserCode")+",").Contains(","+userCode+",") )
					{
						SetAccess(false,"Unauthorized access (invalid user code)");
						return;
					}

//	Referring URL check
					if ( Tools.ConfigValue("Access/ReferURL").Length > 0 )
					{
						string[] referAllow = Tools.ConfigValue("Access/ReferURL").ToUpper().Split(',');
						bool     ok         = false;

						foreach (string refer in referAllow)
							if ( string.IsNullOrWhiteSpace(refer) )
								continue;
							else if ( ( ref1.Length > 0 && ref1.ToUpper().Contains(refer.Trim()) ) ||
							          ( ref2.Length > 0 && ref2.ToUpper().Contains(refer.Trim()) ) )
							{
								ok = true;
								break;
							}
						if ( ! ok )
						{
							SetAccess(false,"Unauthorized access (invalid referring URL)");
							return;
						}
					}
				}

				lblSURL.Text      = ( ref1.Length > 4 ? ref1 : ref2 );
				lblSUserCode.Text = userCode;

				foreach (int bureauCode in Enum.GetValues(typeof(Constants.PaymentProvider)))
					lstProvider.Items.Add(new ListItem(Enum.GetName(typeof(Constants.PaymentProvider),bureauCode),bureauCode.ToString().PadLeft(3,'0')));

				DBConn                   conn       = null;
				ConnectionStringSettings db         = ConfigurationManager.ConnectionStrings["DBConn"];
				string[]                 connString = Tools.NullToString(db.ConnectionString).Split(';');
				int                      k;

				lblSQLServer.Text     = "";
				lblSQLDB.Text         = "";
				lblSQLUser.Text       = "";
				lblSQLStatus.Text     = "";
				lblSMode.Text         = Tools.ConfigValue("SystemMode");
				ViewState["UserCode"] = userCode;

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
				if ( lblSQLUser.Text.Length > 3 )
					lblSQLUser.Text = lblSQLUser.Text.Substring(0,1) + "******" + lblSQLUser.Text.Substring(lblSQLUser.Text.Length-1);
				else
					lblSQLUser.Text = "******";
				if ( Tools.OpenDB(ref conn) )
					lblSQLStatus.Text = "Connected";
				else
					lblSQLStatus.Text = "<span class='Red'>Cannot connect</span>";
				Tools.CloseDB(ref conn);
				conn = null;

				lstCCYear.Items.Clear();
				lstCCYear.Items.Add(new ListItem("(Select one)","0"));
				for ( int y = System.DateTime.Now.Year ; y < System.DateTime.Now.Year+15 ; y++ )
					lstCCYear.Items.Add(new ListItem(y.ToString(),y.ToString()));
			}
		}

		private void SetAccess(bool allow,string errMsg="")
		{
			lblSStatus.Text    = "Blocked";
			pnlButtons.Visible = allow;
			rdoWeb.Enabled     = allow;
			rdoAsynch.Enabled  = allow;
			rdoCard.Enabled    = allow;
			if ( allow )
				lblError.Text   = "";
			else
			{
				lblError.Text   = errMsg;
				if ( lblSUserCode.Text.Length > 0 )
					errMsg       = errMsg + ", User " + lblSUserCode.Text;
				if ( lblSURL.Text.Length > 0 )
					errMsg       = errMsg + ", URL "  + lblSURL.Text;
				Tools.LogInfo("RTR.SetAccess",errMsg,222);
			//	Tools.LogInfo("RTR.SetAccess",errMsg + ", " + lblSUserCode.Text + ", " + lblSURL.Text,222);
			}
		}

		private void EnableButtons(bool enable)
		{
			btnProcess1.Enabled  = enable;
			btnProcess2.Enabled  = enable;
			btnProcess3.Enabled  = enable;
			btnProcess4.Enabled  = enable;
			btnProcess5.Enabled  = enable;
			btnProcess6.Enabled  = enable;
			btnProcess7.Enabled  = enable;
		//	btnProcess8.Enabled  = enable;
			btnProcess9.Enabled  = false; // not implemented yet
			btnProcess10.Enabled = enable;
			btnProcess11.Enabled = enable;
		}
	
		private void ProviderDetails()
		{
			string bureauCode = lstProvider.SelectedValue.Trim();

			using ( Provider provider = new Provider() )
			{
				provider.BureauCode  = bureauCode;
				lblBureauCode.Text   = provider.BureauCode;
				lblBureauName.Text   = lstProvider.SelectedItem.Text;
				lblBureauStatus.Text = provider.BureauStatusName;
				rdoCard.Text         = "Single card payment";
				rdoCard.Enabled      = provider.ThreeDEnabled;
				btnPay.Visible       = provider.ThreeDEnabled;
				if ( ! provider.ThreeDEnabled )
					rdoCard.Checked   = false;

//				if ( provider.ThreeDEnabled )
//					rdoCard.Text      = "Single card payment";
//				else
//				{
//					rdoCard.Text      = "Single card payment (disabled)";
//					rdoCard.Checked   = false;
//					btnPay.Visible    = false;
//				}

				if ( provider.BureauStatusCode == 2 ) // Disabled
				{
				//	provider             = null;
				//	lblBureauURL.Text    = "";
				//	lblMerchantKey.Text  = "";
				//	lblMerchantUser.Text = "";
				//	lblCards.Text        = "";
				//	lblPayments.Text     = "";
					EnableButtons(false);
					return;
				}
				EnableButtons(true);

				if ( bureauCode.Length > 0 )
				{
				//	Ver 1
				//	using (Payments payments = new Payments())
				//	{
				//	//	payments.Summary(provider,bureauCode);
				//	//	lblBureauURL.Text    = provider.BureauURL;
				//	//	lblMerchantKey.Text  = provider.MerchantKey;
				//	//	lblMerchantUser.Text = provider.MerchantUserID;
				//	//	lblCards.Text        = provider.CardsToBeTokenized.ToString()    + ( provider.CardsToBeTokenized    >= Constants.MaxRowsPayment ? "+" : "" );
				//	//	lblPayments.Text     = provider.PaymentsToBeProcessed.ToString() + ( provider.PaymentsToBeProcessed >= Constants.MaxRowsPayment ? "+" : "" );
				//	}

					if ( provider.PaymentType == (byte)Constants.TransactionType.TokenPayment )
					{
//						btnProcess1.Text    = "Get Tokens";
						btnProcess1.Visible = true;
//						btnProcess2.Text    = "Token Payments";
						btnProcess2.Visible = true;
//						btnProcess3.Text    = "Delete Tokens";
						btnProcess3.Visible = true;
					}
					else if ( provider.PaymentType == (byte)Constants.TransactionType.CardPayment ) // Means no tokens, card payments only
					{
//						btnProcess1.Text    = "N/A";
						btnProcess1.Visible = false;
//						btnProcess2.Text    = "N/A";
						btnProcess2.Visible = false;
//						btnProcess3.Text    = "N/A";
						btnProcess3.Visible = false;
					}
				}
			}
		}

		protected void btnTest_Click(Object sender, EventArgs e)
		{
			txtRows.Text = "1";
			ProcessCards((byte)PCIBusiness.Constants.TransactionType.Test);
		}

		protected void btnPay_Click(Object sender, EventArgs e)
		{
			ProcessCards((byte)PCIBusiness.Constants.TransactionType.ManualPayment);
		}

		protected void btnProcess1_Click(Object sender, EventArgs e)
		{
			ProcessCards((byte)Constants.TransactionType.GetToken);
		}

		protected void btnProcess2_Click(Object sender, EventArgs e)
		{
			ProcessCards((byte)Constants.TransactionType.TokenPayment);
		}

		protected void btnProcess3_Click(Object sender, EventArgs e)
		{
			ProcessCards((byte)Constants.TransactionType.DeleteToken);
		}

		protected void btnProcess4_Click(Object sender, EventArgs e)
		{
			ProcessCards((byte)Constants.TransactionType.CardPayment);
		}

		protected void btnProcess5_Click(Object sender, EventArgs e)
		{
			ProcessCards((byte)Constants.TransactionType.CardPaymentThirdParty);
		}

		protected void btnProcess6_Click(Object sender, EventArgs e)
		{
			ProcessCards((byte)Constants.TransactionType.GetTokenThirdParty);
		}

		protected void btnProcess7_Click(Object sender, EventArgs e)
		{
			ProcessCards((byte)Constants.TransactionType.GetCardFromToken);
		}

		protected void btnProcess8_Click(Object sender, EventArgs e)
		{
			ProcessCards((byte)Constants.TransactionType.TransactionLookup);
		}

		protected void btnProcess9_Click(Object sender, EventArgs e)
		{
			ProcessCards((byte)Constants.TransactionType.Transfer);
		}

		protected void btnProcess10_Click(Object sender, EventArgs e)
		{
			ProcessCards((byte)Constants.TransactionType.Reversal);
		}

		protected void btnProcess11_Click(Object sender, EventArgs e)
		{
			ProcessCards((byte)Constants.TransactionType.Refund);
		}

		protected void btnProcess12_Click(Object sender, EventArgs e)
		{
			ProcessCards((byte)Constants.TransactionType.ZeroValueCheck);
		}

		private void ProcessCards(byte transactionType)
		{
			if ( transactionType > 0 && CheckData() == 0 )
				if ( transactionType == (byte)Constants.TransactionType.Test )
					ProcessTest();
				else if ( rdoWeb.Checked )
					ProcessWeb(transactionType);
				else if ( rdoAsynch.Checked )
					ProcessAsynch(transactionType);
				else if ( rdoCard.Checked )
					ProcessPayment();
		}

		private void ProcessAsynch(byte transactionType)
		{
			ProcessStartInfo app = new ProcessStartInfo();

			app.Arguments      =  "TransactionType=" + transactionType.ToString()
			                   + " Rows=" + maxRows.ToString()
			                   + " Provider=" + provider
			                   + ( userCode.Length > 0 ? " UserCode=" + userCode : "" );
			app.WindowStyle    = ProcessWindowStyle.Hidden;
		//	app.WindowStyle    = ProcessWindowStyle.Normal;
		//	app.FileName       = "PCIUnattended.exe";
			app.CreateNoWindow = false;
			app.FileName       = Tools.SystemFolder("Bin") + "PCIUnattended.exe";

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

		private void ProcessTest()
		{
			int ret = 0;

			try
			{
				Tools.LogInfo("RTR.ProcessTest/1","Started, provider '" + provider + "'",10);

				using (Payment payment = new Payment(provider))
				{
					payment.TransactionType = (byte)Constants.TransactionType.Test;
					ret = payment.ProcessPayment();
					if ( ret == 0 && payment.WebForm.Length > 0 )
						try
						{
						//	Busy();
							ret = 987654;
						//	This always throws a "thread aborted" exception ... ignore it
							System.Web.HttpContext.Current.Response.Clear();
							System.Web.HttpContext.Current.Response.Write(payment.WebForm);
							System.Web.HttpContext.Current.Response.End();
						}
						catch
						{ }
				}
				Tools.LogInfo("RTR.ProcessTest/2", "Finished", 10);
			}
			catch (Exception ex)
			{
				if ( ret != 987654 )
					Tools.LogException("RTR.ProcessTest/9","",ex);
			}
		}

		private void ProcessPayment()
		{
			int ret = 0;

			try
			{
				Tools.LogInfo("RTR.ProcessPayment/1","Started, provider '" + provider + "'",10);

				using (Payment payment = new Payment(provider))
				{
					payment.CardNumber        = txtCCNumber.Text;
					payment.CardName          = txtFName.Text + " " + txtLName.Text;
					payment.CardCVV           = txtCCCVV.Text;
					payment.EMail             = txtEMail.Text;
					payment.CurrencyCode      = txtCurrency.Text;
					payment.MerchantReference = txtReference.Text;
					payment.CardExpiryMM      = lstCCMonth.SelectedValue;
					payment.CardExpiryYYYY    = lstCCYear.SelectedValue;
					payment.PaymentAmount     = Tools.StringToInt(txtAmount.Text);
					payment.TransactionType   = (byte)Constants.TransactionType.ManualPayment;
					payment.FirstName         = txtFName.Text;
					payment.LastName          = txtLName.Text;
//					int k = payment.CardName.IndexOf(" ");
//					if ( k > 0 )
//					{
//						payment.FirstName = payment.CardName.Substring(0,k).Trim();
//						payment.LastName  = payment.CardName.Substring(k).Trim();
//					}
//					else
//						payment.LastName  = payment.CardName;

					ret = payment.ProcessPayment();
					if ( ret == 0 && payment.WebForm.Length > 0 )
						try
						{
						//	Busy();
							ret = 987654;
						//	This always throws a "thread aborted" exception ... ignore it
							System.Web.HttpContext.Current.Response.Clear();
							System.Web.HttpContext.Current.Response.Write(payment.WebForm);
							System.Web.HttpContext.Current.Response.End();
						}
						catch
						{ }
					else
					{
						lblJS.Text = "<script type='text/javascript'>PaySingle(8);</script>";
						if ( payment.ReturnMessage.Length > 0 )
							lblError2.Text = payment.ReturnMessage;
						else
							lblError2.Text = "Transaction failed";
					}
				}
				Tools.LogInfo("RTR.ProcessPayment/2","Finished",10);
			}
			catch (System.Threading.ThreadAbortException)
			{
			//	Ignore
			}
			catch (Exception ex)
			{
				if ( ret != 987654 )
					Tools.LogException("RTR.ProcessPayment/9","",ex);
			}
		}

		private void ProcessWeb(byte transactionType)
		{
			try
			{
				string tranType = Tools.TransactionTypeName(transactionType);
//				Tools.LogInfo("ProcessWeb/1","Started, " + tranType + ", provider " + provider,10,this);

				using (Payments payments = new Payments())
				{
					int k         = payments.ProcessCards(provider,transactionType,maxRows,"",222);
					lblError.Text = (payments.CountSucceeded+payments.CountFailed).ToString() + " " + tranType + "(s) processed : " + payments.CountSucceeded.ToString() + " succeeded, " + payments.CountFailed.ToString() + " failed<br />&nbsp;";
				}
//				Tools.LogInfo("ProcessWeb/2","Finished",10,this);
			}
			catch (Exception ex)
			{
				Tools.LogException("ProcessWeb/9","",ex,this);
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

				if ( fDate <= Constants.DateNull )
					return;

				int k    = fileName.LastIndexOf(".");
				fileName = fileName.Substring(0,k) + "-" + PCIBusiness.Tools.DateToString(fDate,7) + fileName.Substring(k);
				fHandle  = File.OpenText(fileName);
				string h = fHandle.ReadToEnd().Trim().Replace("<","&lt;").Replace(">","&gt;");
				h        = h.Replace(Environment.NewLine+"[v","</p><p>[v");
				if ( ! h.EndsWith("<p>") )
					h     = h + "<p>";
				h        = h.Replace(Environment.NewLine,"<br />");
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
				               + "- App version = "            + SystemDetails.AppVersion + "<br />"
				               + "- App date = "               + SystemDetails.AppDate + "<br />"
				               + "- DLL version = "            + PCIBusiness.SystemDetails.AppVersion + "<br />"
				               + "- DLL date = "               + PCIBusiness.SystemDetails.AppDate + "<br />"
				               + "- Owner = "                  + PCIBusiness.SystemDetails.Owner + "<br />"
				               + "- Developer = "              + PCIBusiness.SystemDetails.Developer + "<hr />"
				               + "<u>Environment</u><br />"
				               + "- Machine Name = "           + Environment.MachineName + "<br />"
				               + "- Processors = "             + Environment.ProcessorCount.ToString() + "<br />"
				               + "- Available Memory = "       + Environment.WorkingSet.ToString() + " bytes<br />"
				               + "- Operating System = "       + Environment.OSVersion.ToString() + "<br />"
				               + "- Microsoft .NET Runtime = " + Environment.Version.ToString() + "<br />"
				               + "- User Domain = "            + Environment.UserDomainName + "<br />"
				               + "- User Name = "              + Environment.UserName + "<hr />"
				               + "<u>Internal</u><br />"
				               + "- Server.MachineName = "              + Server.MachineName + "<br />"
				               + "- Server.MapPath = "                  + Server.MapPath("") + "<br />"
				               + "- Request.Url.AbsoluteUri = "         + Request.Url.AbsoluteUri + "<br />"
				               + "- Request.Url.AbsolutePath = "        + Request.Url.AbsolutePath + "<br />"
				               + "- Request.Url.LocalPath = "           + Request.Url.LocalPath + "<br />"
				               + "- Request.Url.PathAndQuery = "        + Request.Url.PathAndQuery + "<br />"
				               + "- Request.RawUrl = "                  + Request.RawUrl + "<br />"
				               + "- Request.PhysicalApplicationPath = " + Request.PhysicalApplicationPath + "<br />"
				               + "- Environment.SystemDirectory = "     + Environment.SystemDirectory + "<br />"
				               + "- Environment.CurrentDirectory = "    + Environment.CurrentDirectory + "<hr />"
				               + "<u>ECentric</u><br />"
				               + "- Certificate File = "                + Tools.SystemFolder("Certificates") + Tools.ConfigValue("ECentric/CertName") + "<br />"
				               + "- Certificate Password = "            + Tools.ConfigValue("ECentric/CertPassword") + "<hr />"
				               + "<u>Authorized Access</u><br />"
				               + "- By user code(s) = "                 + Tools.ConfigValue("Access/UserCode") + "<br />"
				               + "- Via referring URL(s) = "            + Tools.ConfigValue("Access/ReferURL") + "<br />"
				               + "- User code logged in = "             + userCode + "<hr />"
				               + "<u>Application Settings</u><br />"
				               + "- System Mode = "                     + Tools.ConfigValue("SystemMode") + "<br />"
				               + "- Process Mode = "                    + Tools.ConfigValue("ProcessMode") + "<br />"
				               + "- Page timeout = "                    + Server.ScriptTimeout.ToString() + " seconds<br />"
				               + "- Rows to Process per Iteration = "   + Tools.ConfigValue("MaximumRows") + "<br />"
				               + "- Error Logs folder/file = "          + Tools.ConfigValue("LogFileErrors") + "<br />"
				               + "- Info Logs folder/file = "           + Tools.ConfigValue("LogFileInfo") + "<br />"
				               + "- System path = "                     + Tools.ConfigValue("SystemPath") + "<br />"
				               + "- System URL = "                      + Tools.ConfigValue("SystemURL") + "<br />"
				               + "- Success page = "                    + Tools.ConfigValue("SystemURL") + "/Succeed.aspx<hr />"
				               + "<u>Database</u><br />"
				               + "- DB Connection [DBConn] = ";

				ConnectionStringSettings db   = ConfigurationManager.ConnectionStrings["DBConn"];
				DBConn                   conn = null;

				if ( db != null )
				{
					string connStr = db.ConnectionString.Trim();
					int    k       = connStr.ToUpper().IndexOf("PWD=");
					int    j       = connStr.ToUpper().IndexOf(";",k+1);
					if ( k >= 0 )
						connStr     = connStr.Substring(0,k+4) + "******" + ( j > k ? connStr.Substring(j) : "" );
					k              = connStr.ToUpper().IndexOf("UID=");
					j              = connStr.ToUpper().IndexOf(";",k+1);
					if ( k >= 0 )
						connStr     = connStr.Substring(0,k+4) + "******" + ( j > k ? connStr.Substring(j) : "" );
					folder         = folder + connStr;
				}
				try
				{
					Tools.OpenDB(ref conn);
					if ( conn.Execute("select @@VERSION as SysVer,@@SERVERNAME as SrvName,getdate() as SrvDate") )
						folder = folder + "<br />- Server Name = " + conn.ColString("SrvName")
						                + "<br />- Server Date = " + conn.ColDate  ("SrvDate").ToString()
						                + "<br />- SQL Version = " + conn.ColString("SysVer");
				}
				finally
				{
					Tools.CloseDB(ref conn);
				}
				db           = null;
				conn         = null;
				lblTest.Text = folder + "<p>&nbsp;</p>";
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
