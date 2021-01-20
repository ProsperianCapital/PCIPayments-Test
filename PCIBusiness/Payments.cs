using System;

namespace PCIBusiness
{
	public class Payments : BaseList
	{
		private string    bureauCode;
		private int       success;
		private int       fail;
		private int       err;

		public override BaseData NewItem()
		{
			return new   Payment("");
		}

		public int      CountSucceeded
		{
			get { return success; }
		}

		public int      CountFailed
		{
			get { return fail; }
		}
		public Provider Summary(string bureau)
		{
			int      tok        = 0;
			int      pay        = 0;
			Provider provider   = new Provider();
			provider.BureauCode = bureau;

			if ( provider.PaymentType == (byte)Constants.TransactionType.TokenPayment )
				try
				{
		  			sql = "exec sp_Get_CardToToken " + Tools.DBString(bureau) + "," + Constants.MaxRowsPayment.ToString();
					err = ExecuteSQL(null,false,false);
					if ( err > 0 )
						Tools.LogException("Summary/10",sql + " failed, return code " + err.ToString(),null,this);
					else
						while ( ! dbConn.EOF && tok < Constants.MaxRowsPayment )
						{
							if ( pay == 0 && tok == 0 )
								provider.LoadData(dbConn);
							tok++;
							dbConn.NextRow();
						}
					provider.CardsToBeTokenized = tok;

					sql = "exec sp_Get_TokenPayment " + Tools.DBString(bureau) + "," + Constants.MaxRowsPayment.ToString();
//					sql = "exec sp_Get_CardPayment "  + Tools.DBString(bureau) + "," + Constants.MaxRowsPayment.ToString();
					err = ExecuteSQL(null,false,false);
					if ( err > 0 )
						Tools.LogException("Summary/20",sql + " failed, return code " + err.ToString(),null,this);
					else
						while ( ! dbConn.EOF && pay < Constants.MaxRowsPayment )
						{
							if ( pay == 0 && tok == 0 )
								provider.LoadData(dbConn);
							pay++;
							dbConn.NextRow();
						}
					provider.PaymentsToBeProcessed = pay;
				}
				catch (Exception ex)
				{
					Tools.LogException("Summary/30","",ex,this);
				}
				finally
				{
					Tools.CloseDB(ref dbConn);
				}

			else if ( provider.PaymentType == (byte)Constants.TransactionType.CardPayment )
				try
				{
					provider.CardsToBeTokenized = 0;

					sql = "exec sp_Get_CardPayment " + Tools.DBString(bureau) + "," + Constants.MaxRowsPayment.ToString();
					err = ExecuteSQL(null,false,false);
					if ( err > 0 )
						Tools.LogException("Summary/40",sql + " failed, return code " + err.ToString(),null,this);
					else
						while ( ! dbConn.EOF && pay < Constants.MaxRowsPayment )
						{
							if ( pay == 0 )
								provider.LoadData(dbConn);
							pay++;
							dbConn.NextRow();
						}
					provider.PaymentsToBeProcessed = pay;
				}
				catch (Exception ex)
				{
					Tools.LogException("Summary/50","",ex,this);
				}
				finally
				{
					Tools.CloseDB(ref dbConn);
				}

			return provider;
		}

		public int ProcessCards(string bureau,byte transactionType=0,int rowsToProcess=0,string bureaCodeTokenize="")
		{
			int    maxRows  = Tools.StringToInt(Tools.ConfigValue("MaximumRows"));
			int    iter     = 0;
			int    rowsDone = 0;
			string desc     = "";

			bureauCode        = Tools.NullToString(bureau);
			bureaCodeTokenize = Tools.NullToString(bureau);
			success           = 0;
			fail              = 0;
			maxRows           = ( maxRows < 1 ? Constants.MaxRowsPayment : maxRows );

			if ( bureauCode.Length < 1 )
				return 0;

			else if ( transactionType == (byte)Constants.TransactionType.GetToken )
    		{
				sql  = "exec sp_Get_CardToToken " + Tools.DBString(bureauCode);
				desc = "Get Token";
			}
			else if ( transactionType == (byte)Constants.TransactionType.TokenPayment )
    		{
				sql  = "exec sp_Get_TokenPayment " + Tools.DBString(bureauCode);
				desc = "Token Payment";
			}
			else if ( transactionType == (byte)Constants.TransactionType.CardPayment )
    		{
				sql  = "exec sp_Get_CardPayment " + Tools.DBString(bureauCode);
				desc = "(Direct) Card Payment)";
			}
			else if ( transactionType == (byte)Constants.TransactionType.CardPaymentThirdParty )
    		{
				sql  = "exec sp_Get_CardPayment " + Tools.DBString(bureauCode);
				desc = "(TokenEx) Card Payment";
			}
			else if ( transactionType == (byte)Constants.TransactionType.DeleteToken )
    		{
				sql  = "exec sp_Get_TokenToDelete " + Tools.DBString(bureauCode);
				desc = "Delete Token";
			}
			else if ( transactionType == (byte)Constants.TransactionType.GetCardFromToken )
    		{
				sql  = "exec sp_Get_TokenToDecrypt " + Tools.DBString(bureauCode);
				desc = "Get Card from Token";
			}
			else
				return 0;

			if ( rowsToProcess < 1 )
				rowsToProcess = 0;

			if ( maxRows > 0 && rowsToProcess > 0 )
				sql = sql + "," + Math.Min(maxRows,rowsToProcess).ToString();
			else if ( maxRows > 0 )
				sql = sql + "," + maxRows.ToString();
			else if ( rowsToProcess > 0 )
				sql = sql + "," + rowsToProcess.ToString();
			else
				sql = sql + "," + Constants.MaxRowsPayment.ToString();

			Tools.LogInfo("ProcessCards/15",desc + ", MaxRows=" + maxRows.ToString()+", RowsToProcess=" + rowsToProcess.ToString()+", BureauCode="+bureauCode+", SQL="+sql,199,this);

			while ( rowsToProcess < 1 || rowsToProcess > success + fail )
				try
				{
					if ( LoadDataFromSQL(maxRows,"Payments.ProcessCards ("+desc+", "+bureauCode+")") < 1 )
						break;
					Tools.CloseDB(ref dbConn);
					rowsDone = 0;
					iter++;
					foreach (Payment payment in objList)
					{
						payment.BureauCode      = bureauCode;
						payment.TransactionType = transactionType;
						if ( transactionType == (byte)Constants.TransactionType.GetToken )
							err = payment.GetToken();
						else if ( transactionType == (byte)Constants.TransactionType.TokenPayment )
							err = payment.ProcessPayment();
						else if ( transactionType == (byte)Constants.TransactionType.CardPayment )
							err = payment.ProcessPayment();
						else if ( transactionType == (byte)Constants.TransactionType.CardPaymentThirdParty )
						{
							payment.TokenizerCode = bureaCodeTokenize;
							err                   = payment.ProcessPayment(); 
						}
						else if ( transactionType == (byte)Constants.TransactionType.DeleteToken )
							err = payment.DeleteToken();
						else if ( transactionType == (byte)Constants.TransactionType.GetCardFromToken )
							err = payment.Detokenize();
						if ( err == 0 )
							success++;
						else
							fail++;
						rowsDone++;
						if ( rowsToProcess > 0 && rowsToProcess <= success + fail )
							break;
					}
					Tools.LogInfo("ProcessCards/40","Iteration " + iter.ToString() + " (" + rowsDone.ToString() + " " + desc + "s)",199,this);
//	In case of a runaway loop where failures are not rectified ...
					if ( fail > 99 && success == 0 )
						break;
					if ( fail > 999 )
						break;
				}
				catch (Exception ex)
				{
					Tools.LogException("ProcessCards/50","Iteration " + iter.ToString() + ", " + desc + " " + (success+fail).ToString(),ex,this);
				}
				finally
				{
					Tools.CloseDB(ref dbConn);
					Tools.LogInfo("ProcessCards/90","Finished (" + success.ToString() + " " + desc + "s succeeded, " + fail.ToString() + " "+ desc + "s failed)",199,this);
				}

			return success+fail;
		}
	}
}
