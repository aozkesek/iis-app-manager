using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Netas.Nipps.BaseDao.Model.Request;
using Netas.Nipps.LogManager.Data.Model;

namespace Netas.Nipps.LogManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "NippsLogRequest")]
    public class NippsLogRequest : BaseRequest
    {
        [DataMember(Name = "NippsLogs")]
        public List<NippsLog> NippsLogs { get; set; }
    }
}