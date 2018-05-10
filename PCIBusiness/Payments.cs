using System;
using System.Text;

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
			int      k;
			Provider provider   = new Provider();
			provider.BureauCode = bureau;

			try
			{
				k   = 0;
    			sql = "exec sp_Get_CardToToken " + Tools.DBString(bureau) + ",10";
				err = ExecuteSQL(null,false,false);
				if ( err > 0 )
					Tools.LogException("Payments.Summary/10",sql + " failed, return code " + err.ToString());
				else
					while ( ! dbConn.EOF )
					{
						if ( k == 0 )
							provider.LoadData(dbConn);
						k++;
						dbConn.NextRow();
					}
				provider.CardsToBeTokenized = k;

				k   = 0;
    			sql = "exec sp_Get_CardPayment " + Tools.DBString(bureau) + ",10";
				err = ExecuteSQL(null,false,false);
				if ( err > 0 )
					Tools.LogException("Payments.Summary/20",sql + " failed, return code " + err.ToString());
				else
					while ( ! dbConn.EOF )
					{
						if ( k == 0 )
							provider.LoadData(dbConn);
						k++;
						dbConn.NextRow();
					}
				provider.PaymentsToBeProcessed = k;
			}
			catch (Exception ex)
			{
				Tools.LogException("Payments.Summary/30","",ex);
			}
			finally
			{
				Tools.CloseDB(ref dbConn);
			}
			return provider;
		}

		public int ProcessCards(string bureau,byte mode=0,int rowsToProcess=0)
		{
			int    maxRows = Tools.StringToInt(Tools.ConfigValue("MaximumRows"));
			int    iter    = 0;
			string desc    = "";

			bureauCode = Tools.NullToString(bureau);
			success    = 0;
			fail       = 0;
			maxRows    = ( maxRows < 1 || maxRows > 25 ? 25 : maxRows );

			if ( bureauCode.Length < 1 )
				return 10;
			else if ( mode == 1 )
    		{
				sql  = "exec sp_Get_CardToToken " + Tools.DBString(bureauCode);
				desc = "Token";
			}
			else if ( mode == 2 )
    		{
				sql  = "exec sp_Get_CardPayment "  + Tools.DBString(bureauCode);
				desc = "Payment";
			}
			else
				return 20;

			if ( rowsToProcess < 1 )
				rowsToProcess = 0;

			if ( maxRows > 0 && rowsToProcess > 0 )
				sql = sql + "," + Math.Min(maxRows,rowsToProcess).ToString();
			else if ( maxRows > 0 )
				sql = sql + "," + maxRows.ToString();
			else if ( rowsToProcess > 0 )
				sql = sql + "," + rowsToProcess.ToString();
			else
				sql = sql + ",25";

			Tools.LogInfo("Payments.ProcessCards/10","Mode="+mode.ToString()+", MaxRows=" + maxRows.ToString()+", RowsToProcess=" + rowsToProcess.ToString()+", BureauCode="+bureauCode+", SQL="+sql,220);

			try
			{
				bool allDone = false;
				while ( ! allDone )
				{
					err = ExecuteSQL(null,false,false);
					if ( err > 0 )
					{
						Tools.LogException("Payments.ProcessCards/20",sql + " failed, return code " + err.ToString());
						break;
					}
					using (PCIBusiness.Payment payment = new PCIBusiness.Payment(bureauCode))
					{
						int rowsDone = 0;
						iter++;
						while ( ! dbConn.EOF && ! allDone && rowsDone < maxRows ) // Max per iteration
						{
							payment.LoadData(dbConn);
							if ( mode == 1 )
								err = payment.GetToken();
							else
								err = payment.ProcessPayment();
							dbConn.NextRow();
							if ( err == 0 )
								success++;
							else
								fail++;
							rowsDone++;
							if ( rowsToProcess > 0 && success + fail >= rowsToProcess )
								allDone = true;
						}
						if (dbConn.EOF)
							allDone = true;
						Tools.LogInfo("Payments.ProcessCards/30","Iteration " + iter.ToString() + " (" + rowsDone.ToString() + " " + desc + "s processed)",220);
					}
					err = 0;
					Tools.CloseDB(ref dbConn);
				}
				return err;
			}
			catch (Exception ex)
			{
				Tools.LogException("Payments.ProcessCards/40","Iteration " + iter.ToString() + ", " + desc + " " + (success+fail).ToString(),ex);
			}
			finally
			{
				Tools.CloseDB(ref dbConn);
				Tools.LogInfo("Payments.ProcessCards/90","Finished (" + success.ToString() + " " + desc + "s succeeded, " + fail.ToString() + " "+ desc + "s failed)",220);
			}
			return 90;
		}
	}
}
