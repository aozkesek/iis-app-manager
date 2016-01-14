using Netas.Nipps.BaseDao.Model.Response;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Netas.Nipps.ConfigManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name = "NippsParameterResponse")]
    public class NippsParameterResponse : BaseResponse
    {
        [DataMember(Name = "NippsParameters")]
        public List<NippsParameter> NippsParameters { get; set; }
    }
}