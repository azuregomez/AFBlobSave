using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z.Ftp
{
    public class FtpCredentials
    {
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string InboundDir { get; set; }
        public string OutboundDir { get; set; }
        public string Key { get; set; }
    }
}