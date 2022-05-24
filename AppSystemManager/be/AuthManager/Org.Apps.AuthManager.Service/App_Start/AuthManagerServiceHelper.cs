using System;
using System.Text;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;

using Autofac;

using Org.Apps.BaseDao;
using Org.Apps.BaseDao.Model.Response;
using Org.Apps.BaseService;

using Org.Apps.Crypto.Impl;
using Org.Apps.Crypto.Intf;

using Org.Apps.EMail.Impl;
using Org.Apps.EMail.Intf;

using Org.Apps.ConfigManager.Data.Model;
using Org.Apps.ConfigManager.Data.Model.Request;
using Org.Apps.ConfigManager.Data.Model.Response;

using Org.Apps.AuthManager.Data.Model;
using Org.Apps.AuthManager.Data.Impl;
using Org.Apps.AuthManager.Logic.Impl;
using Org.Apps.AuthManager.Logic.Intf;

namespace Org.Apps.AuthManager.Service
{
    public static class AuthManagerServiceHelper
    {
        private static string mLogManagerServiceUrl = ConfigurationManager.AppSettings["LogManagerServiceUrl"];

        public static string LogManagerServiceUrl { get { return mLogManagerServiceUrl; } }
        
        public static void RegisterComponents(ContainerBuilder containerBuilder)
        {
            try
            {
                AppsIoCHelper.RegisterDao<UserDao, IGenericDao<User>>(containerBuilder);

                AppsIoCHelper.RegisterDao<ShaAndBase64Crypto, IGenericCrypto>(containerBuilder);
                AppsIoCHelper.RegisterDao<EMailLogic, IEMailLogic>(containerBuilder);

                AppsIoCHelper.Register<UserLogic, IUserLogic>(containerBuilder);

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
                using (ILifetimeScope scope = AppsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IEMailLogic emailLogic = scope.Resolve<IEMailLogic>();
                    List<AppsParameter> eMailParams = GetEmailParameters();

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

        private static List<AppsParameter> GetEmailParameters()
        {

            string configServiceUrl = ConfigurationManager.AppSettings["ConfigManagerServiceUrl"];

            AppsParameterRequest AppsParameterRequest = new AppsParameterRequest()
            {
                Category = "EMAIL"
            };

            AppsParameterResponse AppsParameterResponse = RestHelper.RestPostObject<AppsParameterResponse, AppsParameterRequest>(configServiceUrl + "List", AppsParameterRequest);
            
            if (AppsParameterResponse.Result == Result.OK)
                return AppsParameterResponse.AppsParameters;
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach(string s in AppsParameterResponse.ResultMessages)
                    sb.Append(s).Append("\n\n");
                NLog.LogManager.GetCurrentClassLogger().Error(sb.ToString());
                throw new Exception(sb.ToString());
            }
                
        }


    }
}