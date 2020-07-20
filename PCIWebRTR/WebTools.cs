using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PCIWebRTR
{
	public static class WebTools
	{
		static short debugMode = -888;

		public static string RequestValueString (HttpRequest req,string parmName)
		{
			try
			{
				return PCIBusiness.Tools.ObjectToString(req[parmName]);
			}
			catch
			{ }
			return "";
		}

		public static int RequestValueInt (HttpRequest req,string parmName,int minValue=0,int maxValue=int.MaxValue)
		{
			try
			{
				return IntValue((req[parmName]).ToString().Trim(),minValue,maxValue);
			}
			catch
			{ }
			return 0;
		}

		public static string ViewStateString (System.Web.UI.StateBag viewState,string parmName)
		{
			try
			{
				return (viewState[parmName]).ToString().Trim();
			}
			catch
			{ }
			return "";
		}

		public static int ViewStateInt (System.Web.UI.StateBag viewState,string parmName,int minValue=0,int maxValue=int.MaxValue)
		{
			try
			{
				return IntValue((viewState[parmName]).ToString().Trim(),minValue,maxValue);
			}
			catch
			{ }
			return 0;
		}

		public static int IntValue(string value,int minValue,int maxValue)
		{
			try
			{
				int  tmp  = System.Convert.ToInt32(value);
				if ( tmp >= minValue && tmp <= maxValue )
					return tmp;
			}
			catch
			{ }
			return 0;
		}

		public static int ListAdd ( ListControl listBox,
		                            int         position,
                                  string      listValue,
                                  string      listText )
		{
			try
			{
				if ( position > listBox.Items.Count )
					position = listBox.Items.Count;
				else if ( position < 0 )
					position = 0;
				listBox.Items.Insert(position,listText);
				listBox.Items[position].Value = listValue;
				if ( listBox.GetType() == typeof(RadioButtonList) )
					listBox.Items[position].Attributes.Add("style","white-space:nowrap");
			}
			catch
			{
				return -1;
			}
			return position;
		}

		public static int ListValue ( ListControl listBox )
		{
			try
			{
				return System.Convert.ToInt32(ListValue(listBox,"0"));
			}
			catch
			{ }
			return 0;
		}

		public static string ListValue ( ListControl listBox,
		                                 string      defaultValue,
		                                 byte        codeColumn = 0 )
		{
			string sel = "";
			try
			{
				sel = listBox.SelectedValue;
				if ( sel == null || sel.Length < 1 )
					sel = defaultValue;
				else if ( codeColumn > 1 ) // Means the "code" contains more than 1 column and we we want the x'th one
				{
					string[] codes   = sel.Split('/');
					if ( codeColumn <= codes.Length )
						sel = codes[codeColumn-1];
				}
			}
			catch
			{
				sel = defaultValue;
			}
			return sel;
		}

		public static void ListSelect ( ListControl listBox,
		                                string      selectValue,
		                                string      defaultValue="0" )
		{
			bool   OK = false;
			string lValue;

			if ( ! selectValue.StartsWith("*/") ) // For lists with values like "3/120"
				try
				{
					listBox.SelectedValue = selectValue;
					OK = true;
				}
				catch { }

			if ( !OK )
				try
				{
					for ( int k = 0 ; k < listBox.Items.Count ; k++ )
					{
						lValue = listBox.Items[k].Value;
						if ( lValue == selectValue )
						{
							listBox.Items[k].Selected = true;
							OK = true;
							break;
						}
						else if ( selectValue.StartsWith("*/") &&
						          lValue.IndexOf("/") > 0 &&
						          selectValue.Substring(2) == lValue.Substring(lValue.IndexOf("/")+1) )
						{
							listBox.Items[k].Selected = true;
							OK = true;
							break;
						}
					}
				}
				catch { }

			if ( !OK && defaultValue.Length > 0 )
				try
				{
					listBox.SelectedValue = defaultValue;
				}
				catch { }
		}

		public static byte ListBind ( ListControl listBox,
		                              string      sql,
		                              object      dataSource,
		                              string      dataFieldKey,
		                              string      dataFieldShow,
		                              string      addZeroRow  = "",
		                              string      selectValue = "",
		                              short       selectIndex = -888 )
		{
			if ( PCIBusiness.Tools.NullToString(sql).Length > 0 )
				return ListBindMultiKey ( listBox,
				                          sql,
				                          new string[] {dataFieldKey},
				                          dataFieldShow,
				                          addZeroRow,
				                          selectValue,
				                          selectIndex );

			else if ( dataSource != null )
			{
				listBox.DataSource     = dataSource;
				listBox.DataValueField = dataFieldKey;
				listBox.DataTextField  = dataFieldShow;
				listBox.DataBind();
				return 0;
			}
			return 99;
		}

		public static byte ListBindMultiKey ( ListControl listBox,
		                                      string      sql,
		                                      string[]    dataFieldKey,
		                                      string      dataFieldShow,
		                                      string      addZeroRow  = "",
		                                      string      selectValue = "",
		                                      short       selectIndex = -888 )
		{
			listBox.Items.Clear();

			try
			{
				if ( PCIBusiness.Tools.NullToString(sql).Length < 5 )
					return 22;

				string dataValue;
				string keyValue;
				int    k;

				using (PCIBusiness.MiscList dList = new PCIBusiness.MiscList())
					if ( dList.ExecQuery(sql,0) == 0 )
						while ( ! dList.EOF )
						{
							if ( dataFieldKey.Length == 1 )
								keyValue = dList.GetColumn(dataFieldKey[0]);
							else
							{
								keyValue = "";
								for ( k = 0 ; k < dataFieldKey.Length ; k++ )
									keyValue = keyValue + "/" + dList.GetColumn(dataFieldKey[k]);
								if ( keyValue.StartsWith("/") )
									keyValue = keyValue.Substring(1);
							}
							dataValue = dList.GetColumn(dataFieldShow);
							listBox.Items.Add(new ListItem(dataValue,keyValue));
							dList.NextRow();
						}
					else
						return 5;

				if ( addZeroRow.Length > 0 )
					listBox.Items.Insert(0,(new ListItem(addZeroRow,"")));

				if ( listBox.Items.Count > 0 )
				{
					if ( selectValue.Length > 0 )
						ListSelect(listBox,selectValue,"0");
					else if ( selectIndex >= listBox.Items.Count )
						listBox.SelectedIndex = listBox.Items.Count - 1;
					else if ( selectIndex >= 0 )
						listBox.SelectedIndex = selectIndex;
				}
				return 0;
			}
			catch
			{ }

			return 99; 
		}

		public static void Redirect (HttpResponse response,string url)
		{
			try
			{
				if ( url.Length < 6 ) url = "Register.aspx";
				response.Redirect(url,false);
			}
			catch
			{ }
		}

		public static short DebugMode(System.Web.UI.Page webPage,bool load=false)
		{
			if ( load || debugMode < 0 )
				try
				{
					string g  = ((Literal)webPage.FindControl("DebugMode")).Text;
					debugMode = System.Convert.ToInt16(g);
					if ( debugMode < 0 )
						debugMode = 0;
				}
				catch
				{
					debugMode = 0;
				}

			return debugMode;
		}

		public static string JavaScriptSource(string newScript,string existingScript="",byte beforeOrAfter=2)
		{
			newScript = newScript.Trim();
			if ( newScript.Length < 1 && existingScript.Length < 1 )
				return "";
			else if ( ! existingScript.ToLower().Contains("<script") )
				existingScript = "<script type='text/javascript'>" + existingScript + "</script>";
//			if ( newScript.Length > 0 && beforeOrAfter == 2 ) // After
//				existingScript = existingScript.Replace("</script>",newScript + ( newScript.EndsWith(";") ? "" : ";" ) + "</script>");
			if ( newScript.Length > 0 && beforeOrAfter == 1 ) // Before
				existingScript = existingScript.Replace("script'>","script'>" + newScript + ( newScript.EndsWith(";") ? "" : ";"  ) );
			else if ( newScript.Length > 0 )
				existingScript = existingScript.Replace("</script>",newScript + ( newScript.EndsWith(";") ? "" : ";" ) + "</script>");
			return existingScript;
		}

		public static string ClientBrowser(HttpRequest req,string otherInfo="")
		{
			HttpBrowserCapabilities bc = req.Browser;
			string                  h  = bc.Browser + " " + bc.Version + " (" + bc.Platform + ")";
			otherInfo                  = otherInfo.Trim();
			if ( otherInfo.Length > 0 )
				h = h + " : " + otherInfo;
			return h;
		}

		public static string ClientReferringURL(HttpRequest req,byte logInfo=0,string logSource="")
		{
			string refer = PCIBusiness.Tools.ObjectToString(req.UrlReferrer);
			if ( refer.Length < 5 )
				refer = PCIBusiness.Tools.ObjectToString(req.Headers["Referer"]); // Yes, this is spelt CORRECTLY! Do not change

			if ( logInfo > 0 )
			{
				if ( logSource.Length < 1 )
					logSource = req.Url.AbsoluteUri;
				PCIBusiness.Tools.LogInfo ( "WebTools.ClientReferringURL", logSource + " (" + logInfo.ToString() + "), Referring URL=" + refer, 244 );
			}
			return refer;
		}
	}
}