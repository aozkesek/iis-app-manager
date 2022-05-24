using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Org.Apps.BaseDao.Model.Request;
using Org.Apps.LogManager.Data.Model;

namespace Org.Apps.LogManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "AppsLogRequest")]
    public class AppsLogRequest : BaseRequest
    {
        [DataMember(Name = "AppsLogs")]
        public List<AppsLog> AppsLogs { get; set; }
    }
}