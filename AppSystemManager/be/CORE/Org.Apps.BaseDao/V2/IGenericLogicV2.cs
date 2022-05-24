using System;
using System.Collections.Generic;

namespace Org.Apps.BaseDao.V2
{
    public interface IGenericLogicV2<T> : IGenericLogic<T>
    {
        List<T> List(T t);

        List<T> List(T t, Int32 pageNo);
    }
}