using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsInstall
{
    [Serializable]
    public class AppsEnvironment
    {
        public string MachineName { get; set; }
        public string IISHostPort { get; set; }
        public string IISwwwroot { get; set; }
        public string IISSiteName { get; set; }
        public string IISApplicationPoolName { get; set; }
        public string IISSMHostPort { get; set; }
        public string IISLocalAdmin { get; set; }
        public string SqlHost { get; set; }
        public string SqlDatabase { get; set; }
        public string SqlLocalAdmin { get; set; }
        public string SqlUser { get; set; }

        public override string ToString()
        {
            return string.Format(
                "[IISHostPort:{0}, IISwwwroot:{1}, IISSiteName:{2}, IISApplicationPoolName:{3}, IISSMHostPort:{4}, IISLocalAdmin:{5}, SqlHost:{6}, SqlDatabase:{7}, SqlLocalAdmin:{8}, SqlUser:{9}, MachineName:{10}]",
                IISHostPort, IISwwwroot, IISSiteName, IISApplicationPoolName, IISSMHostPort, IISLocalAdmin, SqlHost, SqlDatabase, SqlLocalAdmin, SqlUser, MachineName);
        }
    }
}
