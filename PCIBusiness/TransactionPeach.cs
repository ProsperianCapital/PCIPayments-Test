using System;
using System.Text;
using System.Net;
using System.IO;

namespace PCIBusiness
{
	public class TransactionPeach : Transaction
	{
		public  bool   Successful
		{
			get
			{
			//	Always in 999.999.999 format
				resultCode = Tools.NullToString(resultCode).ToUpper();
				if ( ! resultCode.StartsWith("000") )
					return false;
				if ( ! resultCode.StartsWith("000.400") )
					return true;
				if ( resultCode.CompareTo("000.400.101") < 0 ) // 000.400.000 - 000.400.100 is OK, 101+ is an error
					return true;
				return false;
			}
		}

		private int PostHTML(byte transactionType,Payment payment)
		{
			byte   err = 0;
			int    ret = 10;
//			string url = "https://test.oppwa.com/v1/registrations";
			string url = "https://test.oppwa.com/v1/payments";
			strResult  = "";
			payRef     = "";
			resultCode = "999.999.888";
			resultMsg  = "Internal error";

			try
			{
				if ( payment.ProviderURL.Length > 0 )
					url = payment.ProviderURL;
				if ( transactionType == (byte)Constants.TransactionType.TokenPayment )
					url = url + "/" + payment.CardToken + "/payments";

				Tools.LogInfo("TransactionPeach.PostHTML/10","URL=" + url + ", URL data=" + xmlSent,221);

				ret                              = 20;
				byte[]         buffer            = Encoding.UTF8.GetBytes(xmlSent);
				HttpWebRequest request           = (HttpWebRequest)HttpWebRequest.Create(url);
				ret                              = 30;
				request.Method                   = "POST";
				request.Headers["Authorization"] = "Bearer " + payment.ProviderKey;
				request.ContentType              = "application/x-www-form-urlencoded";
				ret                              = 40;
				Stream postData                  = request.GetRequestStream();
				ret                              = 50;
				postData.Write(buffer, 0, buffer.Length);
				postData.Close();

				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					ret                     = 60;
					Stream       dataStream = response.GetResponseStream();
					ret                     = 70;
					StreamReader reader     = new StreamReader(dataStream);
					ret                     = 80;
					strResult               = reader.ReadToEnd();
					ret                     = 90;
//					var s       = new JavaScriptSerializer();
//					xmlReceived = s.Deserialize<Dictionary<string, dynamic>>(reader.ReadToEnd());
					reader.Close();
					dataStream.Close();
				}

//	Moved to below the "Catch"
//					ret        = 100;
//					resultCode = Tools.JSONValue(strResult,"code");
//					resultMsg  = Tools.JSONValue(strResult,"description");
//					ret        = 110;
//					if ( Successful )
//						ret     = 0;
//					else
//						Tools.LogInfo("TransactionPeach.PostHTML/120","resultCode="+resultCode+", resultMsg="+resultMsg,221);
//				}
			}
			catch (WebException ex1)
			{
				err = 1;
				strResult = Tools.DecodeWebException(ex1,"TransactionPeach.PostHTML/197",xmlSent);
			}
			catch (Exception ex2)
			{
				err = 2;
				if ( strResult == null )
					strResult = "";
				Tools.LogInfo     ("TransactionPeach.PostHTML/198","Ret="+ret.ToString()+", Result="+strResult,255);
				Tools.LogException("TransactionPeach.PostHTML/199","Ret="+ret.ToString()+", Result="+strResult,ex2);
			}

			ret        = 200;
			resultCode = Tools.JSONValue(strResult,"code");
			resultMsg  = Tools.JSONValue(strResult,"description");
			ret        = 210;
			if ( Successful && err == 0 )
				ret     = 0;
			else
				Tools.LogInfo("TransactionPeach.PostHTML/220","resultCode="+resultCode+", resultMsg="+resultMsg,221);

			return ret;
		}

		public override int GetToken(Payment payment)
		{
			int ret = 10;

			try
			{
				xmlSent = "entityId="          + Tools.URLString(payment.ProviderUserID)
				        + "&paymentBrand="     + Tools.URLString(payment.CardType.ToUpper())
				        + "&card.number="      + Tools.URLString(payment.CardNumber)
				        + "&card.holder="      + Tools.URLString(payment.CardName)
				        + "&card.expiryMonth=" + Tools.URLString(payment.CardExpiryMM)
				        + "&card.expiryYear="  + Tools.URLString(payment.CardExpiryYYYY)
				        + "&card.cvv="         + Tools.URLString(payment.CardCVV);
				if ( payment.CardType.ToUpper().StartsWith("DINE") ) // Diners Club
					xmlSent = xmlSent + "&shopperResultUrl=https://peachpayments.docs.oppwa.com/server-to-server";

				Tools.LogInfo("TransactionPeach.GetToken/10","Post="+xmlSent+", Key="+payment.ProviderKey,10);

				ret      = PostHTML((byte)Constants.TransactionType.GetToken,payment);
				payToken = Tools.JSONValue(strResult,"id");
				payRef   = Tools.JSONValue(strResult,"ndc");
				if ( payToken.Length < 1 && ret == 0 )
					ret = 247;

				Tools.LogInfo("TransactionPeach.GetToken/20","ResultCode="+ResultCode + ", payRef=" + payRef + ", payToken=" + payToken,221);
			}
			catch (Exception ex)
			{
				Tools.LogException("TransactionPeach.GetToken/90","Ret="+ret.ToString()+", XML Sent=" + xmlSent,ex);
			}
			return ret;
		}

		private void SetUpPaymentXML(Payment payment,byte transactionType)
		{
			if ( transactionType == (byte)Constants.TransactionType.CardPaymentThirdParty )
				xmlSent = "{{{" + Tools.URLString(payment.CardToken) + "}}}";
			else
				xmlSent = Tools.URLString(payment.CardNumber);

			xmlSent = "entityId="               + Tools.URLString(payment.ProviderUserID)
			        + "&paymentBrand="          + Tools.URLString(payment.CardType.ToUpper())
			        + "&card.number="           + xmlSent
			        + "&card.holder="           + Tools.URLString(payment.CardName)
			        + "&card.expiryMonth="      + Tools.URLString(payment.CardExpiryMM)
			        + "&card.expiryYear="       + Tools.URLString(payment.CardExpiryYYYY)
			        + "&card.cvv="              + Tools.URLString(payment.CardCVV)
			        + "&amount="                + Tools.URLString(payment.PaymentAmountDecimal)
			        + "&currency="              + Tools.URLString(payment.CurrencyCode)
			        + "&merchantTransactionId=" + Tools.URLString(payment.MerchantReference)
			        + "&descriptor="            + Tools.URLString(payment.PaymentDescription)
			        + "&merchantInvoiceId="     + Tools.URLString(payment.MerchantReference)
			        + "&shopperResultUrl="      + Tools.ConfigValue("SystemURL")+"/Succeed.aspx?TransRef="+Tools.XMLSafe(payment.MerchantReference)
			        + "&recurringType=REPEATED"
			        + "&paymentType=DB"; // DB = Instant debit, PA = Pre-authorize, CP =
		}

		public override int CardPaymentThirdParty(Payment payment)
		{
			byte   err  = 0;
			int    ret  = 10;
			string pURL = "";
			string tURL = "";
			strResult   = "";
			payRef      = "";
			resultCode  = "999.999.777";
			resultMsg   = "Internal error";

			try
			{
				if ( payment.ProviderURL.Length > 0 )  // The PAYMENT provider (Peach)
					pURL = payment.ProviderURL;
				else
					pURL = "https://test.oppwa.com/v1/registrations";

				ret = 20;

				if ( payment.TokenizerURL.Length > 0 ) // The TOKENIZER/THIRD PARTY (TokenEx)
					tURL = payment.TokenizerURL;
				else if ( bureauCodeTokenizer == Tools.BureauCode(Constants.PaymentProvider.TokenEx) )
					tURL = "https://test-api.tokenex.com/TransparentGatewayAPI/Detokenize";
				else
				{
					Tools.LogInfo("TransactionPeach.CardPaymentThirdParty/20","Unknown Third Party Tokenizer (" + bureauCodeTokenizer + "), data=" + xmlSent,221);
					return ret;
				}

				ret = 30;
				SetUpPaymentXML(payment,(byte)Constants.TransactionType.CardPaymentThirdParty);

				Tools.LogInfo("TransactionPeach.CardPaymentThirdParty/30","pURL=" + pURL + ", tURL=" + tURL + ", data=" + xmlSent,221);

				ret                              = 40;
				byte[]         buffer            = Encoding.UTF8.GetBytes(xmlSent);
				HttpWebRequest request           = (HttpWebRequest)HttpWebRequest.Create(tURL);
				ret                              = 50;
				request.Method                   = "POST";
				request.Headers["Authorization"] = "Bearer " + payment.ProviderKey;
				request.Headers["TX_URL"]        = pURL;
				request.Headers["TX_TokenExID"]  = payment.TokenizerID;  // "4311038889209736";
				request.Headers["TX_APIKey"]     = payment.TokenizerKey; // "54md8h1OmLe9oJwYdp182pCxKF0MUnWzikTZSnOi";
				request.ContentType              = "application/x-www-form-urlencoded";
				ret                              = 70;
				Stream postData                  = request.GetRequestStream();
				ret                              = 80;
				postData.Write(buffer, 0, buffer.Length);
				postData.Close();

				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					ret                     = 100;
					Stream       dataStream = response.GetResponseStream();
					ret                     = 110;
					StreamReader reader     = new StreamReader(dataStream);
					ret                     = 120;
					strResult               = reader.ReadToEnd();
					ret                     = 130;
//					var s       = new JavaScriptSerializer();
//					xmlReceived = s.Deserialize<Dictionary<string, dynamic>>(reader.ReadToEnd());
					reader.Close();
					dataStream.Close();
				}

//				ret        = 160;
//				resultCode = Tools.JSONValue(strResult,"code");
//				resultMsg  = Tools.JSONValue(strResult,"description");
//				payRef     = Tools.JSONValue(strResult,"id");
//				ret        = 170;
//				if ( Successful )
//				{
//					ret = 0;
//					Tools.LogInfo("TransactionPeach.CardPaymentThirdParty/170","(Succeed) Result="+strResult,221);
//				}
//				else
//					Tools.LogInfo("TransactionPeach.CardPaymentThirdParty/180","(Fail) Result="+strResult,221);
			}

			catch (WebException ex1)
			{
				err       = 1;
				strResult = Tools.DecodeWebException(ex1,"TransactionPeach.CardPaymentThirdParty/997",xmlSent);

//	Catch the third party errors here
//				if ( resultCode.Length < 1 && bureauCodeTokenizer == Tools.BureauCode(Constants.PaymentProvider.TokenEx) )
//					try
//					{
//						string tx = Tools.NullToString(ex1.Response.Headers["tx_code"]);
//						if ( tx.Length > 0 )
//							resultCode = tx;
//						tx = Tools.NullToString(ex1.Response.Headers["tx_message"]);
//						if ( tx.Length > 0 )
//							resultMsg = tx;
//					}
//					catch
//					{ }
			}

			catch (Exception ex2)
			{
				err = 2;
				if ( strResult == null )
					strResult = "";
				Tools.LogInfo     ("TransactionPeach.CardPaymentThirdParty/998","Ret="+ret.ToString()+", Result="+strResult,255);
				Tools.LogException("TransactionPeach.CardPaymentThirdParty/999","Ret="+ret.ToString()+", Result="+strResult,ex2);
			}

			ret        = 200;
			resultCode = Tools.JSONValue(strResult,"code");
			resultMsg  = Tools.JSONValue(strResult,"description");
			payRef     = Tools.JSONValue(strResult,"id");
			ret        = 210;
			if ( Successful && err == 0 )
			{
				ret = 0;
				Tools.LogInfo("TransactionPeach.CardPaymentThirdParty/220","(Succeed) Result="+strResult,221);
			}
			else
				Tools.LogInfo("TransactionPeach.CardPaymentThirdParty/230","(Fail/" + err.ToString() + ") Result="+strResult,221);

			return ret;
		}

		public override int CardPayment(Payment payment)
		{
			int ret = 10;

			try
			{
// v1
//				xmlSent = "entityId="               + Tools.URLString(payment.ProviderUserID)
//				        + "&paymentBrand="          + Tools.URLString(payment.CardType.ToUpper())
//				        + "&card.number="           + Tools.URLString(payment.CardNumber)
//				        + "&card.holder="           + Tools.URLString(payment.CardName)
//				        + "&card.expiryMonth="      + Tools.URLString(payment.CardExpiryMM)
//				        + "&card.expiryYear="       + Tools.URLString(payment.CardExpiryYYYY)
//				        + "&card.cvv="              + Tools.URLString(payment.CardCVV)
//				        + "&amount="                + Tools.URLString(payment.PaymentAmountDecimal)
//				        + "&currency="              + Tools.URLString(payment.CurrencyCode)
//				        + "&merchantTransactionId=" + Tools.URLString(payment.MerchantReference)
//				        + "&descriptor="            + Tools.URLString(payment.PaymentDescription)
//				        + "&merchantInvoiceId="     + Tools.URLString(payment.MerchantReference)
//				        + "&merchant.name=Prosperian"
//				        + "&paymentType=DB" // DB = Instant debit, PA = Pre-authorize, CP =
//				        + "&recurringType=REPEATED";
//	//			        + "&merchant.city=[merchant.city]abcdefghijklmnopqrstuvwxyz"

//	v2
//				xmlSent = "entityId="               + Tools.URLString(payment.ProviderUserID)
//				        + "&paymentBrand="          + Tools.URLString(payment.CardType.ToUpper())
//				        + "&card.number="           + Tools.URLString(payment.CardNumber)
//				        + "&card.holder="           + Tools.URLString(payment.CardName)
//				        + "&card.expiryMonth="      + Tools.URLString(payment.CardExpiryMM)
//				        + "&card.expiryYear="       + Tools.URLString(payment.CardExpiryYYYY)
//				        + "&card.cvv="              + Tools.URLString(payment.CardCVV)
//				        + "&amount="                + Tools.URLString(payment.PaymentAmountDecimal)
//				        + "&currency="              + Tools.URLString(payment.CurrencyCode)
//				        + "&merchantTransactionId=" + Tools.URLString(payment.MerchantReference)
//				        + "&descriptor="            + Tools.URLString(payment.PaymentDescription)
//				        + "&merchantInvoiceId="     + Tools.URLString(payment.MerchantReference)
//				        + "&shopperResultUrl="      + Tools.ConfigValue("SystemURL")+"/Succeed.aspx?TransRef="+Tools.XMLSafe(payment.MerchantReference)
//				        + "&merchant.name=Prosperian"
//				        + "&recurringType=REPEATED"
//				        + "&paymentType=DB"; // DB = Instant debit, PA = Pre-authorize, CP =

				SetUpPaymentXML(payment,(byte)Constants.TransactionType.CardPayment);

				Tools.LogInfo("TransactionPeach.CardPayment/10","Post="+xmlSent+", Key="+payment.ProviderKey,10);

				ret      = PostHTML((byte)Constants.TransactionType.GetToken,payment);
//				payToken = Tools.JSONValue(strResult,"id");
//				payRef   = Tools.JSONValue(strResult,"ndc");
				payRef   = Tools.JSONValue(strResult,"id");
				if ( payRef.Length < 1 && ret == 0 )
					ret = 248;

				Tools.LogInfo("TransactionPeach.CardPayment/20","ResultCode="+ResultCode + ", payRef=" + payRef,221);
			}
			catch (Exception ex)
			{
				Tools.LogException("TransactionPeach.CardPayment/90","Ret="+ret.ToString()+", XML Sent=" + xmlSent,ex);
			}
			return ret;
		}

		public override int TokenPayment(Payment payment)
		{
			int ret = 10;

			try
			{
				xmlSent = "entityId="               + Tools.URLString(payment.ProviderUserID)
				        + "&amount="                + Tools.URLString(payment.PaymentAmountDecimal)
				        + "&currency="              + Tools.URLString(payment.CurrencyCode)
				        + "&merchantTransactionId=" + Tools.URLString(payment.MerchantReference)
				        + "&descriptor="            + Tools.URLString(payment.PaymentDescription)
				        + "&paymentType=DB" // DB = Instant debit, PA = Pre-authorize
				        + "&recurringType=REPEATED";

				Tools.LogInfo("TransactionPeach.TokenPayment/10","Post="+xmlSent+", Key="+payment.ProviderKey,10);

				ret    = PostHTML((byte)Constants.TransactionType.TokenPayment,payment);
				payRef = Tools.JSONValue(strResult,"id");
				if ( payRef.Length < 1 && ret == 0 )
					ret = 249;

				Tools.LogInfo("TransactionPeach.TokenPayment/20","ResultCode="+ResultCode + ", payRef=" + payRef,221);
			}
			catch (Exception ex)
			{
				Tools.LogException("TransactionPeach.TokenPayment/90","Ret="+ret.ToString()+", XML Sent=" + xmlSent,ex);
			}
			return ret;
		}

		public TransactionPeach() : base()
		{
			bureauCode                            = Tools.BureauCode(Constants.PaymentProvider.Peach);
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol  = SecurityProtocolType.Tls12;
		}
	}
}
