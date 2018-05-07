using System;
using System.Collections.Generic;
using System.Text;

namespace PCIUnattended
{
	class Program
	{
		static void Main(string[] arg)
		{
			try
			{
				PCIBusiness.Tools.LogInfo("PCIUnattended.Main/10","Launched",210);
				
				int    k;
				int    rows     = 1;
				byte   mode     = 0;
				string provider = "";
				string args     = "";
				string argValue;

				for ( k = 0 ; k < arg.Length ; k++ )
				{
					argValue = arg[k].ToUpper().Trim();
					args     = args + argValue + " ";
//					PCIBusiness.Tools.LogInfo("PCIUnattended.Main/10","Arg[" + k.ToString() + "] = " + argValue,210);

					if ( argValue.StartsWith("MODE=") )
						mode = System.Convert.ToByte(argValue.Substring(5));
					else if ( argValue.StartsWith("ROWS=") )
						rows = System.Convert.ToInt32(argValue.Substring(5));
					else if ( argValue.StartsWith("PROVIDER=") )
						provider = argValue.Substring(9);
				}

				PCIBusiness.Tools.LogInfo("PCIUnattended.Main/20","Args=" + args.Trim(),210);

				using (PCIBusiness.Payments payments = new PCIBusiness.Payments())
				{
					PCIBusiness.Tools.LogInfo("PCIUnattended.Main/30","Start processing, Bureau="+provider+", Mode="+mode.ToString()+", Rows="+rows.ToString(),210);
					k = payments.ProcessCards(provider,mode,rows);
					PCIBusiness.Tools.LogInfo("PCIUnattended.Main/40","Finished, Return code="+k.ToString() + ", " + (payments.CountSucceeded+payments.CountFailed).ToString() + " payment(s) completed : " + payments.CountSucceeded.ToString() + " succeeded, " + payments.CountFailed.ToString() + " failed",210);
				}
			}
			catch (Exception ex)
			{
				PCIBusiness.Tools.LogException("PCIUnattended.Main/90","",ex);
			}
		}
	}
}
