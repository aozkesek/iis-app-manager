using Org.Apps.BaseDao.Model.Request;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Org.Apps.ConfigManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "AppsParameterRequest")]
    public class AppsParameterRequest : BaseRequest
    {
        [DataMember(Name = "Category")]
        public string Category { get; set; }

        [DataMember(Name = "AppsParameters")]
        public List<AppsParameter> AppsParameters { get; set; }
    }
}