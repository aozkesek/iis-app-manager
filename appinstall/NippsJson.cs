using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NippsInstall
{
    [Serializable]
    public class NippsJson
    {
        public NippsEnvironment Environment { get; set; }
        public Nipps Nipps { get; set; }

        public override string ToString()
        {
            return string.Format("[ Environment: {0}, Nipps: {1} ]",
                Environment.ToString(), Nipps.ToString());
        }
    }
}
