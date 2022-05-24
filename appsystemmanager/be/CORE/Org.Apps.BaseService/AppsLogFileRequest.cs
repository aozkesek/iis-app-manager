using System;
using System.Runtime.Serialization;

namespace Org.Apps.BaseService
{
    [Serializable]
    [DataContract(Name = "AppsLogFileRequest")]
    public class AppsLogFileRequest
    {
        [DataMember(Name = "LogStartDate")]
        public DateTime LogStartDate { get; set; }

        [DataMember(Name = "LogFinishDate")]
        public DateTime LogFinishDate { get; set; }
    }
}