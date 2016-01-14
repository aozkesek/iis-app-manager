using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Netas.Nipps.BaseDao.Model.Request;

namespace Netas.Nipps.DeployManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "NippsApplicationRequest")]
    public class NippsApplicationRequest : BaseRequest
    {
        [DataMember(Name = "NippsApplications")]
        public List<NippsApplication> NippsApplications { get; set; }
    }
}
