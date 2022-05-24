using System;

using Autofac;
using Org.Apps.BaseDao;
using Org.Apps.BaseDao.V2;
using Org.Apps.BaseService;

using Org.Apps.LogManager.Data.Model;
using Org.Apps.LogManager.Data.Impl;
using Org.Apps.LogManager.Logic.Impl;

namespace Org.Apps.LogManager.Service
{
    public static class LogManagerServiceHelper
    {

        public static void RegisterComponents(ContainerBuilder containerBuilder)
        {

            try
            {
                AppsIoCHelper.RegisterDao<AppsModuleDao, IGenericDaoV2<AppsModule>>(containerBuilder);
                AppsIoCHelper.RegisterDao<AppsLogDao, IGenericDaoV2<AppsLog>>(containerBuilder);

                AppsIoCHelper.Register<AppsModuleLogic, IGenericLogicV2<AppsModule>>(containerBuilder);
                AppsIoCHelper.Register<AppsLogLogic, IGenericLogicV2<AppsLog>>(containerBuilder);
            
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Fatal(ex);
                RestHelper.ReportCriticalError("LogManager", ex.ToString());
            }
            

        }

        public static void InitializeComponents()
        {

        }

    }

}