using Netas.Nipps.BaseDao.Model.Request;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Netas.Nipps.ConfigManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "NippsParameterRequest")]
    public class NippsParameterRequest : BaseRequest
    {
        [DataMember(Name = "Category")]
        public string Category { get; set; }

        [DataMember(Name = "NippsParameters")]
        public List<NippsParameter> NippsParameters { get; set; }
    }
}