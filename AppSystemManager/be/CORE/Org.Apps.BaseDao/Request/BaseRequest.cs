using System;
using System.Runtime.Serialization;

namespace Org.Apps.BaseDao.Model.Request
{
    [Serializable]
    [DataContract(Name = "BaseRequest")]
    public abstract class BaseRequest
    {
        [DataMember(Name = "Version")]
        public String Version { get; set; }

        [DataMember(Name = "PageNo")]
        public Int32 PageNo { get; set; }

        [DataMember(Name = "PageSize")]
        public Int32 PageSize { get; set; }
    }
}