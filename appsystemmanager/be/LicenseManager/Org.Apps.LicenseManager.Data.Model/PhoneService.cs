using System;
using System.Runtime.Serialization;

namespace Org.Apps.LicenseManager.Data.Model
{
    [Serializable]
    [DataContract(Name = "PhoneService")]
    public class PhoneService
    {
        [DataMember(Name = "Name")]
        public string Name { get; set; }
        [DataMember(Name = "Version")]
        public string Version { get; set; }

        public override string ToString()
        {
            return string.Format("[Name:{0}, Version:{1}]", Name, Version);
        }
    }
}
