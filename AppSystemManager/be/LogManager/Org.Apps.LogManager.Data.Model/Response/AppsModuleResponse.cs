using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Org.Apps.BaseDao.Model.Response;
using Org.Apps.LogManager.Data.Model;

namespace Org.Apps.LogManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name = "AppsModuleResponse")]
    public class AppsModuleResponse : BaseResponse
    {
        [DataMember(Name = "AppsModules")]
        public List<AppsModule> AppsModules { get; set; }
    }
}