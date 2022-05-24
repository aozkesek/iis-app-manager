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
    public class AppsLogDao : AbstractDaoV2<AppsLog, LogDbContext>
    {
        private static NLog.Logger mLogger = NLog.LogManager.GetCurrentClassLogger();
        public const string ModuleName = "LogManager";
        public const string ExceptionTypes = "SqlException;";

        public override IQueryable<AppsLog> GetTByNameQuery(LogDbContext dbContext, string name)
        {
            return from l in dbContext.AppsLogs
                   where l.LogModuleName == name
                   select l;
        }

        public override object GetTId(AppsLog t)
        {
            return t.LogId;
        }

        public override NLog.Logger GetTLogger()
        {
            return mLogger;
        }

        public override IQueryable<AppsLog> GetTQuery(LogDbContext dbContext, AppsLog t)
        {
            return from l in dbContext.AppsLogs
                   where l.LogId == t.LogId
                   select l;
        }

        public override IQueryable<AppsLog> GetTQuery(LogDbContext dbContext, int id)
        {
            return from l in dbContext.AppsLogs
                   where l.LogId == id
                   select l;
        }

        public override IQueryable<AppsLog> ListTQuery(LogDbContext dbContext)
        {
            return from l in dbContext.AppsLogs
                   orderby l.LogId
                   select l;
        }

        public override IQueryable<AppsLog> ListTQuery(LogDbContext dbContext, AppsLog t)
        {
            return from l in dbContext.AppsLogs
                   where l.LogModuleName == t.LogModuleName 
                        && l.CheckedBy == t.CheckedBy
                        && (l.LogLevelId == t.LogLevelId || t.LogLevelId == null)
                   orderby l.LogId
                   select l;
        }

        public override LogDbContext NewDbContext()
        {
            return new LogDbContext(ConnectionName);
        }

        public override DbSet SetT(LogDbContext dbContext)
        {
            return dbContext.AppsLogs;
        }

        public override void UpdateFrom(AppsLog tDest, AppsLog tSource)
        {
            tDest.CheckedBy = tSource.CheckedBy;
            tDest.UpdateDate = DateTime.Now;
        }

        public override IQueryable<AppsLog> UpdateTQuery(LogDbContext dbContext, AppsLog t)
        {
            return from l in dbContext.AppsLogs
                   where l.LogId == t.LogId
                   select l;
        }

        [PerformanceLoggingAdvice]
        public override void Add(AppsLog t)
        {
            t.CreateDate = DateTime.Now;
            t.UpdateDate = t.CreateDate; 
            base.Add(t);
        }

        [PerformanceLoggingAdvice]
        public override AppsLog Get(int id)
        {
            return base.Get(id);
        }

        [PerformanceLoggingAdvice]
        public override AppsLog GetByName(string name)
        {
            return base.GetByName(name);
        }

        [PerformanceLoggingAdvice]
        public override AppsLog GetByT(AppsLog t)
        {
            return base.GetByT(t);
        }

        [PerformanceLoggingAdvice]
        public override void Remove(AppsLog t)
        {
            base.Remove(t);
        }

        [PerformanceLoggingAdvice]
        public override void Update(AppsLog t)
        {
            t.UpdateDate = DateTime.Now;
            base.Update(t);
        }

        [PerformanceLoggingAdvice]
        public override List<AppsLog> List()
        {
            return base.List();
        }

        [PerformanceLoggingAdvice]
        public override List<AppsLog> List(int pageNo)
        {
            return base.List(pageNo);
        }

        [PerformanceLoggingAdvice]
        public override List<AppsLog> List(AppsLog t)
        {
            return base.List(t);
        }

        [PerformanceLoggingAdvice]
        public override List<AppsLog> List(AppsLog t, int pageNo)
        {
            return base.List(t, pageNo);
        }
 
    }
}
