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
			int    ret = 10;
			string url = "https://test.oppwa.com/v1/registrations";
			strResult  = "";
			payRef     = "";
			resultCode = "999.999.999";
			resultMsg  = "Internal error";

			try
			{
				if ( payment.ProviderURL.Length > 0 )
					url = payment.ProviderURL;
				if ( transactionType == (byte)Constants.TransactionType.TokenPayment )
					url = url + "/" + payment.CardToken + "/payments";

				Tools.LogInfo("TransactionPeach.PostHTML/10","URL=" + url + ", URL data=" + xmlSent,221);

				ret                              = 20;
				byte[]         buffer            = Encoding.ASCII.GetBytes(xmlSent);
//				byte[]         buffer            = Encoding.UTF8.GetBytes(xmlSent);
				HttpWebRequest request           = (HttpWebRequest)HttpWebRequest.Create(url);
				ret                              = 30;
				request.Method                   = "POST";
//				request.Headers["Authorization"] = "Bearer OGFjN2E0Yzc3MmI3N2RkZjAxNzJiN2VkMDFmODA2YTF8akE0aEVaOG5ZQQ==";
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
					ret        = 100;
					resultCode = Tools.JSONValue(strResult,"code");
					resultMsg  = Tools.JSONValue(strResult,"description");
					ret        = 110;
					if ( Successful )
						ret     = 0;
					else
						Tools.LogInfo("TransactionPeach.PostHTML/120","resultCode="+resultCode+", resultMsg="+resultMsg,221);
				}
			}
			catch (Exception ex)
			{
				Tools.LogInfo("TransactionPeach.PostHTML/198","Ret="+ret.ToString()+", URL=" + url + ", XML Sent="+xmlSent,255);
				Tools.LogException("TransactionPeach.PostHTML/199","Ret="+ret.ToString()+", URL=" + url + ", XML Sent="+xmlSent,ex);
			}
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

		public override int ProcessPayment(Payment payment)
		{
			int ret = 10;

			try
			{
				xmlSent = "entityId="  + Tools.URLString(payment.ProviderUserID)
				        + "&amount="   + Tools.URLString(payment.PaymentAmountDecimal)
				        + "&currency=" + Tools.URLString(payment.CurrencyCode)
				        + "&paymentType=PA"
				        + "&recurringType=REPEATED";

				Tools.LogInfo("TransactionPeach.ProcessPayment/10","Post="+xmlSent+", Key="+payment.ProviderKey,10);

				ret    = PostHTML((byte)Constants.TransactionType.TokenPayment,payment);
				payRef = Tools.JSONValue(strResult,"id");
				if ( payRef.Length < 1 && ret == 0 )
					ret = 249;

				Tools.LogInfo("TransactionPeach.ProcessPayment/20","ResultCode="+ResultCode + ", payRef=" + payRef,221);
			}
			catch (Exception ex)
			{
				Tools.LogException("TransactionPeach.ProcessPayment/90","Ret="+ret.ToString()+", XML Sent=" + xmlSent,ex);
			}
			return ret;
		}


		public TransactionPeach() : base()
		{
			bureauCode                           = Tools.BureauCode(Constants.PaymentProvider.Peach);
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		}
	}
}
