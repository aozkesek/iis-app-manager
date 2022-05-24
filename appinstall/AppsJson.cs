using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsInstall
{
    [Serializable]
    public class AppsJson
    {
        public AppsEnvironment Environment { get; set; }
        public Apps Apps { get; set; }

        public override string ToString()
        {
            return string.Format("[ Environment: {0}, Apps: {1} ]",
                Environment.ToString(), Apps.ToString());
        }
    }
}
