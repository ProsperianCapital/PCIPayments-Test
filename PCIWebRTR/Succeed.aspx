<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Succeed.aspx.cs" Inherits="PCIWebRTR.Succeed" %>

<!DOCTYPE html>

<html>
<head>
<title>Prosperian Capital</title>
<link rel="stylesheet" href="Payment.css" type="text/css" />
</head>
<body>
<a href="http://prosperian.mu" target="P"><img src="Images/LogoProsperian.png" title="Prosperian Capital" /></a>
<form runat="server" id="frmSucceed">
	<p class="Header">
	Prosperian Capital : Payment Successful
	</p>
	<p class="Detail">
	<b>Thank you!</b>
	<br /><br />
	Your payment has been <span class="Red">successfully processed</span>.
	</p>
	<p class="ButtonRow">
	<input type="button" value="Home" class="Button" onclick="JavaScript:location.href='RTR.aspx'" />
	</p>
	<hr />
	<p class="Footer">
	&nbsp;Phone +230 404 8000&nbsp; | &nbsp;Email <a href="mailto:info@prosperian.mu">Info@prosperian.mu</a>
	<span style="float:right;margin-right:5px"><asp:Literal runat="server" ID="lblVersion"></asp:Literal></span>
	</p>
</form>
</body>
</html>
