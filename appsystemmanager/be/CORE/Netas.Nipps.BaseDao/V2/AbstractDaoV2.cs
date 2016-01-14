using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Netas.Nipps.BaseDao.V2
{
    public abstract class AbstractDaoV2<T, C> : IGenericDaoV2<T> where C : DbContext
    {
        private int mPageSize;
        private string mConnectionName;

        #region ABSTRACT METHODS

        public abstract IQueryable<T> GetTQuery(C dbContext, int id);

        public abstract IQueryable<T> GetTQuery(C dbContext, T t);

        public abstract IQueryable<T> GetTByNameQuery(C dbContext, string name);

        public abstract IQueryable<T> UpdateTQuery(C dbContext, T t);

        public abstract IQueryable<T> ListTQuery(C dbContext);

        public abstract IQueryable<T> ListTQuery(C dbContext, T t);

        public abstract DbSet SetT(C dbContext);

        public abstract void UpdateFrom(T tDest, T tSource);

        public abstract Logger GetTLogger();

        public abstract object GetTId(T t);

        public abstract C NewDbContext();

        #endregion ABSTRACT METHODS

        #region PROPERTIES

        public int PageSize { get { return mPageSize; } set { mPageSize = value; } }

        public string ConnectionName { get { return mConnectionName; } set { mConnectionName = value; } }

        #endregion PROPERTIES

        #region Implementation

        public virtual T Get(int id)
        {
            GetTLogger().Debug(id);
            using (C dbContext = NewDbContext())
            {
                IQueryable<T> records = GetTQuery(dbContext, id);
                if (records != null && records.Count() > 0)
                {
                    GetTLogger().Debug(records.First());
                    return records.First();
                }
                else
                    throw new NoDataFoundException();
            }
        }

        public virtual T GetByT(T t)
        {
            GetTLogger().Debug(t);
            using (C dbContext = NewDbContext())
            {
                IQueryable<T> records = GetTQuery(dbContext, t);
                if (records != null && records.Count() > 0)
                {
                    if (records.Count() > 1)
                        throw new TooManyRecordFoundException();
                    GetTLogger().Debug(records.First());
                    return records.First();
                }
                else
                    throw new NoDataFoundException();
            }
        }

        public virtual T GetByName(string name)
        {
            GetTLogger().Debug(name);
            using (C dbContext = NewDbContext())
            {
                IQueryable<T> records = GetTByNameQuery(dbContext, name);
                if (records != null && records.Count() > 0)
                {
                    if (records.Count() > 1)
                        throw new TooManyRecordFoundException();
                    GetTLogger().Debug(records.First());
                    return records.First();
                }
                else
                    throw new NoDataFoundException();
            }
        }

        public virtual void Add(T t)
        {
            GetTLogger().Debug(t);
            using (C dbContext = NewDbContext())
            {
                SetT(dbContext).Add(t);
                dbContext.SaveChanges();
            }
        }

        public virtual void Update(T t)
        {
            using (C dbContext = NewDbContext())
            {
                T currentT = (T)SetT(dbContext).Find(GetTId(t));
                GetTLogger().Debug("{0} updating with {1}...", currentT, t);
                UpdateFrom(currentT, t);
                dbContext.SaveChanges();
            }
        }

        public virtual List<T> List()
        {
            using (C dbContext = NewDbContext())
            {
                IQueryable<T> records = ListTQuery(dbContext);
                GetTLogger().Debug("{0} of {1} listed.", PageSize, records.Count());
                if (PageSize > 0)
                    return records.Take(PageSize).ToList();
                else
                    return records.ToList();
            }
        }

        public virtual List<T> List(int pageNo)
        {
            using (C dbContext = NewDbContext())
            {
                int startFrom = PageSize * (pageNo - 1);
                IQueryable<T> records = ListTQuery(dbContext);
                GetTLogger().Debug("from {0} to {1} of {2} listed.", startFrom, (startFrom + PageSize), records.Count());
                return records.Skip(startFrom).Take(PageSize).ToList();
            }
        }

        public virtual void Remove(T t)
        {
            using (C dbContext = NewDbContext())
            {
                T currentT = (T)SetT(dbContext).Find(GetTId(t));
                GetTLogger().Debug("{0} deleting...", currentT);
                SetT(dbContext).Remove(currentT);
                dbContext.SaveChanges();
            }
        }

        public virtual List<T> List(T t)
        {
            using (C dbContext = NewDbContext())
            {
                IQueryable<T> records = ListTQuery(dbContext, t);
                GetTLogger().Debug("{0} listed.", records.Count());
                return records.ToList();
            }
        }

        public virtual List<T> List(T t, int pageNo)
        {
            using (C dbContext = NewDbContext())
            {
                int startFrom = PageSize * (pageNo - 1);
                IQueryable<T> records = ListTQuery(dbContext, t);
                GetTLogger().Debug("from {0} to {1} of {2} listed.", startFrom, (startFrom + PageSize), records.Count());
                return records.Skip(startFrom).Take(PageSize).ToList();
            }
        }

        public List<T> ListByCategory(string category)
        {
            throw new NotImplementedException();
        }

        #endregion Implementation
    }
}