using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Z.Logging
{
    public enum FileTransferType { PutFile, GetFile, RemoteDelete };
    public class FileTransferLog
    {
        public string RemoteAddress { get; set; }
        public long Size { get; set; }
        public string Result { get; set; }
        public FileTransferType TransferType { get; set; }
        public string FileName { get; set; }
        public string BlobId { get; set; }

        [ScriptIgnore]
        public string Json
        {
            get
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                string json = js.Serialize(this);
                return json;
            }
        }
      
    }        
}
