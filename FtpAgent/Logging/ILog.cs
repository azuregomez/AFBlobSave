using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z.Logging
{
    public interface ILog
    {
        void Log(string logName, string json);
    }
}
