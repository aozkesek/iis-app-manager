using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Org.Apps.BaseDao.Model.Request;
using Org.Apps.LogManager.Data.Model;

namespace Org.Apps.LogManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "AppsModuleRequest")]
    public class AppsModuleRequest : BaseRequest
    {
        [DataMember(Name = "AppsModules")]
        public List<AppsModule> AppsModules { get; set; }
    }
}