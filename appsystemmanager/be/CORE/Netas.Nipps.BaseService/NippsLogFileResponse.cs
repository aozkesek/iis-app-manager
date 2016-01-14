using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Netas.Nipps.BaseService
{
    [Serializable]
    [DataContract(Name = "NippsLogFileResponse")]
    public class NippsLogFileResponse
    {
        [DataMember(Name = "NippsLogFiles")]
        public List<NippsLogFile> NippsLogFiles { get; set; }
    }
}