using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Netas.Nipps.BaseDao.Model.Request;

namespace Netas.Nipps.DeployManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "NippsSiteRequest")]
    public class NippsSiteRequest : BaseRequest
    {
        [DataMember(Name = "NippsSites")]
        public List<NippsSite> NippsSites { get; set; }
    }
}
