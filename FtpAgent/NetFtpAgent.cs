using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Security;
using System.IO;
using Microsoft.Azure.Storage.Auth;
using Z.Logging;

namespace Z.Ftp
{
    /// <summary>
    /// Ftp Agent that uses FtpRequest that is part of .net
    /// This agent is lame because it does not support SFTP (SSH)
    /// </summary>
    public class NetFtpAgent: BlobFtpAgent
    {

        public NetFtpAgent(FtpCredentials ftpCredentials, StorageCredentials storageCredentials, ILog logger) : base(ftpCredentials, storageCredentials, logger) { }

        protected override long PutFile(List<String> lines, string fname)
        {
           
            byte[] bytes = ToByteArray(lines);

            FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + _ftpCredentials.Hostname + "/" + fname));
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            ftpRequest.Proxy = null;
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = false;
            ftpRequest.Credentials = new NetworkCredential(_ftpCredentials.Username, _ftpCredentials.Password);

            using (Stream writer = ftpRequest.GetRequestStream())
            {
                writer.Write(bytes, 0, bytes.Length);
                writer.Close();
            }
            FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            ftpRequest = null;

            ftpResponse.Close();
            return bytes.Length;
        }

        public override List<String> GetFileNames(string extension = null, string path = null)
        {
            throw new Exception("Not Implemented");
        }

        public override List<String> GetDirectories(string path = null)
        {
            throw new Exception("Not Implemented");
        }


        public override void Delete(string fname)
        {
            throw new Exception("Not Implemented");
        }


        protected override byte[] GetFile(string fname, bool deleteFile=true)
        {
            FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + _ftpCredentials.Hostname + "/" + fname));
            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            ftpRequest.Proxy = null;
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = false;
            ftpRequest.Credentials = new NetworkCredential(_ftpCredentials.Username, _ftpCredentials.Password);
            

            FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            Stream stream = ftpResponse.GetResponseStream();
            byte[] b;
            using (BinaryReader br = new BinaryReader(stream))
            {
                b = br.ReadBytes((int)stream.Length);
            }
            stream.Close();
            if (deleteFile)
            {
                ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            }
            ftpResponse.Close();
            return b;
        }
    }
}
