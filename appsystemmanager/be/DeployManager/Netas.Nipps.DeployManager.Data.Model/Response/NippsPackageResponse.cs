using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Netas.Nipps.BaseDao.Model.Response;

namespace Netas.Nipps.DeployManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name = "NippsPackageResponse")]
    public class NippsPackageResponse : BaseResponse
    {
        [DataMember(Name = "NippsPackages")]
        public List<NippsPackage> NippsPackages { get; set; }
    }
}
