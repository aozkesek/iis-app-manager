using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web;

using Netas.Nipps.BaseService;
using Netas.Nipps.BaseDao.Model.Response;

using Netas.Nipps.DeployManager.Data.Model;
using Netas.Nipps.DeployManager.Data.Model.Request;
using Netas.Nipps.DeployManager.Data.Model.Response;

using Netas.Nipps.ConfigManager.Data.Model;

using Netas.Nipps.LogManager.Data.Model;
using Netas.Nipps.LogManager.Data.Model.Request;
using Netas.Nipps.LogManager.Data.Model.Response;

using Netas.Nipps.SystemManager.Presentation.Base;
using Netas.Nipps.SystemManager.Presentation.Authorize;

using Netas.Nipps.SystemManager.Presentation.Helpers;

namespace Netas.Nipps.SystemManager.Presentation.Controllers
{
    public class DeployManagementController : BaseController
    {
        static readonly string ReturnToAction = "NippsServiceList";
        static readonly string ReturnToController = "DeployManagement";
        
        #region lists
        public JsonResult ListSite(string hostName)
        {
            return Json(NippsSiteHelper.SelectListSite(hostName));
        }

        public JsonResult ListPool(string hostName)
        {
            return Json(NippsSiteHelper.SelectListPool(hostName));
        }

        public JsonResult ListHost()
        {
            return Json(NippsSiteHelper.ListNippsHost());
        }

        [LoginAndAuthorize]
        public ActionResult NippsServiceList()
        {
            
            ViewBag.ResultList = NippsSiteHelper.ListNipps();

            if (ViewBag.ResultList != null && ViewBag.ResultList.Count > 0)
                SetViewBagResult(new NippsPackageResponse { Result = Result.OK }, ViewBag);
            else
                SetViewBagResult(new NippsPackageResponse { Result = Result.FAIL, ResultMessages = new List<string> { Resources.Global.MessageUnknownError } }, ViewBag);
            
            return View();
        }

        #endregion

        #region undeploy
        [LoginAndAuthorize]
        public ActionResult NippsUndeploy(Models.Nipps nipps)
        {

            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.UndeployTitle;
            ViewBag.Name = Resources.Global.Undeploy;

            try
            {
                //first, get and remove nippsmodule, do not care weather exist or not
                RemoveNippsModule(GetNippsModule(nipps));
                
                //then remove application
                NippsSite nippsSite = new NippsSite
                {
                    Name = nipps.SiteName,
                    NippsApplications = new List<NippsApplication> { new NippsApplication { Path = nipps.ApplicationName, PhysicalPath = nipps.PhysicalPath } }
                };

                string svcUrl = NippsSiteHelper.BuildDeploymentServiceUrl(nipps.HostName) + "RemoveNippsSite";
                NippsSiteRequest nippsSiteRequest = new NippsSiteRequest { NippsSites = new List<NippsSite> { nippsSite } };
                NippsSiteResponse response = RestHelper.RestPostObject<NippsSiteResponse, NippsSiteRequest>(svcUrl, nippsSiteRequest);

                if (response.Result == Result.OK)
                    return RedirectToAction("NippsServiceList");

                SetViewBagResult(response, ViewBag);

            }
            catch (Exception ex)
            {
                Logger.Error("{0}: {1}", nipps, ex.ToString());
                SetViewBagResult(new NippsSiteResponse { Result = Result.FAIL, ResultMessages = new List<string> { ex.ToString() } }, ViewBag);
            }

            return View(NippsSiteHelper.ResultMessageView);
        }

        #endregion

        #region recycle
        [LoginAndAuthorize]
        public ActionResult NippsRecycle(Models.Nipps nipps)
        {
            string deploymentServiceUrl = NippsSiteHelper.BuildDeploymentServiceUrl(nipps.HostName) + "RecycleApplicationPool";
            NippsApplicationRequest nippsApplicationRequest = new NippsApplicationRequest
            {
                NippsApplications = new List<NippsApplication> { new NippsApplication { ApplicationPoolName = nipps.ApplicationPoolName } }
            };

            NippsApplicationResponse response = RestHelper.RestPostObject<NippsApplicationResponse, NippsApplicationRequest>(deploymentServiceUrl, nippsApplicationRequest);
            if (response.Result == Result.FAIL)
                foreach (string em in response.ResultMessages)
                    Logger.Error(em);

            return RedirectToAction("NippsServiceList");
        }
        #endregion

        #region stop
        [LoginAndAuthorize]
        public ActionResult NippsStop(Models.Nipps nipps)
        {
            string deploymentServiceUrl = NippsSiteHelper.BuildDeploymentServiceUrl(nipps.HostName) + "StopApplicationPool";
            NippsApplicationRequest nippsApplicationRequest = new NippsApplicationRequest
            {
                NippsApplications = new List<NippsApplication> { new NippsApplication { ApplicationPoolName = nipps.ApplicationPoolName } }
            };

            NippsApplicationResponse response = RestHelper.RestPostObject<NippsApplicationResponse, NippsApplicationRequest>(deploymentServiceUrl, nippsApplicationRequest);
            if (response.Result == Result.FAIL)
                foreach (string em in response.ResultMessages)
                    Logger.Error(em);

            return RedirectToAction("NippsServiceList");
        }
        #endregion 

        #region deploy
        [LoginAndAuthorize]
        public ActionResult NippsDeployConfirm()
        {
            return View();
        }

        [LoginAndAuthorize]
        public ActionResult NippsDeploy(HttpPostedFileBase PackageZIP, Models.Nipps nipps)
        {
            string deploymentServiceUrl = NippsSiteHelper.BuildDeploymentServiceUrl(nipps.HostName) + "AddSite";

            byte[] packageZipBuffer = new byte[PackageZIP.ContentLength];
            PackageZIP.InputStream.Read(packageZipBuffer, 0, PackageZIP.ContentLength);

            string b64Package = Convert.ToBase64String(packageZipBuffer);

            NippsPackageRequest nippsPackageRequest = new NippsPackageRequest
            {
                NippsPackages = new List<NippsPackage>
                {
                    new NippsPackage { 
                        PackageZIP = b64Package,
                        SiteName = nipps.SiteName,
                        HostName = nipps.HostName,
                        ApplicationPoolName = nipps.ApplicationPoolName,
                        ApplicationName = nipps.ApplicationName
                    }
                }
            };

            NippsPackageResponse response = RestHelper.RestPostObject<NippsPackageResponse, NippsPackageRequest>(deploymentServiceUrl, nippsPackageRequest);
            
            if (response.Result == Result.OK)
                return RedirectToAction("NippsServiceList");

            foreach (String em in response.ResultMessages)
                Logger.Error(em); 
                
            ViewBag.Result = response.Result;
            ViewBag.ResultMessages = response.ResultMessages;
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.DeployTitle;

            return View(NippsSiteHelper.ResultMessageView);
            
        }
        #endregion

        #region helper

        NippsModule GetNippsModule(Models.Nipps nipps)
        {
            try
            {
                string moduleName = nipps.HostName + ">" + nipps.SiteName + ">" + nipps.ApplicationName;
                string svcUrl = CommonHelper.LogManagerServiceUrl + "NippsModuleService/GetByName";
                NippsModuleRequest moduleRequest = new NippsModuleRequest { NippsModules = new List<NippsModule> { new NippsModule { ModuleName = moduleName } } };
                NippsModuleResponse moduleResponse = RestHelper.RestPostObject<NippsModuleResponse, NippsModuleRequest>(svcUrl, moduleRequest);

                if (moduleResponse.Result == Result.OK)
                    return moduleResponse.NippsModules[0];

                Logger.Error("{0}: {1}", nipps, moduleResponse.ResultMessages[0]);
            }
            catch (Exception ex)
            {
                Logger.Error("{0}: {1}", nipps, ex.ToString());
            }

            return null;
        }

        bool RemoveNippsModule(NippsModule nippsModule)
        {
            try
            {
                string svcUrl = CommonHelper.LogManagerServiceUrl + "NippsModuleService/Remove";
                NippsModuleRequest moduleRequest = new NippsModuleRequest { NippsModules = new List<NippsModule> { nippsModule } };
                NippsModuleResponse moduleResponse = RestHelper.RestPostObject<NippsModuleResponse, NippsModuleRequest>(svcUrl, moduleRequest);

                if (moduleResponse.Result == Result.OK)
                    return true;

                Logger.Error("{0}: {1}", nippsModule, moduleResponse.ResultMessages[0]);
            }
            catch (Exception ex)
            {
                Logger.Error("{0}: {1}", nippsModule, ex.ToString());
            }

            return false;                    
        }
        #endregion
    }


}
