using System;
using System.Runtime.Serialization;

namespace Org.Apps.BaseService
{
    [Serializable]
    [DataContract(Name = "AppsLogFile")]
    public class AppsLogFile
    {
        [DataMember(Name = "LogFileName")]
        public string LogFileName { get; set; }

        [DataMember(Name = "LogFileContent")]
        public string LogFileContent { get; set; }

        public override string ToString()
        {
            return string.Format(
                "[LogFileName: {0}, LogFileContent: {1}]",
                LogFileName,
                LogFileContent);
        }
    }
}