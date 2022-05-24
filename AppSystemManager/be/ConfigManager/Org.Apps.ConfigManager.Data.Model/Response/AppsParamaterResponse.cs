using Org.Apps.BaseDao.Model.Response;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Org.Apps.ConfigManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name = "AppsParameterResponse")]
    public class AppsParameterResponse : BaseResponse
    {
        [DataMember(Name = "AppsParameters")]
        public List<AppsParameter> AppsParameters { get; set; }
    }
}