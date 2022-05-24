using System;
using System.Collections.Generic;

namespace Org.Apps.BaseDao.V2
{
    public abstract class AbstractLogicV2<T> : IGenericLogicV2<T>
    {
        private IGenericDaoV2<T> mTDao;

        public Int32 PageSize { get { return TDao.PageSize; } set { TDao.PageSize = value; } }

        public IGenericDaoV2<T> TDao { get { return mTDao; } set { mTDao = value; } }

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

        public virtual List<T> List(T t)
        {
            return TDao.List(t);
        }

        public virtual List<T> List(T t, int pageNo)
        {
            return TDao.List(t, pageNo);
        }
    }
}