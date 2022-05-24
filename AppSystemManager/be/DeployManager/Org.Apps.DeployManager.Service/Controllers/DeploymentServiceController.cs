using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Diagnostics;


using Microsoft.Web.Administration;

using NLog;

using Org.Apps.Aspect;
using Org.Apps.BaseService;
using Org.Apps.BaseDao;
using Org.Apps.BaseDao.Model.Response;
using Org.Apps.DeployManager.Data.Model;
using Org.Apps.DeployManager.Data.Model.Response;
using Org.Apps.DeployManager.Data.Model.Request;

using Org.Apps.DeployManager.Service.Helpers;

namespace Org.Apps.DeployManager.Service.Controllers
{
    

    [RoutePrefix("api/DeploymentService")]
    public class DeploymentServiceController : BaseApiController
    {

        #region list
        [HttpGet]
        [Route("ListSite")]
        [PerformanceLoggingAdvice]
        public AppsSiteResponse ListSite()
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            AppsSiteResponse response = new AppsSiteResponse();
            response.ResultMessages = new List<string>();

            try
            {
                using (ServerManager serverManager = ServerManager.OpenRemote("localhost"))
                {
                    
                    List<AppsSite> sites = new List<AppsSite>();

                    foreach (Site site in serverManager.Sites)
                    {
                        
                        AppsSite AppsSite = new AppsSite();
                        AppsSite.Name = site.Name;
                        AppsSite.AppsApplications = new List<AppsApplication>();

                        foreach (Application application in site.Applications)
                        {
                        
                            try
                            {
                                AppsSite.AppsApplications.Add(new AppsApplication
                                {
                                    ApplicationPoolName = application.ApplicationPoolName,
                                    Path = application.Path,
                                    PhysicalPath = ServerManagerHelper.PutEnvVarValue(application.VirtualDirectories[0].PhysicalPath),
                                    ApplicationPoolState = serverManager.ApplicationPools[application.ApplicationPoolName].State,
                                    Version = GetVersion(application)
                                });

                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.ToString());
                                response.ResultMessages.Add(ex.ToString());
                            }

                        }

                        try
                        {
                            AppsSite.State = site.State;
                            AppsSite.Protocol = site.Bindings[0].Protocol;
                            AppsSite.Port = site.Bindings[0].EndPoint.Port.ToString();
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.ToString());
                            response.ResultMessages.Add(ex.ToString());
                        }

                        sites.Add(AppsSite);

                    }

                    response.AppsSites = sites;
                    response.Result = Result.OK;
                    logger.Debug(sites);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.ToString());
            }
       
            return response;

        }
        
        [HttpGet]
        [Route("ListAppsSite")]
        [PerformanceLoggingAdvice]
        public AppsSiteResponse ListAppsSite()
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            AppsSiteResponse response = new AppsSiteResponse();
            response.ResultMessages = new List<string>();

            try
            {
                using (ServerManager serverManager = ServerManager.OpenRemote("localhost"))
                {
                    List<AppsSite> sites = new List<AppsSite>();

                    foreach (Site site in serverManager.Sites)
                    {
                        AppsSite AppsSite = new AppsSite();
                        AppsSite.Name = site.Name;
                        AppsSite.AppsApplications = new List<AppsApplication>();

                        foreach (Application application in site.Applications)
                        {
                            if (application.Path.StartsWith("/Org.Apps.") || application.Path.Equals("/") && site.Name.StartsWith("Org.Apps."))
                                try
                                {
                                    AppsSite.AppsApplications.Add(new AppsApplication
                                    {
                                        ApplicationPoolName = application.ApplicationPoolName,
                                        Path = application.Path,
                                        PhysicalPath = ServerManagerHelper.PutEnvVarValue(application.VirtualDirectories[0].PhysicalPath),
                                        ApplicationPoolState = serverManager.ApplicationPools[application.ApplicationPoolName].State,
                                        Version = GetVersion(application)
                                    });
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex);
                                    response.ResultMessages.Add(ex.ToString());
                                }

                        }

                        try
                        {
                            AppsSite.State = site.State;
                            AppsSite.Protocol = site.Bindings[0].Protocol;
                            AppsSite.Port = site.Bindings[0].EndPoint.Port.ToString();
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                            response.ResultMessages.Add(ex.ToString());
                        }

                        //add only the site includes Org.Apps application
                        if (AppsSite.AppsApplications.Count > 0)
                            sites.Add(AppsSite);

                    }

                    response.AppsSites = sites;
                    response.Result = Result.OK;
                    logger.Debug(sites);

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.ToString());
            }

            return response;

            
        }

        [HttpGet]
        [Route("ListApplicationPool")]
        [PerformanceLoggingAdvice]
        public AppsApplicationPoolResponse ListApplicationPool()
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            AppsApplicationPoolResponse response = new AppsApplicationPoolResponse();
            response.ResultMessages = new List<string>();

            try
            {   
                using (ServerManager serverManager = ServerManager.OpenRemote("localhost"))
                {
                    response.AppsApplicationPools = new List<String>();
                    foreach (ApplicationPool appPool in serverManager.ApplicationPools)
                        response.AppsApplicationPools.Add(appPool.Name);
                    response.Result = Result.OK;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.ToString());
            }

            return response;

        }

        [PerformanceLoggingAdvice]
        public override HttpResponseMessage LogGetZipFileList(AppsLogFileRequest logFileRequest)
        {
            return base.LogGetZipFileList(logFileRequest);
        }

        [PerformanceLoggingAdvice]
        public override HttpResponseMessage LogGetFileList()
        {
            return base.LogGetFileList();
        }

        #endregion

        #region add
        [HttpPost]
        [Route("AddSite")]
        [PerformanceLoggingAdvice]
        public AppsPackageResponse AddSite(AppsPackageRequest AppsPackageRequest)
        {
            bool isSucceededOne = false;
            Logger logger = NLog.LogManager.GetCurrentClassLogger();
            AppsPackageResponse response = new AppsPackageResponse();
            response.ResultMessages = new List<string>();
            response.Result = Result.OK;

            try
            {
                using (ServerManager serverManager = ServerManager.OpenRemote("localhost"))
                {
                    foreach (AppsPackage AppsPackage in AppsPackageRequest.AppsPackages) 
                    {
                        try
                        {
                            
                            Site site = serverManager.Sites
                                .Where(s => s.Name.Equals(AppsPackage.SiteName))
                                .Single();
                        
                            if (site != null)
                            {
                                string basePath = SavePackage(AppsPackage, site);
                                string appPath = basePath + AppsPackage.ApplicationName;

                                logger.Debug(string.Format("Application>{0}", appPath));

                                //execute SQL if exist
                                BaseDaoHelper.ExecuteUpgradeScript(appPath);
                                
                                //add site
                                Application app = site.Applications.Add("/" + AppsPackage.ApplicationName, appPath.Replace("/","\\"));
                                app.ApplicationPoolName = AppsPackage.ApplicationPoolName;

                                response.ResultMessages.Add(
                                    string.Format("[{0} - {1} - {2}]", AppsPackage.SiteName, AppsPackage.ApplicationPoolName, AppsPackage.ApplicationName));

                                isSucceededOne = true;
                            }

                            
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.ToString());
                            if (isSucceededOne)
                                response.Result = Result.SUCCESSWITHWARN;
                            response.ResultMessages.Add(ex.ToString());
                            
                        }

                    }

                    if (isSucceededOne)
                        serverManager.CommitChanges();
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.ToString());
            }

            return response;
        }
        #endregion

        #region remove
        [HttpPost]
        [Route("RemoveAppsSite")]
        [PerformanceLoggingAdvice]
        public AppsSiteResponse RemoveAppsSite(AppsSiteRequest AppsSiteRequest)
        {
            bool isRemoved;
            bool isSucceededOne = false;
            Logger logger = NLog.LogManager.GetCurrentClassLogger();
            AppsSiteResponse response = new AppsSiteResponse();
            response.ResultMessages = new List<string>();
            response.Result = Result.OK;

            try
            {
                using (ServerManager serverManager = ServerManager.OpenRemote("localhost"))
                {
                    foreach (AppsSite AppsSite in AppsSiteRequest.AppsSites)
                        if (AppsSite.AppsApplications.Count > 0)
                            foreach (Site site in serverManager.Sites)
                                if (site.Name.Equals(AppsSite.Name))
                                {

                                    isRemoved = false;
                                    
                                    foreach (AppsApplication AppsApplication in AppsSite.AppsApplications)
                                    {
                                        try
                                        {
                                            //remove application
                                            logger.Debug("the app we remove -> " + AppsApplication.Path);
                                            Application application = FindApplication(site.Applications, AppsApplication.Path); 
                                            site.Applications.Remove(application);
                                            
                                            //then folder
                                            string removeFolder = application
                                                .VirtualDirectories
                                                    .Where(v => v.Path.Equals("/"))
                                                    .Single()
                                                    .PhysicalPath;

                                            removeFolder = ServerManagerHelper.PutEnvVarValue(removeFolder);

                                            logger.Debug("the path we remove -> " + removeFolder);
                                            if (Directory.Exists(removeFolder))
                                                Directory.Delete(removeFolder, true);
                                            else
                                                logger.Error(removeFolder + " not found.");

                                            isSucceededOne = true;
                                            isRemoved = true;

                                            //response.ResultMessages.Add(
                                    //string.Format("[{0} - {1} - {2}]", AppsPackage.SiteName, AppsPackage.ApplicationPoolName, AppsPackage.ApplicationName));

                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Error(ex.ToString());
                                            if (isSucceededOne)
                                                response.Result = Result.SUCCESSWITHWARN;
                                            response.ResultMessages.Add(ex.ToString());
                                        }
                                    }

                                    if (isRemoved)
                                    {
                                        serverManager.CommitChanges(); 
                                        break;
                                    }
                                        
                                }
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.ToString());
            }

            return response;

        }
        #endregion

        #region recycle
        [HttpPost]
        [Route("RecycleApplicationPool")]
        [PerformanceLoggingAdvice]
        public AppsApplicationResponse RecycleApplicationPool(AppsApplicationRequest AppsApplicationRequest)
        {
            bool isSucceededOne = false;
            Logger logger = NLog.LogManager.GetCurrentClassLogger();
            AppsApplicationResponse response = new AppsApplicationResponse();
            response.ResultMessages = new List<string>();

            try
            {
                using (ServerManager serverManager = ServerManager.OpenRemote("localhost"))
                {
                    response.Result = Result.OK;

                    foreach (AppsApplication AppsApplication in AppsApplicationRequest.AppsApplications)
                    {
                        try
                        {
                            ApplicationPool appPool = serverManager.ApplicationPools[AppsApplication.ApplicationPoolName];
                            if (appPool.State == ObjectState.Started)
                                appPool.Recycle();
                            else
                                appPool.Start();
                            isSucceededOne = true;
                        }
                        catch (Exception ex)
                        {
                            if (isSucceededOne)
                                response.Result = Result.SUCCESSWITHWARN;
                            logger.Error("{0} -> {1}", AppsApplication, ex.ToString());
                            response.ResultMessages.Add(string.Format("{0} -> {1}", AppsApplication, ex.ToString()));
                        }
                        
                    }

                    serverManager.CommitChanges();

                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.ToString());
            }
            
            return response;
        }
        #endregion

        #region stop
        [HttpPost]
        [Route("StopApplicationPool")]
        [PerformanceLoggingAdvice]
        public AppsApplicationResponse StopApplicationPool(AppsApplicationRequest AppsApplicationRequest)
        {
            bool isSucceededOne = false;
            Logger logger = NLog.LogManager.GetCurrentClassLogger();
            AppsApplicationResponse response = new AppsApplicationResponse();
            response.ResultMessages = new List<string>();

            try
            {
                using (ServerManager serverManager = ServerManager.OpenRemote("localhost"))
                {
                    response.Result = Result.OK;

                    foreach (AppsApplication AppsApplication in AppsApplicationRequest.AppsApplications)
                    {
                        try
                        {
                            ApplicationPool appPool = serverManager.ApplicationPools[AppsApplication.ApplicationPoolName];
                            if (appPool.State == ObjectState.Started)
                                appPool.Stop();
                            isSucceededOne = true;
                        }
                        catch (Exception ex)
                        {
                            if (isSucceededOne)
                                response.Result = Result.SUCCESSWITHWARN;
                            logger.Error("{0} -> {1}", AppsApplication, ex.ToString());
                            response.ResultMessages.Add(string.Format("{0} -> {1}", AppsApplication, ex.ToString()));
                        }

                    }

                    serverManager.CommitChanges();

                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.ToString());
            }

            return response;
        }
        #endregion

        #region helpers

        private string GetVersion(Application application)
        {
            
            string fileName = application.VirtualDirectories[0].PhysicalPath + @"\bin\" + application.Path.Replace(@"/", "") + ".dll";

            if (File.Exists(fileName))
                return FileVersionInfo.GetVersionInfo(fileName).FileVersion;
            else
                return "";
        }
        
        private string SavePackage(AppsPackage AppsPackage, Site site)
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            logger.Debug(AppsPackage);

            //decode zip-file
            byte[] packageZipBuffer = Convert.FromBase64String(AppsPackage.PackageZIP);
            
            //calculate physical-path of the / -default- app&virtualpath
            string basePath = site.Applications
                    .Where(a => a.Path.Equals("/"))
                    .Single()
                    .VirtualDirectories
                        .Where(v => v.Path.Equals("/"))
                        .Single()
                        .PhysicalPath;

            basePath += "\\";

            basePath = ServerManagerHelper.PutEnvVarValue(basePath);

            logger.Debug(basePath);

            AppsZipHelper.Unzip(basePath, packageZipBuffer);

            return basePath;
        }

        private static Application FindApplication(ApplicationCollection applications, string path)
        {
            foreach (Application application in applications)
                if (application.Path.Equals(path))
                    return application;
            
            return null;
        }

        #endregion

    }
}
