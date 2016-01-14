using System;
using System.Collections.Generic;

namespace Netas.Nipps.BaseDao
{
    public abstract class AbstractLogic<T> : IGenericLogic<T>
    {
        private IGenericDao<T> mTDao;

        public Int32 PageSize { get { return TDao.PageSize; } set { TDao.PageSize = value; } }

        public IGenericDao<T> TDao { get { return mTDao; } set { mTDao = value; } }

        public virtual T Get(int id)
        {
            return TDao.Get(id);
        }

        public virtual T GetByName(string name)
        {
            return TDao.GetByName(name);
        }

        public virtual void Add(T t)
        {
            TDao.Add(t);
        }

        public virtual void Update(T t)
        {
            TDao.Update(t);
        }

        public virtual List<T> List()
        {
            return TDao.List();
        }

        public virtual List<T> List(int pageNo)
        {
            return TDao.List(pageNo);
        }

        public virtual void Remove(T t)
        {
            TDao.Remove(t);
        }
    }
}