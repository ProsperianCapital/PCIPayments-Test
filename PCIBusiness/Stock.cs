using System;

namespace PCIBusiness
{
	public class Stock : BaseData
	{
		private int     stockId;
		private string  symbol;
		private string  securityType;
		private string  currencyCode;
		private string  exchangeCode;
		private string  primaryExchange;
		private string  resolution;
		private long    fromDate;
		private long    toDate;

		private double  price;
		private int     quantity;
		private int     tickType;

		public  int     StockId
		{
			get { return stockId; }
			set { stockId = value; }
		}
		public  int     TickType
		{
			get { return tickType; }
			set { tickType = value; }
		}
		public  double  Price
		{
			get { return price; }
			set { price = value; }
		}
		public  int     Quantity
		{
			get { return quantity; }
			set { quantity = value; }
		}
		public  string  Symbol
		{
			get { return Tools.NullToString(symbol); }
			set { symbol = value.Trim(); }
		}
		public  string  CurrencyCode
		{
			get { return Tools.NullToString(currencyCode).ToUpper(); }
			set { currencyCode = value.Trim(); }
		}
		public  string  SecurityType
		{
			get
			{
				securityType = Tools.NullToString(securityType);
				if ( securityType.Length < 1 )
					securityType = "STK";
				return securityType;
			}
			set { securityType = value.Trim(); }
		}

		public  string  ExchangeCode
		{
			get { return Tools.NullToString(exchangeCode); }
			set { exchangeCode = value.Trim(); }
		}

		public  string  PrimaryExchange
		{
			get { return Tools.NullToString(primaryExchange); }
			set { primaryExchange = value.Trim(); }
		}

		public  string  Resolution
		{
			get { return Tools.NullToString(resolution); }
		}

		public  long    FromDate
		{
			get { return fromDate; }
		}

		public  long    ToDate
		{
			get { return toDate; }
		}

		public int UpdatePrice()
		{
			if ( price > 0 && stockId > 0 && tickType >= 0 )
				try
				{
					sql = "exec sp_Ins_TickerCurrentRaw"
						 + " @StockID  = " + stockId.ToString()
					    + ",@DateTime = " + Tools.DateToSQL(System.DateTime.Now,5)
					    + ",@TickType = " + tickType.ToString()
					    + ",@Value    = " + price.ToString();
					Tools.LogInfo("Stock.UpdatePrice/1",sql,222);
					return ExecuteSQL(null,2);
				}
				catch (Exception ex)
				{
					Tools.LogException("Stock.UpdatePrice/2",sql,ex);
				}
			return 8199;
		}

		public int UpdateQuantity()
		{
			if ( quantity > 0 && stockId > 0 && tickType >= 0 )
				try
				{
					sql = "exec sp_Ins_TickerCurrentRaw"
						 + " @StockID  = " + stockId.ToString()
					    + ",@DateTime = " + Tools.DateToSQL(System.DateTime.Now,5)
					    + ",@TickType = " + tickType.ToString()
					    + ",@Value    = " + quantity.ToString();
					Tools.LogInfo("Stock.UpdateQuantity/1",sql,222);
					return ExecuteSQL(null,2);
				}
				catch (Exception ex)
				{
					Tools.LogException("Stock.UpdateQuantity/2",sql,ex);
				}
			return 9199;
		}

		public override void LoadData(DBConn dbConn)
		{
			dbConn.SourceInfo = "Stock.LoadData";
			stockId           = dbConn.ColLong  ("StockId");
			symbol            = dbConn.ColString("Symbol");
			exchangeCode      = dbConn.ColString("BrokerExchangeCode",0);
			primaryExchange   = dbConn.ColString("PrimaryExchange",0);
			securityType      = dbConn.ColString("SecType",0);
			currencyCode      = dbConn.ColString("CUR",0);
			resolution        = dbConn.ColString("Resolution",0);
			fromDate          = dbConn.ColBig   ("FromDate",0);
			toDate            = dbConn.ColBig   ("ToDate",0);
			price             = 0;
			quantity          = 0;
			tickType          = 0;
		}
	}
}
