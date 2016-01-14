using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Netas.Nipps.BaseDao.Model.Response;

namespace Netas.Nipps.DeployManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name="NippsSiteResponse")]
    public class NippsSiteResponse : BaseResponse
    {
        [DataMember(Name="NippsSites")]
        public List<NippsSite> NippsSites { get; set; }
    }
}
