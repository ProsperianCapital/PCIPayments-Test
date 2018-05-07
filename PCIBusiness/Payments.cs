using System;
using System.Text;

namespace PCIBusiness
{
	public class Payments : BaseList
	{
		private string  bureauCode;
		private int     success;
		private int     fail;
		private int     err;

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
    			sql = "exec sp_Get_CardToToken " + Tools.DBString(bureau);
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
    			sql = "exec sp_Get_CardPayment " + Tools.DBString(bureau);
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
			bureauCode = Tools.NullToString(bureau);
			success    = 0;
			fail       = 0;

			if ( bureauCode.Length < 1 )
				return 10;
			else if ( mode == 1 )
    			sql = "exec sp_Get_CardToToken " + Tools.DBString(bureauCode);
			else if ( mode == 2 )
    			sql = "exec sp_Get_CardPayment "  + Tools.DBString(bureauCode);
			else
				return 20;

			Tools.LogInfo("Payments.ProcessCards/10","Mode="+mode.ToString()+", Rows=" + rowsToProcess.ToString() + ", BureauCode="+bureauCode+", SQL="+sql,10);

			try
			{
				err = ExecuteSQL(null,false,false);
				if ( err > 0 )
					Tools.LogException("Payments.ProcessCards/20",sql + " failed, return code " + err.ToString());
				else
					using (PCIBusiness.Payment payment = new PCIBusiness.Payment(bureauCode))
					{
						while ( ! dbConn.EOF )
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
							if ( rowsToProcess > 0 && success + fail >= rowsToProcess )
								break;
						}
						return 0;
					}
			}
			catch (Exception ex)
			{
				Tools.LogException("Payments.ProcessCards/30","Payment " + (success+fail).ToString(),ex);
			}
			finally
			{
				Tools.CloseDB(ref dbConn);
				Tools.LogInfo("Payments.ProcessCards/90","Finished (" + (success+fail).ToString() + " cards/tokens processed)",20);
			}
			return 90;
		}
	}
}
