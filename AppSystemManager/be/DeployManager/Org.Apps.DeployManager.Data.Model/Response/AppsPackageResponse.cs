using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Org.Apps.BaseDao.Model.Response;

namespace Org.Apps.DeployManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name = "AppsPackageResponse")]
    public class AppsPackageResponse : BaseResponse
    {
        [DataMember(Name = "AppsPackages")]
        public List<AppsPackage> AppsPackages { get; set; }
    }
}
