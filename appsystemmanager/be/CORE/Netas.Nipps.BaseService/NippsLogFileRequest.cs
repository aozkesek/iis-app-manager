using System;
using System.Runtime.Serialization;

namespace Netas.Nipps.BaseService
{
    [Serializable]
    [DataContract(Name = "NippsLogFileRequest")]
    public class NippsLogFileRequest
    {
        [DataMember(Name = "LogStartDate")]
        public DateTime LogStartDate { get; set; }

        [DataMember(Name = "LogFinishDate")]
        public DateTime LogFinishDate { get; set; }
    }
}