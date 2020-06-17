using System;

namespace PCIBusiness
{
	public static class Constants
	{

	//	General stand-alone constants
	//	-----------------------------
		public static DateTime C_NULLDATE()
		{
			return System.Convert.ToDateTime("1799/12/31");
		}
		public static string C_HTMLBREAK()
		{
			return "<br />";
		}
//		public static string C_TEXTBREAK()
//		{
//			return Environment.NewLine; // "\n";
//		}
		public static short C_MAXSQLROWS()
		{
			return 1000;
		}
		public static short C_MAXPAYMENTROWS()
		{
			return 50;
		}
		public enum DBColumnStatus : byte
		{
			InvalidColumn = 1,
			EOF           = 2,
			ValueIsNull   = 3,
			ColumnOK      = 4
		}

		public enum PaymentStatus : byte
		{
			WaitingToPay     =   1,
			FailedTryAgain   =  21,
			FailedDoNotRetry =  22,
			BusyProcessing   =  51,
			Successful       = 101
		}

		public enum PaymentProvider : int
		{
			MyGate     =  2,
			T24        =  6,
			Ikajo      = 15,
			PayU       = 16,
			PayGate    = 17,
			PayGenius  = 18,
			Ecentric   = 19,
			eNETS      = 20,
			Peach      = 21

//	Not implemented yet
//			DinersClub = 22
//			PayFast    = 23
		}

		public enum CreditCardType : byte
		{
			Visa            = 1,
			MasterCard      = 2,
			AmericanExpress = 3,
			DinersClub      = 4
		}

		public enum PagingMode : byte
		{
			None              = 0,
			AllowScreenPaging = 209,
			DoNotReadNextRow  = 244
		}

		public enum BureauStatus : byte
		{
			Unknown     = 0,
			Development = 1,
			Testing     = 2,
			Live        = 3
		}

		public enum SystemMode : byte
		{
			Development = 1,
			Test        = 2,
			Live        = 3
		}
		public enum ProcessMode : int
		{
			FullUpdate                 =  0, // Live
			UpdateToken                = 10,
			DeleteToken                = 11,
			UpdatePaymentStep1         = 21,
			UpdatePaymentStep2         = 22,
			UpdatePaymentStep1AndStep2 = 23,
			NoUpdate                   = 99
		}
		public enum TransactionType : byte
		{
			GetToken       =  1,
			TokenPayment   =  2,
			CardPayment    =  3,
			DeleteToken    =  4,
			ManualPayment  = 73
		}

//	iTextSharp stuff

		public enum PdfFontSize : int
		{
			HugeHeading      = 40,
			MajorHeading     = 32,
			MinorHeading     = 20,
			SubHeading       = 16,
			TableHeading     = 12,
			TableCell        = 10,
			ParagraphSpacing = 10,
			ParagraphPadding =  5
		}

		public enum PdfAlign : int // These must match iTextSharp.Element.ALIGN_LEFT, etc values
		{
			Left   = 0,
			Right  = 2,
			Centre = 1,
			Middle = 5
		}

		public enum TickerType : int
		{
			IBStockPrices          =  1,
			IBExchangeRates        =  2,
			IBPortfolio            =  3,
			IBOrders               =  4,
			IBExchangeCandles      =  5,
			FinnHubStockPrices     = 21,
			FinnHubStockHistory    = 22,
			FinnHubExchangeRates   = 23,
			FinnHubExchangeCandles = 24 // Not implemented yet
		}

		public enum DataFormat : int
		{
			CSV = 31,
			PDF = 32			
		}

//		public enum PaymentType : byte
//		{
//			Tokens      = 10,
//			CardNumbers = 20,
//			Vault       = 30
//		}
	}
}