using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System.IO;
using Microsoft.Azure.Storage.Auth;

namespace Z.Ftp
{
    public class BlobAgent
    {

        private CloudBlobClient _blobClient = null;
        private CloudBlobContainer _blobContainer = null;


        public BlobAgent(StorageCredentials credentials, string blobContainerName)
        {
            
            CloudStorageAccount cloudStorage = new CloudStorageAccount(credentials, true);
            _blobClient = cloudStorage.CreateCloudBlobClient();
            _blobContainer = _blobClient.GetContainerReference(blobContainerName.ToLower());
            bool created = _blobContainer.CreateIfNotExists();
            if (created)
            {
                var permissions = new BlobContainerPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;                
                _blobContainer.SetPermissions(permissions);
            }
        }


        /// <summary>
        /// Not tested
        /// </summary>
        /// <param name="lastModified"></param>
        /// <returns></returns>
        public int RemoveIfLastModifiedBefore(DateTime lastModified)
        {
            int removed = 0;
            List<string> names = new List<string>();
            foreach (IListBlobItem bi in _blobContainer.ListBlobs())
            {
                string blobName = bi.Uri.LocalPath.Substring(bi.Uri.LocalPath.LastIndexOf("/") + 1);
                ICloudBlob b = _blobContainer.GetBlobReferenceFromServer(blobName);
                b.FetchAttributes();
                if (b.Properties.LastModified < lastModified)
                {
                    b.Delete();
                    removed++;
                }
            }
            return removed;
        }
                
        public void Add(string blobId, byte[] bytes)
        {
            ICloudBlob blob = _blobContainer.GetBlockBlobReference(blobId);
            // delete if previously exists
            blob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);            
            blob.UploadFromByteArray(bytes,0,bytes.Length);
            blob.Properties.ContentType = "ByteArray";
            blob.SetProperties();
        }

        public void UploadFile(string blobId, string localPath)
        {
            ICloudBlob blob = _blobContainer.GetBlobReferenceFromServer(blobId);
            // delete if previously exists
            blob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
            blob.UploadFromFile(localPath);
            blob.Properties.ContentType = "ByteArray";
            blob.SetProperties();
        }

        public string GetText(string blobId)
        {
            ICloudBlob blob = _blobContainer.GetBlobReferenceFromServer(blobId);            
            Stream target = new MemoryStream();
            blob.DownloadToStream(target);
            target.Position = 0;
            StreamReader reader = new StreamReader(target);
            string text = reader.ReadToEnd();
            return text;
        }


    }
}
