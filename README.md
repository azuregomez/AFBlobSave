<H2>Serverless solution to get files from a remote FTP Server and save to Blob</h2>
<h3>Business Case</h3>
There are cases when business partners or internal applications integrate through an FTP Server so that files are delivered in the FTP Server and we have to get them into Aure Blob Storage.
We would like to do that in an automated way.
<h3>Solution</h3>
This solution is an Azure Function that is scheduled every 5 minutes to:
<ol>
<li>Login to an FTP Server and get existing files
<li>Sabe the files to a Blob Container
<li>Remove the copied files from the FTP Server
<li>Log activity to Log Analytics.  You can get alerted on FTP failures and create file transfer dashboards.<br><br>
  Query to alert on FTP failures for the last hour:<br><br>
  FtpTransfer_CL<br>
| where Result_s != "Success" and TimeGenerated  > ago(1h) <br><br>
  Query for a dashboard with number of files and total GB transferred:<br><br>
FtpTransfer_CL <br>
| where Result_s == "Success" and TimeGenerated  > ago(24h) <br>
| summarize Files = count(), GB =  sum(Size_d)/1000000000 <br>
</ol>
If 5 minutes is too often you can change the frequency.
<h3>Pre-requisites</h3>
<ul>
<li>A Log Analytics Workspace. Not really required because teh code would graciously keep going anyway.
<li>An Azure Function App that will host the function.  Consumption plan is good enough.
<li>A Storage Accounnt with a Blob Container to receive the files
<li>Credentials from the remote FTP Server
<li>A Rebex FTP client license. The code provided uses the Rebex SFTP Nuget package and needs a license key. You can get it here: https://www.rebex.net/sftp.net/
</ul>
<h3>Comments</h3>
<ul>
<li>This solution is an alternative to Logic Apps: <br>https://azure.microsoft.com/en-us/resources/templates/101-logic-app-ftp-to-blob/
<li>An Azure Function is a lot more extensible than Logic Apps. You can add some fancy file processing, save stuff to a database or be part of a larger workflow with durable functions.
<li>If you fully control the FTP Server, you can always host it in Azure and drop the files straight to Storage:<br>
https://docs.microsoft.com/en-us/samples/azure-samples/sftp-creation-template/sftp-on-azure/
<ul>
