using System;
using System.Runtime.Serialization;
using Netas.Nipps.BaseDao.Model.Request;

namespace Netas.Nipps.LicenseManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "NippsLicenseRequest")]
    public class NippsLicenseRequest : BaseRequest
    {
        [DataMember(Name = "Content")]
        public string Content { get; set; }
        [DataMember(Name = "Service")]
        public PhoneService Service { get; set; }

        public override string ToString()
        {
            return string.Format("[Content:{0}, Service:{1} | {2}]", Content, Service == null ? "(empty)" : Service.ToString(), base.ToString());
        }
    }
}
