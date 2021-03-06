<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RTR.aspx.cs" Inherits="PCIWeb.RTR" %>

<!DOCTYPE html>

<html>
<head>
	<title>Prosperian Capital</title>
	<link rel="stylesheet" href="Payment.css" type="text/css" />
</head>
<body>
<script type="text/javascript">
function GetElt(eltID)
{
	try
	{
		var p = document.getElementById(eltID);
		return p;
	}
	catch (x)
	{ }
	return null;
}
function ShowElt(eltID,show)
{
	try
	{
		var p = GetElt(eltID);
		if ( show > 0 )
		{
			p.style.visibility = "visible";
			p.style.display    = "";
		}
		else
		{
			p.style.visibility = "hidden";
			p.style.display    = "none";
		}
	}
	catch (x)
	{ }
}
function Busy(show,msg)
{
	try
	{
		ShowElt('divBusy',show);
		if ( show > 0 && msg != null )
			GetElt('msgBusy').innerHTML = msg;
	}
	catch (x)
	{ }
}
</script>
<a href="http://prosperian.mu" target="P"><img src="LogoProsperian.png" title="Prosperian Capital" /></a>
<form runat="server" id="frmRTR">
	<div class="Header">
	Prosperian Capital : RTR Payments
	</div>
	<hr />
	<table class="BoxRight">
	<tr>
		<td>SQL Server</td><td> : <b><i><asp:Literal runat="server" ID="lblSQLServer"></asp:Literal></i></b></td></tr>
	<tr>
		<td>SQL Database</td><td> : <b><i><asp:Literal runat="server" ID="lblSQLDB"></asp:Literal></i></b></td></tr>
	<tr>
		<td>SQL User</td><td> : <b><i><asp:Literal runat="server" ID="lblSQLUser"></asp:Literal></i></b></td></tr>
	<tr>
		<td>SQL Status</td><td> : <b><i><asp:Literal runat="server" ID="lblSQLStatus"></asp:Literal></i></b></td></tr>
	<tr>
		<td colspan="2"><hr /></td></tr>
	<tr>
		<td>System Mode</td><td class="Red"> : <b><i><asp:Literal runat="server" ID="lblSMode"></asp:Literal></i></b></td></tr>
	<tr>
		<td>System Status</td><td class="Red"> : <b><i><asp:Literal runat="server" ID="lblSStatus"></asp:Literal></i></b></td></tr>
	</table>
	<table class="Detail">
	<tr>
		<td>Payment Provider</td>
		<td>
			<asp:DropDownList runat="server" id="lstProvider" AutoPostBack="true"></asp:DropDownList></td></tr>
	<tr>
		<td>Process only the top (or "ALL")</td>
		<td><asp:TextBox runat="server" ID="txtRows" Width="50px">ALL</asp:TextBox> row(s)</td></tr>
	<tr>
		<td>Process via</td>
		<td>
			<asp:RadioButton runat="server" GroupName="rdoP" ID="rdoWeb" />Synchronous (this web page)<br />
			<asp:RadioButton runat="server" GroupName="rdoP" ID="rdoAsynch" />Asynchronous (a separate EXE)</td></tr>
	<tr>
		<td colspan="2"><hr /></td></tr>
	<tr>
		<td>Bureau Code</td>
		<td> : <asp:Literal runat="server" ID="lblBureauCode"></asp:Literal></td></tr>
	<tr>
		<td>Payment URL</td>
		<td> : <asp:Literal runat="server" ID="lblBureauURL"></asp:Literal></td></tr>
	<tr>
		<td>Prosperian Account/Key</td>
		<td> : <asp:Literal runat="server" ID="lblMerchantKey"></asp:Literal></td></tr>
	<tr>
		<td>Prosperian User ID</td>
		<td> : <asp:Literal runat="server" ID="lblMerchantUser"></asp:Literal></td></tr>
	<tr>
		<td>Cards waiting to be tokenized</td>
		<td> : <asp:Literal runat="server" ID="lblCards"></asp:Literal></td></tr>
	<tr>
		<td>Payments waiting to be processed</td>
		<td> : <asp:Literal runat="server" ID="lblPayments"></asp:Literal></td></tr>
	<tr>
		<td>Status</td>
		<td> : <asp:Literal runat="server" ID="lblBureauStatus"></asp:Literal></td></tr>
	</table>
	<div class="ButtonRow">
	<asp:Button  runat="server" ID="btnProcess1" CssClass="Button" OnClientClick="JavaScript:Busy(1,'Getting tokens ... please be patient')" onclick="btnProcess1_Click" Text="Get Tokens" />
	<asp:Button  runat="server" ID="btnProcess2" CssClass="Button" OnClientClick="JavaScript:Busy(1,'Processing payments ... please be patient')" onclick="btnProcess2_Click" Text="Process Payments" />
	<asp:Button  runat="server" ID="btnConfig"   CssClass="Button" OnClientClick="JavaScript:Busy(1)" onclick="btnConfig_Click" Text="Config" />
	<input type="button" class="Button" onclick="JavaScript:ShowElt('divLogs',1)" value="Logs" />
	<asp:Button  runat="server" ID="btnSQL"      CssClass="Button" OnClientClick="JavaScript:Busy(1,'Executing SQL ...')" onclick="btnSQL_Click" Text="Test SQL ..." />
	<asp:Button  runat="server" ID="btnJSON"     CssClass="Button" OnClientClick="JavaScript:Busy(1,'Parsing JSON string ...')" onclick="btnJSON_Click" Text="Test JSON ..." visible="false" />
	<asp:TextBox runat="server" ID="txtTest" Width="560px"></asp:TextBox>
	<!--
	<hr />
	<asp Button  run@t="server" ID="btnInfo"     CssClass="Button" OnClientClick="JavaScript:Busy(1)" onclick="btnInfo_Click"   Text="Today's Info Log" />
	<asp Button  run@t="server" ID="btnError"    CssClass="Button" OnClientClick="JavaScript:Busy(1)" onclick="btnError_Click"  Text="Today's Error Log" />
	<asp Button  run@t="server" ID="btnInfoX"    CssClass="Button" OnClientClick="JavaScript:Busy(1)" onclick="btnInfoX_Click"   Text="Info Log dd ..." />
	<asp Button  run@t="server" ID="btnErrorX"   CssClass="Button" OnClientClick="JavaScript:Busy(1)" onclick="btnErrorX_Click"  Text="Error Log dd ..." />
	-->
	</div>
	<hr />
	<asp:Label runat="server" ID="lblTest"></asp:Label>
	<asp:Label runat="server" ID="lblError" CssClass="Error"></asp:Label>
	<p class="Footer">
	&nbsp;Phone +230 404 8000&nbsp; | &nbsp;Email <a href="mailto:info@prosperian.mu">Info@prosperian.mu</a>
	<span style="float:right;margin-right:5px"><asp:Literal runat="server" ID="lblVersion"></asp:Literal></span>
	</p>

	<div id="divLogs" style="display:none;left:10px;bottom:40px;position:fixed;border:1px solid #000000;padding:5px;background-color:aquamarine">
		<div style="background-color:hotpink;padding:5px;font-size:18px;font-weight:bold">Log Files</div>
		Show logs for ...<br />
		<asp:RadioButton runat="server" ID="rdo0" GroupName="rdoLog" />Today<br />
		<asp:RadioButton runat="server" ID="rdo1" GroupName="rdoLog" />Yesterday<br />
		<asp:RadioButton runat="server" ID="rdo2" GroupName="rdoLog" />2 days ago<br />
		<asp:RadioButton runat="server" ID="rdoX" GroupName="rdoLog" onclick="JavaScript:GetElt('txtDate').focus()" />This date:&nbsp;<asp:TextBox runat="server" ID="txtDate" MaxLength="10" Width="85px"></asp:TextBox>&nbsp;dd/mm/yyyy
		<hr />
		<asp:Button runat="server" CssClass="Button" ID="btnError" OnClientClick="JavaScript:Busy(1)" onclick="btnError_Click" Text="Errors" />
		<asp:Button runat="server" CssClass="Button" ID="btnInfo"  OnClientClick="JavaScript:Busy(1)" onclick="btnInfo_Click" Text="Info" />
		<input type="button" class="Button" onclick="JavaScript:ShowElt('divLogs',0)" value="Close" />
	</div>

</form>

<div id="divBusy" style="left:10px;top:20px;position:fixed;border:1px solid #000000;padding:5px;background-color:aquamarine">
	<table style="background-color:aquamarine">
		<tr>
			<td><img src="Busy.gif" title="Busy ..." /></td>
			<td id="msgBusy" style="font-size:18px;font-style:italic;color:red;font-weight:bold">Busy ... please wait</td></tr>
	</table>
</div>

<asp:Literal runat="server" ID="lblJS"></asp:Literal>

<script type="text/javascript">
Busy(0);
</script>
</body>
</html>
