using Autofac;
using Netas.Nipps.BaseDao.V2;
using Netas.Nipps.BaseService;
using Netas.Nipps.ConfigManager.Data.Impl;
using Netas.Nipps.ConfigManager.Data.Model;

using Netas.Nipps.ConfigManager.Logic.Impl.V2;
using Netas.Nipps.ConfigManager.Logic.Intf.V2;

using System;
using System.Configuration;

namespace Netas.Nipps.ConfigManager.Service
{
    public static class ConfigManagerServiceHelper
    {
        private static string mLogManagerServiceUrl = ConfigurationManager.AppSettings["LogManagerServiceUrl"];

        public static string LogManagerServiceUrl { get { return mLogManagerServiceUrl; } }

        public static void RegisterComponents(ContainerBuilder containerBuilder)
        {
            try
            {
                NippsIoCHelper.RegisterDao<NippsParameterDao, IGenericDaoV2<NippsParameter>>(containerBuilder);

                NippsIoCHelper.Register<NippsParameterLogicV2, INippsParameterLogicV2>(containerBuilder);
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