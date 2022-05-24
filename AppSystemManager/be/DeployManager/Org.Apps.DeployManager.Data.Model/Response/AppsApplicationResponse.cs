using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Org.Apps.BaseDao.Model.Response;

namespace Org.Apps.DeployManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name = "AppsApplicationResponse")]
    public class AppsApplicationResponse : BaseResponse
    {
        [DataMember(Name = "AppsApplications")]
        public List<AppsApplication> AppsApplications { get; set; }
    }
}
