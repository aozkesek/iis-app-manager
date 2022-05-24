using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Org.Apps.BaseService
{
    [Serializable]
    [DataContract(Name = "AppsLogFileResponse")]
    public class AppsLogFileResponse
    {
        [DataMember(Name = "AppsLogFiles")]
        public List<AppsLogFile> AppsLogFiles { get; set; }
    }
}