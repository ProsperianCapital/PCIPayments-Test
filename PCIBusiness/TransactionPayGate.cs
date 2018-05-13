using System;
using System.Text;
using System.Xml;
using System.Net;
using System.Net.Http;
using System.IO;
using PCIBusiness.PayGateVault;

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

		public int GetTokenViaTransaction(Payment payment) // This works but you need at least 1 cent to go through
		{
			int ret  = 300;
			xmlSent  = "";
			payToken = "";

			Tools.LogInfo("TransactionPayGate.GetToken/710","RESERVE, Merchant Ref=" + payment.MerchantReference,199);

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

				Tools.LogInfo("TransactionPayGate.GetToken/720","ResultCode="+ResultCode,199);
			}
			catch (Exception ex)
			{
				Tools.LogInfo("TransactionPayGate.GetToken/798","Ret="+ret.ToString()+", XML Sent="+xmlSent,255);
				Tools.LogException("TransactionPayGate.GetToken/799","Ret="+ret.ToString()+", XML Sent="+xmlSent,ex);
			}
			return ret;
		}


		public override int GetToken(Payment payment)
		{
			int ret   = 300;
			xmlSent   = "";
			payToken  = "";
			resultMsg = "";
			xmlResult = new XmlDocument();

			Tools.LogInfo("TransactionPayGate.GetToken/10","RESERVE, Merchant Ref=" + payment.MerchantReference,220);

			try
			{
			/*
				xmlSent = "<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'"
						  +                  " xmlns:putCard='urn:paygate.payvault'>"
						  + "<soapenv:Header />"
						  + "<soapenv:Body>"
						  +   "<putCard:vaultData>"
						  +     "<putCard:cardNo>" + Tools.XMLSafe(payment.CardNumber) + "</cardNo>"
						  +     "<putCard:expMonth>" + Tools.XMLSafe(payment.CardExpiryMM) + "</expMonth>"
						  +     "<putCard:expYear>" + Tools.XMLSafe(payment.CardExpiryYYYY) + "</expYear>"
						  +   "</putCard:vaultData>"
						  + "</soapenv:Body>"
						  + "</soapenv:Envelope>";

				xmlSent = "<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'>"
						  + "<soapenv:Header>"
						  +   "<login>" + Tools.XMLSafe(payment.ProviderUserID) + "</login>"
						  +   "<password>" + Tools.XMLSafe(payment.ProviderPassword) + "</password>"
						  + "</soapenv:Header>"
						  + "<soapenv:Body>"
						  +   "<putCard xmlns='urn:paygate.payvault'>"
						  +   "<vaultData>"
						  +     "<cardNo>" + Tools.XMLSafe(payment.CardNumber) + "</cardNo>"
						  +     "<expMonth>" + Tools.XMLSafe(payment.CardExpiryMM) + "</expMonth>"
						  +     "<expYear>" + Tools.XMLSafe(payment.CardExpiryYYYY) + "</expYear>"
						  +   "</vaultData>"
						  +   "</putCard>"
						  + "</soapenv:Body>"
						  + "</soapenv:Envelope>";

				xmlSent = "<Envelope xmlns='http://schemas.xmlsoap.org/soap/envelope/'>"
						  + "<Body>"
						  +   "<putCard xmlns='urn:paygate.payvault'>"
						  +   "<vaultData>"
						  +     "<cardNo>" + Tools.XMLSafe(payment.CardNumber) + "</cardNo>"
						  +     "<expMonth>" + Tools.XMLSafe(payment.CardExpiryMM) + "</expMonth>"
						  +     "<expYear>" + Tools.XMLSafe(payment.CardExpiryYYYY) + "</expYear>"
						  +   "</vaultData>"
						  +   "</putCard>"
						  + "</Body>"
						  + "</Envelope>";
			*/

				xmlSent = "<Envelope xmlns='http://schemas.xmlsoap.org/soap/envelope/'>"
						  + "<Body>"
						  +   "<putCard xmlns='urn:paygate.payvault'>"
						  +   "<vaultData>"
						  +     "<cardNo>" + Tools.XMLSafe(payment.CardNumber) + "</cardNo>"
						  +     "<expMonth>" + Tools.XMLSafe(payment.CardExpiryMM) + "</expMonth>"
						  +     "<expYear>" + Tools.XMLSafe(payment.CardExpiryYYYY) + "</expYear>"
						  +   "</vaultData>"
						  +   "</putCard>"
						  + "</Body>"
						  + "</Envelope>";

			//	Version 1
			//	ret = SendXML(payment.ProviderURL);

			//	Version 2
			//	ret = CallWebService(payment.ProviderURL);

			//	Version 3
				ret = 310;
				using (PayGateVault.PayVault vault = new PayVault())
				{
					ret                             = 320;
					PayGateVault.vaultData cardData = new vaultData();
					ICredentials           login    = new NetworkCredential(payment.ProviderUserID,payment.ProviderPassword);	

					ret               = 330;
					cardData.cardNo   = payment.CardNumber;
					ret               = 340;
					cardData.expMonth = payment.CardExpiryMM;
					ret               = 350;
					cardData.expYear  = payment.CardExpiryYYYY;
					ret               = 360;
					vault.Credentials = login;
					ret               = 370;
					payToken          = vault.putCard(cardData);
					ret               = 380;
					login             = null;
					cardData          = null;
					ret               = 999; // Means all OK
				}
				Tools.LogInfo("TransactionPayGate.GetToken/20","Ret="+ret.ToString()+", VaultId="+payToken,220);
			}
			catch (Exception ex)
			{
				resultMsg = ex.Message;
				Tools.LogInfo("TransactionPayGate.GetToken/98","Ret="+ret.ToString(),255);
				Tools.LogException("TransactionPayGate.GetToken/99","Ret="+ret.ToString(),ex);
			}

			if ( ret == 999 && payToken.Length > 0 ) // Success
			{
				ret        = 0;
				resultCode = "990017";
				xmlResult.LoadXml("<VaultId>" + Tools.XMLSafe(payToken) + "</VaultId>");
			}
			else
			{
				if ( resultMsg.Length == 0 )
					resultMsg = "Tokenization failed";
				xmlResult.LoadXml("<Error><Code>" + ret.ToString() + "</Code><Message>" + Tools.XMLSafe(resultMsg) + "</Message></Error>");
				resultCode = ret.ToString();
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
//	Testing ...
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

			if ( Tools.NullToString(url).Length == 0 )
				url = "https://secure.paygate.co.za/payhost/process.trans";

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

					if ( Successful )
						return 0;

					if ( resultCode.Length == 0 && resultMsg.Length == 0 )
					{
						ret        = 60;
						resultCode = Tools.XMLNode(xmlResult,"faultcode"); // Namespace not needed
						resultMsg  = Tools.XMLNode(xmlResult,"faultstring");
					//	Tools.LogInfo("TransactionPayGate.CallWebService/60","faultcode="+resultCode,199);
					}
				}
			}
			catch (Exception ex)
			{
				Tools.LogInfo("TransactionPayGate.CallWebService/98","ret="+ret.ToString(),200);
				Tools.LogException("TransactionPayGate.CallWebService/99","ret="+ret.ToString(),ex);
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
