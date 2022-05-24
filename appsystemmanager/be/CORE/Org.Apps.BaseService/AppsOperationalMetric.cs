using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Org.Apps.BaseService
{
    [Serializable]
    [DataContract(Name = "AppsOperationalMetric")]
    public class AppsOperationalMetric
    {
        [DataMember(Name="Name")]
        public string Name { get; set; }
        [DataMember(Name = "Path")]
        public string Path { get; set; }
        [DataMember(Name = "Active")]
        public bool Active { get; set; }
        [DataMember(Name = "Headers")]
        public List<string> Headers { get; set; } 
    }
}
