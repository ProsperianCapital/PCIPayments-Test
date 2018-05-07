using System;
using System.Text;
using System.Xml;
using System.Net;
using System.Net.Http;
using System.IO;

namespace PCIBusiness
{
	public class TransactionPayGate : Transaction
	{
		private string resultSuccessful;

		public  bool   Successful
		{
			get { return Tools.NullToString(resultSuccessful).ToUpper() == "TRUE"; }
		}

		private int SendXML(string url)
		{
			int    ret         = 10;
			string xmlReceived = "";
			payRef             = "";

			try
			{
				if ( Tools.NullToString(url).Length == 0 )
					url = "https://secure.paygate.co.za/payhost/process.trans?wsdl";

				Tools.LogInfo("TransactionPayGate.SendXML/10","URL=" + url + ", XML Sent=" + xmlSent,10);

			// Construct soap object
				ret                         = 20;
				XmlDocument soapEnvelopeXml = new XmlDocument();
				soapEnvelopeXml.LoadXml(xmlSent);

			// Construct web request object
				Tools.LogInfo("TransactionPayGate.SendXML/20","Create/set up web request, URL=" + url,10);
				ret                       = 30;
				HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
				webRequest.Headers.Add(@"SOAP:Action");
				webRequest.ContentType = "text/xml;charset=\"utf-8\"";
				webRequest.Accept      = "text/xml";
				webRequest.Method      = "POST";

			// Insert soap envelope into web request
				Tools.LogInfo("TransactionPayGate.SendXML/30","Save web request",10);
				ret = 50;
				using (Stream stream = webRequest.GetRequestStream())
					soapEnvelopeXml.Save(stream);

			// Get the completed web request XML
				Tools.LogInfo("TransactionPayGate.SendXML/40","Get web response",10);
				ret = 60;

				using (WebResponse webResponse = webRequest.GetResponse())
				{
					ret = 63;
					Tools.LogInfo("TransactionPayGate.SendXML/45","Read web response stream",10);
					using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
					{
						ret         = 66;
						xmlReceived = rd.ReadToEnd();
					}
				}

				Tools.LogInfo("TransactionPayGate.SendXML/50","XML Received=" + xmlReceived,10);

			// Create an empty soap result object
				ret       = 70;
				xmlResult = new XmlDocument();
				xmlResult.LoadXml(xmlReceived.ToString());

//			//	Get data from result XML
				ret              = 80;
				resultSuccessful = Tools.XMLNode(xmlResult,"successful");
				resultCode       = Tools.XMLNode(xmlResult,"resultCode");
				resultMsg        = Tools.XMLNode(xmlResult,"resultMessage");
//				payRef           = Tools.XMLNode(xmlResult,"payUReference");
//				payToken         = Tools.XMLNode(xmlResult,"pmId");

//				if ( Successful )
//					return 0;

				Tools.LogInfo("TransactionPayGate.SendXML/80","URL=" + url + ", XML Sent=" + xmlSent+", XML Received="+xmlReceived,220);
			}
			catch (Exception ex)
			{
				Tools.LogInfo("TransactionPayGate.SendXML/85","Ret="+ret.ToString()+", URL=" + url + ", XML Sent="+xmlSent,255);
				Tools.LogException("TransactionPayGate.SendXML/90","Ret="+ret.ToString()+", URL=" + url + ", XML Sent="+xmlSent,ex);
			}
			return ret;
		}

		public override int GetToken(Payment payment)
		{
			int ret = 300;
			xmlSent = "";

			Tools.LogInfo("TransactionPayGate.GetToken/10","RESERVE, Merchant Ref=" + payment.MerchantReference,10);

			try
			{
				xmlSent = SetUpXML(payment,1)
				        + "<pay:CardNumber>" + Tools.XMLSafe(payment.CardNumber) + "</pay:CardNumber>"
				        + "<pay:CardExpiryDate>" + Tools.XMLSafe(payment.CardExpiryMM) + Tools.XMLSafe(payment.CardExpiryYYYY) + "</pay:CardExpiryDate>"
				        + "<pay:CVV>" + Tools.XMLSafe(payment.CardCVV) + "</pay:CVV>"
				        + "<pay:Vault>true</pay:Vault>"
				        + "<pay:BudgetPeriod>0</pay:BudgetPeriod>"
				        + SetUpXML(payment,2);

			//	Version 1
				ret    = SendXML(payment.ProviderURL);

			//	Version 2
				CallWebService(payment.ProviderURL,xmlSent);

//				payRef = Tools.XMLNode(xmlResult,"payUReference");

				Tools.LogInfo("TransactionPayGate.GetToken/30","ResultCode="+ResultCode,30);
			}
			catch (Exception ex)
			{
				Tools.LogInfo("TransactionPayGate.GetToken/85","Ret="+ret.ToString()+", XML Sent="+xmlSent,255);
				Tools.LogException("TransactionPayGate.GetToken/90","Ret="+ret.ToString()+", XML Sent="+xmlSent,ex);
			}
			return ret;
		}


		public override int ProcessPayment(Payment payment)
		{
			int ret = 600;
			xmlSent = "";

			Tools.LogInfo("TransactionPayGate.ProcessPayment/10","PAYMENT, Merchant Ref=" + payment.MerchantReference,10);

			try
			{
				xmlSent = SetUpXML(payment,1)
				        + "<pay:CVV>" + Tools.XMLSafe(payment.CardCVV) + "</pay:CVV>"
				        + "<pay:VaultId>" + Tools.XMLSafe(payment.CardToken) + "</pay:VaultId>"
				        + "<pay:BudgetPeriod>0</pay:BudgetPeriod>"
				        + SetUpXML(payment,2);

				ret    = SendXML(payment.ProviderURL);
//				payRef = Tools.XMLNode(xmlResult,"payUReference");

				Tools.LogInfo("TransactionPayGate.ProcessPayment/30","ResultCode="+ResultCode,30);
			}
			catch (Exception ex)
			{
				Tools.LogInfo("TransactionPayGate.ProcessPayment/85","Ret="+ret.ToString()+", XML Sent="+xmlSent,255);
				Tools.LogException("TransactionPayGate.ProcessPayment/90","Ret="+ret.ToString()+", XML Sent="+xmlSent,ex);
			}
			return ret;
		}

		private string SetUpXML(Payment payment,byte section)
		{
			if ( section == 1 )
				return "<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'"
				     +                  " xmlns:pay='http://www.paygate.co.za/PayHOST'>"
				     + "<soapenv:Header />"
				     + "<soapenv:Body>"
				     + "<pay:SinglePaymentRequest>"
				     +   "<pay:CardPaymentRequest>"
				     +     "<pay:Account>"
				     +       "<pay:PayGateId>" + Tools.XMLSafe(payment.ProviderUserID) + "</pay:PayGateId>"
				     +       "<pay:Password>" + Tools.XMLSafe(payment.ProviderPassword) + "</pay:Password>"
				     +     "</pay:Account>"
				     +     "<pay:Customer>"
				     +       "<pay:FirstName>" + Tools.XMLSafe(payment.FirstName) + "</pay:FirstName>"
				     +       "<pay:LastName>" + Tools.XMLSafe(payment.LastName) + "</pay:LastName>"
				     +       "<pay:Mobile>" + Tools.XMLSafe(payment.PhoneCell) + "</pay:Mobile>"
				     +       "<pay:Email>" + Tools.XMLSafe(payment.EMail) + "</pay:Email>"
				     +     "</pay:Customer>";
			else
				return     "<pay:Order>"
				     +       "<pay:MerchantOrderId>" + Tools.XMLSafe(payment.MerchantReference) + "</pay:MerchantOrderId>"
				     +       "<pay:Currency>" + Tools.XMLSafe(payment.CurrencyCode) + "</pay:Currency>"
				     +       "<pay:Amount>" + payment.PaymentAmount.ToString() + "</pay:Amount>"
				     +     "</pay:Order>"
				     +   "</pay:CardPaymentRequest>"
				     + "</pay:SinglePaymentRequest>"
				     + "</soapenv:Body>"
				     + "</soapenv:Envelope>";


//				        +       "<pay:Telephone></pay:Telephone>"
//				        +     "<pay:Redirect>"
//				        +       "<pay:NotifyUrl></pay:NotifyUrl>"
//				        +       "<pay:ReturnUrl></pay:ReturnUrl>"
//				        +     "</pay:Redirect>"

		}

		private XmlDocument CallWebService(string url, string soap)
      {
			try
			{
				using ( WebClient wc = new WebClient() )
				{
				//	ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11
					XmlDocument retXMLDoc = new XmlDocument();
 
				//	wc.Headers.Add("Content-Type", "application/soap+xml; charset=utf-8")
					wc.Encoding = System.Text.Encoding.UTF8;
					string ret = wc.UploadString(url,soap);
				//	Dim _byteOrderMarkUtf8 As String = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble())
				//	If retString.StartsWith(_byteOrderMarkUtf8) Then
				//    retString = retString.Remove(0, _byteOrderMarkUtf8.Length)
				//	End If
					retXMLDoc.LoadXml(ret);
					return retXMLDoc;
				}
			}
			catch (Exception ex)
			{
//				if ( ex.tyTypeOf (ex) Is ThreadAbortException Then
//                'Do nothing - just continue
//            Else
//                Response.Redirect("~/Site/SiteError.aspx")
//            End If
			}
			return null;
		}

		public TransactionPayGate() : base()
		{
			bureauCode = Tools.BureauCode(Constants.PaymentProvider.PayGate);
		}
	}
}
