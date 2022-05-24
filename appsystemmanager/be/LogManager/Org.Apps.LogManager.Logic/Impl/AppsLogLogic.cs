using System.Collections.Generic;
using System.Linq;

using Org.Apps.Aspect;
using Org.Apps.BaseDao;
using Org.Apps.BaseDao.V2;

using Org.Apps.LogManager.Data.Model;

namespace Org.Apps.LogManager.Logic.Impl
{
    public class AppsLogLogic : IGenericLogicV2<AppsLog>
    {
        private IGenericDaoV2<AppsLog> mAppsLogDao;
        private IGenericDaoV2<AppsModule> mAppsModuleDao;

        public IGenericDaoV2<AppsLog> AppsLogDao { get { return mAppsLogDao; } set { mAppsLogDao = value; } }
        public IGenericDaoV2<AppsModule> AppsModuleDao { get { return mAppsModuleDao; } set { mAppsModuleDao = value; } }

        [PerformanceLoggingAdvice]
        public void Add(AppsLog t)
        {
            AppsModule AppsModule = AppsModuleDao.GetByName(t.LogModuleName);
            //LogModule validated. go on...
            AppsLogDao.Add(t);
        }

        public AppsLog Get(int id)
        {
            return AppsLogDao.Get(id);
        }

        public AppsLog GetByName(string name)
        {
            return AppsLogDao.GetByName(name);
        }

        public List<AppsLog> List(int pageNo)
        {
            return AppsLogDao.List(pageNo);
        }

        public List<AppsLog> List()
        {
            return AppsLogDao.List();
        }

        public int PageSize {  get { return AppsLogDao.PageSize; } set { AppsLogDao.PageSize = value; } }

        public void Remove(AppsLog t)
        {
            //nobody can remove the AppsLog!!!
        }

        [PerformanceLoggingAdvice]
        public void Update(AppsLog t)
        {
            //only CheckedBy field can be updated by the requester!!! 
            AppsLog oldLog = AppsLogDao.Get(t.LogId);
            oldLog.CheckedBy = t.CheckedBy;
            AppsLogDao.Update(oldLog);
        }

        public List<AppsLog> List(AppsLog t, int pageNo)
        {
            //do not care the record not Fatal and checked before by anyone
            t.LogLevelId = AppsLogLevel.Fatal;
            t.CheckedBy = null;
            
            return AppsLogDao.List(t, pageNo);
        }

        public List<AppsLog> List(AppsLog t)
        {
            //do not care the record not Fatal and checked before by anyone
            t.LogLevelId = AppsLogLevel.Fatal;
            t.CheckedBy = null;
 
            return AppsLogDao.List(t);
        }
    }
}
