using System;
using System.Text;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;

using Autofac;

using Netas.Nipps.BaseDao;
using Netas.Nipps.BaseDao.Model.Response;
using Netas.Nipps.BaseService;

using Netas.Nipps.Crypto.Impl;
using Netas.Nipps.Crypto.Intf;

using Netas.Nipps.EMail.Impl;
using Netas.Nipps.EMail.Intf;

using Netas.Nipps.ConfigManager.Data.Model;
using Netas.Nipps.ConfigManager.Data.Model.Request;
using Netas.Nipps.ConfigManager.Data.Model.Response;

using Netas.Nipps.AuthManager.Data.Model;
using Netas.Nipps.AuthManager.Data.Impl;
using Netas.Nipps.AuthManager.Logic.Impl;
using Netas.Nipps.AuthManager.Logic.Intf;

namespace Netas.Nipps.AuthManager.Service
{
    public static class AuthManagerServiceHelper
    {
        private static string mLogManagerServiceUrl = ConfigurationManager.AppSettings["LogManagerServiceUrl"];

        public static string LogManagerServiceUrl { get { return mLogManagerServiceUrl; } }
        
        public static void RegisterComponents(ContainerBuilder containerBuilder)
        {
            try
            {
                NippsIoCHelper.RegisterDao<UserDao, IGenericDao<User>>(containerBuilder);

                NippsIoCHelper.RegisterDao<ShaAndBase64Crypto, IGenericCrypto>(containerBuilder);
                NippsIoCHelper.RegisterDao<EMailLogic, IEMailLogic>(containerBuilder);

                NippsIoCHelper.Register<UserLogic, IUserLogic>(containerBuilder);

            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            
        }

        public static void InitializeComponents()
        {
            try
            {
                using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IEMailLogic emailLogic = scope.Resolve<IEMailLogic>();
                    List<NippsParameter> eMailParams = GetEmailParameters();

                    emailLogic.EmailHost = eMailParams.Where(x => x.ParameterName.Equals("EmailHost")).First().ParameterValue;
                    emailLogic.EmailPort = int.Parse(eMailParams.Where(x => x.ParameterName.Equals("EmailPort")).First().ParameterValue);
                    emailLogic.EmailUser = eMailParams.Where(x => x.ParameterName.Equals("EmailUser")).First().ParameterValue;
                    emailLogic.EmailPassword = eMailParams.Where(x => x.ParameterName.Equals("EmailPassword")).First().ParameterValue;
                    emailLogic.EmailFrom = eMailParams.Where(x => x.ParameterName.Equals("EmailFrom")).First().ParameterValue;

                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            


        }

        private static List<NippsParameter> GetEmailParameters()
        {

            string configServiceUrl = ConfigurationManager.AppSettings["ConfigManagerServiceUrl"];

            NippsParameterRequest nippsParameterRequest = new NippsParameterRequest()
            {
                Category = "EMAIL"
            };

            NippsParameterResponse nippsParameterResponse = RestHelper.RestPostObject<NippsParameterResponse, NippsParameterRequest>(configServiceUrl + "List", nippsParameterRequest);
            
            if (nippsParameterResponse.Result == Result.OK)
                return nippsParameterResponse.NippsParameters;
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach(string s in nippsParameterResponse.ResultMessages)
                    sb.Append(s).Append("\n\n");
                NLog.LogManager.GetCurrentClassLogger().Error(sb.ToString());
                throw new Exception(sb.ToString());
            }
                
        }


    }
}