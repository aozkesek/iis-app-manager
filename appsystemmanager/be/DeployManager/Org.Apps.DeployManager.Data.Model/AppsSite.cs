using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Web.Administration;

namespace Org.Apps.DeployManager.Data.Model
{
    [Serializable]
    [DataContract(Name = "AppsSite")]
    public class AppsSite
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
        public List<AppsApplication> AppsApplications { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("[");
            
            sb.AppendFormat("Name:{0}; ", Name);
            sb.AppendFormat("State:{0}; ", State);
            sb.AppendFormat("AppsApplications:{0}; ", AppsApplications);

            sb.Append("]");
            return sb.ToString();
        }
    }

}
