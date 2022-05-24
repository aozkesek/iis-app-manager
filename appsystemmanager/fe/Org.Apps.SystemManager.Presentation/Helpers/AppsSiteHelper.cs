using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

using Org.Apps.BaseService;

using Org.Apps.BaseDao.Model.Response;

using Org.Apps.LogManager.Data.Model;
using Org.Apps.LogManager.Data.Model.Request;
using Org.Apps.LogManager.Data.Model.Response;

using Org.Apps.ConfigManager.Data.Model;
using Org.Apps.ConfigManager.Data.Model.Request;
using Org.Apps.ConfigManager.Data.Model.Response;

using Org.Apps.DeployManager.Data.Model;
using Org.Apps.DeployManager.Data.Model.Response;

using Org.Apps.SystemManager.Presentation.Base;

namespace Org.Apps.SystemManager.Presentation.Helpers
{
    public sealed class AppsSiteHelper
    {
        public static readonly string ResultMessageView = "../Shared/ResultMessage";

        public static string BuildDeploymentServiceUrl(string hostName, string serviceName = "DeploymentService/")
        {
            string deploymentServiceUrl = CommonHelper.DeployManagerServiceUrl;
            int svcPos = deploymentServiceUrl.IndexOf("/Org.Apps.");
            int hostPos = deploymentServiceUrl.IndexOf("://");
            if (string.IsNullOrEmpty(hostName))
                return deploymentServiceUrl += serviceName;
            else
                return deploymentServiceUrl = deploymentServiceUrl.Substring(0, hostPos + 3)
                    + hostName
                    + deploymentServiceUrl.Substring(svcPos)
                    + serviceName;
        }

        public static List<AppsParameter> ListAppsHost()
        {

            string configServiceUrl = CommonHelper.ConfigManagerServiceUrl;

            AppsParameterRequest AppsParameterRequest = new AppsParameterRequest()
            {
                AppsParameters = new List<AppsParameter> { new AppsParameter { CategoryName = "AppsHOST", ParameterName = "AppsHOSTS" } }
            };

            AppsParameterResponse AppsParameterResponse = RestHelper.RestPostObject<AppsParameterResponse, AppsParameterRequest>(configServiceUrl + "AppsParameterService/Get", AppsParameterRequest);

            if (AppsParameterResponse.Result == Result.OK)
                return AppsParameterResponse.AppsParameters;
            else
                throw new Exception(AppsParameterResponse.ResultMessages[0]);

        }

        public static List<AppsSite> ListSite(string hostName)
        {
            string deploymentServiceUrl = BuildDeploymentServiceUrl(hostName) + "ListSite";
            AppsSiteResponse response = RestHelper.RestGet<AppsSiteResponse>(deploymentServiceUrl);
            if (response.Result == Result.OK)
                return response.AppsSites;

            foreach (string em in response.ResultMessages)
                NLog.LogManager.GetCurrentClassLogger().Error(em);

            return new List<AppsSite>();

        }

        public static List<AppsSite> ListAppsSite(string hostName)
        {
            string deploymentServiceUrl = BuildDeploymentServiceUrl(hostName) + "ListAppsSite";
            AppsSiteResponse response = RestHelper.RestGet<AppsSiteResponse>(deploymentServiceUrl);
            if (response.Result == Result.OK)
                return response.AppsSites;

            foreach (string em in response.ResultMessages)
                NLog.LogManager.GetCurrentClassLogger().Error(em);

            return new List<AppsSite>();

        }

        public static List<String> ListApplicationPool(string hostName)
        {
            string deploymentServiceUrl = BuildDeploymentServiceUrl(hostName) + "ListApplicationPool";
            AppsApplicationPoolResponse response = RestHelper.RestGet<AppsApplicationPoolResponse>(deploymentServiceUrl);
            if (response.Result == Result.OK)
                return response.AppsApplicationPools;

            foreach (String em in response.ResultMessages)
                NLog.LogManager.GetCurrentClassLogger().Error(em);

            return new List<string>();
        }

        public static List<Models.Apps> ListApps()
        {
            List<Models.Apps> AppsList = new List<Models.Apps>();
            NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

            try
            {
                //get list of the service-hosts
                List<AppsParameter> AppsHosts = AppsSiteHelper.ListAppsHost();

                foreach (AppsParameter AppsHost in AppsHosts)
                    foreach (string value in AppsHost.ParameterValue.Split('|'))
                    {
                        try
                        {
                            //get applications from the host
                            List<AppsSite> AppsSites = AppsSiteHelper.ListAppsSite(value);
                            foreach (AppsSite AppsSite in AppsSites)
                                foreach (AppsApplication AppsApplication in AppsSite.AppsApplications)
                                {
                                    //add only Org ip phone service application to the result list
                                    if (AppsApplication.Path.StartsWith("/Org.Apps.Service."))
                                    {
                                        Models.Apps Apps = new Models.Apps
                                        {
                                            HostName = value,
                                            ApplicationName = AppsApplication.Path,
                                            ApplicationPoolName = AppsApplication.ApplicationPoolName,
                                            SiteName = AppsSite.Name,
                                            State = AppsApplication.ApplicationPoolState,
                                            PhysicalPath = AppsApplication.PhysicalPath,
                                            Version = AppsApplication.Version,
                                            ModuleId = -1
                                        };
                                        AppsList.Add(Apps);
                                    }

                                }


                        }
                        catch (Exception ex)
                        {
                            Logger.Error("{0}: {1}", value, ex.ToString());
                        }

                    }

            }
            catch (Exception ex)
            {
                Logger.Error("{0}", ex.ToString());
            }

            return AppsList;
        }

        public static SelectList SelectListSite(string hostName)
        {
            List<SelectListItem> sites = new List<SelectListItem>();
            sites.Add(new SelectListItem { Text = Resources.Global.MessageSiteSelect, Value = "" });

            if (!String.IsNullOrEmpty(hostName))
                foreach (AppsSite site in ListSite(hostName))
                    sites.Add(new SelectListItem { Value = site.Name, Text = site.Name });

            return new SelectList(sites, "Value", "Text");
        }

        public static SelectList SelectListPool(string hostName)
        {
            List<SelectListItem> pools = new List<SelectListItem>();
            pools.Add(new SelectListItem { Text = Resources.Global.MessagePoolSelect, Value = "" });

            if (!String.IsNullOrEmpty(hostName))
                foreach (String pool in ListApplicationPool(hostName))
                    pools.Add(new SelectListItem { Value = pool, Text = pool });

            return new SelectList(pools, "Value", "Text");
        }

        public static SelectList SelectListHost()
        {
            List<SelectListItem> hosts = new List<SelectListItem>();
            hosts.Add(new SelectListItem { Text = Resources.Global.MessageHostSelect, Value = "" });

            foreach (AppsParameter host in ListAppsHost())
                foreach (string value in host.ParameterValue.Split('|'))
                    hosts.Add(new SelectListItem { Value = value, Text = value });
    
            return new SelectList(hosts, "Value", "Text");
        }

        public static List<AppsModule> ListAppsModule()
        {
            string logServiceUrl = CommonHelper.LogManagerServiceUrl + "AppsModuleService/List";
            AppsModuleRequest request = new AppsModuleRequest { PageNo = 1, PageSize = 1000 };
            AppsModuleResponse response = RestHelper.RestPostObject<AppsModuleResponse, AppsModuleRequest>(logServiceUrl, request);
            if (response.Result == Result.OK)
                return response.AppsModules;

            foreach (string em in response.ResultMessages)
                NLog.LogManager.GetCurrentClassLogger().Error(em);

            return new List<AppsModule>();

        }

        public static string ServiceLogUrl(ModuleNameParser mnp)
        {
            return ServiceBaseUrl(mnp.Host, mnp.Site, mnp.Application) + "/api/" + ConfigurationManager.AppSettings[mnp.Service + "LogUrl"];
        }

        public static string ServiceBaseUrl(string hostName, string siteName, string appName)
        {
            //find site from given host
            AppsSite AppsSite = AppsSiteHelper.ListAppsSite(hostName)
                .Where(s => s.Name.Equals(siteName))
                .Single();

            //build url from site bindings
            string baseUrl = AppsSite.Protocol
                + "://" + hostName
                + ":" + AppsSite.Port + appName;

            return baseUrl;
        }
    }
}