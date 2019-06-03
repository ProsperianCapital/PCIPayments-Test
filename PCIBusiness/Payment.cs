﻿using System;

namespace PCIBusiness
{
	public class Payment : BaseData
	{
//		private int      paymentCode;
//		private int      paymentAuditCode;
//	...Ecentric fields, but not needed after all
//		private string   authorizationCode;         // This is payment provider's auth code (may not be there)
//		private string   orderNumber;               // This is a Prosperian reference (maybe not unique)

		private string   merchantReference;         // This is Prosperian's unique reference for THIS transaction
		private string   merchantReferenceOriginal; // This is Prosperian's reference for the original token transaction 
		private string   transactionID;             // This is payment provider's transaction reference (may not be there)
		private string   countryCode;
		private string   firstName;
		private string   lastName;
		private string   address1;
		private string   address2;
		private string   postalCode;
		private string   provinceCode;
		private string   regionalId;
		private string   email;
		private string   phoneCell;
		private string   ipAddress;
		private int      paymentAmount;
//		private byte     paymentStatus;
		private string   paymentDescription;
		private string   currencyCode;

		private string   ccNumber;
		private string   ccType;
		private string   ccExpiryMonth;
		private string   ccExpiryYear;
		private string   ccName;
		private string   ccCVV;
		private string   ccPIN;
		private string   ccToken;

		private string   bureauCode;
		private string   providerAccount;
		private string   providerKey;
		private string   providerUserID;
		private string   providerPassword;
		private string   providerURL;

		private byte     paymentMode; // This indicates MANUAL (73) or AUTO
		private string   threeDForm;

//		private Provider    provider;
		private Transaction transaction;


//		Payment Provider stuff

		public string    ProviderAccount
		{
			get 
			{
				if ( Tools.SystemIsLive() )
				{
					if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.eNETS) )
						return "UMID_858445001";
					return "";
				}

				else if ( Tools.NullToString(providerAccount).Length > 0 )
					return providerAccount;
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.PayU) )
					return "2237055";
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.T24) )
					return "567654452";
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.Ikajo) )
					return "6861-finaidhk";
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.MyGate) )
					return "MY014473";
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.eNETS) )
					return "UMID_858445001";

				return "";
			}
		}
		public string    BureauCode
		{
			get { return  Tools.NullToString(bureauCode); }
			set { bureauCode = value.Trim(); }
		}
		public string    ProviderKey
		{
			set { providerKey = value.Trim(); }
			get
			{
				if ( Tools.NullToString(providerKey).Length > 0 )
					return providerKey;

				else if ( Tools.SystemIsLive() )
				{
					if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.eNETS) )
						return "27ededae-4ba3-486a-a243-8da1e4c1a067";
					return "";
				}
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.eNETS) )
					return "27ededae-4ba3-486a-a243-8da1e4c1a067";

				return "";
			}
		}
		public string    ProviderUserID
		{
			get { return  Tools.NullToString(providerUserID); }
		}
		public string    ThreeDForm
		{
			get { return  Tools.NullToString(threeDForm); }
		}
		public string    ProviderPassword
		{
			get
			{
				if ( Tools.NullToString(providerPassword).Length > 0 )
					return providerPassword;

				else if ( Tools.SystemIsLive() )
				{
					if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.eNETS) )
						return "27ededae-4ba3-486a-a243-8da1e4c1a067";
					return "";
				}
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.eNETS) )
					return "70f4046b-542f-41c8-b928-dffabfb0650c";

				return "";
			}
		}
		public string    ProviderURL
		{
			get
			{
				if ( Tools.NullToString(providerURL).Length > 0 )
					return providerURL;

				else if ( Tools.SystemIsLive() )
				{
					if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.eNETS) )
						return "https://uat-api.nets.com.sg:9065/GW2/TxnReqListener";
					return "";
				}
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.T24) )
					return "https://payment.ccp.boarding.transact24.com/PaymentCard";
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.MyGate) )
					return "https://www.mygate.co.za/Collections/1x0x0/pinManagement.cfc?wsdl";
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.PayGate) )
					return "https://secure.paygate.co.za/payhost/process.trans";
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.PayGenius) )
					return "https://developer.paygenius.co.za";
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.eNETS) )
					return "https://uat-api.nets.com.sg:9065/GW2/TxnReqListener";

				return "";
			}
		}

//		public string    MerchantUserId
//		{
//			get { return  Tools.NullToString(merchantUserId); }
//		}
//		public string    Address(byte line)
//		{
//			if ( address == null || line < 1 || ( line > address.Length && line < 255 ) )
//				return "";
//			if ( line == 255 ) // Last non-blank address
//			{
//				for ( int k = address.Length ; k > 0 ; k-- )
//					if ( address[k-1].Length > 0 )
//						return address[k-1];
//				return "";
//			}
//			return address[line-1];
////			while ( line < address.Length )
////			{
////				if ( address[line].Length > 0 )
////					return address[line];
////				line++;
////			}
////			return "";
//		}

//		Customer stuff
		public string    FirstName
		{
			get { return  Tools.NullToString(firstName); }
		}
		public string    LastName
		{
			get { return  Tools.NullToString(lastName); }
		}
		public string    EMail
		{
			get { return  Tools.NullToString(email); }
		}
		public string    PhoneCell
		{
			get { return  Tools.NullToString(phoneCell); }
		}
		public string    RegionalId
		{
			get { return  Tools.NullToString(regionalId); }
		}
		public string    Address1
		{
			get { return  Tools.NullToString(address1); }
		}
		public string    Address2
		{
			get { return  Tools.NullToString(address2); }
		}
		public string    PostalCode
		{
			get { return  Tools.NullToString(postalCode); }
		}
		public string    ProvinceCode
		{
			get { return  Tools.NullToString(provinceCode); }
		}
		public string    State // Province, but only if USA
		{
			get
			{
				string x = Tools.NullToString(countryCode).ToUpper();
				if ( x.Length > 1 && x.StartsWith("US") )
					return ProvinceCode;
				return "";
			}
		}
		public string    CountryCode
		{
			get { return  Tools.NullToString(countryCode); }
		}

//		payment stuff
//		public string    OrderNumber
//		{
//			get { return  Tools.NullToString(orderNumber); }
//		}
		public string    MerchantReference
		{
			get { return  Tools.NullToString(merchantReference); }
			set { merchantReference = value.Trim(); }
		}
		public string    MerchantReferenceOriginal
		{
			get { return  Tools.NullToString(merchantReferenceOriginal); }
		}
		public string    TransactionID
		{
			get
			{
				if ( Tools.NullToString(transactionID).Length < 1 )
					transactionID = (Guid.NewGuid()).ToString();
				return Tools.NullToString(transactionID);
			}
		}
//		public string    AuthorizationCode
//		{
//			get { return  Tools.NullToString(authorizationCode); }
//		}
		public string    CurrencyCode
		{
			get { return  Tools.NullToString(currencyCode); }
			set { currencyCode = value.Trim().ToUpper(); }
		}
		public string    IPAddress
		{
			get { return  Tools.NullToString(ipAddress); }
		}
		public string    PaymentDescription
		{
			get { return  Tools.NullToString(paymentDescription); }
		}
		public  int      PaymentAmount
		{
//	Cents
			get { return  paymentAmount; }
			set
			{
				paymentAmount = value;
				if ( paymentAmount < 1 )
					paymentAmount = 0;
			}
		}
		public  string   PaymentAmountDecimal
		{
//	Rands
			get
			{
				if ( paymentAmount < 1 )
					return "0.00";
				string amt = paymentAmount.ToString();
				if ( amt.Length == 1 )
					return "0.0" + amt;
				if ( amt.Length == 2 )
					return "0." + amt;
				return amt.Substring(0,amt.Length-2) + "." + amt.Substring(amt.Length-2);
			}
		}
//		public  byte     PaymentStatus
//		{
//			get { return  paymentStatus; }
//		}

//		Card stuff
		public  string   CardToken
		{
			get { return  Tools.NullToString(ccToken); }
		}
		public  string   CardPIN
		{
			get { return  Tools.NullToString(ccPIN); }
		}
		public  string   CardType
		{
			get { return  Tools.NullToString(ccType); }
		}
		public  string   CardNumber
		{
			get { return  Tools.NullToString(ccNumber); }
			set { ccNumber = value.Trim(); }
		}
		public  byte     CardExpiryMonth
		{
			get
			{
				try
				{
					byte x = Convert.ToByte(ccExpiryMonth);
					if ( x > 0 && x < 13 )
						return x;
				}
				catch
				{ }
				return 0;
			}
		}
		public  string   CardExpiryMM // Pad with zeroes, eg. 07
		{
			get { return  Tools.NullToString(ccExpiryMonth).PadLeft(2,'0'); }
			set 
			{
				ccExpiryMonth = value.Trim();
				if ( Tools.StringToInt(ccExpiryMonth) < 1 || Tools.StringToInt(ccExpiryMonth) > 12 || ccExpiryMonth.Length > 2 )
					ccExpiryMonth = "";
			}
		}
		public  string   CardExpiryYY
		{
			get
			{
				ccExpiryYear = Tools.NullToString(ccExpiryYear);
				if ( ccExpiryYear.Length == 4 )
					return ccExpiryYear.Substring(2,2);
				if ( ccExpiryYear.Length == 2 )
					return ccExpiryYear;
				return "";
			}
		}
		public  string   CardExpiryYYYY
		{
			get
			{
				ccExpiryYear = Tools.NullToString(ccExpiryYear);
				if ( ccExpiryYear.Length == 4 )
					return ccExpiryYear;
				if ( ccExpiryYear.Length == 2 )
					return "20" + ccExpiryYear;
				return "";
			}
			set 
			{
				ccExpiryYear = value.Trim();
				if ( Tools.StringToInt(ccExpiryYear) < System.DateTime.Now.Year || Tools.StringToInt(ccExpiryYear) > 2999 || ccExpiryYear.Length != 4 )
					ccExpiryYear = "";
			}
		}
		public  string   CardName
		{
			get { return  Tools.NullToString(ccName); }
			set { ccName = value.Trim(); }
		}
		public  string   CardCVV
		{
			get { return  Tools.NullToString(ccCVV); }
			set { ccCVV = value.Trim(); }
		}
		public  byte     PaymentMode
		{
			get { return  paymentMode; }
			set { paymentMode = value; }
		}
//		public Provider  Provider
//		{
//			get { return  provider; }
//		}

		public int GetToken()
		{
			int processMode = Tools.StringToInt(Tools.ConfigValue("ProcessMode"));
			int ret         = 64020;
			sql             = "";
			Tools.LogInfo("Payment.GetToken/10","Merchant Ref=" + merchantReference,10);

			if ( transaction == null || transaction.BureauCode != bureauCode )
			{
				if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.PayU) )
					transaction = new TransactionPayU();
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.Ikajo) )
					transaction = new TransactionIkajo();
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.T24) )
					transaction = new TransactionT24();
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.MyGate) )
					transaction = new TransactionMyGate();
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.PayGate) )
					transaction = new TransactionPayGate();
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.PayGenius) )
					transaction = new TransactionPayGenius();
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.Ecentric) )
					transaction = new TransactionEcentric();
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.eNETS) )
					return ret; // eNETS does not have tokenization ... yet
				else
					return ret;
			}
			ret = transaction.GetToken(this);

			if ( processMode == (int)Constants.ProcessMode.FullUpdate ||
			     processMode == (int)Constants.ProcessMode.UpdateToken )
			{
				sql = "exec sp_Upd_CardTokenVault @MerchantReference = "           + Tools.DBString(merchantReference) // nvarchar(20),
				                              + ",@PaymentBureauCode = "           + Tools.DBString(bureauCode)        // char(3),
			                                 + ",@PaymentBureauToken = "          + Tools.DBString(transaction.PaymentToken)
			                                 + ",@BureauSubmissionSoap = "        + Tools.DBString(transaction.XMLSent,3)
			                                 + ",@BureauResultSoap = "            + Tools.DBString(transaction.XMLResult,3)
			                                 + ",@TransactionStatusCode = "       + Tools.DBString(transaction.ResultCode)
		                                    + ",@CardTokenisationStatusCode = '" + ( ret == 0 ? "007'" : "001'" );
				Tools.LogInfo("Payment.GetToken/20","SQL=" + sql,20);
				int k = ExecuteSQLUpdate();
			}
			Tools.LogInfo("Payment.GetToken/90","Ret=" + ret.ToString(),20);
			return ret;
		}

		public int ProcessPayment()
		{
			returnMessage   = "Invalid payment provider";
			int processMode = Tools.StringToInt(Tools.ConfigValue("ProcessMode"));
			int ret         = 37020;
			int k;
			Tools.LogInfo("Payment.ProcessPayment/10","Merchant Ref=" + merchantReference,10);

			if ( transaction == null || transaction.BureauCode != bureauCode )
			{
				if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.PayU) )
					transaction = new TransactionPayU();
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.Ikajo) )
					transaction = new TransactionIkajo();
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.T24) )
					transaction = new TransactionT24();
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.MyGate) )
					transaction = new TransactionMyGate();
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.PayGate) )
					transaction = new TransactionPayGate();
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.PayGenius) )
					transaction = new TransactionPayGenius();
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.Ecentric) )
					transaction = new TransactionEcentric();
				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.eNETS) )
					transaction = new TransactionENets();
//				else if ( bureauCode == Tools.BureauCode(Constants.PaymentProvider.PayFast) )
//					transaction = new TransactionPayFast();
				else
					return ret;
			}

			if ( paymentMode == (byte)Constants.TransactionType.ManualPayment ) // Manual card payment
				Tools.LogInfo("Payment.ProcessPayment/20","Manual card payment",20);

			else if ( processMode == (int)Constants.ProcessMode.FullUpdate         ||
			          processMode == (int)Constants.ProcessMode.UpdatePaymentStep1 ||
			          processMode == (int)Constants.ProcessMode.UpdatePaymentStep1AndStep2 )
			{
				sql = "exec sp_Upd_CardPayment @MerchantReference = " + Tools.DBString(merchantReference)
			                              + ",@TransactionStatusCode = '77'";
				Tools.LogInfo("Payment.ProcessPayment/30","SQL 1=" + sql,20);
				k   = ExecuteSQLUpdate();
				Tools.LogInfo("Payment.ProcessPayment/40","SQL 1 complete",20);
			}
			else
				Tools.LogInfo("Payment.ProcessPayment/50","SQL 1 skipped",20);

			ret           = transaction.ProcessPayment(this);
			threeDForm    = "";
			returnMessage = transaction.ResultMessage;

			if ( paymentMode == (byte)Constants.TransactionType.ManualPayment ) // Manual card payment
			{
				Tools.LogInfo("Payment.ProcessPayment/60","Manual card payment, ret=" + ret.ToString() + ", acsUrl=" + transaction.ThreeDacsUrl,199);
				if ( transaction.ThreeDRequired )
					threeDForm = "<html><body onload='document.forms[\"frm3D\"].submit()'>"
					           + "<form name='frm3D' method='POST'   action='" + transaction.ThreeDacsUrl + "'>"
					           + "<input type='hidden' name='PaReq'   value='" + transaction.ThreeDpaReq + "' />"
					           + "<input type='hidden' name='TermUrl' value='" + transaction.ThreeDtermUrl + "' />"
					           + "<input type='hidden' name='MD'      value='" + transaction.ThreeDmd + "' />"
					           + "</form></body></html>";
			}
			else if ( processMode == (int)Constants.ProcessMode.FullUpdate         ||
			          processMode == (int)Constants.ProcessMode.UpdatePaymentStep2 ||
			          processMode == (int)Constants.ProcessMode.UpdatePaymentStep1AndStep2 )
			{
				sql = "exec sp_Upd_CardPayment @MerchantReference = " + Tools.DBString(merchantReference)
			                              + ",@TransactionStatusCode = " + Tools.DBString(transaction.ResultCode);
				Tools.LogInfo("Payment.ProcessPayment/70","SQL 2=" + sql,20);
				k   = ExecuteSQLUpdate();
				Tools.LogInfo("Payment.ProcessPayment/80","SQL 2 complete",20);
			}
			else
				Tools.LogInfo("Payment.ProcessPayment/90","SQL 2 skipped",20);

			return ret;
		}

		public override void LoadData(DBConn dbConn)
		{
		//	Payment Provider
			providerKey      = dbConn.ColString ("Safekey");
			providerURL      = dbConn.ColString ("url");
			providerAccount  = dbConn.ColString ("MerchantAccount",0);
			providerUserID   = dbConn.ColString ("MerchantUserId",0);
			providerPassword = dbConn.ColString ("MerchantUserPassword",0);

		//	Customer
			if ( dbConn.ColStatus("lastName") == Constants.DBColumnStatus.ColumnOK )
			{
				firstName     = dbConn.ColUniCode("firstName");
				lastName      = dbConn.ColUniCode("lastName");
				email         = dbConn.ColString ("email");
				phoneCell     = dbConn.ColString ("mobile");
				regionalId    = dbConn.ColString ("regionalId");
				address1      = dbConn.ColString ("address1");
				address2      = dbConn.ColString ("city");
				postalCode    = dbConn.ColString ("zip_code");
				provinceCode  = dbConn.ColString ("State");
				countryCode   = dbConn.ColString ("CountryCode");
				ipAddress     = dbConn.ColString ("IPAddress");
			}

		//	Payment
			merchantReference         = dbConn.ColString("merchantReference");
			merchantReferenceOriginal = dbConn.ColString("merchantReferenceOriginal",0); // Only really for Ikajo, don't log error
			paymentAmount             = dbConn.ColLong  ("amountInCents");
			currencyCode              = dbConn.ColString("currencyCode");
			paymentDescription        = dbConn.ColString("description",0);

		//	Card/token/transaction details, not always present, don't log errors
			ccName        = dbConn.ColString("nameOnCard",0);
			ccNumber      = dbConn.ColString("cardNumber",0);
			ccExpiryMonth = dbConn.ColString("cardExpiryMonth",0);
			ccExpiryYear  = dbConn.ColString("cardExpiryYear",0);
			ccType        = dbConn.ColString("cardType",0);
			ccCVV         = dbConn.ColString("cvv",0);
			ccToken       = dbConn.ColString("token",0);
			ccPIN         = dbConn.ColString("PIN",0);
			transactionID = dbConn.ColString("TransactionID",0);
		}

		public override void CleanUp()
		{
			transaction = null;
			paymentMode = 0;
		}

		public Payment(string bureau) : base()
		{
			bureauCode  = Tools.NullToString(bureau);
			paymentMode = 0;
			threeDForm  = "";
		}
	}
}
