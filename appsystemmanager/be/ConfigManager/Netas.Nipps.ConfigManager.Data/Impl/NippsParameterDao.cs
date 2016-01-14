using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Netas.Nipps.BaseDao;
using Netas.Nipps.BaseDao.V2;
using Netas.Nipps.BaseDao.Model;

using Netas.Nipps.ConfigManager.Data.Model;

namespace Netas.Nipps.ConfigManager.Data.Impl
{
    public class NippsParameterDao : AbstractDaoV2<NippsParameter, ConfigDbContext>
    {

        private static NLog.Logger mLogger = NLog.LogManager.GetCurrentClassLogger();

        #region abstract methods implementation
        public override IQueryable<NippsParameter> GetTByNameQuery(ConfigDbContext dbContext, string name)
        {
            return from sp in dbContext.SystemParamaters
                        join pc in dbContext.ParamaterCategories on sp.CategoryId equals pc.CategoryId
                   where sp.ParameterName == name
                   select new NippsParameter {
                       CategoryName = pc.CategoryName,
                       ParameterName = sp.ParameterName,
                       ParameterValue = sp.ParameterValue,
                       CreateDate = sp.CreateDate,
                       UpdateDate = sp.UpdateDate };
        }

        public override object GetTId(NippsParameter t)
        {
            throw new NotImplementedException();
        }

        public override NLog.Logger GetTLogger()
        {
            return mLogger;
        }

        public override IQueryable<NippsParameter> GetTQuery(ConfigDbContext dbContext, NippsParameter t)
        {
            return from sp in dbContext.SystemParamaters
                        join pc in dbContext.ParamaterCategories on sp.CategoryId equals pc.CategoryId
                   where sp.ParameterName == t.ParameterName
                   select new NippsParameter { 
                       CategoryName = pc.CategoryName, 
                       ParameterName = sp.ParameterName, 
                       ParameterValue = sp.ParameterValue, 
                       CreateDate = sp.CreateDate, 
                       UpdateDate = sp.UpdateDate };
        }

        public override IQueryable<NippsParameter> GetTQuery(ConfigDbContext dbContext, int id)
        {
            return from sp in dbContext.SystemParamaters
                        join pc in dbContext.ParamaterCategories on sp.CategoryId equals pc.CategoryId
                   where sp.ParameterId == id
                   select new NippsParameter {
                       CategoryName = pc.CategoryName,
                       ParameterName = sp.ParameterName,
                       ParameterValue = sp.ParameterValue,
                       CreateDate = sp.CreateDate,
                       UpdateDate = sp.UpdateDate };

        }

        public override IQueryable<NippsParameter> ListTQuery(ConfigDbContext dbContext, NippsParameter t)
        {
            return from sp in dbContext.SystemParamaters
                        join pc in dbContext.ParamaterCategories on sp.CategoryId equals pc.CategoryId
                   where pc.CategoryName.Equals(t.CategoryName)
                   orderby sp.CategoryId
                   select new NippsParameter {
                       CategoryName = pc.CategoryName,
                       ParameterName = sp.ParameterName,
                       ParameterValue = sp.ParameterValue,
                       CreateDate = sp.CreateDate,
                       UpdateDate = sp.UpdateDate };

        }

        public override IQueryable<NippsParameter> ListTQuery(ConfigDbContext dbContext)
        {
            return from sp in dbContext.SystemParamaters
                        join pc in dbContext.ParamaterCategories on sp.CategoryId equals pc.CategoryId
                   orderby sp.CategoryId 
                   select new NippsParameter {
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

        public override void UpdateFrom(NippsParameter tDest, NippsParameter tSource)
        {
            tDest.CategoryName = tSource.CategoryName;
            tDest.ParameterValue = tSource.ParameterValue;
            tDest.ParameterValue = tSource.ParameterValue;
        }

        public override IQueryable<NippsParameter> UpdateTQuery(ConfigDbContext dbContext, NippsParameter t)
        {
            return from sp in dbContext.SystemParamaters
                        join pc in dbContext.ParamaterCategories on sp.CategoryId equals pc.CategoryId
                   where sp.ParameterName == t.ParameterName
                   select new NippsParameter {
                       CategoryName = pc.CategoryName,
                       ParameterName = sp.ParameterName,
                       ParameterValue = sp.ParameterValue,
                       CreateDate = sp.CreateDate,
                       UpdateDate = sp.UpdateDate };

        }
        #endregion

        #region overriden methods

        public override void Add(NippsParameter t)
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

        public override void Update(NippsParameter t)
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

        public override void Remove(NippsParameter t)
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
