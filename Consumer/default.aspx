<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="AsyncRabbitWeb._default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Uhuru Software Q Demo</title>
    <link href="http://fonts.googleapis.com/css?family=Oswald" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" type="text/css" href="css/style.css" />

</head>
<body>
<div id="wrapper">
	<div id="header">
		<div id="logo">
			<h1><a href="#">.Net Consumer to RabbitMQ</a></h1>
			
		</div>
	</div>
    <div id="page">
	    <div id="content">
		    <div id="post1">
			    <img class="alignleft" src="images/img03.jpg" width="200" height="200" alt="" />
			    <p>
				    This is <strong>Uhuru</strong>, a comprehensive, integrated application development platform, tools and services.
			    </p>
                <p>We enable rapid development and deployment of solutions in private and public clouds by enterprises and service providers</p>
                <h2>Hetrogeneous Platform as a Service</h2>
		    </div>
            <div id="post2">            
                <form id="frmMain" runat="server">
                    <table>
                    <tr>
                        <td><asp:GridView ID="grdView" runat="server"></asp:GridView></td>
                    </tr>
                    <tr>
                        <td><asp:Button ID="btnRec" runat="server" Text="Receive Message" onclick="btnRec_Click" /></td>
                    </tr>
                    <tr><td><asp:Button ID="btnTrunc" runat="server" Text="Clear DB" onclick="btnTrunc_Click" /></td></tr>
                    </table>
                        <br class="clearfix" />
                        <h5>Connection Information</h5> 
                        <h5><asp:Label ID="lblExgName" runat="server" Text="Exchange Name :"></asp:Label></h5> 
                        <h5><asp:Label ID="lblExgType" runat="server" Text="Exchange Type :"></asp:Label></h5>
                        <h5><asp:Label ID="lblQName" runat="server" Text="Queue name :"></asp:Label></h5>
                </form>
            </div>            
		    <div id="post3">
			    <h3>Queuing Demo</h3>
			    <ul class="section-list">
				    <li class="first"><h4>.Net Producer</h4><span>.Net application that is used to generate messages into a fan out "Incident Exchange"</span></li>
				    <li><h4>.Net Receiver</h4><span>.Net application that is subscribed to "Incident Exchange" which puts the message into a SQL Storage layer</span></li>
				    <li><h4>Java Producer</h4><span>Java app that is used to generate messages into fan out "Incident Exchange"</span></li>
				    <li class="last"><h4>Java Receiver</h4><span>Java application that is subscribed to "Incident Exchange" which puts the message into a MySQL Storage layer</span></li>
			    </ul>
		    </div>
            <div id="footer">
	            Copyright (c) 2012 UhuruSoftware.com. All rights reserved.
            </div>
	    </div>
	</div>
</div>
</body>
</html>