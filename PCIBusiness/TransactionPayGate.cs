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
		private string nsPrefix;
		private string nsURL;

		public  bool Successful
		{
			get { return Tools.NullToString(resultCode) == "990017"; }
		}

		public override int GetToken(Payment payment)
		{
			int ret  = 300;
			xmlSent  = "";
			payToken = "";

			Tools.LogInfo("TransactionPayGate.GetToken/10","RESERVE, Merchant Ref=" + payment.MerchantReference,199);

			try
			{
				xmlSent = SetUpXML(payment,1)
				        + "<pay:CardNumber>" + Tools.XMLSafe(payment.CardNumber) + "</pay:CardNumber>"
				        + "<pay:CardExpiryDate>" + Tools.XMLSafe(payment.CardExpiryMM) + Tools.XMLSafe(payment.CardExpiryYYYY) + "</pay:CardExpiryDate>"
				        + "<pay:CVV>" + Tools.XMLSafe(payment.CardCVV) + "</pay:CVV>"
				        + "<pay:Vault>true</pay:Vault>"
				        + SetUpXML(payment,2);

			//	Version 1
			//	ret = SendXML(payment.ProviderURL);

			//	Version 2
				ret = CallWebService(payment.ProviderURL);

				if ( ret == 0 ) // Success
					payToken = Tools.XMLNode(xmlResult,"VaultId",nsPrefix,nsURL);

				Tools.LogInfo("TransactionPayGate.GetToken/20","ResultCode="+ResultCode,199);
			}
			catch (Exception ex)
			{
				Tools.LogInfo("TransactionPayGate.GetToken/98","Ret="+ret.ToString()+", XML Sent="+xmlSent,255);
				Tools.LogException("TransactionPayGate.GetToken/99","Ret="+ret.ToString()+", XML Sent="+xmlSent,ex);
			}
			return ret;
		}


		public override int ProcessPayment(Payment payment)
		{
			int ret = 600;
			xmlSent = "";

			Tools.LogInfo("TransactionPayGate.ProcessPayment/10","PAYMENT, Merchant Ref=" + payment.MerchantReference,199);

			try
			{
				xmlSent = SetUpXML(payment,1)
				        + "<pay:VaultId>" + Tools.XMLSafe(payment.CardToken) + "</pay:VaultId>"
				        + "<pay:CVV>" + Tools.XMLSafe(payment.CardCVV) + "</pay:CVV>"
				        + SetUpXML(payment,2);

			//	Version 1
			//	ret = SendXML(payment.ProviderURL);

			//	Version 2
				ret = CallWebService(payment.ProviderURL);

				Tools.LogInfo("TransactionPayGate.ProcessPayment/20","ResultCode="+ResultCode,199);
			}
			catch (Exception ex)
			{
				Tools.LogInfo("TransactionPayGate.ProcessPayment/98","Ret="+ret.ToString()+", XML Sent="+xmlSent,255);
				Tools.LogException("TransactionPayGate.ProcessPayment/99","Ret="+ret.ToString()+", XML Sent="+xmlSent,ex);
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
//	Testing ...
//				     +       "<pay:PayGateId>10011064270</pay:PayGateId>"
//				     +       "<pay:Password>test</pay:Password>"
				     +     "</pay:Account>"
				     +     "<pay:Customer>"
				     +       "<pay:FirstName>" + Tools.XMLSafe(payment.FirstName) + "</pay:FirstName>"
				     +       "<pay:LastName>" + Tools.XMLSafe(payment.LastName) + "</pay:LastName>"
				     +       "<pay:Mobile>" + Tools.XMLSafe(payment.PhoneCell) + "</pay:Mobile>"
				     +       "<pay:Email>" + Tools.XMLSafe(payment.EMail) + "</pay:Email>"
				     +     "</pay:Customer>";
			else
				return     "<pay:BudgetPeriod>0</pay:BudgetPeriod>"
				     +     "<pay:Order>"
				     +       "<pay:MerchantOrderId>" + Tools.XMLSafe(payment.MerchantReference) + "</pay:MerchantOrderId>"
				     +       "<pay:Currency>" + Tools.XMLSafe(payment.CurrencyCode) + "</pay:Currency>"
				     +       "<pay:Amount>" + payment.PaymentAmount.ToString() + "</pay:Amount>"
				     +     "</pay:Order>"
				     +   "</pay:CardPaymentRequest>"
				     + "</pay:SinglePaymentRequest>"
				     + "</soapenv:Body>"
				     + "</soapenv:Envelope>";
		}

		private int CallWebService(string url)
      {
			int ret = 10;

			try
			{
				using ( WebClient wc = new WebClient() )
				{
				//	XmlDocument retXMLDoc = new XmlDocument();
				//	wc.Headers.Add("Content-Type", "application/soap+xml; charset=utf-8")

					Tools.LogInfo("TransactionPayGate.CallWebService/10","XML In="+xmlSent,199);

					ret           = 20;
					wc.Encoding   = System.Text.Encoding.UTF8;
					string xmlOut = wc.UploadString(url,xmlSent);

				//	Dim _byteOrderMarkUtf8 As String = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble())
				//	If retString.StartsWith(_byteOrderMarkUtf8) Then
				//    retString = retString.Remove(0, _byteOrderMarkUtf8.Length)
				//	End If

					ret        = 30;
					xmlResult  = new XmlDocument();
					xmlResult.LoadXml(xmlOut);
					ret        = 40;
					resultCode = Tools.XMLNode(xmlResult,"ResultCode"       ,nsPrefix,nsURL);
					resultMsg  = Tools.XMLNode(xmlResult,"ResultDescription",nsPrefix,nsURL);
					payRef     = Tools.XMLNode(xmlResult,"PayRequestId"     ,nsPrefix,nsURL);
					ret        = 50;

					Tools.LogInfo("TransactionPayGate.CallWebService/50","XML Out="+xmlOut,199);

					if ( resultCode == "990017" ) // Successful
						return 0;

					if ( resultCode.Length == 0 && resultMsg.Length == 0 )
					{
						ret        = 60;
					//	resultCode = Tools.XMLNode(xmlResult,"faultcode"); // Namespace not needed
						resultMsg  = Tools.XMLNode(xmlResult,"faultstring");
						Tools.LogInfo("TransactionPayGate.CallWebService/60","faultcode="+resultCode,199);
					}
				}
			}
			catch (Exception ex)
			{
				Tools.LogInfo("TransactionPayGate.CallWebService/98","ret="+ret.ToString(),200);
				Tools.LogException("TransactionPayGate.CallWebService/99","(ret="+ret.ToString()+"), xmlSent="+xmlSent,ex);
			}
			return ret;
		}

		public TransactionPayGate() : base()
		{
			bureauCode = Tools.BureauCode(Constants.PaymentProvider.PayGate);

		//	Force TLS 1.2
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol  = SecurityProtocolType.Tls12;

		//	Namespace for result XML
			nsPrefix = "ns2";
			nsURL    = "http://www.paygate.co.za/PayHOST";
		}
	}
}
