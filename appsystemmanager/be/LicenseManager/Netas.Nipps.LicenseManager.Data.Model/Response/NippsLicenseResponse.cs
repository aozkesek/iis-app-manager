using System;
using System.Runtime.Serialization;
using Netas.Nipps.BaseDao.Model.Response;

namespace Netas.Nipps.LicenseManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name = "NippsLicenseResponse")]
    public class NippsLicenseResponse : BaseResponse
    {
        [DataMember(Name = "License")]
        public NippsLicense License { get; set; }
    }
}
