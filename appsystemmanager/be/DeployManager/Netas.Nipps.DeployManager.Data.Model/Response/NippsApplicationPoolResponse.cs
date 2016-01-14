using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Netas.Nipps.BaseDao.Model.Response;

namespace Netas.Nipps.DeployManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name = "NippsApplicationPoolResponse")]
    public class NippsApplicationPoolResponse : BaseResponse
    {
        [DataMember(Name = "NippsApplicationPools")]
        public List<String> NippsApplicationPools { get; set; }
    }
}
