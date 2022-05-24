using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Org.Apps.BaseDao.Model.Request;

namespace Org.Apps.DeployManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "AppsApplicationRequest")]
    public class AppsApplicationRequest : BaseRequest
    {
        [DataMember(Name = "AppsApplications")]
        public List<AppsApplication> AppsApplications { get; set; }
    }
}
