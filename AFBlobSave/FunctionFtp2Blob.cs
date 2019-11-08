using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Z.Ftp;
using Microsoft.Azure.Storage.Auth;
using System.Collections.Generic;
using System.Configuration;
using Z.Logging;

namespace AFBlobSave
{
    public static class FunctionFtp2Blob
    {
        [FunctionName("FunctionFtp2Blob")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executing at: {DateTime.Now}");

            FtpCredentials ftpCredentials = new FtpCredentials()
            {
                Hostname = ConfigurationManager.AppSettings["FtpHostname"],
                InboundDir = ConfigurationManager.AppSettings["FtpInboundDir"],
                OutboundDir = ConfigurationManager.AppSettings["FtpOutboundDir"],
                Password = ConfigurationManager.AppSettings["FtpPassword"],
                Username = ConfigurationManager.AppSettings["FtpUsername"],
                Key = ConfigurationManager.AppSettings["FtpKey"]
            };
            string accountName = ConfigurationManager.AppSettings["StorageAccountName"];
            string accountKey = ConfigurationManager.AppSettings["StorageAccountKey"];
            string blobContainerName = ConfigurationManager.AppSettings["BlobContainerName"];
            StorageCredentials storageCredentials = new StorageCredentials(accountName, accountKey);

            OmsCredentials omsCredentials = new OmsCredentials()
            {
                WorkspaceId = ConfigurationManager.AppSettings["OmsWorkspaceId"],
                SharedKey = ConfigurationManager.AppSettings["OmsLogAnalyticsSharedKey"]
            };

            ILog omsLogger = new OmsLogger(omsCredentials); 
;
            BlobFtpAgent theAgent = new RebexFtpAgent(ftpCredentials, storageCredentials, omsLogger);
            List<String> fnames = theAgent.GetFileNames();
            if (fnames.Count == 0)
            {
                log.Info("No files found");
            }
            else
            {
                foreach (string fname in fnames)
                {
                    log.Info("Transfering Inbound File: " + fname);
                    var logEntry = theAgent.Get(fname, blobContainerName);
                    if (logEntry != null)
                    {
                        if (logEntry.Result.Equals("Success"))
                        {
                            log.Info(fname + " Transferred");
                        }
                        else
                        {
                            log.Info("Failed to Transfer File " + fname);
                        }

                    }
                }
            }
            

        }
    }
}
