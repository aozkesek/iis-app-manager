using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

using Netas.Nipps.Aspect;
using Netas.Nipps.BaseDao;
using Netas.Nipps.BaseDao.V2;

using Netas.Nipps.LogManager.Data.Model;

namespace Netas.Nipps.LogManager.Data.Impl
{
    public class NippsModuleDao : AbstractDaoV2<NippsModule, LogDbContext>
    {
        private static NLog.Logger mLogger = NLog.LogManager.GetCurrentClassLogger();
        public const string ModuleName = "LogManager";
        public const string ExceptionTypes = "SqlException;";

        public override IQueryable<NippsModule> GetTByNameQuery(LogDbContext dbContext, string name)
        {
            return from l in dbContext.NippsModules
                   where l.ModuleName == name
                   select l;
        }

        public override object GetTId(NippsModule t)
        {
            return t.ModuleId;
        }

        public override NLog.Logger GetTLogger()
        {
            return mLogger;
        }

        public override IQueryable<NippsModule> GetTQuery(LogDbContext dbContext, NippsModule t)
        {
            return from l in dbContext.NippsModules
                   where l.ModuleId == t.ModuleId
                   select l;
        }

        public override IQueryable<NippsModule> GetTQuery(LogDbContext dbContext, int id)
        {
            return from l in dbContext.NippsModules
                   where l.ModuleId == id
                   select l;
        }

        public override IQueryable<NippsModule> ListTQuery(LogDbContext dbContext)
        {
            return from l in dbContext.NippsModules
                   orderby l.ModuleId
                   select l;
        }

        public override LogDbContext NewDbContext()
        {
            return new LogDbContext(ConnectionName);
        }

        public override DbSet SetT(LogDbContext dbContext)
        {
            return dbContext.NippsModules;
        }

        public override void UpdateFrom(NippsModule tDest, NippsModule tSource)
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

        public override IQueryable<NippsModule> UpdateTQuery(LogDbContext dbContext, NippsModule t)
        {
            return from l in dbContext.NippsModules
                   where l.ModuleId == t.ModuleId
                   select l;
        }

        [PerformanceLoggingAdvice]
        public override void Add(NippsModule t)
        {
            t.CreateDate = DateTime.Now;
            t.UpdateDate = t.CreateDate;
            base.Add(t);
        }

        [PerformanceLoggingAdvice]
        public override NippsModule Get(int id)
        {
            return base.Get(id);
        }

        [PerformanceLoggingAdvice]
        public override NippsModule GetByName(string name)
        {
            return base.GetByName(name);
        }

        [PerformanceLoggingAdvice]
        public override NippsModule GetByT(NippsModule t)
        {
            return base.GetByT(t);
        }

        [PerformanceLoggingAdvice]
        public override List<NippsModule> List()
        {
            return base.List();
        }

        [PerformanceLoggingAdvice]
        public override List<NippsModule> List(int pageNo)
        {
            return base.List(pageNo);
        }

        [PerformanceLoggingAdvice]
        public override void Remove(NippsModule t)
        {
            base.Remove(t);
        }

        [PerformanceLoggingAdvice]
        public override void Update(NippsModule t)
        {
            base.Update(t);
        }

        [PerformanceLoggingAdvice]
        public override List<NippsModule> List(NippsModule t)
        {
            return base.List(t);
        }

        [PerformanceLoggingAdvice]
        public override List<NippsModule> List(NippsModule t, int pageNo)
        {
            return base.List(t, pageNo);
        }

        public override IQueryable<NippsModule> ListTQuery(LogDbContext dbContext, NippsModule t)
        {
            return from l in dbContext.NippsModules
                   where (l.ParentId == t.ParentId) 
                        && (String.IsNullOrEmpty(t.ModuleName) || l.ModuleName.Contains(t.ModuleName))
                   orderby l.ModuleId
                   select l; 
        }
    }
}
