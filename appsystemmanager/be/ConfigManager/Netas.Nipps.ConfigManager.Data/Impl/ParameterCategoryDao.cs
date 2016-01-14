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
    public class ParameterCategoryDao : AbstractDao<ParameterCategory, ConfigDbContext>
    {
        private static Logger mLogger = LogManager.GetCurrentClassLogger();
        public const string ModuleName = "ConfigManager";
        public const string ExceptionTypes = "SqlException;";

        #region abstract method implementation

        public override IQueryable<ParameterCategory> GetTQuery(ConfigDbContext dbContext, int id)
        {
            return from pc in dbContext.ParamaterCategories
                   where pc.CategoryId == id
                   select pc;
        }

        public override IQueryable<ParameterCategory> GetTQuery(ConfigDbContext dbContext, ParameterCategory t)
        {
            return from pc in dbContext.ParamaterCategories
                   where pc.CategoryId == t.CategoryId
                   select pc;
        }

        public override IQueryable<ParameterCategory> GetTByNameQuery(ConfigDbContext dbContext, string name)
        {
            return from pc in dbContext.ParamaterCategories
                   where pc.CategoryName == name
                   select pc;
        }

        public override IQueryable<ParameterCategory> UpdateTQuery(ConfigDbContext dbContext, ParameterCategory t)
        {
            return from pc in dbContext.ParamaterCategories
                   where pc.CategoryId == t.CategoryId
                   select pc;
        }

        public override IQueryable<ParameterCategory> ListTQuery(ConfigDbContext dbContext)
        {
            return from pc in dbContext.ParamaterCategories
                   orderby pc.CategoryId
                   select pc;
        }

        public override DbSet SetT(ConfigDbContext dbContext)
        {
            return dbContext.ParamaterCategories;
        }

        public override void UpdateFrom(ParameterCategory tDest, ParameterCategory tSource)
        {
            tDest.UpdateFrom(tSource);
        }

        public override Logger GetTLogger()
        {
            return mLogger;
        }

        public override object GetTId(ParameterCategory t)
        {
            return t.CategoryId;
        }

        public override ConfigDbContext NewDbContext()
        {
            return new ConfigDbContext(ConnectionName);
        }

        #endregion abstract method implementation

        [PerformanceLoggingAdvice]
        public override ParameterCategory Get(int id)
        {
            return base.Get(id);
        }

        [PerformanceLoggingAdvice]
        public override ParameterCategory GetByName(string name)
        {
            return base.GetByName(name);
        }

        [PerformanceLoggingAdvice]
        public override List<ParameterCategory> List()
        {
            return base.List();
        }

        [PerformanceLoggingAdvice]
        public override List<ParameterCategory> List(int pageNo)
        {
            return base.List(pageNo);
        }

        [PerformanceLoggingAdvice]
        public override void Add(ParameterCategory t)
        {
            t.CreateDate = DateTime.Now;
            t.UpdateDate = t.CreateDate;
            base.Add(t);
        }

        [PerformanceLoggingAdvice]
        public override void Update(ParameterCategory t)
        {
            base.Update(t);
        }

        [PerformanceLoggingAdvice]
        public override void Remove(ParameterCategory t)
        {
            base.Remove(t);
        }

        public override IQueryable<ParameterCategory> ListTQueryByName(ConfigDbContext dbContext, string name)
        {
            throw new NotImplementedException();
        }
    }
}