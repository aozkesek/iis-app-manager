using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Netas.Nipps.BaseDao.Model.Response;

namespace Netas.Nipps.DeployManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name = "NippsApplicationResponse")]
    public class NippsApplicationResponse : BaseResponse
    {
        [DataMember(Name = "NippsApplications")]
        public List<NippsApplication> NippsApplications { get; set; }
    }
}
