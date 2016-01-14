using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Netas.Nipps.BaseDao.Model.Response;
using Netas.Nipps.LogManager.Data.Model;

namespace Netas.Nipps.LogManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name = "NippsModuleResponse")]
    public class NippsModuleResponse : BaseResponse
    {
        [DataMember(Name = "NippsModules")]
        public List<NippsModule> NippsModules { get; set; }
    }
}