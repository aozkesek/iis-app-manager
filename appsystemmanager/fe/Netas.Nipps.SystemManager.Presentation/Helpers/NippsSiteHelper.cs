using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

using Netas.Nipps.BaseService;

using Netas.Nipps.BaseDao.Model.Response;

using Netas.Nipps.LogManager.Data.Model;
using Netas.Nipps.LogManager.Data.Model.Request;
using Netas.Nipps.LogManager.Data.Model.Response;

using Netas.Nipps.ConfigManager.Data.Model;
using Netas.Nipps.ConfigManager.Data.Model.Request;
using Netas.Nipps.ConfigManager.Data.Model.Response;

using Netas.Nipps.DeployManager.Data.Model;
using Netas.Nipps.DeployManager.Data.Model.Response;

using Netas.Nipps.SystemManager.Presentation.Base;

namespace Netas.Nipps.SystemManager.Presentation.Helpers
{
    public sealed class NippsSiteHelper
    {
        public static readonly string ResultMessageView = "../Shared/ResultMessage";

        public static string BuildDeploymentServiceUrl(string hostName, string serviceName = "DeploymentService/")
        {
            string deploymentServiceUrl = CommonHelper.DeployManagerServiceUrl;
            int svcPos = deploymentServiceUrl.IndexOf("/Netas.Nipps.");
            int hostPos = deploymentServiceUrl.IndexOf("://");
            if (string.IsNullOrEmpty(hostName))
                return deploymentServiceUrl += serviceName;
            else
                return deploymentServiceUrl = deploymentServiceUrl.Substring(0, hostPos + 3)
                    + hostName
                    + deploymentServiceUrl.Substring(svcPos)
                    + serviceName;
        }

        public static List<NippsParameter> ListNippsHost()
        {

            string configServiceUrl = CommonHelper.ConfigManagerServiceUrl;

            NippsParameterRequest nippsParameterRequest = new NippsParameterRequest()
            {
                NippsParameters = new List<NippsParameter> { new NippsParameter { CategoryName = "NIPPSHOST", ParameterName = "NIPPSHOSTS" } }
            };

            NippsParameterResponse nippsParameterResponse = RestHelper.RestPostObject<NippsParameterResponse, NippsParameterRequest>(configServiceUrl + "NippsParameterService/Get", nippsParameterRequest);

            if (nippsParameterResponse.Result == Result.OK)
                return nippsParameterResponse.NippsParameters;
            else
                throw new Exception(nippsParameterResponse.ResultMessages[0]);

        }

        public static List<NippsSite> ListSite(string hostName)
        {
            string deploymentServiceUrl = BuildDeploymentServiceUrl(hostName) + "ListSite";
            NippsSiteResponse response = RestHelper.RestGet<NippsSiteResponse>(deploymentServiceUrl);
            if (response.Result == Result.OK)
                return response.NippsSites;

            foreach (string em in response.ResultMessages)
                NLog.LogManager.GetCurrentClassLogger().Error(em);

            return new List<NippsSite>();

        }

        public static List<NippsSite> ListNippsSite(string hostName)
        {
            string deploymentServiceUrl = BuildDeploymentServiceUrl(hostName) + "ListNippsSite";
            NippsSiteResponse response = RestHelper.RestGet<NippsSiteResponse>(deploymentServiceUrl);
            if (response.Result == Result.OK)
                return response.NippsSites;

            foreach (string em in response.ResultMessages)
                NLog.LogManager.GetCurrentClassLogger().Error(em);

            return new List<NippsSite>();

        }

        public static List<String> ListApplicationPool(string hostName)
        {
            string deploymentServiceUrl = BuildDeploymentServiceUrl(hostName) + "ListApplicationPool";
            NippsApplicationPoolResponse response = RestHelper.RestGet<NippsApplicationPoolResponse>(deploymentServiceUrl);
            if (response.Result == Result.OK)
                return response.NippsApplicationPools;

            foreach (String em in response.ResultMessages)
                NLog.LogManager.GetCurrentClassLogger().Error(em);

            return new List<string>();
        }

        public static List<Models.Nipps> ListNipps()
        {
            List<Models.Nipps> nippsList = new List<Models.Nipps>();
            NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

            try
            {
                //get list of the service-hosts
                List<NippsParameter> nippsHosts = NippsSiteHelper.ListNippsHost();

                foreach (NippsParameter nippsHost in nippsHosts)
                    foreach (string value in nippsHost.ParameterValue.Split('|'))
                    {
                        try
                        {
                            //get applications from the host
                            List<NippsSite> nippsSites = NippsSiteHelper.ListNippsSite(value);
                            foreach (NippsSite nippsSite in nippsSites)
                                foreach (NippsApplication nippsApplication in nippsSite.NippsApplications)
                                {
                                    //add only netas ip phone service application to the result list
                                    if (nippsApplication.Path.StartsWith("/Netas.Nipps.Service."))
                                    {
                                        Models.Nipps nipps = new Models.Nipps
                                        {
                                            HostName = value,
                                            ApplicationName = nippsApplication.Path,
                                            ApplicationPoolName = nippsApplication.ApplicationPoolName,
                                            SiteName = nippsSite.Name,
                                            State = nippsApplication.ApplicationPoolState,
                                            PhysicalPath = nippsApplication.PhysicalPath,
                                            Version = nippsApplication.Version,
                                            ModuleId = -1
                                        };
                                        nippsList.Add(nipps);
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

            return nippsList;
        }

        public static SelectList SelectListSite(string hostName)
        {
            List<SelectListItem> sites = new List<SelectListItem>();
            sites.Add(new SelectListItem { Text = Resources.Global.MessageSiteSelect, Value = "" });

            if (!String.IsNullOrEmpty(hostName))
                foreach (NippsSite site in ListSite(hostName))
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

            foreach (NippsParameter host in ListNippsHost())
                foreach (string value in host.ParameterValue.Split('|'))
                    hosts.Add(new SelectListItem { Value = value, Text = value });
    
            return new SelectList(hosts, "Value", "Text");
        }

        public static List<NippsModule> ListNippsModule()
        {
            string logServiceUrl = CommonHelper.LogManagerServiceUrl + "NippsModuleService/List";
            NippsModuleRequest request = new NippsModuleRequest { PageNo = 1, PageSize = 1000 };
            NippsModuleResponse response = RestHelper.RestPostObject<NippsModuleResponse, NippsModuleRequest>(logServiceUrl, request);
            if (response.Result == Result.OK)
                return response.NippsModules;

            foreach (string em in response.ResultMessages)
                NLog.LogManager.GetCurrentClassLogger().Error(em);

            return new List<NippsModule>();

        }

        public static string ServiceLogUrl(ModuleNameParser mnp)
        {
            return ServiceBaseUrl(mnp.Host, mnp.Site, mnp.Application) + "/api/" + ConfigurationManager.AppSettings[mnp.Service + "LogUrl"];
        }

        public static string ServiceBaseUrl(string hostName, string siteName, string appName)
        {
            //find site from given host
            NippsSite nippsSite = NippsSiteHelper.ListNippsSite(hostName)
                .Where(s => s.Name.Equals(siteName))
                .Single();

            //build url from site bindings
            string baseUrl = nippsSite.Protocol
                + "://" + hostName
                + ":" + nippsSite.Port + appName;

            return baseUrl;
        }
    }
}