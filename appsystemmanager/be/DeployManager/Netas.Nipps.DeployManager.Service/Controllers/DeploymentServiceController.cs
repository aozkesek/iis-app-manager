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

using Netas.Nipps.Aspect;
using Netas.Nipps.BaseService;
using Netas.Nipps.BaseDao;
using Netas.Nipps.BaseDao.Model.Response;
using Netas.Nipps.DeployManager.Data.Model;
using Netas.Nipps.DeployManager.Data.Model.Response;
using Netas.Nipps.DeployManager.Data.Model.Request;

using Netas.Nipps.DeployManager.Service.Helpers;

namespace Netas.Nipps.DeployManager.Service.Controllers
{
    

    [RoutePrefix("api/DeploymentService")]
    public class DeploymentServiceController : BaseApiController
    {

        #region list
        [HttpGet]
        [Route("ListSite")]
        [PerformanceLoggingAdvice]
        public NippsSiteResponse ListSite()
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            NippsSiteResponse response = new NippsSiteResponse();
            response.ResultMessages = new List<string>();

            try
            {
                using (ServerManager serverManager = ServerManager.OpenRemote("localhost"))
                {
                    
                    List<NippsSite> sites = new List<NippsSite>();

                    foreach (Site site in serverManager.Sites)
                    {
                        
                        NippsSite nippsSite = new NippsSite();
                        nippsSite.Name = site.Name;
                        nippsSite.NippsApplications = new List<NippsApplication>();

                        foreach (Application application in site.Applications)
                        {
                        
                            try
                            {
                                nippsSite.NippsApplications.Add(new NippsApplication
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
                            nippsSite.State = site.State;
                            nippsSite.Protocol = site.Bindings[0].Protocol;
                            nippsSite.Port = site.Bindings[0].EndPoint.Port.ToString();
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.ToString());
                            response.ResultMessages.Add(ex.ToString());
                        }

                        sites.Add(nippsSite);

                    }

                    response.NippsSites = sites;
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
        [Route("ListNippsSite")]
        [PerformanceLoggingAdvice]
        public NippsSiteResponse ListNippsSite()
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            NippsSiteResponse response = new NippsSiteResponse();
            response.ResultMessages = new List<string>();

            try
            {
                using (ServerManager serverManager = ServerManager.OpenRemote("localhost"))
                {
                    List<NippsSite> sites = new List<NippsSite>();

                    foreach (Site site in serverManager.Sites)
                    {
                        NippsSite nippsSite = new NippsSite();
                        nippsSite.Name = site.Name;
                        nippsSite.NippsApplications = new List<NippsApplication>();

                        foreach (Application application in site.Applications)
                        {
                            if (application.Path.StartsWith("/Netas.Nipps.") || application.Path.Equals("/") && site.Name.StartsWith("Netas.Nipps."))
                                try
                                {
                                    nippsSite.NippsApplications.Add(new NippsApplication
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
                            nippsSite.State = site.State;
                            nippsSite.Protocol = site.Bindings[0].Protocol;
                            nippsSite.Port = site.Bindings[0].EndPoint.Port.ToString();
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                            response.ResultMessages.Add(ex.ToString());
                        }

                        //add only the site includes Netas.Nipps application
                        if (nippsSite.NippsApplications.Count > 0)
                            sites.Add(nippsSite);

                    }

                    response.NippsSites = sites;
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
        public NippsApplicationPoolResponse ListApplicationPool()
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            NippsApplicationPoolResponse response = new NippsApplicationPoolResponse();
            response.ResultMessages = new List<string>();

            try
            {   
                using (ServerManager serverManager = ServerManager.OpenRemote("localhost"))
                {
                    response.NippsApplicationPools = new List<String>();
                    foreach (ApplicationPool appPool in serverManager.ApplicationPools)
                        response.NippsApplicationPools.Add(appPool.Name);
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
        public override HttpResponseMessage LogGetZipFileList(NippsLogFileRequest logFileRequest)
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
        public NippsPackageResponse AddSite(NippsPackageRequest nippsPackageRequest)
        {
            bool isSucceededOne = false;
            Logger logger = NLog.LogManager.GetCurrentClassLogger();
            NippsPackageResponse response = new NippsPackageResponse();
            response.ResultMessages = new List<string>();
            response.Result = Result.OK;

            try
            {
                using (ServerManager serverManager = ServerManager.OpenRemote("localhost"))
                {
                    foreach (NippsPackage nippsPackage in nippsPackageRequest.NippsPackages) 
                    {
                        try
                        {
                            
                            Site site = serverManager.Sites
                                .Where(s => s.Name.Equals(nippsPackage.SiteName))
                                .Single();
                        
                            if (site != null)
                            {
                                string basePath = SavePackage(nippsPackage, site);
                                string appPath = basePath + nippsPackage.ApplicationName;

                                logger.Debug(string.Format("Application>{0}", appPath));

                                //execute SQL if exist
                                BaseDaoHelper.ExecuteUpgradeScript(appPath);
                                
                                //add site
                                Application app = site.Applications.Add("/" + nippsPackage.ApplicationName, appPath.Replace("/","\\"));
                                app.ApplicationPoolName = nippsPackage.ApplicationPoolName;

                                response.ResultMessages.Add(
                                    string.Format("[{0} - {1} - {2}]", nippsPackage.SiteName, nippsPackage.ApplicationPoolName, nippsPackage.ApplicationName));

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
        [Route("RemoveNippsSite")]
        [PerformanceLoggingAdvice]
        public NippsSiteResponse RemoveNippsSite(NippsSiteRequest nippsSiteRequest)
        {
            bool isRemoved;
            bool isSucceededOne = false;
            Logger logger = NLog.LogManager.GetCurrentClassLogger();
            NippsSiteResponse response = new NippsSiteResponse();
            response.ResultMessages = new List<string>();
            response.Result = Result.OK;

            try
            {
                using (ServerManager serverManager = ServerManager.OpenRemote("localhost"))
                {
                    foreach (NippsSite nippsSite in nippsSiteRequest.NippsSites)
                        if (nippsSite.NippsApplications.Count > 0)
                            foreach (Site site in serverManager.Sites)
                                if (site.Name.Equals(nippsSite.Name))
                                {

                                    isRemoved = false;
                                    
                                    foreach (NippsApplication nippsApplication in nippsSite.NippsApplications)
                                    {
                                        try
                                        {
                                            //remove application
                                            logger.Debug("the app we remove -> " + nippsApplication.Path);
                                            Application application = FindApplication(site.Applications, nippsApplication.Path); 
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
                                    //string.Format("[{0} - {1} - {2}]", nippsPackage.SiteName, nippsPackage.ApplicationPoolName, nippsPackage.ApplicationName));

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
        public NippsApplicationResponse RecycleApplicationPool(NippsApplicationRequest nippsApplicationRequest)
        {
            bool isSucceededOne = false;
            Logger logger = NLog.LogManager.GetCurrentClassLogger();
            NippsApplicationResponse response = new NippsApplicationResponse();
            response.ResultMessages = new List<string>();

            try
            {
                using (ServerManager serverManager = ServerManager.OpenRemote("localhost"))
                {
                    response.Result = Result.OK;

                    foreach (NippsApplication nippsApplication in nippsApplicationRequest.NippsApplications)
                    {
                        try
                        {
                            ApplicationPool appPool = serverManager.ApplicationPools[nippsApplication.ApplicationPoolName];
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
                            logger.Error("{0} -> {1}", nippsApplication, ex.ToString());
                            response.ResultMessages.Add(string.Format("{0} -> {1}", nippsApplication, ex.ToString()));
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
        public NippsApplicationResponse StopApplicationPool(NippsApplicationRequest nippsApplicationRequest)
        {
            bool isSucceededOne = false;
            Logger logger = NLog.LogManager.GetCurrentClassLogger();
            NippsApplicationResponse response = new NippsApplicationResponse();
            response.ResultMessages = new List<string>();

            try
            {
                using (ServerManager serverManager = ServerManager.OpenRemote("localhost"))
                {
                    response.Result = Result.OK;

                    foreach (NippsApplication nippsApplication in nippsApplicationRequest.NippsApplications)
                    {
                        try
                        {
                            ApplicationPool appPool = serverManager.ApplicationPools[nippsApplication.ApplicationPoolName];
                            if (appPool.State == ObjectState.Started)
                                appPool.Stop();
                            isSucceededOne = true;
                        }
                        catch (Exception ex)
                        {
                            if (isSucceededOne)
                                response.Result = Result.SUCCESSWITHWARN;
                            logger.Error("{0} -> {1}", nippsApplication, ex.ToString());
                            response.ResultMessages.Add(string.Format("{0} -> {1}", nippsApplication, ex.ToString()));
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
        
        private string SavePackage(NippsPackage nippsPackage, Site site)
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            logger.Debug(nippsPackage);

            //decode zip-file
            byte[] packageZipBuffer = Convert.FromBase64String(nippsPackage.PackageZIP);
            
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

            NippsZipHelper.Unzip(basePath, packageZipBuffer);

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
