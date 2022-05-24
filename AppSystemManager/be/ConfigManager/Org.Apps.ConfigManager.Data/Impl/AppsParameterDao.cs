using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Org.Apps.BaseDao;
using Org.Apps.BaseDao.V2;
using Org.Apps.BaseDao.Model;

using Org.Apps.ConfigManager.Data.Model;

namespace Org.Apps.ConfigManager.Data.Impl
{
    public class AppsParameterDao : AbstractDaoV2<AppsParameter, ConfigDbContext>
    {

        private static NLog.Logger mLogger = NLog.LogManager.GetCurrentClassLogger();

        #region abstract methods implementation
        public override IQueryable<AppsParameter> GetTByNameQuery(ConfigDbContext dbContext, string name)
        {
            return from sp in dbContext.SystemParamaters
                        join pc in dbContext.ParamaterCategories on sp.CategoryId equals pc.CategoryId
                   where sp.ParameterName == name
                   select new AppsParameter {
                       CategoryName = pc.CategoryName,
                       ParameterName = sp.ParameterName,
                       ParameterValue = sp.ParameterValue,
                       CreateDate = sp.CreateDate,
                       UpdateDate = sp.UpdateDate };
        }

        public override object GetTId(AppsParameter t)
        {
            throw new NotImplementedException();
        }

        public override NLog.Logger GetTLogger()
        {
            return mLogger;
        }

        public override IQueryable<AppsParameter> GetTQuery(ConfigDbContext dbContext, AppsParameter t)
        {
            return from sp in dbContext.SystemParamaters
                        join pc in dbContext.ParamaterCategories on sp.CategoryId equals pc.CategoryId
                   where sp.ParameterName == t.ParameterName
                   select new AppsParameter { 
                       CategoryName = pc.CategoryName, 
                       ParameterName = sp.ParameterName, 
                       ParameterValue = sp.ParameterValue, 
                       CreateDate = sp.CreateDate, 
                       UpdateDate = sp.UpdateDate };
        }

        public override IQueryable<AppsParameter> GetTQuery(ConfigDbContext dbContext, int id)
        {
            return from sp in dbContext.SystemParamaters
                        join pc in dbContext.ParamaterCategories on sp.CategoryId equals pc.CategoryId
                   where sp.ParameterId == id
                   select new AppsParameter {
                       CategoryName = pc.CategoryName,
                       ParameterName = sp.ParameterName,
                       ParameterValue = sp.ParameterValue,
                       CreateDate = sp.CreateDate,
                       UpdateDate = sp.UpdateDate };

        }

        public override IQueryable<AppsParameter> ListTQuery(ConfigDbContext dbContext, AppsParameter t)
        {
            return from sp in dbContext.SystemParamaters
                        join pc in dbContext.ParamaterCategories on sp.CategoryId equals pc.CategoryId
                   where pc.CategoryName.Equals(t.CategoryName)
                   orderby sp.CategoryId
                   select new AppsParameter {
                       CategoryName = pc.CategoryName,
                       ParameterName = sp.ParameterName,
                       ParameterValue = sp.ParameterValue,
                       CreateDate = sp.CreateDate,
                       UpdateDate = sp.UpdateDate };

        }

        public override IQueryable<AppsParameter> ListTQuery(ConfigDbContext dbContext)
        {
            return from sp in dbContext.SystemParamaters
                        join pc in dbContext.ParamaterCategories on sp.CategoryId equals pc.CategoryId
                   orderby sp.CategoryId 
                   select new AppsParameter {
                       CategoryName = pc.CategoryName,
                       ParameterName = sp.ParameterName,
                       ParameterValue = sp.ParameterValue,
                       CreateDate = sp.CreateDate,
                       UpdateDate = sp.UpdateDate };

        }

        public override ConfigDbContext NewDbContext()
        {
            return new ConfigDbContext(ConnectionName);
        }

        public override System.Data.Entity.DbSet SetT(ConfigDbContext dbContext)
        {
            throw new NotImplementedException();
        }

        public override void UpdateFrom(AppsParameter tDest, AppsParameter tSource)
        {
            tDest.CategoryName = tSource.CategoryName;
            tDest.ParameterValue = tSource.ParameterValue;
            tDest.ParameterValue = tSource.ParameterValue;
        }

        public override IQueryable<AppsParameter> UpdateTQuery(ConfigDbContext dbContext, AppsParameter t)
        {
            return from sp in dbContext.SystemParamaters
                        join pc in dbContext.ParamaterCategories on sp.CategoryId equals pc.CategoryId
                   where sp.ParameterName == t.ParameterName
                   select new AppsParameter {
                       CategoryName = pc.CategoryName,
                       ParameterName = sp.ParameterName,
                       ParameterValue = sp.ParameterValue,
                       CreateDate = sp.CreateDate,
                       UpdateDate = sp.UpdateDate };

        }
        #endregion

        #region overriden methods

        public override void Add(AppsParameter t)
        {
            GetTLogger().Debug(t);
            using (ConfigDbContext dbContext = NewDbContext())
            {
                DateTime now = DateTime.Now;
                ParameterCategory pc;
                try
                {
                    pc = (from c in dbContext.ParamaterCategories
                         where c.CategoryName.Equals(t.CategoryName)
                         select c).First();
                }
                catch (Exception ex) 
                {
                    pc = new ParameterCategory { CategoryName = t.CategoryName, CreateDate = now, UpdateDate = now };
                    dbContext.ParamaterCategories.Add(pc);
                    dbContext.SaveChanges();
                }

                SystemParameter sp = new SystemParameter
                {
                    CategoryId = pc.CategoryId,
                    ParameterName = t.ParameterName,
                    ParameterValue = t.ParameterValue,
                    CreateDate = now,
                    UpdateDate = now
                };

                dbContext.SystemParamaters.Add(sp);

                dbContext.SaveChanges();
            }
        }

        public override void Update(AppsParameter t)
        {
            using (ConfigDbContext dbContext = NewDbContext())
            {
                SystemParameter currentT = (from sp in dbContext.SystemParamaters
                                               join pc in dbContext.ParamaterCategories on t.CategoryName equals pc.CategoryName
                                               where sp.ParameterName.Equals(t.ParameterName)
                                               select sp).First();
                
                GetTLogger().Debug("{0} updating with {1}...", currentT, t);

                currentT.ParameterValue = t.ParameterValue;
                currentT.UpdateDate = DateTime.Now;

                dbContext.SaveChanges();
            }
        }

        public override void Remove(AppsParameter t)
        {
            using (ConfigDbContext dbContext = NewDbContext())
            {
                SystemParameter currentT = (from sp in dbContext.SystemParamaters
                                            join pc in dbContext.ParamaterCategories on t.CategoryName equals pc.CategoryName
                                            where sp.ParameterName.Equals(t.ParameterName)
                                            select sp).First();

                GetTLogger().Debug("{0} deleting...", currentT);
                dbContext.SystemParamaters.Remove(currentT);
                dbContext.SaveChanges();
            }
        }

        #endregion
    }
}
