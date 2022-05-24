using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Org.Apps.BaseDao.Model.Response;

namespace Org.Apps.DeployManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name="AppsSiteResponse")]
    public class AppsSiteResponse : BaseResponse
    {
        [DataMember(Name="AppsSites")]
        public List<AppsSite> AppsSites { get; set; }
    }
}
