using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web;

using Org.Apps.BaseService;
using Org.Apps.BaseDao.Model.Response;

using Org.Apps.DeployManager.Data.Model;
using Org.Apps.DeployManager.Data.Model.Request;
using Org.Apps.DeployManager.Data.Model.Response;

using Org.Apps.ConfigManager.Data.Model;

using Org.Apps.LogManager.Data.Model;
using Org.Apps.LogManager.Data.Model.Request;
using Org.Apps.LogManager.Data.Model.Response;

using Org.Apps.SystemManager.Presentation.Base;
using Org.Apps.SystemManager.Presentation.Authorize;

using Org.Apps.SystemManager.Presentation.Helpers;

namespace Org.Apps.SystemManager.Presentation.Controllers
{
    public class DeployManagementController : BaseController
    {
        static readonly string ReturnToAction = "AppsServiceList";
        static readonly string ReturnToController = "DeployManagement";
        
        #region lists
        public JsonResult ListSite(string hostName)
        {
            return Json(AppsSiteHelper.SelectListSite(hostName));
        }

        public JsonResult ListPool(string hostName)
        {
            return Json(AppsSiteHelper.SelectListPool(hostName));
        }

        public JsonResult ListHost()
        {
            return Json(AppsSiteHelper.ListAppsHost());
        }

        [LoginAndAuthorize]
        public ActionResult AppsServiceList()
        {
            
            ViewBag.ResultList = AppsSiteHelper.ListApps();

            if (ViewBag.ResultList != null && ViewBag.ResultList.Count > 0)
                SetViewBagResult(new AppsPackageResponse { Result = Result.OK }, ViewBag);
            else
                SetViewBagResult(new AppsPackageResponse { Result = Result.FAIL, ResultMessages = new List<string> { Resources.Global.MessageUnknownError } }, ViewBag);
            
            return View();
        }

        #endregion

        #region undeploy
        [LoginAndAuthorize]
        public ActionResult AppsUndeploy(Models.Apps Apps)
        {

            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.UndeployTitle;
            ViewBag.Name = Resources.Global.Undeploy;

            try
            {
                //first, get and remove Appsmodule, do not care weather exist or not
                RemoveAppsModule(GetAppsModule(Apps));
                
                //then remove application
                AppsSite AppsSite = new AppsSite
                {
                    Name = Apps.SiteName,
                    AppsApplications = new List<AppsApplication> { new AppsApplication { Path = Apps.ApplicationName, PhysicalPath = Apps.PhysicalPath } }
                };

                string svcUrl = AppsSiteHelper.BuildDeploymentServiceUrl(Apps.HostName) + "RemoveAppsSite";
                AppsSiteRequest AppsSiteRequest = new AppsSiteRequest { AppsSites = new List<AppsSite> { AppsSite } };
                AppsSiteResponse response = RestHelper.RestPostObject<AppsSiteResponse, AppsSiteRequest>(svcUrl, AppsSiteRequest);

                if (response.Result == Result.OK)
                    return RedirectToAction("AppsServiceList");

                SetViewBagResult(response, ViewBag);

            }
            catch (Exception ex)
            {
                Logger.Error("{0}: {1}", Apps, ex.ToString());
                SetViewBagResult(new AppsSiteResponse { Result = Result.FAIL, ResultMessages = new List<string> { ex.ToString() } }, ViewBag);
            }

            return View(AppsSiteHelper.ResultMessageView);
        }

        #endregion

        #region recycle
        [LoginAndAuthorize]
        public ActionResult AppsRecycle(Models.Apps Apps)
        {
            string deploymentServiceUrl = AppsSiteHelper.BuildDeploymentServiceUrl(Apps.HostName) + "RecycleApplicationPool";
            AppsApplicationRequest AppsApplicationRequest = new AppsApplicationRequest
            {
                AppsApplications = new List<AppsApplication> { new AppsApplication { ApplicationPoolName = Apps.ApplicationPoolName } }
            };

            AppsApplicationResponse response = RestHelper.RestPostObject<AppsApplicationResponse, AppsApplicationRequest>(deploymentServiceUrl, AppsApplicationRequest);
            if (response.Result == Result.FAIL)
                foreach (string em in response.ResultMessages)
                    Logger.Error(em);

            return RedirectToAction("AppsServiceList");
        }
        #endregion

        #region stop
        [LoginAndAuthorize]
        public ActionResult AppsStop(Models.Apps Apps)
        {
            string deploymentServiceUrl = AppsSiteHelper.BuildDeploymentServiceUrl(Apps.HostName) + "StopApplicationPool";
            AppsApplicationRequest AppsApplicationRequest = new AppsApplicationRequest
            {
                AppsApplications = new List<AppsApplication> { new AppsApplication { ApplicationPoolName = Apps.ApplicationPoolName } }
            };

            AppsApplicationResponse response = RestHelper.RestPostObject<AppsApplicationResponse, AppsApplicationRequest>(deploymentServiceUrl, AppsApplicationRequest);
            if (response.Result == Result.FAIL)
                foreach (string em in response.ResultMessages)
                    Logger.Error(em);

            return RedirectToAction("AppsServiceList");
        }
        #endregion 

        #region deploy
        [LoginAndAuthorize]
        public ActionResult AppsDeployConfirm()
        {
            return View();
        }

        [LoginAndAuthorize]
        public ActionResult AppsDeploy(HttpPostedFileBase PackageZIP, Models.Apps Apps)
        {
            string deploymentServiceUrl = AppsSiteHelper.BuildDeploymentServiceUrl(Apps.HostName) + "AddSite";

            byte[] packageZipBuffer = new byte[PackageZIP.ContentLength];
            PackageZIP.InputStream.Read(packageZipBuffer, 0, PackageZIP.ContentLength);

            string b64Package = Convert.ToBase64String(packageZipBuffer);

            AppsPackageRequest AppsPackageRequest = new AppsPackageRequest
            {
                AppsPackages = new List<AppsPackage>
                {
                    new AppsPackage { 
                        PackageZIP = b64Package,
                        SiteName = Apps.SiteName,
                        HostName = Apps.HostName,
                        ApplicationPoolName = Apps.ApplicationPoolName,
                        ApplicationName = Apps.ApplicationName
                    }
                }
            };

            AppsPackageResponse response = RestHelper.RestPostObject<AppsPackageResponse, AppsPackageRequest>(deploymentServiceUrl, AppsPackageRequest);
            
            if (response.Result == Result.OK)
                return RedirectToAction("AppsServiceList");

            foreach (String em in response.ResultMessages)
                Logger.Error(em); 
                
            ViewBag.Result = response.Result;
            ViewBag.ResultMessages = response.ResultMessages;
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.DeployTitle;

            return View(AppsSiteHelper.ResultMessageView);
            
        }
        #endregion

        #region helper

        AppsModule GetAppsModule(Models.Apps Apps)
        {
            try
            {
                string moduleName = Apps.HostName + ">" + Apps.SiteName + ">" + Apps.ApplicationName;
                string svcUrl = CommonHelper.LogManagerServiceUrl + "AppsModuleService/GetByName";
                AppsModuleRequest moduleRequest = new AppsModuleRequest { AppsModules = new List<AppsModule> { new AppsModule { ModuleName = moduleName } } };
                AppsModuleResponse moduleResponse = RestHelper.RestPostObject<AppsModuleResponse, AppsModuleRequest>(svcUrl, moduleRequest);

                if (moduleResponse.Result == Result.OK)
                    return moduleResponse.AppsModules[0];

                Logger.Error("{0}: {1}", Apps, moduleResponse.ResultMessages[0]);
            }
            catch (Exception ex)
            {
                Logger.Error("{0}: {1}", Apps, ex.ToString());
            }

            return null;
        }

        bool RemoveAppsModule(AppsModule AppsModule)
        {
            try
            {
                string svcUrl = CommonHelper.LogManagerServiceUrl + "AppsModuleService/Remove";
                AppsModuleRequest moduleRequest = new AppsModuleRequest { AppsModules = new List<AppsModule> { AppsModule } };
                AppsModuleResponse moduleResponse = RestHelper.RestPostObject<AppsModuleResponse, AppsModuleRequest>(svcUrl, moduleRequest);

                if (moduleResponse.Result == Result.OK)
                    return true;

                Logger.Error("{0}: {1}", AppsModule, moduleResponse.ResultMessages[0]);
            }
            catch (Exception ex)
            {
                Logger.Error("{0}: {1}", AppsModule, ex.ToString());
            }

            return false;                    
        }
        #endregion
    }


}
