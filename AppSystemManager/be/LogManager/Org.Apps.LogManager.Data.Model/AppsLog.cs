using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Org.Apps.BaseDao;

namespace Org.Apps.LogManager.Data.Model
{
    [Serializable]
    [DataContract]
    public class AppsLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember(Name = "LogId")]
        public int LogId { get; set; }

        [DataMember(Name = "LogModuleName")]
        public string LogModuleName { get; set; }

        [DataMember(Name = "LogLevelId")]
        public AppsLogLevel LogLevelId { get; set; }

        [DataMember(Name = "CreateDate")]
        public DateTime CreateDate { get; set; }

        [DataMember(Name = "UpdateDate")]
        public DateTime UpdateDate { get; set; }

        [DataMember(Name = "LogMessage")]
        public string LogMessage { get; set; }

        [DataMember(Name = "CheckedBy")]
        public string CheckedBy { get; set; }


        public override string ToString()
        {
            return string.Format(
                "[LogId={0}, LogModuleName={1}, LogLevelId={2}, CreateDate={3}, UpdateDate={4}, CheckedBy={5}, LogMessage={6} ]",
                LogId,
                LogModuleName,
                AppsLogLevelHelper.ToString(LogLevelId),
                CreateDate,
                UpdateDate,
                CheckedBy,
                LogMessage);
        }

    }
}
