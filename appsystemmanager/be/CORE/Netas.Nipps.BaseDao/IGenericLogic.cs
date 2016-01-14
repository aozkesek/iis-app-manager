using System;
using System.Collections.Generic;

namespace Netas.Nipps.BaseDao
{
    public interface IGenericLogic<T>
    {
        int PageSize { get; set; }

        T Get(Int32 id);

        T GetByName(String name);

        void Add(T t);

        void Update(T t);

        void Remove(T t);

        List<T> List();

        List<T> List(Int32 pageNo);
    }
}