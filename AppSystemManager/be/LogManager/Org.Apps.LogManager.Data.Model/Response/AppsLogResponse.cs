using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Org.Apps.BaseDao.Model.Response;
using Org.Apps.LogManager.Data.Model;

namespace Org.Apps.LogManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name = "AppsLogResponse")]
    public class AppsLogResponse : BaseResponse
    {
        [DataMember(Name = "AppsLogs")]
        public List<AppsLog> AppsLogs { get; set; }
    }
}