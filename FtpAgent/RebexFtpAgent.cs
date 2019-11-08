using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebex.Net;
using System.IO;
using Z.Logging;
using Microsoft.Azure.Storage.Auth;

namespace Z.Ftp
{

    // https://github.com/Azure/Azure-Functions/issues/665

    public class RebexFtpAgent: BlobFtpAgent
    {

        Sftp _sftp;
       
        public RebexFtpAgent(FtpCredentials ftpCredentials, StorageCredentials storageCredentials, ILog logger) : base(ftpCredentials, storageCredentials, logger)
        {
            Rebex.Licensing.Key = ftpCredentials.Key;
            _sftp = new Sftp();
        }

       

        protected override long PutFile(List<String> lines, string fname)
        {
            byte[] bytes = ToByteArray(lines);
            _sftp.Connect(_ftpCredentials.Hostname);
            _sftp.Login(_ftpCredentials.Username, _ftpCredentials.Password);
            MemoryStream stream = new MemoryStream(bytes);
            long nbytes = _sftp.PutFile(stream, _ftpCredentials.OutboundDir + "/" + fname);
            // disconnect 
            _sftp.Disconnect();
            return nbytes;
        }

        public override List<String> GetFileNames(string extension=null,string path=null)
        {
            List<String> fileList = new List<string>();            
            
            _sftp.Connect(_ftpCredentials.Hostname, 22);
            _sftp.Login(_ftpCredentials.Username, _ftpCredentials.Password);
            SftpItemCollection list;
			if(path == null)
			{
				if (string.IsNullOrEmpty(_ftpCredentials.InboundDir))
				{
					list = _sftp.GetList();
				}
				else
				{
					list = _sftp.GetList(_ftpCredentials.InboundDir);
				}
			}
			else
			{
				list = _sftp.GetList(path);
			}
            _sftp.Disconnect();
            foreach (SftpItem item in list)
            {
                if (item.IsFile && item.Length > 0 && (extension==null?true:item.Name.Contains(extension)))
                {
                    fileList.Add(item.Name);
                }
            }
            return fileList;
        }

        public override List<String> GetDirectories(string path = null)
        {
            List<String> dirList = new List<string>();
            _sftp.Connect(_ftpCredentials.Hostname);
            _sftp.Login(_ftpCredentials.Username, _ftpCredentials.Password);
            SftpItemCollection list = path == null ? _sftp.GetList() : _sftp.GetList(path);
            _sftp.Disconnect();
            foreach (SftpItem item in list)
            {
                if (item.IsDirectory)
                {
                    dirList.Add(item.Name);
                }
            }
            return dirList;
        }

        protected override byte[] GetFile(string fname, bool deleteFile=true)
        {
            _sftp.Connect(_ftpCredentials.Hostname);
            _sftp.Login(_ftpCredentials.Username, _ftpCredentials.Password);
            MemoryStream stream = new MemoryStream();
            long nbytes = _sftp.GetFile(_ftpCredentials.InboundDir + "/" + fname, stream);
            byte[] b = stream.ToArray();
            if (deleteFile) _sftp.DeleteFile(_ftpCredentials.InboundDir + "/" + fname);
            _sftp.Disconnect();
            return b;
        }


        public override void Delete(string fname)
        {
            _sftp.Connect(_ftpCredentials.Hostname);
            _sftp.Login(_ftpCredentials.Username, _ftpCredentials.Password);
            _sftp.DeleteFile(_ftpCredentials.InboundDir + "/" + fname);
            _sftp.Disconnect();
        }

    }
}
