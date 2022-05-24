using System;
using System.Collections.Generic;

namespace Org.Apps.BaseDao
{
    public interface IGenericDao<T>
    {
        Int32 PageSize { get; set; }

        String ConnectionName { get; set; }

        T Get(Int32 id);

        T GetByName(String name);

        T GetByT(T t);

        void Add(T t);

        void Update(T t);

        void Remove(T t);

        List<T> List();

        List<T> ListByCategory(string name);

        List<T> List(Int32 pageNo);
    }
}