<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RTR.aspx.cs" Inherits="PCIWebRTR.RTR" %>

<!DOCTYPE html>

<html>
<head>
	<title>Prosperian Capital</title>
	<link rel="stylesheet" href="Payment.css" type="text/css" />
</head>
<body>
<script type="text/javascript" src="JS/Utils.js"></script>
<script type="text/javascript">
function PaySingle(mode)
{
	if ( mode > 0 )
	{
		ShowElt('divCard',1);
		GetElt('txtFName').focus();
		if ( mode == 33 )
			SetEltValue('lblError2','');
	}
	else
		ShowElt('divCard',0);
}
</script>
<form runat="server" id="frmRTR">
	<table>
		<tr>
			<td class="Header">Prosperian Capital : RTR Payments</td>
			<td style="width:1%"><a href="http://prosperian.mu" target="P"><img src="Images/LogoProsperian.png" title="Prosperian Capital" /></a></td></tr>
	</table>
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
	<tr>
		<td colspan="2"><hr /></td></tr>
	<tr>
		<td>PlaNet User Code</td><td class="Red"> : <b><i><asp:Literal runat="server" ID="lblSUserCode"></asp:Literal></i></b></td></tr>
	<tr>
		<td>Referred URL</td><td class="Red"> : <b><i><asp:Literal runat="server" ID="lblSURL"></asp:Literal></i></b></td></tr>
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
			<asp:RadioButton runat="server" GroupName="rdoP" ID="rdoWeb" Text="Synchronous (this web page)" /><br />
			<asp:RadioButton runat="server" GroupName="rdoP" ID="rdoAsynch" Text="Asynchronous (a separate EXE)" /><br />
			<asp:RadioButton runat="server" GroupName="rdoP" ID="rdoCard" onclick="JavaScript:PaySingle(33)" Text="Single card payment" /></td></tr>
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
	<asp:Panel runat="server" ID="pnlButtons" CssClass="ButtonRow">
		<asp:Button  runat="server" ID="btnProcess1" CssClass="Button" OnClientClick="JavaScript:Busy(1,'Getting tokens ... please be patient')" onclick="btnProcess1_Click" Text="Get Tokens" />
		<asp:Button  runat="server" ID="btnProcess2" CssClass="Button" OnClientClick="JavaScript:Busy(1,'Processing payments ... please be patient')" onclick="btnProcess2_Click" Text="Token Payments" />
		<asp:Button  runat="server" ID="btnProcess4" CssClass="Button" OnClientClick="JavaScript:Busy(1,'Processing payments ... please be patient')" onclick="btnProcess4_Click" Text="Card Payments" />
		<asp:Button  runat="server" ID="btnProcess3" CssClass="Button" OnClientClick="JavaScript:Busy(1,'Deleting tokens ... please be patient')" onclick="btnProcess3_Click" Text="Delete Tokens" />
		<asp:Button  runat="server" ID="btnConfig"   CssClass="Button" OnClientClick="JavaScript:Busy(1)" onclick="btnConfig_Click" Text="Config" />
		<input type="button" class="Button" onclick="JavaScript:ShowElt('divLogs',1)" value="Logs" />
		<asp:Button  runat="server" ID="btnSQL"      CssClass="Button" OnClientClick="JavaScript:Busy(1,'Executing SQL ...')" onclick="btnSQL_Click" Text="Test SQL ..." />
		<asp:Button  runat="server" ID="btnJSON"     CssClass="Button" OnClientClick="JavaScript:Busy(1,'Parsing JSON string ...')" onclick="btnJSON_Click" Text="Test JSON ..." visible="false" />
		<asp:TextBox runat="server" ID="txtTest" Width="560px"></asp:TextBox>
	</asp:Panel>
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

	<div id="divCard" class="PopupBox" style="visibility:hidden;display:none">
	<div style="background-color:hotpink;padding:5px;font-size:18px;font-weight:bold">Single Card Payment : <asp:Literal runat="server" ID="lblBureauName" /></div>
	<table class="Detail">
	<tr>
		<td>First Name</td>
		<td colspan="2">
			<asp:TextBox runat="server" id="txtFName" Width="300px"></asp:TextBox></td></tr>
	<tr>
		<td>Last Name</td>
		<td colspan="2">
			<asp:TextBox runat="server" id="txtLName" Width="300px"></asp:TextBox></td></tr>
	<tr>
		<td>EMail</td>
		<td colspan="2">
			<asp:TextBox runat="server" id="txtEMail" Width="300px"></asp:TextBox></td></tr>
	<tr>
		<td>Card Number</td>
		<td colspan="2">
			<asp:TextBox runat="server" id="txtCCNumber" Width="160px" MaxLength="20"></asp:TextBox></td></tr>
	<tr>
		<td>Expiry Date</td>
		<td colspan="2">
			<asp:DropDownList runat="server" id="lstCCMonth">
				<asp:ListItem Value="00" Text="(Select one)"></asp:ListItem>
				<asp:ListItem Value="01" Text="01 (January)"></asp:ListItem>
				<asp:ListItem Value="02" Text="02 (February)"></asp:ListItem>
				<asp:ListItem Value="03" Text="03 (March)"></asp:ListItem>
				<asp:ListItem Value="04" Text="04 (April)"></asp:ListItem>
				<asp:ListItem Value="05" Text="05 (May)"></asp:ListItem>
				<asp:ListItem Value="06" Text="06 (June)"></asp:ListItem>
				<asp:ListItem Value="07" Text="07 (July)"></asp:ListItem>
				<asp:ListItem Value="08" Text="08 (August)"></asp:ListItem>
				<asp:ListItem Value="09" Text="09 (September)"></asp:ListItem>
				<asp:ListItem Value="10" Text="10 (September)"></asp:ListItem>
				<asp:ListItem Value="11" Text="11 (November)"></asp:ListItem>
				<asp:ListItem Value="12" Text="12 (December)"></asp:ListItem>
			</asp:DropDownList>
			<asp:DropDownList runat="server" id="lstCCYear"></asp:DropDownList></td></tr>
	<tr>
		<td>CVV/CVC</td>
		<td>
			<asp:TextBox runat="server" id="txtCCCVV" Width="50px" MaxLength="4"></asp:TextBox></td>
		<td>
			3/4 digit number on back of card</td></tr>
	<tr>
		<td>Currency</td>
		<td>
			<asp:TextBox runat="server" id="txtCurrency" Width="50px" MaxLength="3"></asp:TextBox></td>
		<td>
			Currency code as per the provider's<br />
			requirements (eg. ZAR, USD, GBP)</td></tr>
	<tr>
		<td>Amount</td>
		<td>
			<asp:TextBox runat="server" id="txtAmount" Width="75px"></asp:TextBox></td>
		<td>
			Cents, so ZAR 987 means R9.87</td></tr>
	<tr>
		<td>Prosperian Reference</td>
		<td colspan="2">
			<asp:TextBox runat="server" id="txtReference" Width="300px"></asp:TextBox></td></tr>
	</table>
	<hr />
	<asp:Button runat="server" ID="btnPay" CssClass="Button" Text="Pay" OnClick="btnPay_Click" />
	<input type="button" class="Button" value="Cancel" onclick="JavaScript:PaySingle(0)" />
	<asp:Label runat="server" ID="lblError2" CssClass="Error"></asp:Label>
	</div>

</form>

<div id="divBusy" style="left:10px;top:20px;position:fixed;border:1px solid #000000;padding:5px;background-color:aquamarine">
	<table style="background-color:aquamarine">
		<tr>
			<td><img src="Images/Busy.gif" title="Busy ..." /></td>
			<td id="msgBusy" style="font-size:18px;font-style:italic;color:red;font-weight:bold">Busy ... please wait</td></tr>
	</table>
</div>

<asp:Literal runat="server" ID="lblJS"></asp:Literal>

<script type="text/javascript">
Busy(0);
</script>
</body>
</html>
