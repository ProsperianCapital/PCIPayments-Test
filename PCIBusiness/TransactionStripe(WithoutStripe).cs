using System;
using System.Text;
using System.Net;
using System.IO;

namespace PCIBusiness
{
	public class TransactionStripe : Transaction
	{
		public  bool Successful
		{
			get { return Tools.JSONValue(strResult,"success").ToUpper() == "TRUE"; }
		}

		public override int GetToken(Payment payment)
		{
			return 99010;
		}

		public override int TokenPayment(Payment payment)
		{
			return 99020;
		}

		public override int CardPayment(Payment payment)
		{
			return 99030;
		}

		private int TestService(byte live=0)
      {
			return 99040;;
		}

		public TransactionStripe() : base()
		{
			base.LoadBureauDetails(Constants.PaymentProvider.Stripe);
			xmlResult = null;
		}
	}
}
