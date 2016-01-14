using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Text;
using System;

namespace Netas.Nipps.LicenseManager.Data.Model
{
    [Serializable]
    [DataContract(Name="NippsLicense")]
    public class NippsLicense
    {
        [DataMember(Name="Type")]
        public string Type { get; set; }
        [DataMember(Name = "ValidFor")]
        public string ValidFor { get; set; }
        [DataMember(Name = "LicensedTo")]
        public string LicensedTo { get; set; }
        [DataMember(Name = "IssuedBy")]
        public string IssuedBy { get; set; }
        [DataMember(Name = "Version")]
        public string Version { get; set; }
        [DataMember(Name = "Services")]
        public List<PhoneService> Services { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendFormat(
                "[Type:{0}, ValidFor:{1}, LicensedTo:{2}, IssuedBy:{3}, Version:{4}, Services:[", 
                Type, ValidFor, LicensedTo, IssuedBy, Version);
            
            foreach (PhoneService ps in Services)
                sb.AppendFormat("{0},", ps.ToString());

            sb.Append("]");
            
            return sb.ToString().Replace(",]","]");
        }
    }
}
