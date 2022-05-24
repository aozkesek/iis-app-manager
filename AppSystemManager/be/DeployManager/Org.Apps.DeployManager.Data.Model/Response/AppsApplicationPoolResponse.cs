using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Org.Apps.BaseDao.Model.Response;

namespace Org.Apps.DeployManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name = "AppsApplicationPoolResponse")]
    public class AppsApplicationPoolResponse : BaseResponse
    {
        [DataMember(Name = "AppsApplicationPools")]
        public List<String> AppsApplicationPools { get; set; }
    }
}
