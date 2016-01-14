using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Netas.Nipps.BaseDao.Model.Response;
using Netas.Nipps.LogManager.Data.Model;

namespace Netas.Nipps.LogManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name = "NippsLogResponse")]
    public class NippsLogResponse : BaseResponse
    {
        [DataMember(Name = "NippsLogs")]
        public List<NippsLog> NippsLogs { get; set; }
    }
}