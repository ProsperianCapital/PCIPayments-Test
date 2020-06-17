namespace PCIBusiness
{
	public class Stocks : BaseList
	{
		public override BaseData NewItem()
		{
			return new Stock();
		}

		public Stock LoadOne(string stockSymbol)
		{
		//	sql = "exec Blah " + Tools.DBString(bureauCode);
		//	if ( LoadDataFromSQL(1) > 0 )
		//		return (Provider)Item(0);
		//	return null;

			return null;
		}

		public int LoadAll(string secType="")
		{
//			if ( secType.ToUpper() == "CASH" )
//				sql = "select 1 as StockId,'USD' as Symbol,'FH' as BrokerExchangeCode,'CASH' as SecType,'' as CUR"
//				    +     " union select 2,'EUR','FH','CASH',''"
//				    +     " union select 3,'ZAR','FH','CASH',''";
//			else
//				sql = "exec sp_Get_StockListB"
//				    + ( secType.Length > 0 ? " @SecType = " + Tools.DBString(secType) : "" );

			if ( secType.ToUpper() == "STK-HISTORY" )
				sql = "exec sp_Get_StockCandles";
			else
				sql = "exec sp_Get_StockListB"
				    + ( secType.Length > 0 ? " @SecType = " + Tools.DBString(secType) : "" );
			Tools.LogInfo("Stocks.LoadAll",sql,10);
			return LoadDataFromSQL();
		}
	}
}