using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

using Org.Apps.Aspect;
using Org.Apps.BaseDao;
using Org.Apps.BaseDao.V2;

using Org.Apps.LogManager.Data.Model;

namespace Org.Apps.LogManager.Data.Impl
{
    public class AppsModuleDao : AbstractDaoV2<AppsModule, LogDbContext>
    {
        private static NLog.Logger mLogger = NLog.LogManager.GetCurrentClassLogger();
        public const string ModuleName = "LogManager";
        public const string ExceptionTypes = "SqlException;";

        public override IQueryable<AppsModule> GetTByNameQuery(LogDbContext dbContext, string name)
        {
            return from l in dbContext.AppsModules
                   where l.ModuleName == name
                   select l;
        }

        public override object GetTId(AppsModule t)
        {
            return t.ModuleId;
        }

        public override NLog.Logger GetTLogger()
        {
            return mLogger;
        }

        public override IQueryable<AppsModule> GetTQuery(LogDbContext dbContext, AppsModule t)
        {
            return from l in dbContext.AppsModules
                   where l.ModuleId == t.ModuleId
                   select l;
        }

        public override IQueryable<AppsModule> GetTQuery(LogDbContext dbContext, int id)
        {
            return from l in dbContext.AppsModules
                   where l.ModuleId == id
                   select l;
        }

        public override IQueryable<AppsModule> ListTQuery(LogDbContext dbContext)
        {
            return from l in dbContext.AppsModules
                   orderby l.ModuleId
                   select l;
        }

        public override LogDbContext NewDbContext()
        {
            return new LogDbContext(ConnectionName);
        }

        public override DbSet SetT(LogDbContext dbContext)
        {
            return dbContext.AppsModules;
        }

        public override void UpdateFrom(AppsModule tDest, AppsModule tSource)
        {   
            tDest.LogLevelId = tSource.LogLevelId;
            tDest.LogReportLevelId = tSource.LogReportLevelId;
            tDest.MaxArchiveFiles = tSource.MaxArchiveFiles;
            tDest.ParentId = tSource.ParentId;
            tDest.ModuleLogInfo = tSource.ModuleLogInfo;
            tDest.ModuleStatus = tSource.ModuleStatus;
            tDest.ArchiveAboveSize = tSource.ArchiveAboveSize;
            tDest.ArchiveEvery = tSource.ArchiveEvery;
            tDest.UpdateDate = DateTime.Now;
            
        }

        public override IQueryable<AppsModule> UpdateTQuery(LogDbContext dbContext, AppsModule t)
        {
            return from l in dbContext.AppsModules
                   where l.ModuleId == t.ModuleId
                   select l;
        }

        [PerformanceLoggingAdvice]
        public override void Add(AppsModule t)
        {
            t.CreateDate = DateTime.Now;
            t.UpdateDate = t.CreateDate;
            base.Add(t);
        }

        [PerformanceLoggingAdvice]
        public override AppsModule Get(int id)
        {
            return base.Get(id);
        }

        [PerformanceLoggingAdvice]
        public override AppsModule GetByName(string name)
        {
            return base.GetByName(name);
        }

        [PerformanceLoggingAdvice]
        public override AppsModule GetByT(AppsModule t)
        {
            return base.GetByT(t);
        }

        [PerformanceLoggingAdvice]
        public override List<AppsModule> List()
        {
            return base.List();
        }

        [PerformanceLoggingAdvice]
        public override List<AppsModule> List(int pageNo)
        {
            return base.List(pageNo);
        }

        [PerformanceLoggingAdvice]
        public override void Remove(AppsModule t)
        {
            base.Remove(t);
        }

        [PerformanceLoggingAdvice]
        public override void Update(AppsModule t)
        {
            base.Update(t);
        }

        [PerformanceLoggingAdvice]
        public override List<AppsModule> List(AppsModule t)
        {
            return base.List(t);
        }

        [PerformanceLoggingAdvice]
        public override List<AppsModule> List(AppsModule t, int pageNo)
        {
            return base.List(t, pageNo);
        }

        public override IQueryable<AppsModule> ListTQuery(LogDbContext dbContext, AppsModule t)
        {
            return from l in dbContext.AppsModules
                   where (l.ParentId == t.ParentId) 
                        && (String.IsNullOrEmpty(t.ModuleName) || l.ModuleName.Contains(t.ModuleName))
                   orderby l.ModuleId
                   select l; 
        }
    }
}
