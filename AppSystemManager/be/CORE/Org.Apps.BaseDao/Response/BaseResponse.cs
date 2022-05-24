using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Org.Apps.BaseDao.Model.Response
{
    [Serializable]
    [DataContract(Name = "BaseResponse")]
    public abstract class BaseResponse
    {
        [DataMember(Name = "Version")]
        public String Version { get; set; }

        [DataMember(Name = "Result")]
        public Result Result { get; set; }

        [DataMember(Name = "ResultMessages")]
        public List<String> ResultMessages { get; set; }
    }
}