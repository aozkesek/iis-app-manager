using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Org.Apps.DeployManager.Data.Model
{
    [Serializable]
    [DataContract(Name = "AppsPackage")]
    public class AppsPackage
    {
        [DataMember(Name="HostName")]
        public string HostName { get; set; }

        [DataMember(Name = "SiteName")]
        public string SiteName { get; set; }

        [DataMember(Name = "ApplicationPoolName")]
        public string ApplicationPoolName { get; set; }

        [DataMember(Name = "ApplicationName")]
        public string ApplicationName { get; set; }
        
        [DataMember(Name="PackageZIP")]
        public string PackageZIP { get; set; }

        public override string ToString()
        {
            return string.Format(
                "[HostName={0}, ApplicationPoolName={1}, SiteName={2}, ApplicationName={3}]",
                HostName, ApplicationPoolName, SiteName, ApplicationName
                );
        }
    }
}
