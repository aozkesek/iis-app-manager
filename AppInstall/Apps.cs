using System;
using System.Collections.Generic;
using System.Text;

namespace AppsInstall
{
    
    [Serializable]
    public class Apps
    {
        public string Version { get; set; }
        public string MinVersion { get; set; }
        public List<AppsModule> IPPhoneServices { get; set; }
        public List<AppsModule> SMModules { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[ Version: {0}, MinVersion: {1}, IpPhoneServices: (",
                Version, MinVersion);
            foreach (AppsModule m in IPPhoneServices)
                sb.AppendFormat("{0} ", m.ToString());
            sb.AppendFormat("), SMModules: (");
            foreach (AppsModule m in SMModules)
                sb.AppendFormat("{0} ", m.ToString());
            sb.AppendFormat(") ]");
            return sb.ToString();
        }
    }

}
