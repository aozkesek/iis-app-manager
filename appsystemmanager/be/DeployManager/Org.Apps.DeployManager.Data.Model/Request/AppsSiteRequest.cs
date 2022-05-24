using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Org.Apps.BaseDao.Model.Request;

namespace Org.Apps.DeployManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "AppsSiteRequest")]
    public class AppsSiteRequest : BaseRequest
    {
        [DataMember(Name = "AppsSites")]
        public List<AppsSite> AppsSites { get; set; }
    }
}
