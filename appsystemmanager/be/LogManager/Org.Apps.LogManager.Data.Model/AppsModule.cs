using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using NLog.Targets;

using Org.Apps.BaseDao;


namespace Org.Apps.LogManager.Data.Model
{
    
    
    [Serializable]
    [DataContract(Name = "AppsModule")]
    public class AppsModule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember(Name = "ModuleId")]
        public int ModuleId { get; set; }

        [DataMember(Name = "ParentId")]
        public int ParentId { get; set; }

        [DataMember(Name = "ModuleName")]
        public string ModuleName { get; set; }

        [DataMember(Name = "ModuleStatus")]
        public AppsModuleStatus ModuleStatus { get; set; }

        [DataMember(Name = "ModuleLogInfo")]
        public string ModuleLogInfo { get; set; }
        
        [DataMember(Name = "LogLevelId")]
        public AppsLogLevel LogLevelId { get; set; }

        [DataMember(Name = "LogReportLevelId")]
        public AppsLogLevel LogReportLevelId { get; set; }

        [DataMember(Name = "CreateDate")]
        public DateTime CreateDate { get; set; }

        [DataMember(Name = "UpdateDate")]
        public DateTime UpdateDate { get; set; }

        [DataMember(Name = "ArchiveEvery")]
        public FileArchivePeriod ArchiveEvery { get; set; }

        [DataMember(Name = "ArchiveAboveSize")]
        public int ArchiveAboveSize { get; set; }

        [DataMember(Name = "MaxArchiveFiles")]
        public int MaxArchiveFiles { get; set; }

        public override string ToString()
        {
            return string.Format(
                "[ModuleId={0}, ModuleName={1}, LogLevelId={2}, LogReportLevelId={3}, CreateDate={4}, UpdateDate={5}, ArchiveEvery={6}, ArchiveAboveSize={7}, MaxArchiveFiles={8}, ParentId={9}, ModuleStatus={10}, ModuleLogInfo={11}]",
                ModuleId,
                ModuleName,
                AppsLogLevelHelper.ToString(LogLevelId),
                AppsLogLevelHelper.ToString(LogReportLevelId),
                CreateDate,
                UpdateDate,
                ArchiveEvery.ToString(),
                ArchiveAboveSize,
                MaxArchiveFiles,
                ParentId,
                AppsModuleStatusHelper.ToString(ModuleStatus),
                ModuleLogInfo);
        }


    }
}
