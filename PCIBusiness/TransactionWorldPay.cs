using System;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;

namespace PCIBusiness
{
	public class TransactionWorldPay : Transaction
	{
//		public  bool Successful
//		{
//			get { return Tools.JSONValue(strResult,"success").ToUpper() == "TRUE"; }
//		}

		public override int GetToken(Payment payment)
		{
			int ret  = 10;
			payToken = "";

			try
			{
				Tools.LogInfo("GetToken/10","Merchant Ref=" + payment.MerchantReference,10,this);

				string descr = payment.PaymentDescription;
				if ( descr.Length < 1 )
					descr = "Recurring payment token";

				xmlSent = "<?xml version='1.0' encoding='UTF-8'?>"
				        + "<!DOCTYPE paymentService PUBLIC"
				        +          " '-//WorldPay//DTD WorldPay PaymentService v1//EN'"
				        +          " 'http://dtd.worldpay.com/paymentService_v1.dtd'>"
				        + "<paymentService version='1.4' merchantCode='" + payment.ProviderAccount + "'>"
				        +   "<submit>"
				        +     "<paymentTokenCreate>"
				        +       "<createToken tokenScope='merchant'>"
				        +         "<tokenEventReference>" + payment.MerchantReference + "</tokenEventReference>"
				        +         "<tokenReason>" + descr + "</tokenReason>"
				        +       "</createToken>"
				        +       "<paymentInstrument>"
				        +         "<cardDetails>"
				        +           "<cardNumber>" + payment.CardNumber + "</cardNumber>"
				        +           "<expiryDate>"
				        +             "<date month='" + payment.CardExpiryMM + "' year='" + payment.CardExpiryYYYY + "' />"
				        +           "</expiryDate>"
				        +           "<cardHolderName>" + payment.CardName + "</cardHolderName>"
				        +         "</cardDetails>"
				        +       "</paymentInstrument>"
				        +     "</paymentTokenCreate>"
				        +   "</submit>"
				        + "</paymentService>";
/*

<?xml version="1.0" encoding="UTF-8"?> 
<!DOCTYPE paymentService PUBLIC "-//WorldPay//DTD WorldPay PaymentService v1//EN"
 "http://dtd.worldpay.com/paymentService_v1.dtd">
<paymentService version="1.4" merchantCode="MYMERCHANT">
  <submit>
    <paymentTokenCreate> <!--used instead of order element-->
      <createToken tokenScope="merchant">
        <tokenEventReference>TOK7854321</tokenEventReference>
        <tokenReason>ClothesDepartment</tokenReason>
      </createToken>
      <paymentInstrument>
        <cardDetails>
          <cardNumber>4444333322221111</cardNumber>
          <expiryDate>
            <date month="06" year="2019" />
          </expiryDate>
          <cardHolderName>J. Shopper</cardHolderName>
          <cardAddress>
            <address>
              <address1>47A</address1>
              <address2>Queensbridge Road</address2>
              <address3>Suburbia</address3>
              <postalCode>CB94BQ</postalCode>
              <city>Cambridge</city>
              <state>Cambridgeshire</state>
              <countryCode>GB</countryCode>
            </address>
          </cardAddress>
        </cardDetails>
      </paymentInstrument>
    </paymentTokenCreate>
  </submit>
</paymentService>
*/
				ret      = 20;
				ret      = CallWebService(payment,(byte)Constants.TransactionType.GetToken);
				if ( ret == 0 && payToken.Length > 0 )
					return 0;

				Tools.LogInfo("GetToken/50","ret="+ret.ToString()+", payToken="+payToken+", XML Sent="+xmlSent+", XML Rec="+strResult,199,this);
			}
			catch (Exception ex)
			{
				Tools.LogInfo     ("GetToken/98","Ret="+ret.ToString()+", XML Sent="+xmlSent,255,this);
				Tools.LogException("GetToken/99","Ret="+ret.ToString()+", XML Sent="+xmlSent,ex ,this);
			}
			return ret;
		}

		public override int TokenPayment(Payment payment)
		{
			if ( ! EnabledFor3d(payment.TransactionType) )
				return 590;

			int ret = 10;
			payRef  = "";

			Tools.LogInfo("TokenPayment/10","Merchant Ref=" + payment.MerchantReference,10,this);

//	Testing
//			payment.CurrencyCode = "USD";
//			payment.CardToken    = "9963184968580547777";
//	Testing

			try
			{
				xmlSent = "<?xml version='1.0' encoding='UTF-8'?>"
				        + "<!DOCTYPE paymentService PUBLIC"
				        +          " '-//WorldPay//DTD WorldPay PaymentService v1//EN'"
				        +          " 'http://dtd.worldpay.com/paymentService_v1.dtd'>"
				        + "<paymentService version='1.4' merchantCode='" + payment.ProviderAccount + "'>"
				        +   "<submit>"
				        +     "<order orderCode='" + payment.TransactionID + "'>"
				        +       "<description>" + payment.PaymentDescription + "</description>"
				        +       "<amount currencyCode='" + payment.CurrencyCode + "'"
				        +              " exponent='2'"
				        +              " value='" + payment.PaymentAmount.ToString() + "' />"
				        +       "<paymentDetails>"
				        +         "<TOKEN-SSL tokenScope='merchant'>"
				        +           "<paymentTokenID>" + payment.CardToken + "</paymentTokenID>"
				        +           "<paymentInstrument>"
				        +             "<cardDetails>"
				        +               "<cvc>" + payment.CardCVV + "</cvc>"
				        +             "</cardDetails>"
				        +           "</paymentInstrument>"
				        +         "</TOKEN-SSL>"
				        +       "</paymentDetails>"
				        +     "</order>"
				        +   "</submit>"
				        + "</paymentService>";
/*
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE paymentService PUBLIC "-//WorldPay//DTD WorldPay PaymentService v1//EN" "http://dtd.worldpay.com/paymentService_v1.dtd">
<paymentService version="1.4" merchantCode="YOUR_MERCHANT_CODE"> 
  <submit>
    <order orderCode="YOUR_ORDER_CODE">
      <description>20 red roses from the MyMerchant webshop.</description>
      <amount currencyCode="GBP" exponent="2" value="5000"/>
      <paymentDetails>
        <TOKEN-SSL tokenScope="shopper"> 
          <paymentTokenID>efnhiuh7438rhf3hd9i3</paymentTokenID>
        </TOKEN-SSL>
        <session shopperIPAddress="123.123.123" id="0215ui8ib1" />
      </paymentDetails>
      <shopper>
        <shopperEmailAddress>jshopper@myprovider.int</shopperEmailAddress>
        <authenticatedShopperID>shopperID1234</authenticatedShopperID> <!--Mandatory for shopper tokens, don't send for merchant tokens-->
        <browser>
          <acceptHeader>text/html,application/xhtml+xml,application/xml ;q=0.9,* / *;q=0.8 </acceptHeader>
          <userAgentHeader>Mozilla/5.0 (Windows; U; Windows NT 5.1;en-GB; rv:1.9.1.5) Gecko/20091102 Firefox/3.5.5 (.NET CLR 3.5.30729) </userAgentHeader>
        </browser>
      </shopper>
    </order>
  </submit>
</paymentService>
*/

				ret      = 20;
				ret      = CallWebService(payment,(byte)Constants.TransactionType.TokenPayment);
				if ( ret == 0 && payRef.Length > 0 )
					return 0;
			}
			catch (Exception ex)
			{
				Tools.LogInfo     ("TokenPayment/98","Ret="+ret.ToString()+", XML Sent="+xmlSent,255,this);
				Tools.LogException("TokenPayment/99","Ret="+ret.ToString()+", XML Sent="+xmlSent,ex ,this);
			}
			return ret;
		}

		private int CallWebService(Payment payment,byte transactionType)
      {
			int    ret = 10;
			string url = payment.ProviderURL;

			if ( Tools.NullToString(url).Length == 0 )
				url = BureauURL;

			ret = 20;
			if ( url.EndsWith("/") )
				url = url.Substring(0,url.Length-1);

//	Testing
//			url                      = "https://secure-test.worldpay.com/jsp/merchant/xml/paymentService.jsp";
//			string usr               = payment.ProviderUserID;
//			string pwd               = payment.ProviderPassword;
//			payment.ProviderUserID   = "2LHRK1HBEPDYVP9OKG8S";
//			payment.ProviderPassword = "st0nE#481";
//	Testing

			ret       = 60;
			payToken  = "";
			payRef    = "";
			otherRef  = "";
			strResult = "";
			ret       = 70;
			SetError ("99","Internal error connecting to " + url);

			try
			{
				byte[]         page                 = Encoding.UTF8.GetBytes(xmlSent);
				byte[]         authArray            = Encoding.ASCII.GetBytes(payment.ProviderUserID+":"+payment.ProviderPassword);
			//	byte[]         authArray            = Encoding.ASCII.GetBytes($"{UserName}:{Password}");
				string         auth64               = Convert.ToBase64String(authArray);
				HttpWebRequest webRequest           = (HttpWebRequest)WebRequest.Create(url);
				webRequest.ContentType              = "text/xml;charset=\"utf-8\"";
				webRequest.Accept                   = "text/xml";
				webRequest.Method                   = "POST";
				ret                                 = 80;
				webRequest.Headers["Authorization"] = "Basic " + auth64;
				ret                                 = 100;

				Tools.LogInfo("CallWebService/20",
				              "Transaction Type=" + Tools.TransactionTypeName(transactionType) +
				            ", URL=" + url +
				            ", Account=" + payment.ProviderAccount +
				            ", User=" + payment.ProviderUserID +
				            ", Pwd=" + payment.ProviderPassword +
				            ", Authorization=" + auth64 +
				            ", XML Sent=" + xmlSent, 222, this);

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
						rd.Close();
					}
					webResponse.Close();
				}

				if ( strResult.Length == 0 )
				{
					ret = 150;
					SetError ("98","No data returned from " + url);
					Tools.LogInfo("CallWebService/30","Failed, XML Rec=(empty)",199,this);
				}

				else
					try
					{
						SetError ("97","Unable to read XML returned");
						ret       = 160;
						xmlResult = new XmlDocument();
						xmlResult.LoadXml(strResult);
						if ( strResult.Contains("reply><error") )
						{
							resultMsg  = xmlResult.SelectNodes("/paymentService/reply")[0].InnerText;
							resultCode = xmlResult.SelectNodes("/paymentService/reply/error")[0].Attributes[0].InnerText;
						}
						else if ( transactionType == (byte)Constants.TransactionType.GetToken )
						{
							ret      = 190;
							payToken = Tools.XMLNode(xmlResult,"paymentTokenID");
							if ( payToken.Length > 0 )
								SetError ("00","");
							else
								SetError ("96","Unable to retrieve token");
						}
						else if ( transactionType == (byte)Constants.TransactionType.TokenPayment )
						{
							ret       = 200;
							resultMsg = Tools.XMLNode(xmlResult,"lastEvent");
							payRef    = Tools.XMLNode(xmlResult,"cardNumber");
							if ( resultMsg.ToUpper().StartsWith("AUTHORI") )
								SetError ("00",resultMsg);
							else if ( resultMsg.Length > 0 )
								SetError ("95","Payment failed (" + resultMsg + ")");
							else
								SetError ("94","Payment failed");
						}
						if ( resultCode == "00" )
							ret = 0;
					}
					catch (Exception ex3)
					{
						Tools.LogInfo     ("CallWebService/290","ret="+ret.ToString()+", "+strResult,220,this);
						Tools.LogException("CallWebService/291","ret="+ret.ToString()+", "+strResult,ex3,this);
						if ( resultMsg.Length == 0 )
							resultMsg  = "(93) Unable to read XML returned";
						if ( resultCode.Length == 0 )
							resultCode = "93";
					}
			}
			catch (WebException ex1)
			{
				Tools.DecodeWebException(ex1,ClassName+".CallWebService/297","ret="+ret.ToString());
			}
			catch (Exception ex2)
			{
				Tools.LogInfo     ("CallWebService/298","ret="+ret.ToString(),220,this);
				Tools.LogException("CallWebService/299","ret="+ret.ToString(),ex2,this);
			}
			return ret;
		}

		private void SetError(string eCode,string eMsg)
		{
			resultCode = eCode;
			if ( Tools.StringToInt(eCode) > 0 && eMsg.Length > 0 )
				resultMsg = "(" + eCode + ") " + eMsg;
			else
				resultMsg = eMsg;
		}

		public TransactionWorldPay() : base()
		{
			base.LoadBureauDetails(Constants.PaymentProvider.WorldPay);
			ServicePointManager.SecurityProtocol  = SecurityProtocolType.Tls12;
		}
	}
}
