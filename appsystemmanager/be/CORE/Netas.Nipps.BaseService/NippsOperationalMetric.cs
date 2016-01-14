using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Netas.Nipps.BaseService
{
    [Serializable]
    [DataContract(Name = "NippsOperationalMetric")]
    public class NippsOperationalMetric
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
