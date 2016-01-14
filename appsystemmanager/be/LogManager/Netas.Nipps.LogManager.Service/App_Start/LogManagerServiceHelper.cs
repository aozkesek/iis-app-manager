using System;

using Autofac;
using Netas.Nipps.BaseDao;
using Netas.Nipps.BaseDao.V2;
using Netas.Nipps.BaseService;

using Netas.Nipps.LogManager.Data.Model;
using Netas.Nipps.LogManager.Data.Impl;
using Netas.Nipps.LogManager.Logic.Impl;

namespace Netas.Nipps.LogManager.Service
{
    public static class LogManagerServiceHelper
    {

        public static void RegisterComponents(ContainerBuilder containerBuilder)
        {

            try
            {
                NippsIoCHelper.RegisterDao<NippsModuleDao, IGenericDaoV2<NippsModule>>(containerBuilder);
                NippsIoCHelper.RegisterDao<NippsLogDao, IGenericDaoV2<NippsLog>>(containerBuilder);

                NippsIoCHelper.Register<NippsModuleLogic, IGenericLogicV2<NippsModule>>(containerBuilder);
                NippsIoCHelper.Register<NippsLogLogic, IGenericLogicV2<NippsLog>>(containerBuilder);
            
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