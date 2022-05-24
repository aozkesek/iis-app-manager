using System;
using System.Runtime.Serialization;
using Org.Apps.BaseDao.Model.Request;

namespace Org.Apps.LicenseManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "AppsLicenseRequest")]
    public class AppsLicenseRequest : BaseRequest
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
