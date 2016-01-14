using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Web.Administration;

namespace Netas.Nipps.DeployManager.Data.Model
{
    [Serializable]
    [DataContract(Name = "NippsSite")]
    public class NippsSite
    {
        [DataMember(Name = "Name")]
        public string Name { get; set; }

        [DataMember(Name = "Port")]
        public string Port { get; set; }

        [DataMember(Name = "Protocol")]
        public string Protocol { get; set; }

        [DataMember(Name = "State")]
        public ObjectState State { get; set; }

        [DataMember(Name = "Applications")]
        public List<NippsApplication> NippsApplications { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("[");
            
            sb.AppendFormat("Name:{0}; ", Name);
            sb.AppendFormat("State:{0}; ", State);
            sb.AppendFormat("NippsApplications:{0}; ", NippsApplications);

            sb.Append("]");
            return sb.ToString();
        }
    }

}
