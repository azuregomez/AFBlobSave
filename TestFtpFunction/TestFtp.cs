using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Z.Ftp;
using System.Configuration;

namespace TestFtpFunction
{
    public static class TestFtp
    {
        [FunctionName("TestFtp")]
        public static void Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            FtpCredentials ftpCredentials = new FtpCredentials()
            {
                Hostname = ConfigurationManager.AppSettings["FtpHostname"],
                InboundDir = ConfigurationManager.AppSettings["FtpInboundDir"],
                OutboundDir = ConfigurationManager.AppSettings["FtpOutboundDir"],
                Password = ConfigurationManager.AppSettings["FtpPassword"],
                Username = ConfigurationManager.AppSettings["FtpUsername"],
                Key = ConfigurationManager.AppSettings["FtpKey"]
            };
            BlobFtpAgent theAgent = new RebexFtpAgent(ftpCredentials, null , null);

            req.CreateResponse(HttpStatusCode.OK, "Hello ");            
        }
    }
}
