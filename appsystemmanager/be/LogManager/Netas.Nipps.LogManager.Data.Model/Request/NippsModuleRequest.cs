using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Netas.Nipps.BaseDao.Model.Request;
using Netas.Nipps.LogManager.Data.Model;

namespace Netas.Nipps.LogManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "NippsModuleRequest")]
    public class NippsModuleRequest : BaseRequest
    {
        [DataMember(Name = "NippsModules")]
        public List<NippsModule> NippsModules { get; set; }
    }
}