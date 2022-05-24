using System;
using System.Runtime.Serialization;
using Org.Apps.BaseDao.Model.Response;

namespace Org.Apps.LicenseManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name = "AppsLicenseResponse")]
    public class AppsLicenseResponse : BaseResponse
    {
        [DataMember(Name = "License")]
        public AppsLicense License { get; set; }
    }
}
