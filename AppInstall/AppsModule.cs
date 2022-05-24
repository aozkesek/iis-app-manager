using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsInstall
{
    [Serializable]
    public class AppsModule
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string MinVersion { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationPoolName { get; set; }

        public override string ToString()
        {
            return string.Format("Name:{0}, Version:{1}, MinVersion:{2}, ApplicationName:{3}, ApplicationPoolName:{4}",
                Name, Version, MinVersion, ApplicationName, ApplicationPoolName
                );
        }
    }

}
