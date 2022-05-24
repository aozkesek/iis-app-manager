using Autofac;
using Org.Apps.BaseDao.V2;
using Org.Apps.BaseService;
using Org.Apps.ConfigManager.Data.Impl;
using Org.Apps.ConfigManager.Data.Model;

using Org.Apps.ConfigManager.Logic.Impl.V2;
using Org.Apps.ConfigManager.Logic.Intf.V2;

using System;
using System.Configuration;

namespace Org.Apps.ConfigManager.Service
{
    public static class ConfigManagerServiceHelper
    {
        private static string mLogManagerServiceUrl = ConfigurationManager.AppSettings["LogManagerServiceUrl"];

        public static string LogManagerServiceUrl { get { return mLogManagerServiceUrl; } }

        public static void RegisterComponents(ContainerBuilder containerBuilder)
        {
            try
            {
                AppsIoCHelper.RegisterDao<AppsParameterDao, IGenericDaoV2<AppsParameter>>(containerBuilder);

                AppsIoCHelper.Register<AppsParameterLogicV2, IAppsParameterLogicV2>(containerBuilder);
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Fatal(ex.ToString());
                
            }
        }

        public static void InitializeComponents()
        {
        }
    }
}