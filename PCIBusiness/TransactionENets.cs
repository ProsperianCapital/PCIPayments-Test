using System;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography;

namespace PCIBusiness
{
	public class TransactionENets : Transaction
	{
		public  bool Successful
		{
			get
			{
				string h = Tools.JSONValue(strResult,"netsTxnStatus");
				return ( h == "0" || h == "00" || h == "000" );
			}
		}

//		Not used by eNETS
//		public override int GetToken(Payment payment)
//		{
//			return 0;
//		}

		public override int ProcessPayment(Payment payment)
		{
			int ret = 10;
			payRef  = "";

			Tools.LogInfo("TransactionENets.ProcessPayment/10","Merchant Ref=" + payment.MerchantReference,10);

			try
			{
				xmlSent = "{ \"ss\"  : \"1\","
				        +  " \"msg\" : { " + Tools.JSONPair("txnAmount"      ,payment.PaymentAmount.ToString(),1)
				        +                    Tools.JSONPair("merchantTxnRef" ,payment.MerchantReference,1)
				        +                    Tools.JSONPair("netsMid"        ,payment.ProviderAccount,1)
				        +                    Tools.JSONPair("merchantTxnDtm" ,Tools.DateToString(DateTime.Now,5,5),1)
				        +                    Tools.JSONPair("cardHolderName" ,payment.CardName,1)
				        +                    Tools.JSONPair("cvv"            ,payment.CardCVV,1)
				        +                    Tools.JSONPair("expiryDate"     ,payment.CardExpiryYY+payment.CardExpiryMM,1)
				        +                    Tools.JSONPair("pan"            ,payment.CardNumber,1)
				        +                    Tools.JSONPair("currencyCode"   ,payment.CurrencyCode,1)
				        +                    Tools.JSONPair("submissionMode" ,"S",1)
				        +                    Tools.JSONPair("paymentType"    ,"SALE",1)
				        +                    Tools.JSONPair("paymentMode"    ,"CC",1,"","}")
				        + "}";
				ret     = 20;
				ret     = CallWebService(payment);
				ret     = 30;
				payRef  = Tools.JSONValue(XMLResult,"netsTxnRef");
				ret     = 40;
				if ( Successful && payRef.Length > 0 )
					ret  = 0;
			}
			catch (Exception ex)
			{
				Tools.LogInfo("TransactionENets.ProcessPayment/98","Ret="+ret.ToString()+", JSON Sent="+xmlSent,255);
				Tools.LogException("TransactionENets.ProcessPayment/99","Ret="+ret.ToString()+", JSON Sent="+xmlSent,ex);
			}
			return ret;
		}

		private int CallWebService(Payment payment)
      {
			int    ret = 10;
			string url = payment.ProviderURL;

			if ( Tools.NullToString(url).Length == 0 )
				if ( Tools.LiveTestOrDev() != Constants.SystemMode.Live )
					url = "https://uat-api.nets.com.sg:9065/GW2/TxnReqListener";

			ret        = 30;
			strResult  = "";
			resultCode = "99999";
			resultMsg  = "Internal error connecting to " + url;
			ret        = 50;

			try
			{
				string         sig;
				byte[]         page         = Encoding.UTF8.GetBytes(xmlSent);
				HttpWebRequest webRequest   = (HttpWebRequest)WebRequest.Create(url);
				ret                         = 60;
				webRequest.ContentType      = "application/json";
				webRequest.Accept           = "application/json";
				webRequest.Method           = "POST";
				ret                         = 70;
				webRequest.Headers["keyId"] = payment.ProviderKey;
				ret                         = 80;
				sig                         = GetSignature(xmlSent,payment.ProviderPassword);
				webRequest.Headers["hmac"]  = sig;
				ret                         = 100;

				Tools.LogInfo("TransactionENets.CallWebService/10",
				              "URL=" + url +
				            ", MID=" + payment.ProviderAccount +
				            ", KeyId=" + payment.ProviderKey +
				            ", SecretKey=" + payment.ProviderPassword +
				            ", Signature=" + sig +
				            ", JSON Sent=" + xmlSent, 199);

				using (Stream stream = webRequest.GetRequestStream())
				{
					ret = 110;
					stream.Write(page, 0, page.Length);
					stream.Flush();
					stream.Close();
				}

				ret = 120;
				using (WebResponse webResponse = webRequest.GetResponse())
				{
					ret = 130;
					using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
					{
						ret        = 140;
						strResult  = rd.ReadToEnd();
					}
					if ( strResult.Trim().Length == 0 )
					{
						ret        = 150;
						resultMsg  = "No data returned from " + url;
						Tools.LogInfo("TransactionENets.CallWebService/20","JSON Received=(blank)",199);
					}
					else
					{
						Tools.LogInfo("TransactionENets.CallWebService/30","JSON Received=" + strResult,199);

						ret        = 160;
						resultMsg  = Tools.JSONValue(strResult,"netsTxnMsg");
						resultCode = Tools.JSONValue(strResult,"stageRespCode");

						if ( resultCode.Length > 0 )
							try
							{
								ret        = 170;
								string rex = resultCode.Trim().ToUpper();
								int    k   = rex.IndexOf("-");
								if ( k >= 0 && k < rex.Length-1 )
									rex  = rex.Substring(k+1);
								else if ( k >= 0 )
									rex  = rex.Substring(0,k);
								ret        = 180;
								resultCode = rex;
							}
							catch
							{ }

						ret = 190;
						if ( ! Successful || resultMsg.Length > 0 )
							resultMsg = resultMsg + " (netsTxnStatus=" + Tools.JSONValue(strResult,"netsTxnStatus") + ")";

//						resultCode = Tools.JSONValue(strResult,"netsTxnStatus");
//
//						if (Successful)
//							resultCode = "00";
//						else
//						{
//							if ( Tools.StringToInt(resultCode) == 0 )
//								resultCode = "99";
//							string x = Tools.JSONValue(strResult,"stageRespCode");
//							if ( x.Trim().Length > 0 )
//								resultMsg = "(" + x + ") " + resultMsg;
//						}

					}
				}
				ret = 0;
			}
			catch (Exception ex)
			{
				Tools.LogInfo("TransactionENets.CallWebService/298","ret="+ret.ToString(),220);
				Tools.LogException("TransactionENets.CallWebService/299","ret="+ret.ToString(),ex);
			}
			return ret;
		}

		private string GetSignature(string txnReq,string secretKey)
		{
			using (SHA256 sha256Hash = SHA256.Create())
			{
				byte[] hash = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(txnReq+secretKey));
				return System.Convert.ToBase64String(hash);
			}
		}

		public TransactionENets() : base()
		{
			bureauCode = Tools.BureauCode(Constants.PaymentProvider.PayGenius);

		//	Force TLS 1.2
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol  = SecurityProtocolType.Tls12;
		}
	}
}
