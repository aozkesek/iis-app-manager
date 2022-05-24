using System;
using System.Collections.Generic;

using NLog;

using Org.Apps.Aspect;
using Org.Apps.BaseDao;
using Org.Apps.BaseDao.V2;

using Org.Apps.LogManager.Data.Model;

namespace Org.Apps.LogManager.Logic.Impl
{
    public class AppsModuleLogic : IGenericLogicV2<AppsModule>
    {

        private IGenericLogicV2<AppsLog> mAppsLogLogic;
        private IGenericDaoV2<AppsModule> mAppsModuleDao;

        public IGenericLogicV2<AppsLog> AppsLogLogic { get { return mAppsLogLogic; } set { mAppsLogLogic = value; } }
        public IGenericDaoV2<AppsModule> AppsModuleDao { get { return mAppsModuleDao; } set { mAppsModuleDao = value; } }

        [PerformanceLoggingAdvice]
        public void Add(AppsModule t)
        {
            NormalizeLogReportLevel(t);

            AppsModuleDao.Add(t);
        }

        [PerformanceLoggingAdvice]
        public AppsModule Get(int id)
        {
            AppsModule AppsModule = AppsModuleDao.Get(id);
            AppsModule.ModuleLogInfo = GetLogInfo(AppsModule.ModuleId, AppsModule.ModuleName);
            return AppsModule;
        }

        [PerformanceLoggingAdvice]
        public AppsModule GetByName(string name)
        {
            AppsModule AppsModule = AppsModuleDao.GetByName(name);
            AppsModule.ModuleLogInfo = GetLogInfo(AppsModule.ModuleId, AppsModule.ModuleName);
            return AppsModule;
        }

        [PerformanceLoggingAdvice]
        public List<AppsModule> List(int pageNo)
        {
            List<AppsModule> AppsModules = AppsModuleDao.List(pageNo);
            for (int i = 0; i < AppsModules.Count; i++)
                AppsModules[i].ModuleLogInfo = GetLogInfo(AppsModules[i].ModuleId, AppsModules[i].ModuleName);
            return AppsModules;
        }

        [PerformanceLoggingAdvice]
        public List<AppsModule> List()
        {
            List<AppsModule> AppsModules = AppsModuleDao.List();
            for (int i = 0; i < AppsModules.Count; i++)
                AppsModules[i].ModuleLogInfo = GetLogInfo(AppsModules[i].ModuleId, AppsModules[i].ModuleName);
            return AppsModules;
        }

        public int PageSize { get { return AppsModuleDao.PageSize; } set { AppsModuleDao.PageSize = value; } }

        [PerformanceLoggingAdvice]
        public void Remove(AppsModule t)
        {
            AppsModuleDao.Remove(t);
        }

        [PerformanceLoggingAdvice]
        public void Update(AppsModule t)
        {
            NormalizeLogReportLevel(t);

            AppsModuleDao.Update(t);
        }

        private void NormalizeLogReportLevel(AppsModule t)
        {
            //it can not be less than Warn, if so higher to Error
            if (t.LogReportLevelId < AppsLogLevel.Warn)
                t.LogReportLevelId = AppsLogLevel.Error;
        }

        private string GetLogInfo(int moduleId, string moduleName)
        {
            int logCount = 0;

            try
            {
                //collect sub-modules' log 
                List<AppsModule> AppsModules = AppsModuleDao.List(new AppsModule { ParentId = moduleId });
                foreach (AppsModule AppsModule in AppsModules)
                    try
                    {
                        logCount += AppsLogLogic.List(new AppsLog { LogModuleName = AppsModule.ModuleName }).Count;
                    }
                    catch (Exception ex) { }
            }
            catch (Exception ex) { }
            
            try
            {
                //then own log
                logCount += AppsLogLogic.List(new AppsLog { LogModuleName = moduleName }).Count;
            }
            catch (Exception ex) { }

            if (logCount > 0)
                return string.Format("Fatal: [{0}]", logCount);
            else
                return null;

        }

        public List<AppsModule> List(AppsModule t, int pageNo)
        {
            List<AppsModule> AppsModules = AppsModuleDao.List(t, pageNo);
            for (int i = 0; i < AppsModules.Count; i++)
                AppsModules[i].ModuleLogInfo = GetLogInfo(AppsModules[i].ModuleId, AppsModules[i].ModuleName);
            return AppsModules;
        }

        public List<AppsModule> List(AppsModule t)
        {
            List<AppsModule> AppsModules = AppsModuleDao.List(t);
            for (int i = 0; i < AppsModules.Count; i++)
                AppsModules[i].ModuleLogInfo = GetLogInfo(AppsModules[i].ModuleId, AppsModules[i].ModuleName);
            return AppsModules;
        }
    }
}
