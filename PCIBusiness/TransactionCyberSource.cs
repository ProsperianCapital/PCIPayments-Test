using System;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.ServiceModel;

namespace PCIBusiness
{
	public class TransactionCyberSource : Transaction
	{
//		private string[,]      fieldS; // "Signed" fields
//		private string[,]      fieldU; // "Unsigned" fields
		private string         webForm;
		private List<string[]> fieldSig;
		private List<string[]> fieldUns;

		public  bool Successful
		{
			get { return resultCode.ToUpper() == "ACCEPT"; }
//			Accept, Reject, Error, Review (CyberSource)
//			Failed                        (Internal)
		}

		public override string WebForm
		{
			get { return Tools.NullToString(webForm); }
		}

		public override int CardTest(Payment payment)
		{
//			TestTransactionV1();
//			TestTransactionV2(2);
			using (Payment x = new Payment())
			{
				x.TransactionType = (byte)Constants.TransactionType.TokenPayment;
				TokenPayment(x);
			//	x.TransactionType = (byte)Constants.TransactionType.GetToken;
			//	GetToken(x);
			}
			return 0;
		}

		private string GenerateSignature(string data, string secretKey)
		{
			UTF8Encoding encoding     = new System.Text.UTF8Encoding();
			byte[]       keyByte      = encoding.GetBytes(secretKey);
			HMACSHA256   hmacsha256   = new HMACSHA256(keyByte);
			byte[]       messageBytes = encoding.GetBytes(data);
			return Convert.ToBase64String(hmacsha256.ComputeHash(messageBytes));
		}

		private void SetUpPaymentData(Payment payment)
		{
			string tranType    = "sale";
			string cardOrToken = payment.CardNumber;
  			if ( payment.TransactionType == (byte)Constants.TransactionType.CardPaymentThirdParty )
				cardOrToken = "{{{" + payment.CardToken + "}}}";
  			else if ( payment.TransactionType == (byte)Constants.TransactionType.GetToken )
				tranType    = "create_payment_token";

			fieldSig = new List<string[]>();
//	Signed, mandatory
			fieldSig.Add(new string[] { "transaction_type" , tranType });
			fieldSig.Add(new string[] { "currency"         , payment.CurrencyCode }); // Must be ZAR
			fieldSig.Add(new string[] { "amount"           , payment.PaymentAmountDecimal });

			if ( payment.TransactionType == (byte)Constants.TransactionType.TokenPayment )
			{
				fieldSig.Add(new string[] { "payment_token" , payment.CardToken });
				fieldUns = null;
				return;
			}

			fieldSig.Add(new string[] { "payment_method"          , "card" });
			fieldSig.Add(new string[] { "card_cvn"                , payment.CardCVV });
			fieldSig.Add(new string[] { "bill_to_forename"        , payment.FirstName });
			fieldSig.Add(new string[] { "bill_to_surname"         , payment.LastName });
			fieldSig.Add(new string[] { "bill_to_address_line1"   , payment.Address1 });
			fieldSig.Add(new string[] { "bill_to_address_city"    , payment.Address2 });
			fieldSig.Add(new string[] { "bill_to_address_country" , payment.CountryCode });

//	Signed, optional
			if ( payment.PostalCode.Length > 0 )
				fieldSig.Add(new string[] { "bill_to_address_postal_code" , payment.PostalCode });
			if ( payment.EMail.Length > 0 )
				fieldSig.Add(new string[] { "bill_to_email"               , payment.EMail });
			if ( payment.PhoneCell.Length > 0 )
				fieldSig.Add(new string[] { "bill_to_phone"               , payment.PhoneCell });

			fieldUns = new List<string[]>();
//	Unsigned, mandatory
			fieldUns.Add(new string[] { "card_type"        , payment.CardType });
			fieldUns.Add(new string[] { "card_number"      , cardOrToken });
			fieldUns.Add(new string[] { "card_expiry_date" , payment.CardExpiryMM + "-" + payment.CardExpiryYYYY });

//			fieldS = new string[,] { { "transaction_type"            , tranType }
//			                       , { "currency"                    , payment.CurrencyCode } // Must be ZAR
//			                       , { "amount"                      , payment.PaymentAmountDecimal }
//			                       , { "payment_method"              , "card" }
//			                       , { "card_cvn"                    , payment.CardCVV }
//			                       , { "bill_to_forename"            , payment.FirstName }
//			                       , { "bill_to_surname"             , payment.LastName }
//			                       , { "bill_to_email"               , payment.EMail }
//			                       , { "bill_to_phone"               , payment.PhoneCell }
//			                       , { "bill_to_address_line1"       , payment.Address1 }
//			                       , { "bill_to_address_city"        , payment.Address2 }
//			                       , { "bill_to_address_postal_code" , payment.PostalCode }
//			                       , { "bill_to_address_country"     , payment.CountryCode } };
//			fieldU = new string[,] { { "card_type"                   , payment.CardType } // 001 = VISA, 002 = MC, 003 = AmEx, 005 = Diners
//			                       , { "card_number"                 , cardOrToken }
//			                       , { "card_expiry_date"            , payment.CardExpiryMM + "-" + payment.CardExpiryYYYY } };
		}

		public override int CardPayment(Payment payment)
		{
			int ret = 10;
			try
			{
				SetUpPaymentData(payment);
				ret = 50;
				ret = CallWebService(payment);
			}
			catch (Exception ex)
			{
				Tools.LogInfo     ("CardPayment/298","ret="+ret.ToString(),220,this);
				Tools.LogException("CardPayment/299","ret="+ret.ToString(),ex ,this);
			}
			return ret;
		}
		public override int CardPayment3rdParty(Payment payment)
		{
			return CardPayment(payment);
		}
		public override int TokenPayment(Payment payment)
		{
			return CardPayment(payment);
		}
		public override int GetToken(Payment payment)
		{
			return CardPayment(payment);
		}

/*
		public override int GetToken(Payment payment)
		{
			int ret = 10;
			try
			{
			//	Sample values that work
			//	fieldS = new string[,] { { "reference_number"            , "123456" }
			//	                       , { "transaction_type"            , "create_payment_token" }
			//	                       , { "currency"                    , "ZAR" }
			//	                       , { "amount"                      , "1.00" }
			//	                       , { "transaction_uuid"            , System.Guid.NewGuid().ToString() }
			//	                       , { "payment_method"              , "card" }
			//	                       , { "card_cvn"                    , "005" }
			//	                       , { "bill_to_forename"            , "Pete" }
			//	                       , { "bill_to_surname"             , "Smith" }
			//	                       , { "bill_to_email"               , "test@hotmail.com" }
			//	                       , { "bill_to_phone"               , "0885556666" }
			//	                       , { "bill_to_address_line1"       , "133 Fiddlers Lane" }
			//	                       , { "bill_to_address_city"        , "Knysna" }
			//	                       , { "bill_to_address_postal_code" , "4083" }
			//	                       , { "bill_to_address_country"     , "ZA" } };
			//	fieldU = new string[,] { { "card_type"                   , "001" }
			//	                       , { "card_number"                 , "4111111111111111" }
			//	                       , { "card_expiry_date"            , "12-2022" } };

				fieldS = new string[,] { { "transaction_type"            , "create_payment_token" }
				                       , { "currency"                    , payment.CurrencyCode } // Must be ZAR
				                       , { "amount"                      , "1.00" }
				                       , { "payment_method"              , "card" }
				                       , { "card_cvn"                    , payment.CardCVV }
				                       , { "bill_to_forename"            , payment.FirstName }
				                       , { "bill_to_surname"             , payment.LastName }
				                       , { "bill_to_email"               , payment.EMail }
				                       , { "bill_to_phone"               , payment.PhoneCell }
				                       , { "bill_to_address_line1"       , payment.Address1 }
				                       , { "bill_to_address_city"        , payment.Address2 }
				                       , { "bill_to_address_postal_code" , payment.PostalCode }
				                       , { "bill_to_address_country"     , payment.CountryCode } };
				ret    = 20;
				fieldU = new string[,] { { "card_type"                   , payment.CardType } // 001 = VISA, 002 = MC, 003 = AmEx, 005 = Diners
				                       , { "card_number"                 , payment.CardNumber }
				                       , { "card_expiry_date"            , payment.CardExpiryMM + "-" + payment.CardExpiryYYYY } };
				ret      = 30;
				ret      = CallWebService(payment); // ,payment.TransactionType);
				payToken = Tools.XMLNode(xmlResult,"payment_token");
			}
			catch (Exception ex)
			{
				Tools.LogInfo     ("GetToken/298","ret="+ret.ToString(),220,this);
				Tools.LogException("GetToken/299","ret="+ret.ToString(),ex ,this);
			}
			return ret;
		}

		public override int TokenPayment(Payment payment)
		{
			if ( ! EnabledFor3d(payment.TransactionType) )
				return 590;

			int ret = 10;
			try
			{
				fieldS = new string[,] { { "transaction_type" , "sale" }
				                       , { "currency"         , payment.CurrencyCode }
				                       , { "amount"           , payment.PaymentAmountDecimal }
				                       , { "payment_token"    , payment.CardToken } };
				fieldU = null;
				ret    = 30;
				ret    = CallWebService(payment);
			}
			catch (Exception ex)
			{
				Tools.LogInfo     ("TokenPayment/298","ret="+ret.ToString(),220,this);
				Tools.LogException("TokenPayment/299","ret="+ret.ToString(),ex ,this);
			}
			return ret;
		}
*/
		private int CallWebService(Payment payment)
      {
			resultCode   = "Failed";
			resultStatus = "999";
			resultMsg    = "Internal error";
			payRef       = "";

			if ( payment == null )
				return 1010;

			int    k;
			int    ret       = 1020;
			string sigX      = "";
			string sigF      = "";
			string sigS      = "";
			string sigU      = "";
			string unsForm   = "";
			string unsSent   = "";
			string url       = payment.ProviderURL;
			string profileId = payment.ProviderUserID;
			string accessKey = payment.ProviderPassword;
			string secretKey = payment.ProviderKey;

			if ( ! Tools.SystemIsLive() )
			{
				if ( url.Length       < 5 ) url       = "https://testsecureacceptance.cybersource.com/silent";
				if ( profileId.Length < 5 ) profileId = "3C857FA4-ED86-4A08-A119-24170A74C760";
				if ( accessKey.Length < 5 ) accessKey = "8b031c20a1ad343c97afe1869e2e7994";
				if ( secretKey.Length < 5 ) secretKey = "2ea6c71fa7e04304a78f417c1e4d95677abb9673c7cd45ec803a08696041ea62b5eae10527704f4580ae8da223c295c0b42f97808adf4b6db1a2bf032eb74bd7376d9d1393f1443aaf8bcba7cd4d1148b3157119169c404fa74be9e4cd5cf9cacc34f76976f54bfa93136e4de6b1f53750a5e9d4b1cc4fcebd67a14bbcc156c3";
			}

			if ( url.Length < 5 )
				return ret;
			if ( url.EndsWith("/") )
				url = url.Substring(0,url.Length-1);

			ret = 1030;

			try
			{
				string[,] fieldX = new string[,] { { "profile_id"       , profileId }
				                                 , { "access_key"       , accessKey }
				                                 , { "signed_date_time" , DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") }
				                                 , { "locale"           , "en" }
				                                 , { "transaction_uuid" , System.Guid.NewGuid().ToString() }
				                                 , { "reference_number" , payment.MerchantReference } };

				ret = 1040;

				if ( payment.TransactionType == (byte)Constants.TransactionType.GetToken )
					url  = url + "/token/create";
				else
					url  = url + "/pay";
				xmlSent = "";
				webForm = "<html><body onload='document.forms[\"frmX\"].submit()'>" + Environment.NewLine
				        + "<form name='frmX' method='POST' action='" + url + "'>" + Environment.NewLine;
				ret     = 1050;

//	Standard signed fields
				for ( k = 0 ; k < fieldX.GetLength(0) ; k++ )
				{
					ret     = 1060;
					xmlSent = xmlSent + "&" + fieldX[k,0] + "=" + Tools.URLString(fieldX[k,1],1);
					webForm = webForm + "<input type='hidden' id='" + fieldX[k,0] + "' name='" + fieldX[k,0] + "' value='" + fieldX[k,1] + "' />" + Environment.NewLine;
					sigX    = sigX + fieldX[k,0] + "=" + fieldX[k,1] + ",";
					sigS    = sigS + fieldX[k,0] + ",";
				}

//	Transaction-specific signed fields
// v1
//				if ( fieldS != null )
//					for ( k = 0 ; k < fieldS.GetLength(0) ; k++ )
//					{
//						ret     = 1080;
//						xmlSent = xmlSent + "&" + fieldS[k,0] + "=" + Tools.URLString(fieldS[k,1],1);
//						webForm = webForm + "<input type='hidden' id='" + fieldS[k,0] + "' name='" + fieldS[k,0] + "' value='" + fieldS[k,1] + "' />" + Environment.NewLine;
//						sigX    = sigX + fieldS[k,0] + "=" + fieldS[k,1] + ",";
//						sigS    = sigS + fieldS[k,0] + ",";
//					}

				if ( fieldSig != null )
					foreach ( string[] fld in fieldSig )
					{
						ret     = 1080;
						xmlSent = xmlSent + "&" + fld[0] + "=" + Tools.URLString(fld[1],1);
						webForm = webForm + "<input type='hidden' id='" + fld[0] + "' name='" + fld[0] + "' value='" + fld[1] + "' />" + Environment.NewLine;
						sigX    = sigX + fld[0] + "=" + fld[1] + ",";
						sigS    = sigS + fld[0] + ",";
					}

//	Transaction-specific unsigned fields
// v1
//				if ( fieldU != null )
//					for ( k = 0 ; k < fieldU.GetLength(0) ; k++ )
//					{
//						ret     = 1110;
//						unsSent = unsSent + "&" + fieldU[k,0] + "=" + Tools.URLString(fieldU[k,1],1);
//						unsForm = unsForm + "<input type='hidden' id='" + fieldU[k,0] + "' name='" + fieldU[k,0] + "' value='" + fieldU[k,1] + "' />" + Environment.NewLine;
//						sigU    = sigU + fieldU[k,0] + ",";
//					}

				if ( fieldUns != null )
					foreach ( string[] fld in fieldUns )
					{
						ret     = 1110;
						unsSent = unsSent + "&" + fld[0] + "=" + Tools.URLString(fld[1],1);
						unsForm = unsForm + "<input type='hidden' id='" + fld[0] + "' name='" + fld[0] + "' value='" + fld[1] + "' />" + Environment.NewLine;
						sigU    = sigU + fld[0] + ",";
					}

				ret     = 1160;
				sigS    = sigS + "signed_field_names,unsigned_field_names";
				sigX    = sigX + "signed_field_names=" + sigS + ",unsigned_field_names=" + sigU;
				xmlSent = xmlSent.Substring(1);
				sigF    = GenerateSignature(sigX,secretKey);
				ret     = 1170;
				webForm = webForm + "<input type='hidden' id='signed_field_names' name='signed_field_names' value='" + sigS + "' />" + Environment.NewLine
				                  + "<input type='hidden' id='unsigned_field_names' name='unsigned_field_names' value='" + sigU + "' />" + Environment.NewLine
				                  + unsForm
				                  + "<input type='hidden' id='signature' name='signature' value='" + sigF + "' />" + Environment.NewLine
				                  + "</form></body></html>";
				ret     = 1180;
				xmlSent = xmlSent + "&signed_field_names="   + Tools.URLString(sigS)
				                  + "&unsigned_field_names=" + Tools.URLString(sigU)
				                  + unsSent
				                  + "&signature="            + Tools.URLString(sigF);

				Tools.LogInfo("CallWebService/40","Provider="+Tools.BureauCode(Constants.PaymentProvider.CyberSource)
				                              +" | URL="+url
				                              +" | TransactionType="+payment.TransactionTypeName
				                              +" | Profile Id="+profileId
				                              +" | Access Key="+accessKey
				                              +" | Secret Key="+secretKey ,222,this);

				HttpWebRequest webRequest;

				if ( payment.TransactionType == (byte)Constants.TransactionType.CardPaymentThirdParty )
				{
					ret         = 1190;
					string tURL = payment.TokenizerURL; // The TOKENIZER/THIRD PARTY (TokenEx)
					if ( tURL.Length < 1 || bureauCodeTokenizer.Length < 1 )
					{
						Tools.LogInfo("CardPayment3rdParty/20","Unknown Third Party Tokenizer (" + bureauCodeTokenizer + "), data=" + xmlSent,221,this);
						return ret;
					}
					if ( ! tURL.ToUpper().EndsWith("DETOKENIZE") )
						tURL = tURL + "/TransparentGatewayAPI/Detokenize";

					ret                                = 1200;
//					webForm                            = webForm.Replace("POST-TO-URL",tURL);
					webRequest                         = (HttpWebRequest)WebRequest.Create(tURL);
					webRequest.Headers["TX_URL"]       = url;
					webRequest.Headers["TX_TokenExID"] = payment.TokenizerID;
					webRequest.Headers["TX_APIKey"]    = payment.TokenizerKey;
					Tools.LogInfo("CallWebService/50","Token Provider="+Tools.BureauCode(Constants.PaymentProvider.TokenEx)
					                              +" | Tx URL="+tURL
					                              +" | Tx Id="+payment.TokenizerID
					                              +" | Tx Key="+payment.TokenizerKey,222,this);
				}
				else
				{
					ret        = 1210;
//					webForm    = webForm.Replace("POST-TO-URL",url);
					webRequest = (HttpWebRequest)WebRequest.Create(url);
				}

				Tools.LogInfo("CallWebService/60","Signature Input="+sigX , 10,this);
				Tools.LogInfo("CallWebService/70","Signature Output="+sigF, 10,this);
				Tools.LogInfo("CallWebService/80","Web form="+webForm     , 10,this);
				Tools.LogInfo("CallWebService/90","URL params="+xmlSent   ,222,this);

				ret                    = 1220;
				strResult              = "";
				webRequest.Method      = "POST";
				webRequest.ContentType = "application/x-www-form-urlencoded";
				webRequest.Accept      = "application/x-www-form-urlencoded";
				byte[] page            = Encoding.UTF8.GetBytes(xmlSent);
				ret                    = 1230;

				using (Stream stream = webRequest.GetRequestStream())
				{
					stream.Write(page, 0, page.Length);
//					stream.Flush();
					stream.Close();
				}

				ret = 1240;
				using (WebResponse webResponse = webRequest.GetResponse())
				{
					ret = 1250;
					using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
					{
						ret       = 1260;
						strResult = rd.ReadToEnd();
					}
				}

				Tools.LogInfo("CallWebService/160","XML Rec=" + strResult.ToString(),222,this);

				ret          = 1270;
				webForm      = "";
				resultCode   = Tools.HTMLValue(strResult,"decision");
				resultStatus = Tools.HTMLValue(strResult,"reason_code");
				resultMsg    = Tools.HTMLValue(strResult,"message");
				payRef       = Tools.HTMLValue(strResult,"transaction_id");
				sigX         = Tools.HTMLValue(strResult,"payment_token");
				if ( sigX.Length > 0 )
					payToken  = sigX;
				ret          = 1290;

				if ( Successful && payRef.Length > 0 )
					ret = 0;
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

//	Code from CyberSource
//	Start
//		private string GenerateDigestV1(string jsonData)
//		{
//			try
//			{
//	//		//	string jsonData = "{ your JSON payload }";
//				using (SHA256 sha256hash = SHA256.Create())
//				{
//					byte[] payloadBytes = sha256hash.ComputeHash(Encoding.UTF8.GetBytes(jsonData));
//					string digest       = Convert.ToBase64String(payloadBytes);
//					return "SHA-256=" + digest;
//				}
//			}
//			catch (Exception ex)
//			{
//				Tools.LogException("GenerateDigest",jsonData,ex,this);
//			}
//			return "";
//		}
//		private string GenerateSignatureV1(string signatureParams, string secretKey)
//		{
//			var sigBytes      = Encoding.UTF8.GetBytes(signatureParams);
//			var decodedSecret = Convert.FromBase64String(secretKey);
//			var hmacSha256    = new HMACSHA256(decodedSecret);
//			var messageHash   = hmacSha256.ComputeHash(sigBytes);
//			return Convert.ToBase64String(messageHash);
//		}
//	End

		public void TestTransactionV2(byte mode)
		{
		//	Mode = 1 : Web form submit
		//	Mode = 2 : URL fields

			string url       = "https://testsecureacceptance.cybersource.com/silent/token/create";
//			string url       = "https://testsecureacceptance.cybersource.com/silent/pay";
//			string profileId = "466381AB-5F66-4679-AFD4-5035EA9077A7";
//			string accessKey = "faaf4d2bcc42365d90f853daa4096cdc";
//			string secretKey = "73004ef0b7c041be93e03c995261fddb651b0d62b2e34ec093452c706da8c08bd8dd69ff8747423f9f27f05b01f3e9d2efb12af5a2834989b08b0ed461dfe55df4d43a7cb81942439fd3496724037cce5d62874a64fe450380f037603e120b5e9caebdf35d1d4fb98c2c52202ea0aae7fc640bd9b8f64709b2dd1598e6c5dd4f";
			string profileId = "3C857FA4-ED86-4A08-A119-24170A74C760";
			string accessKey = "8b031c20a1ad343c97afe1869e2e7994";
			string secretKey = "2ea6c71fa7e04304a78f417c1e4d95677abb9673c7cd45ec803a08696041ea62b5eae10527704f4580ae8da223c295c0b42f97808adf4b6db1a2bf032eb74bd7376d9d1393f1443aaf8bcba7cd4d1148b3157119169c404fa74be9e4cd5cf9cacc34f76976f54bfa93136e4de6b1f53750a5e9d4b1cc4fcebd67a14bbcc156c3";
			int    ret       = 10;

			try
			{
				DateTime   dt    = DateTime.Now.ToUniversalTime();
				string[,] fieldS = new string[,] { { "reference_number"            , "123456" }
				                                 , { "transaction_type"            , "sale" }
				                                 , { "currency"                    , "ZAR" }
				                                 , { "amount"                      , "19.37" }
				                                 , { "locale"                      , "en" }
				                                 , { "profile_id"                  , profileId }
				                                 , { "access_key"                  , accessKey }
				                                 , { "transaction_uuid"            , System.Guid.NewGuid().ToString() }
				                                 , { "signed_date_time"            , dt.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") }
				                                 , { "payment_method"              , "card" }
				                                 , { "card_cvn"                    , "005" }
				                                 , { "bill_to_forename"            , "Pete" }
				                                 , { "bill_to_surname"             , "Smith" }
				                                 , { "bill_to_email"               , "test@hotmail.com" }
				                                 , { "bill_to_phone"               , "0885556666" }
				                                 , { "bill_to_address_line1"       , "133 Fiddlers Lane" }
				                                 , { "bill_to_address_city"        , "Knysna" }
				                                 , { "bill_to_address_postal_code" , "4083" }
				                                 , { "bill_to_address_country"     , "ZA" } };
				string[,] fieldU = new string[,] { { "card_type"                   , "001" }
				                                 , { "card_number"                 , "4111111111111111" }
				                                 , { "card_expiry_date"            , "12-2022" } };
				string sigX    = "";
				string sigF    = "";
				string sigS    = "";
				string sigU    = "";
				string unsForm = "";
				string unsSent = "";

				ret     = 20;
				xmlSent = "";
				webForm = "<html><body onload='document.forms[\"frmX\"].submit()'>" + Environment.NewLine
				        + "<form name='frmX' method='POST' action='" + url + "'>" + Environment.NewLine;

				for ( int k = 0 ; k < fieldS.GetLength(0) ; k++ )
				{
					ret     = 30;
					xmlSent = xmlSent + "&" + fieldS[k,0] + "=" + Tools.URLString(fieldS[k,1]);
					webForm = webForm + "<input type='hidden' id='" + fieldS[k,0] + "' name='" + fieldS[k,0] + "' value='" + fieldS[k,1] + "' />" + Environment.NewLine;
					sigX    = sigX + "," + fieldS[k,0] + "=" + fieldS[k,1];
					sigS    = sigS + "," + fieldS[k,0];
				}

				for ( int k = 0 ; k < fieldU.GetLength(0) ; k++ )
				{
					ret     = 40;
					unsSent = unsSent + "&" + fieldU[k,0] + "=" + Tools.URLString(fieldU[k,1]);
					unsForm = unsForm + "<input type='hidden' id='" + fieldU[k,0] + "' name='" + fieldU[k,0] + "' value='" + fieldU[k,1] + "' />" + Environment.NewLine;
					sigU    = sigU + "," + fieldU[k,0];
				}

				ret     = 50;
				sigU    = sigU.Substring(1);
				sigS    = sigS.Substring(1) + ",signed_field_names,unsigned_field_names";
				sigX    = sigX.Substring(1) + ",signed_field_names=" + sigS + ",unsigned_field_names=" + sigU;
				xmlSent = xmlSent.Substring(1);
				sigF    = GenerateSignature(sigX,secretKey);
				ret     = 60;
				webForm = webForm + "<input type='hidden' id='signed_field_names' name='signed_field_names' value='" + sigS + "' />" + Environment.NewLine
				                  + "<input type='hidden' id='unsigned_field_names' name='unsigned_field_names' value='" + sigU + "' />" + Environment.NewLine
				                  + unsForm
				                  + "<input type='hidden' id='signature' name='signature' value='" + sigF + "' />" + Environment.NewLine
				                  + "</form></body></html>";
				ret     = 70;
				xmlSent = xmlSent + "&signed_field_names="   + Tools.URLString(sigS)
				                  + "&unsigned_field_names=" + Tools.URLString(sigU)
				                  + unsSent
				                  + "&signature="            + Tools.URLString(sigF);

				Tools.LogInfo("TestTransactionV2/10","Profile Id="+profileId,222,this);
				Tools.LogInfo("TestTransactionV2/20","Access Key="+accessKey,222,this);
				Tools.LogInfo("TestTransactionV2/30","Secret Key="+secretKey,222,this);
				Tools.LogInfo("TestTransactionV2/40","Signature Input="+sigX,222,this);
				Tools.LogInfo("TestTransactionV2/50","Signature Output="+sigF,222,this);
				Tools.LogInfo("TestTransactionV2/60","Web form="+webForm,222,this);
				Tools.LogInfo("TestTransactionV2/70","URL params="+xmlSent,222,this);

				if ( mode == 1 ) //	Web form
					return;

				HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
				ret                       = 110;
				strResult                 = "";
				webRequest.Method         = "POST";
				webRequest.ContentType    = "application/x-www-form-urlencoded";
				webRequest.Accept         = "application/x-www-form-urlencoded";
				byte[] page               = Encoding.UTF8.GetBytes(xmlSent);

				ret = 120;
				using (Stream stream = webRequest.GetRequestStream())
				{
					stream.Write(page, 0, page.Length);
//					stream.Flush();
					stream.Close();
				}

				ret = 130;
				using (WebResponse webResponse = webRequest.GetResponse())
				{
					ret = 140;
					using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
					{
						ret       = 150;
						strResult = rd.ReadToEnd();
					}
				}

				Tools.LogInfo("TestTransactionV2/160","XML Rec=" + strResult.ToString(),222,this);
				ret       = 160;
				webForm   = "";
			//	xmlResult = new XmlDocument();
			//	xmlResult.LoadXml(strResult.ToString());
			}
			catch (WebException ex1)
			{
				Tools.DecodeWebException(ex1,ClassName+".TestTransactionV2/297","ret="+ret.ToString());
			}
			catch (Exception ex2)
			{
				Tools.LogInfo     ("TestTransactionV2/298","ret="+ret.ToString(),220,this);
				Tools.LogException("TestTransactionV2/299","ret="+ret.ToString(),ex2,this);
			}
		}

		public void TestTransactionV1() // SOAP
		{
			CyberSource.RequestMessage request = new CyberSource.RequestMessage();

			request.merchantID = "2744639";
			request.merchantReferenceCode = "A1-TEST-82348687";

		//	Help with trouble-shooting
			request.clientLibrary        = ".NET WCF";
			request.clientLibraryVersion = Environment.Version.ToString();
			request.clientEnvironment    = Environment.OSVersion.Platform + Environment.OSVersion.Version.ToString();

			request.ccAuthService = new CyberSource.CCAuthService();
			request.ccAuthService.run = "true";

			CyberSource.BillTo billTo = new CyberSource.BillTo();
			billTo.firstName = "John";
			billTo.lastName = "Doe";
			billTo.street1 = "1295 Charleston Road";
			billTo.city = "Mountain View";
			billTo.state = "CA";
			billTo.postalCode = "94043";
			billTo.country = "US";
			billTo.email = "null@cybersource.com";
			billTo.ipAddress = "10.7.111.111";
			request.billTo = billTo;

			CyberSource.Card card = new CyberSource.Card();
			card.accountNumber = "4111111111111111";
			card.expirationMonth = "12";
			card.expirationYear = "2020";
			request.card = card;

			CyberSource.PurchaseTotals purchaseTotals = new CyberSource.PurchaseTotals();
			purchaseTotals.currency = "USD";
			request.purchaseTotals = purchaseTotals;

			request.item = new CyberSource.Item[2];

			CyberSource.Item item = new CyberSource.Item();
			item.id = "0";
			item.unitPrice = "12.34";
			request.item[0] = item;

			item = new CyberSource.Item();
			item.id = "1";
			item.unitPrice = "56.78";
			request.item[1] = item;

			try
			{
				CyberSource.TransactionProcessor proc = new CyberSource.TransactionProcessor();

//	proc.ChannelFactory does not exist in the API.
//	How do I define the merchant id and key/password?

//				proc.ChannelFactory.Credentials.UserName.UserName = request.merchantID;
//				proc.ChannelFactory.Credentials.UserName.Password = "1b5e6b316fd0e4a9885a354523f958fd78ef9b8c";

				CyberSource.ReplyMessage reply = proc.runTransaction(request);

				Tools.LogInfo("TestTransaction/5","decision = " + reply.decision
				                              + ", reasonCode = " + reply.reasonCode
				                              + ", requestID = " + reply.requestID
				                              + ", requestToken = " + reply.requestToken
				                              + ", ccAuthReply.reasonCode = " + reply.ccAuthReply.reasonCode,244);
			}
			catch (TimeoutException ex)
			{
				Tools.LogException("TestTransaction/10","TimeoutException",ex,this);
			}
			catch (FaultException ex)
			{
				Tools.LogException("TestTransaction/15","FaultException",ex,this);
			}
			catch (CommunicationException ex)
			{
				Tools.LogException("TestTransaction/20","CommunicationException",ex,this);
			}
			catch (Exception ex)
			{
				Tools.LogException("TestTransaction/25","Exception",ex,this);
			}
		}

      public override void Close()
		{
			fieldSig = null;
			fieldUns = null;
			base.Close();
		}

		public TransactionCyberSource() : base()
		{
			base.LoadBureauDetails(Constants.PaymentProvider.CyberSource);
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol  = SecurityProtocolType.Tls12;
		}
	}
}
