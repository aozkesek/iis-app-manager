using System;
using System.Collections.Generic;

using NLog;

using Netas.Nipps.Aspect;
using Netas.Nipps.BaseDao;
using Netas.Nipps.BaseDao.V2;

using Netas.Nipps.LogManager.Data.Model;

namespace Netas.Nipps.LogManager.Logic.Impl
{
    public class NippsModuleLogic : IGenericLogicV2<NippsModule>
    {

        private IGenericLogicV2<NippsLog> mNippsLogLogic;
        private IGenericDaoV2<NippsModule> mNippsModuleDao;

        public IGenericLogicV2<NippsLog> NippsLogLogic { get { return mNippsLogLogic; } set { mNippsLogLogic = value; } }
        public IGenericDaoV2<NippsModule> NippsModuleDao { get { return mNippsModuleDao; } set { mNippsModuleDao = value; } }

        [PerformanceLoggingAdvice]
        public void Add(NippsModule t)
        {
            NormalizeLogReportLevel(t);

            NippsModuleDao.Add(t);
        }

        [PerformanceLoggingAdvice]
        public NippsModule Get(int id)
        {
            NippsModule nippsModule = NippsModuleDao.Get(id);
            nippsModule.ModuleLogInfo = GetLogInfo(nippsModule.ModuleId, nippsModule.ModuleName);
            return nippsModule;
        }

        [PerformanceLoggingAdvice]
        public NippsModule GetByName(string name)
        {
            NippsModule nippsModule = NippsModuleDao.GetByName(name);
            nippsModule.ModuleLogInfo = GetLogInfo(nippsModule.ModuleId, nippsModule.ModuleName);
            return nippsModule;
        }

        [PerformanceLoggingAdvice]
        public List<NippsModule> List(int pageNo)
        {
            List<NippsModule> nippsModules = NippsModuleDao.List(pageNo);
            for (int i = 0; i < nippsModules.Count; i++)
                nippsModules[i].ModuleLogInfo = GetLogInfo(nippsModules[i].ModuleId, nippsModules[i].ModuleName);
            return nippsModules;
        }

        [PerformanceLoggingAdvice]
        public List<NippsModule> List()
        {
            List<NippsModule> nippsModules = NippsModuleDao.List();
            for (int i = 0; i < nippsModules.Count; i++)
                nippsModules[i].ModuleLogInfo = GetLogInfo(nippsModules[i].ModuleId, nippsModules[i].ModuleName);
            return nippsModules;
        }

        public int PageSize { get { return NippsModuleDao.PageSize; } set { NippsModuleDao.PageSize = value; } }

        [PerformanceLoggingAdvice]
        public void Remove(NippsModule t)
        {
            NippsModuleDao.Remove(t);
        }

        [PerformanceLoggingAdvice]
        public void Update(NippsModule t)
        {
            NormalizeLogReportLevel(t);

            NippsModuleDao.Update(t);
        }

        private void NormalizeLogReportLevel(NippsModule t)
        {
            //it can not be less than Warn, if so higher to Error
            if (t.LogReportLevelId < NippsLogLevel.Warn)
                t.LogReportLevelId = NippsLogLevel.Error;
        }

        private string GetLogInfo(int moduleId, string moduleName)
        {
            int logCount = 0;

            try
            {
                //collect sub-modules' log 
                List<NippsModule> nippsModules = NippsModuleDao.List(new NippsModule { ParentId = moduleId });
                foreach (NippsModule nippsModule in nippsModules)
                    try
                    {
                        logCount += NippsLogLogic.List(new NippsLog { LogModuleName = nippsModule.ModuleName }).Count;
                    }
                    catch (Exception ex) { }
            }
            catch (Exception ex) { }
            
            try
            {
                //then own log
                logCount += NippsLogLogic.List(new NippsLog { LogModuleName = moduleName }).Count;
            }
            catch (Exception ex) { }

            if (logCount > 0)
                return string.Format("Fatal: [{0}]", logCount);
            else
                return null;

        }

        public List<NippsModule> List(NippsModule t, int pageNo)
        {
            List<NippsModule> nippsModules = NippsModuleDao.List(t, pageNo);
            for (int i = 0; i < nippsModules.Count; i++)
                nippsModules[i].ModuleLogInfo = GetLogInfo(nippsModules[i].ModuleId, nippsModules[i].ModuleName);
            return nippsModules;
        }

        public List<NippsModule> List(NippsModule t)
        {
            List<NippsModule> nippsModules = NippsModuleDao.List(t);
            for (int i = 0; i < nippsModules.Count; i++)
                nippsModules[i].ModuleLogInfo = GetLogInfo(nippsModules[i].ModuleId, nippsModules[i].ModuleName);
            return nippsModules;
        }
    }
}
