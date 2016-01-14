using System;
using System.Collections.Generic;
using System.Text;

namespace NippsInstall
{
    
    [Serializable]
    public class Nipps
    {
        public string Version { get; set; }
        public string MinVersion { get; set; }
        public List<NippsModule> IPPhoneServices { get; set; }
        public List<NippsModule> SMModules { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[ Version: {0}, MinVersion: {1}, IpPhoneServices: (",
                Version, MinVersion);
            foreach (NippsModule m in IPPhoneServices)
                sb.AppendFormat("{0} ", m.ToString());
            sb.AppendFormat("), SMModules: (");
            foreach (NippsModule m in SMModules)
                sb.AppendFormat("{0} ", m.ToString());
            sb.AppendFormat(") ]");
            return sb.ToString();
        }
    }

}
