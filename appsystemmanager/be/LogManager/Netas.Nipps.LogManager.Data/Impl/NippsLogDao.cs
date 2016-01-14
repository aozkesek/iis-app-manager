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
    public class NippsLogDao : AbstractDaoV2<NippsLog, LogDbContext>
    {
        private static NLog.Logger mLogger = NLog.LogManager.GetCurrentClassLogger();
        public const string ModuleName = "LogManager";
        public const string ExceptionTypes = "SqlException;";

        public override IQueryable<NippsLog> GetTByNameQuery(LogDbContext dbContext, string name)
        {
            return from l in dbContext.NippsLogs
                   where l.LogModuleName == name
                   select l;
        }

        public override object GetTId(NippsLog t)
        {
            return t.LogId;
        }

        public override NLog.Logger GetTLogger()
        {
            return mLogger;
        }

        public override IQueryable<NippsLog> GetTQuery(LogDbContext dbContext, NippsLog t)
        {
            return from l in dbContext.NippsLogs
                   where l.LogId == t.LogId
                   select l;
        }

        public override IQueryable<NippsLog> GetTQuery(LogDbContext dbContext, int id)
        {
            return from l in dbContext.NippsLogs
                   where l.LogId == id
                   select l;
        }

        public override IQueryable<NippsLog> ListTQuery(LogDbContext dbContext)
        {
            return from l in dbContext.NippsLogs
                   orderby l.LogId
                   select l;
        }

        public override IQueryable<NippsLog> ListTQuery(LogDbContext dbContext, NippsLog t)
        {
            return from l in dbContext.NippsLogs
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
            return dbContext.NippsLogs;
        }

        public override void UpdateFrom(NippsLog tDest, NippsLog tSource)
        {
            tDest.CheckedBy = tSource.CheckedBy;
            tDest.UpdateDate = DateTime.Now;
        }

        public override IQueryable<NippsLog> UpdateTQuery(LogDbContext dbContext, NippsLog t)
        {
            return from l in dbContext.NippsLogs
                   where l.LogId == t.LogId
                   select l;
        }

        [PerformanceLoggingAdvice]
        public override void Add(NippsLog t)
        {
            t.CreateDate = DateTime.Now;
            t.UpdateDate = t.CreateDate; 
            base.Add(t);
        }

        [PerformanceLoggingAdvice]
        public override NippsLog Get(int id)
        {
            return base.Get(id);
        }

        [PerformanceLoggingAdvice]
        public override NippsLog GetByName(string name)
        {
            return base.GetByName(name);
        }

        [PerformanceLoggingAdvice]
        public override NippsLog GetByT(NippsLog t)
        {
            return base.GetByT(t);
        }

        [PerformanceLoggingAdvice]
        public override void Remove(NippsLog t)
        {
            base.Remove(t);
        }

        [PerformanceLoggingAdvice]
        public override void Update(NippsLog t)
        {
            t.UpdateDate = DateTime.Now;
            base.Update(t);
        }

        [PerformanceLoggingAdvice]
        public override List<NippsLog> List()
        {
            return base.List();
        }

        [PerformanceLoggingAdvice]
        public override List<NippsLog> List(int pageNo)
        {
            return base.List(pageNo);
        }

        [PerformanceLoggingAdvice]
        public override List<NippsLog> List(NippsLog t)
        {
            return base.List(t);
        }

        [PerformanceLoggingAdvice]
        public override List<NippsLog> List(NippsLog t, int pageNo)
        {
            return base.List(t, pageNo);
        }
 
    }
}
