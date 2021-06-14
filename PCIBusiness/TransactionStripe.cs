using System;
using Stripe;

namespace PCIBusiness
{
	public class TransactionStripe : Transaction
	{
		private string err;

	//	Moved to Transaction.cs
	//	private string customerId;
	//	private string paymentMethodId;

	//	public  string CustomerId
	//	{
	//		get { return Tools.NullToString(customerId); }
	//	}
	//	public  string PaymentMethodId
	//	{
	//		get { return Tools.NullToString(paymentMethodId); }
	//	}

		public override int GetToken(Payment payment)
		{
			int ret         = 10;
			err             = "";
			payToken        = "";
			customerId      = "";
			paymentMethodId = "";
			strResult       = "";
			resultCode      = "991";
			resultMsg       = "failed";

			Tools.LogInfo("GetToken/10","Merchant Ref=" + payment.MerchantReference,10,this);

			try
			{
				ret                        = 20;
				StripeConfiguration.ApiKey = payment.ProviderPassword; // Secret key

				ret                        = 30;
				var tokenOptions           = new TokenCreateOptions
				{
					Card = new TokenCardOptions
					{
						Number   = payment.CardNumber,
						ExpMonth = payment.CardExpiryMonth,
						ExpYear  = payment.CardExpiryYear,
						Cvc      = payment.CardCVV
					}
				};
				ret              = 40;
				var tokenService = new TokenService();
				var token        = tokenService.Create(tokenOptions);
				payToken         = token.Id;
				err              = err + ", tokenId="+Tools.NullToString(payToken);

				ret                      = 50;
				var paymentMethodOptions = new PaymentMethodCreateOptions
				{
					Type = "card",
					Card = new PaymentMethodCardOptions
					{
						Token = token.Id
					}
				};
				ret                      = 60;
				var paymentMethodService = new PaymentMethodService();
				var paymentMethod        = paymentMethodService.Create(paymentMethodOptions);
				paymentMethodId          = paymentMethod.Id;
				err                      = err + ", paymentMethodId="+Tools.NullToString(paymentMethodId);

				ret                 = 70;
				var customerOptions = new CustomerCreateOptions
				{
					Name          = payment.CardName, // (payment.FirstName + " " + payment.LastName).Trim(),
					Email         = payment.EMail,
					Phone         = payment.PhoneCell,
					PaymentMethod = paymentMethod.Id,
				};
				ret                  = 80;
				var customerService  = new CustomerService();
				var customer         = customerService.Create(customerOptions);
//				customer.Currency    = payment.CurrencyCode.ToLower();
				customerId           = customer.Id;
				err                  = err + ", customerId="+Tools.NullToString(customerId);

				ret                  = 100;
				strResult            = customer.StripeResponse.Content;
//				resultMsg            = Tools.JSONValue(strResult,"status");
				resultCode           = customer.StripeResponse.ToString();
				int k                = resultCode.ToUpper().IndexOf(" STATUS=");
				ret                  = 110;
				err                  = err + ", StripeResponse="+Tools.NullToString(resultCode);

//	customer.StripeResponse.ToString() is as follows:
//	<Stripe.StripeResponse status=200 Request-Id=req_bI0B5glG6r6DNe Date=2021-05-28T09:35:23>

				if ( k > 0 )
				{
					resultCode = resultCode.Substring(k+8).Trim();
					k          = resultCode.IndexOf(" ");
					if ( k > 0 )
						resultCode = resultCode.Substring(0,k);
				}
				else
					resultCode = "999";

				ret                  = 120;
				err                  = err + ", strResult=" +Tools.NullToString(strResult)
				                           + ", resultCode="+Tools.NullToString(resultCode);
				customer             = null;
				customerService      = null;
				customerOptions      = null;
				paymentMethod        = null;
				paymentMethodService = null;
				paymentMethodOptions = null;
				token                = null;
				tokenService         = null;
				tokenOptions         = null;

				if ( resultCode.StartsWith("2") && payToken.Length > 0 && paymentMethodId.Length > 0 && customerId.Length > 0 )
				{
					resultMsg = "succeeded";
					ret       = 0;
					Tools.LogInfo ("GetToken/189","Ret=0"+err,255,this);
				}
				else
					Tools.LogInfo ("GetToken/197","Ret=" + ret.ToString() + err,231,this);
			}
			catch (Exception ex)
			{
				err = "Ret=" + ret.ToString() + err;
				Tools.LogInfo     ("GetToken/198",err,255,this);
				Tools.LogException("GetToken/199",err,ex ,this);
			}
			return ret;
		}

		public override int TokenPayment(Payment payment)
		{
			if ( ! EnabledFor3d(payment.TransactionType) )
				return 590;

			int ret    = 10;
			payRef     = "";
			strResult  = "";
			err        = "";
			resultMsg  = "failed";
			resultCode = "981";

			Tools.LogInfo("TokenPayment/10","Merchant Ref=" + payment.MerchantReference,10,this);

			try
			{
				ret                        = 20;
				StripeConfiguration.ApiKey = payment.ProviderPassword; // Secret key
				ret                        = 24;
				err                        = err + ", customerId="      + Tools.NullToString(payment.CustomerID)
				                                 + ", paymentMethodId=" + Tools.NullToString(payment.PaymentMethodID)
				                                 + ", tokenId="         + Tools.NullToString(payment.CardToken);
				ret                        = 30;
				var paymentIntentOptions   = new PaymentIntentCreateOptions
				{
					Amount              = payment.PaymentAmount,
					Currency            = payment.CurrencyCode.ToLower(), // Must be "usd" not "USD"
					StatementDescriptor = payment.PaymentDescriptionLeft(22),
					Customer            = payment.CustomerID,
					PaymentMethod       = payment.PaymentMethodID,
					Description         = payment.MerchantReference,
					ConfirmationMethod  = "manual"
//					SetupFutureUsage    = "off_session",
//					Confirm             = true,
//					PaymentMethodData   = new PaymentIntentPaymentMethodDataOptions
//					{
//						Type = "card"
//					},
				};
				ret                      = 40;
				var paymentIntentService = new PaymentIntentService();
				var paymentIntent        = paymentIntentService.Create(paymentIntentOptions);	
				err                      = err + ", paymentIntentId="+Tools.NullToString(paymentIntent.Id);

				ret                  = 50;
				var confirmOptions   = new PaymentIntentConfirmOptions
				{
					PaymentMethod     = payment.PaymentMethodID
				};
				ret                  = 60;
				var paymentConfirm   = paymentIntentService.Confirm(paymentIntent.Id,confirmOptions);
				payRef               = paymentConfirm.Id;
				err                  = err + ", paymentConfirmId="+Tools.NullToString(payRef);

				ret                  = 70;
				strResult            = paymentConfirm.StripeResponse.Content;
				resultMsg            = Tools.JSONValue(strResult,"status");
				resultCode           = paymentConfirm.StripeResponse.ToString();
				int k                = resultCode.ToUpper().IndexOf(" STATUS=");
				ret                  = 80;
				err                  = err + ", StripeResponse="+Tools.NullToString(resultCode);

				if ( k > 0 )
				{
					resultCode = resultCode.Substring(k+8).Trim();
					k          = resultCode.IndexOf(" ");
					if ( k > 0 )
						resultCode = resultCode.Substring(0,k);
				}
				else
					resultCode = "989";

				ret                  = 120;
				err                  = err + ", strResult=" +Tools.NullToString(strResult)
				                           + ", resultCode="+Tools.NullToString(resultCode);
				paymentIntentService = null;
				paymentIntent        = null;
				confirmOptions       = null;
				paymentConfirm       = null;

				if ( resultCode.StartsWith("2") && payRef.Length > 0 )
				{
					ret = 0;
					Tools.LogInfo ("TokenPayment/189","Ret=0"+err,255,this);
				}
				else
					Tools.LogInfo ("TokenPayment/197","Ret=" + ret.ToString() + err,231,this);
			}
			catch (Exception ex)
			{
				err = "Ret=" + ret.ToString() + err;
				Tools.LogInfo     ("TokenPayment/198",err,255,this);
				Tools.LogException("TokenPayment/199",err,ex ,this);
			}
			return ret;
		}

		public override int CardPayment(Payment payment)
		{
			int ret = 10;

			try
			{
				Tools.LogInfo("CardPayment/10","Merchant Ref=" + payment.MerchantReference,10,this);
			}
			catch (Exception ex)
			{
				Tools.LogInfo     ("CardPayment/198","Ret="+ret.ToString(),255,this);
				Tools.LogException("CardPayment1/99","Ret="+ret.ToString(),ex ,this);
			}
			return ret;
		}

		private int TestService(byte live=0)
      {
			try
			{
			}
			catch (Exception ex)
			{
				Tools.LogException("TestService/199","",ex,this);
			}
			return 99040;
		}
		public override int ThreeDSecureCheck(string transID)
		{
//	Return
//	   0     : Payment succeeded
//	   1-999 : Payment processed but declined or rejected
// 1001-    : Internal error

			int    ret = 10010;
			resultCode = "XX";
			resultMsg  = "Internal failure";

			try
			{
				ret                        = 10020;
				StripeConfiguration.ApiKey = Tools.ProviderCredentials("Stripe","SecretKey");
				ret                        = 10030;
				var paymentIntentService   = new PaymentIntentService();
				ret                        = 10040;
			//	var paymentIntent          = paymentIntentService.Get(clientSecretId);
				var paymentIntent          = paymentIntentService.Get(transID);
				ret                        = 10050;
				resultCode                 = paymentIntent.Status;
				ret                        = 10060;
				if ( resultCode.ToUpper() == "SUCCEEDED" )
				{
					resultMsg = "Payment successful";
					return 0;
				}
				resultMsg = "Payment NOT successful; further action required";
				return 37;
			}
			catch (Exception ex)
			{
				Tools.LogInfo     ("ThreeDSecureCheck/198","Ret="+ret.ToString(),222,this);
				Tools.LogException("ThreeDSecureCheck/199","Ret="+ret.ToString(), ex,this);
			}
			return ret;
		}

		public override int ThreeDSecurePayment(Payment payment,Uri postBackURL,string languageCode="",string languageDialectCode="")
		{
			int    ret = 10;
			string url = "";

			try
			{
				StripeConfiguration.ApiKey = payment.ProviderPassword; // Secret key

				if ( postBackURL == null )
					url = Tools.ConfigValue("SystemURL");
				else
					url = postBackURL.GetLeftPart(UriPartial.Authority);
				if ( ! url.EndsWith("/") )
					url = url + "/";
				d3Form = "";
				ret    = 20;
				url    = url + "RegisterThreeD.aspx?ProviderCode="+bureauCode
				             +                    "&TransRef="+Tools.XMLSafe(payment.MerchantReference);

				ret                      = 50;
				var paymentMethodOptions = new PaymentMethodCreateOptions
				{
					Type = "card",
					Card = new PaymentMethodCardOptions
					{
						Number   = payment.CardNumber,
						ExpMonth = payment.CardExpiryMonth,
						ExpYear  = payment.CardExpiryYear,
						Cvc      = payment.CardCVV
					}
				};
				ret                      = 60;
				var paymentMethodService = new PaymentMethodService();
				var paymentMethod        = paymentMethodService.Create(paymentMethodOptions);
				err                      = err + ", paymentMethodId="+Tools.NullToString(paymentMethod.Id);

				ret                 = 70;
				var customerOptions = new CustomerCreateOptions
				{
					Name          = payment.CardName, // (payment.FirstName + " " + payment.LastName).Trim(),
					Email         = payment.EMail,
					Phone         = payment.PhoneCell,
					PaymentMethod = paymentMethod.Id
				};
				ret                  = 80;
				var customerService  = new CustomerService();
				var customer         = customerService.Create(customerOptions);
				err                  = err + ", customerId="+Tools.NullToString(customer.Id);

//				if ( payment.PaymentDescription.Length < 1 )
//					payment.PaymentDescription = "CareAssist";
//				else if ( payment.PaymentDescription.Length > 22 )
//					payment.PaymentDescription = payment.PaymentDescription.Substring(0,22);

//	Stripe needs a minimum payment of 50 US cents
				var paymentIntentOptions = new PaymentIntentCreateOptions
				{
					Amount                = 050,   // payment.PaymentAmount,
					Currency              = "usd", // payment.CurrencyCode.ToLower(), // Must be "usd" not "USD"
					StatementDescriptor   = payment.PaymentDescriptionLeft(22),
					Customer              = customer.Id,
					PaymentMethod         = paymentMethod.Id,
					Description           = payment.MerchantReference,
					ConfirmationMethod    = "manual"
				};
				ret                      = 40;
				var paymentIntentService = new PaymentIntentService();
				var paymentIntent        = paymentIntentService.Create(paymentIntentOptions);	
				err                      = err + ", paymentIntentId="+Tools.NullToString(paymentIntent.Id);

				ret                = 50;
				var confirmOptions = new PaymentIntentConfirmOptions
				{
					PaymentMethod   = paymentMethod.Id,
					ReturnUrl       = url
				};
				ret                = 60;
				var paymentConfirm = paymentIntentService.Confirm(paymentIntent.Id,confirmOptions);
				payRef             = paymentConfirm.Id;
				err                = err + ", paymentConfirmId="+Tools.NullToString(payRef);

				ret                = 70;
				strResult          = paymentConfirm.StripeResponse.Content;
				d3Form             = Tools.JSONValue(strResult,"url","next_action");
				d3Form             = Tools.JSONRaw(d3Form);
				resultMsg          = Tools.JSONValue(strResult,"status");
				ret                = 80;
				resultCode         = paymentConfirm.StripeResponse.ToString();
				int k              = resultCode.ToUpper().IndexOf(" STATUS=");
				err                = err + ", StripeResponse="+Tools.NullToString(resultCode);
				ret                = 90;

				Tools.LogInfo("ThreeDSecurePayment/60","strResult="+strResult,221,this);

				string sql = "exec sp_WP_PaymentRegister3DSecA @ContractCode="    + Tools.DBString(payment.MerchantReference)
				           +                                 ",@ReferenceNumber=" + Tools.DBString(payRef)
				           +                                 ",@Status='77'"; // Means payment pending
				if ( languageCode.Length > 0 )
					sql = sql + ",@LanguageCode="        + Tools.DBString(languageCode);
				if ( languageDialectCode.Length > 0 )
					sql = sql + ",@LanguageDialectCode=" + Tools.DBString(languageDialectCode);
				using (MiscList mList = new MiscList())
					mList.ExecQuery(sql,0,"",false,true);

				Tools.LogInfo("ThreeDSecurePayment/80","PayRef=" + payRef + "; SQL=" + sql + "; " + d3Form,10,this);
				return 0;
			}
			catch (Exception ex)
			{
				Tools.LogException("ThreeDSecurePayment/99","Ret="+ret.ToString(),ex,this);
			}
			return ret;
		}

		public TransactionStripe() : base()
		{
			base.LoadBureauDetails(Constants.PaymentProvider.Stripe);
			err = "";
		}
	}
}
