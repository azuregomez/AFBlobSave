using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Z.Logging;
using Microsoft.Azure.Storage.Auth;

namespace Z.Ftp
{
    

    public abstract class BlobFtpAgent
    {

        protected FtpCredentials _ftpCredentials;
        protected StorageCredentials _storageCredentials;
        protected ILog _logger;
        protected string _logName; 

        /// <summary>
        /// Constructor that takes site, user and pwd separated by semicolon;
        /// </summary>
        /// <param name="settingstr"></param>
        /// <param name="secure"></param>
        public BlobFtpAgent(FtpCredentials ftpCredentials, StorageCredentials storageCredentials, ILog logger)
        {
            _ftpCredentials = ftpCredentials;
            _storageCredentials = storageCredentials;
            _logger = logger;
            _logName = "FtpTransfer";           
        }


        /// <summary>
        /// Sends an array of strings to a file over FTP
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="fname"></param>
        /// <returns></returns>
        protected abstract long PutFile(List<String> lines, string fname);
        public abstract List<String> GetFileNames(string extension=null,string path=null);
        protected abstract byte[] GetFile(string fname, bool deleteFile = true);
        public abstract void Delete(string fname);
        public abstract List<String> GetDirectories(string path = null);
        /// <summary>
        /// Transafer a file 
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="fname"></param>
        /// <returns></returns>
        public bool Send(List<String> lines, string fname)
        {
            FileTransferLog ftLog = new FileTransferLog()
            {
                TransferType = FileTransferType.PutFile,
                FileName = fname,
                RemoteAddress = _ftpCredentials.Hostname
            };
            bool success;
            try
            {
                ftLog.Size=PutFile(lines, fname);
                ftLog.Result = "Success";
                success = true;
            }
            catch (Exception x)
            {
                StringBuilder xsb = new StringBuilder(x.Message);
                if (x.InnerException != null)
                {
                    xsb.Append(Environment.NewLine + x.InnerException.Message);
                }
                xsb.Append(Environment.NewLine + x.StackTrace);
                ftLog.Result = xsb.ToString();
                success = false;
            }
            _logger.Log(_logName,ftLog.Json);
            return success;            
        }



        /// <summary>
        /// Gets a file from ftp server and stores it in a blob
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="blobContainerName"></param>
        /// <param name="deleteFile"></param>
        /// <returns></returns>
        public FileTransferLog Get(string fname, string blobContainerName, bool deleteFile = true)
        {
             // prepare the log entry:
            FileTransferLog ftLog = new FileTransferLog()
            {
                TransferType = FileTransferType.GetFile,
                FileName = fname,
                RemoteAddress = _ftpCredentials.Hostname
            };
            try
            {
                // FTP transfer:
                byte[] bytes = GetFile(fname, deleteFile);
                ftLog.Result = "Success";
                ftLog.Size = bytes.Length;
                // Blob Storage:
                string blobId = fname;
                BlobAgent blobAgent = new BlobAgent(_storageCredentials,blobContainerName);
                blobAgent.Add(blobId, bytes);
                ftLog.BlobId = string.Format("{0}/{1}", blobContainerName, blobId);
               
            }
            catch (Exception x)
            {
                StringBuilder xsb = new StringBuilder(x.Message);
                if (x.InnerException != null)
                {
                    xsb.Append(Environment.NewLine + x.InnerException.Message);
                }
                xsb.Append(Environment.NewLine + x.StackTrace);
                ftLog.Result = xsb.ToString();                
            }
            _logger.Log(_logName, ftLog.Json);
            return ftLog;

        }

        

        protected byte[] ToByteArray(List<String> lines)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.Append(line);
                sb.Append(Environment.NewLine);
            }
            return Encoding.ASCII.GetBytes(sb.ToString());
        }

    }
}
