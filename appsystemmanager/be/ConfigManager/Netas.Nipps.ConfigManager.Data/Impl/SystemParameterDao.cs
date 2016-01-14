using Netas.Nipps.Aspect;
using Netas.Nipps.BaseDao;
using Netas.Nipps.ConfigManager.Data.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Netas.Nipps.ConfigManager.Data.Impl
{
    public class SystemParameterDao : AbstractDao<SystemParameter, ConfigDbContext>
    {
        private static Logger mLogger = LogManager.GetCurrentClassLogger();

        #region abstract method implementation

        public override IQueryable<SystemParameter> GetTQuery(ConfigDbContext dbContext, int id)
        {
            return from pc in dbContext.SystemParamaters
                   where pc.ParameterId == id
                   select pc;
        }

        public override IQueryable<SystemParameter> GetTQuery(ConfigDbContext dbContext, SystemParameter t)
        {
            return from sp in dbContext.SystemParamaters
                   join pc in dbContext.ParamaterCategories on sp.CategoryId equals pc.CategoryId
                   where sp.ParameterName == t.ParameterName && pc.CategoryId == t.CategoryId
                   select sp;
        }

        public override IQueryable<SystemParameter> GetTByNameQuery(ConfigDbContext dbContext, string name)
        {
            return from pc in dbContext.SystemParamaters
                   where pc.ParameterName == name
                   select pc;
        }

        public override IQueryable<SystemParameter> UpdateTQuery(ConfigDbContext dbContext, SystemParameter t)
        {
            return from sp in dbContext.SystemParamaters
                   where sp.ParameterId == t.ParameterId
                   select sp;
        }

        public override IQueryable<SystemParameter> ListTQuery(ConfigDbContext dbContext)
        {
            return from sp in dbContext.SystemParamaters
                   orderby sp.ParameterId
                   select sp;
        }

        public override IQueryable<SystemParameter> ListTQueryByName(ConfigDbContext dbContext, string name)
        {
            return from sp in dbContext.SystemParamaters
                   join pc in dbContext.ParamaterCategories on sp.CategoryId equals pc.CategoryId
                   where pc.CategoryName == name
                   select sp;
        }

        public override DbSet SetT(ConfigDbContext dbContext)
        {
            return dbContext.SystemParamaters;
        }

        public override void UpdateFrom(SystemParameter tDest, SystemParameter tSource)
        {
            tDest.UpdateFrom(tSource);
        }

        public override Logger GetTLogger()
        {
            return mLogger;
        }

        public override object GetTId(SystemParameter t)
        {
            return t.ParameterId;
        }

        public override ConfigDbContext NewDbContext()
        {
            return new ConfigDbContext(ConnectionName);
        }

        #endregion abstract method implementation

        [PerformanceLoggingAdvice]
        public override SystemParameter Get(int id)
        {
            return base.Get(id);
        }

        [PerformanceLoggingAdvice]
        public override SystemParameter GetByName(string name)
        {
            return base.GetByName(name);
        }

        [PerformanceLoggingAdvice]
        public override List<SystemParameter> List()
        {
            return base.List();
        }

        [PerformanceLoggingAdvice]
        public override List<SystemParameter> List(int pageNo)
        {
            return base.List(pageNo);
        }

        [PerformanceLoggingAdvice]
        public override void Add(SystemParameter t)
        {
            t.CreateDate = DateTime.Now;
            t.UpdateDate = t.CreateDate;
            base.Add(t);
        }

        [PerformanceLoggingAdvice]
        public override void Update(SystemParameter t)
        {
            base.Update(t);
        }

        [PerformanceLoggingAdvice]
        public override void Remove(SystemParameter t)
        {
            base.Remove(t);
        }
    }
}