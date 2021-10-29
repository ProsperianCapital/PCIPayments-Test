//	Misc JavaScript functions

//	(c)Paul Kilfoil
//		Software Development & IT Consulting
//		+27 84 438 5400 (phone)
//		PaulKilfoil..[at]..gmail.com
//		www.PaulKilfoil.co.za

//	Do not copy without the permission of the author

function CursorStyle(eltID,style)
{
	try
	{
		var p = GetElt(eltID);
		var h = 'auto'; // 1
		if      ( style == 2 ) h = 'help';
		else if ( style == 3 ) h = 'pointer';
		else if ( style == 4 ) h = 'progress';
		else if ( style == 5 ) h = 'not-allowed';
		else if ( style == 6 ) h = 'none';
		p.style.cursor = h;
	}
	catch (x)
	{ }
}

function ToInteger(theValue,defaultReturn)
{
	try
	{
		if ( defaultReturn == null )
			defaultReturn = 0;
		var p = parseInt(theValue,10); // Force base 10
		if ( ! isNaN(p) ) // It's an INT
			return p;
	}
	catch (x)
	{ }
	return defaultReturn;
}

function GetElt(eltID)
{
	try
	{
		var p;
		if ( typeof(eltID) == 'object' )
			p = eltID;
		else
			p = document.getElementById(eltID);
		return p;
	}
	catch (x)
	{ }
	return null;
}

function GetEltValue(eltID)
{
	var p = GetElt(eltID);
	try
	{
		var h = Trim(p.value.toString());
		return h;
	}
	catch (x)
	{ }
	try
	{
		var k = Trim(p.innerHTML);
		return k;
	}
	catch (x)
	{ }
	return "";
}

function GetEltValueInt(eltID)
{
	var p = GetElt(eltID);
	try
	{
		var h = Trim(p.innerHTML);
		if ( h.length == 0 )
			h = p.value;
		var k = ToInteger(h);
		if ( k > 0 )
			return k;
	}
	catch (x)
	{ }
	return 0;
}

function SetEltValue(eltID,value)
{
	var p = GetElt(eltID);
	try
	{
		p.value = value;
	}
	catch (x)
	{ }
	try
	{
		p.innerHTML = value;
	}
	catch (x)
	{ }
}

function DisableElt(eltID,disable)
{
	try
	{
		var p = GetElt(eltID);
		p.disabled = ( disable > 0 );
		CursorStyle(p,(disable>0?5:1));
		if ( disable>0 && p.title.length < 1 )
			p.title = 'Disabled';
		else if ( disable<1 && p.title == 'Disabled' )
			p.title = '';
	}
	catch (x)
	{ }
}

function DisableForm(frmID,disable)
{
	var k;
	var elts = GetElt(frmID).elements;
	for ( k=0 ; k < elts.length ; k++ )
		elts[k].disabled = ( disable > 0 );
}

function Trim(theValue)
{
	return theValue.replace(/^\s+/g, '').replace(/\s+$/g, '');
}

function ValidEmail(email)
{
	try
	{
		email = Trim(email);
		if ( email.length < 6 )
			return false;
		if ( email.indexOf(' ') >= 0 || email.indexOf('/') >= 0 || email.indexOf('\\') >= 0 || email.indexOf('<') >= 0 || email.indexOf('>') >= 0 || email.indexOf('(') >= 0 || email.indexOf(')') >= 0 )
			return false;
		var k = email.indexOf('@');
		if ( k < 1 )
			return false;
		var j = email.lastIndexOf('@');
		if ( k != j )
			return false;
		j = email.lastIndexOf('.');
		if ( j < k )
			return false;
		if ( email.substring(k-1,k) == '.' || email.substring(k+1,k+2) == '.' || email.substring(0,1) == '.' || email.substring(email.length-1,email.length) == '.' )
			return false;
	}
	catch (x)
	{ }
	return true;
}

function ShowBackground(show)
{
	try
	{
		if ( show > 0 )
			document.body.className = '';
		else
			document.body.className = 'greyBackground';
	}
	catch (x)
	{ }
}

function ShowElt(eltID, show)
{
	try
	{
		var p = GetElt(eltID);
		if (show > 0)
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

function SetListValue(eltID,listValue)
{
	try
	{
		var p = GetElt(eltID);
		var k;

		for (k=0; k < p.options.length; k++)
			if ( p[k].value == listValue )
			{
				p.selectedIndex = k;
				return;
			}
		p.selectedIndex = 0;
	}
	catch (x)
	{ }
}

function GetListValue(eltID)
{
	try
	{
		var p = GetElt(eltID);
		var h = p.options[p.selectedIndex].value;
		return ToInteger(h);
	}
	catch (x)
	{ }
	return 0;
}

function GetListText(eltID)
{
	try
	{
		var p = GetElt(eltID);
		return p.options[p.selectedIndex].text;
	}
	catch (x)
	{ }
	return "";
}

function ListAdd(eltID,code,text)
{
	var p   = GetElt(eltID);
	var h   = document.createElement('option');
	h.value = code.toString();
	h.text  = text.toString();

	try
	{
		p.add(h,null); // non-IE
	}
	catch (x)
	{
		p.add(h);      // MS IE
	}
}

function ValidDate(dd,mm,yy,eltName)
{
// Standard date validation, assuming that all years ending in '00' are leap years (they aren't)

	var msg = eltName + " is not valid<br />";

	try
	{
		dd = ToInteger(dd);
		mm = ToInteger(mm);
		yy = ToInteger(yy);

		if (dd < 1 || dd > 31 || mm < 1 || mm > 12 || yy < 1900 || yy > 2999)
			return msg;
		else if (dd > 30 && (mm == 4 || mm == 6 || mm == 9 || mm == 11))
			return msg;
		else if (mm == 2 && dd > 29)
			return msg;
		else if (mm == 2 && dd == 29 && yy % 4 != 0)
			return msg;
		return "";
	}
	catch (x)
	{ }
	return msg;
}

function ShowPopup(eltID,info,event,endLR,eltObj)
{
	try
	{
		if ( info.length > 0 )
		{
			var q = GetElt(eltID);
			q.style.visibility = "visible";
			q.style.display = '';
			q.innerHTML = info;
			if ( event != null )
			{
				q.style.left  = "auto";
				q.style.right = "auto";
				q.style.top   = event.clientY.toString() + "px";
				if ( endLR == 'R' )
					q.style.right = (window.innerWidth-event.clientX-10).toString() + "px";
				else
					q.style.left  = (event.clientX+10).toString() + "px";
			}
			if ( eltObj != null )
			{
				var ctl = eltObj.getBoundingClientRect();
				var scr = document.body.getBoundingClientRect();
				q.style.top  = ( ctl.top  - scr.top  -  40 ).toString()  + "px";
				q.style.left = ( ctl.left - scr.left + (ctl.right-ctl.left)/2 ).toString() + "px";
			}
		}
		else
			ShowElt(eltID,0);
	}
	catch (x)
	{ }
}

function Busy(show, msg)
{
	try
	{
		ShowElt('divBusy', show);
		if (show > 0 && msg != null)
			GetElt('msgBusy').innerHTML = msg;
	}
	catch (x)
	{ }
}

function ScreenCheck(a,funcToEval)
{
	if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino/i.test(a)||/1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0,4)))
		funcToEval();
}