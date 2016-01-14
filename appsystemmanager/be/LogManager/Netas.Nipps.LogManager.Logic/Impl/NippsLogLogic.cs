using System.Collections.Generic;
using System.Linq;

using Netas.Nipps.Aspect;
using Netas.Nipps.BaseDao;
using Netas.Nipps.BaseDao.V2;

using Netas.Nipps.LogManager.Data.Model;

namespace Netas.Nipps.LogManager.Logic.Impl
{
    public class NippsLogLogic : IGenericLogicV2<NippsLog>
    {
        private IGenericDaoV2<NippsLog> mNippsLogDao;
        private IGenericDaoV2<NippsModule> mNippsModuleDao;

        public IGenericDaoV2<NippsLog> NippsLogDao { get { return mNippsLogDao; } set { mNippsLogDao = value; } }
        public IGenericDaoV2<NippsModule> NippsModuleDao { get { return mNippsModuleDao; } set { mNippsModuleDao = value; } }

        [PerformanceLoggingAdvice]
        public void Add(NippsLog t)
        {
            NippsModule nippsModule = NippsModuleDao.GetByName(t.LogModuleName);
            //LogModule validated. go on...
            NippsLogDao.Add(t);
        }

        public NippsLog Get(int id)
        {
            return NippsLogDao.Get(id);
        }

        public NippsLog GetByName(string name)
        {
            return NippsLogDao.GetByName(name);
        }

        public List<NippsLog> List(int pageNo)
        {
            return NippsLogDao.List(pageNo);
        }

        public List<NippsLog> List()
        {
            return NippsLogDao.List();
        }

        public int PageSize {  get { return NippsLogDao.PageSize; } set { NippsLogDao.PageSize = value; } }

        public void Remove(NippsLog t)
        {
            //nobody can remove the NippsLog!!!
        }

        [PerformanceLoggingAdvice]
        public void Update(NippsLog t)
        {
            //only CheckedBy field can be updated by the requester!!! 
            NippsLog oldLog = NippsLogDao.Get(t.LogId);
            oldLog.CheckedBy = t.CheckedBy;
            NippsLogDao.Update(oldLog);
        }

        public List<NippsLog> List(NippsLog t, int pageNo)
        {
            //do not care the record not Fatal and checked before by anyone
            t.LogLevelId = NippsLogLevel.Fatal;
            t.CheckedBy = null;
            
            return NippsLogDao.List(t, pageNo);
        }

        public List<NippsLog> List(NippsLog t)
        {
            //do not care the record not Fatal and checked before by anyone
            t.LogLevelId = NippsLogLevel.Fatal;
            t.CheckedBy = null;
 
            return NippsLogDao.List(t);
        }
    }
}
