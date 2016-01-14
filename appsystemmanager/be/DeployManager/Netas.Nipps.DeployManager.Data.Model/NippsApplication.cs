using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Web.Administration;

namespace Netas.Nipps.DeployManager.Data.Model
{
    [Serializable]
    [DataContract(Name = "NippsApplication")]
    public class NippsApplication
    {
        [DataMember(Name = "PhysicalPath")]
        public string PhysicalPath { get; set; }

        [DataMember(Name = "Path")]
        public string Path { get; set; }

        [DataMember(Name = "Version")]
        public string Version { get; set; }
        
        [DataMember(Name = "ApplicationPoolName")]
        public string ApplicationPoolName { get; set; }

        [DataMember(Name = "ApplicationPoolState")]
        public ObjectState ApplicationPoolState { get; set; }

        public override string ToString()
        {
            return string.Format(
                "[PhysicalPath:{0}, Path:{1}, ApplicationPoolName:{2}, ApplicationPoolState:{3}]", 
                PhysicalPath, Path, ApplicationPoolName, ApplicationPoolState);
        }
    }
}
