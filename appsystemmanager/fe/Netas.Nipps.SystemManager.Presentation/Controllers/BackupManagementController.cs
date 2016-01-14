using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Configuration;
using System.Text.RegularExpressions;

using Netas.Nipps.BaseService;

using Netas.Nipps.BaseDao.Model.Response;

using Netas.Nipps.DeployManager.Data.Model;
using Netas.Nipps.DeployManager.Data.Model.Request;
using Netas.Nipps.DeployManager.Data.Model.Response;

using Netas.Nipps.SystemManager.Presentation.Authorize;
using Netas.Nipps.SystemManager.Presentation.Helpers;
using Netas.Nipps.SystemManager.Presentation.Base;

namespace Netas.Nipps.SystemManager.Presentation.Controllers
{
    public class BackupManagementController : BaseController
    {
        static readonly string ReturnToAction = "NippsServiceList";
        static readonly string ReturnToController = "DeployManagement";

        #region backup
        [LoginAndAuthorize]
        public ActionResult NippsBackup(Models.Nipps nipps)
        {

            ViewBag.Title = Resources.Global.BackupTitle;

            string backupServiceUrl = NippsSiteHelper.BuildDeploymentServiceUrl(nipps.HostName, "BackupService/BackupApplication");
            NippsPackageRequest backupRequest = new NippsPackageRequest 
            {
                NippsPackages = new List<NippsPackage>
                {
                    new NippsPackage
                    {
                        ApplicationName = nipps.ApplicationName,
                        SiteName = nipps.SiteName,
                        HostName = nipps.HostName
                    }
                }
            };

            NippsPackageResponse backupResponse = RestHelper.RestPostObject<NippsPackageResponse, NippsPackageRequest>(backupServiceUrl, backupRequest);

            ViewBag.Result = backupResponse.Result;
            ViewBag.ResultMessages = backupResponse.ResultMessages;
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.BackupTitle;

            return View(NippsSiteHelper.ResultMessageView);
        }

        [LoginAndAuthorize]
        public ActionResult NippsBackupConfirm(Models.Nipps nipps)
        {
            return View(nipps);
        }
        #endregion

        #region restore
        [LoginAndAuthorize]
        public ActionResult NippsRestore(Models.Nipps nipps, string restore)
        {

            ViewBag.Title = Resources.Global.RestoreTitle;

            string restoreServiceUrl = NippsSiteHelper.BuildDeploymentServiceUrl(nipps.HostName, "BackupService/RestoreApplication");
            NippsPackageRequest request = new NippsPackageRequest
            {
                NippsPackages = new List<NippsPackage>
                {
                    new NippsPackage
                    {
                        HostName = nipps.HostName,
                        SiteName = nipps.SiteName,
                        ApplicationName = nipps.ApplicationName,
                        ApplicationPoolName = nipps.ApplicationPoolName,
                        PackageZIP = restore
                    }
                }
            };

            NippsPackageResponse response = RestHelper.RestPostObject<NippsPackageResponse, NippsPackageRequest>(restoreServiceUrl, request);

            ViewBag.Result = response.Result;
            ViewBag.ResultMessages = response.ResultMessages;
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.RestoreTitle;

            return View(NippsSiteHelper.ResultMessageView);
        }

        [LoginAndAuthorize]
        public ActionResult NippsRestoreConfirm(Models.Nipps nipps, String restoreItem)
        {
            ViewBag.RestoreItem = restoreItem;

            return View(nipps);
        }

        [LoginAndAuthorize]
        public ActionResult NippsRestoreList(Models.Nipps nipps)
        {
            string restoreServiceUrl = NippsSiteHelper.BuildDeploymentServiceUrl(nipps.HostName, "BackupService/RestoreApplicationList");
            NippsPackageRequest restoreRequest = new NippsPackageRequest { NippsPackages = new List<NippsPackage>() };

            if (!String.IsNullOrEmpty(nipps.HostName))
                restoreRequest.NippsPackages.Add(new NippsPackage { 
                    HostName = nipps.HostName,
                    SiteName = nipps.SiteName,
                    ApplicationName = nipps.ApplicationName
                });

            NippsPackageResponse restoreResponse = RestHelper.RestPostObject<NippsPackageResponse, NippsPackageRequest>(restoreServiceUrl, restoreRequest);
            ViewBag.ResultList = restoreResponse.ResultMessages;
            SetViewBagResult(restoreResponse, ViewBag);

            return View(nipps);
        }
        #endregion

        #region upgrade
        [LoginAndAuthorize]
        public ActionResult NippsUpgradeList(Models.Nipps nipps)
        {
            string upgradeServiceUrl = NippsSiteHelper.BuildDeploymentServiceUrl(nipps.HostName, "BackupService/UpgradeApplicationList");
            NippsPackageRequest request = new NippsPackageRequest
            {
                NippsPackages = new List<NippsPackage>()
            };

            if (!String.IsNullOrEmpty(nipps.HostName))
                request.NippsPackages.Add(new NippsPackage
                {
                    HostName = nipps.HostName,
                    SiteName = nipps.SiteName,
                    ApplicationName = nipps.ApplicationName
                });

            NippsPackageResponse response = RestHelper.RestPostObject<NippsPackageResponse, NippsPackageRequest>(upgradeServiceUrl, request);
            ViewBag.ResultList = response.ResultMessages;
            SetViewBagResult(response, ViewBag);

            return View(nipps);
        }

        [LoginAndAuthorize]
        public ActionResult NippsUpgradeConfirm(Models.Nipps nipps, string upgradeItem)
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            logger.Debug("{0}, {1}", nipps, upgradeItem);
            ViewBag.UpgradeItem = upgradeItem;

            return View(nipps);
        }

        [LoginAndAuthorize]
        public ActionResult NippsUpgrade(Models.Nipps nipps, string upgrade)
        {
            ViewBag.Title = Resources.Global.UpgradeTitle;

            string restoreServiceUrl = NippsSiteHelper.BuildDeploymentServiceUrl(nipps.HostName, "BackupService/UpgradeApplication");
            NippsPackageRequest request = new NippsPackageRequest
            {
                NippsPackages = new List<NippsPackage>
                {
                    new NippsPackage
                    {
                        HostName = nipps.HostName,
                        SiteName = nipps.SiteName,
                        ApplicationName = nipps.ApplicationName,
                        ApplicationPoolName = nipps.ApplicationPoolName,
                        PackageZIP = upgrade
                    }
                }
            };

            NippsPackageResponse response = RestHelper.RestPostObject<NippsPackageResponse, NippsPackageRequest>(restoreServiceUrl, request);

            ViewBag.Result = response.Result;
            ViewBag.ResultMessages = response.ResultMessages;
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.UpgradeTitle;

            return View(NippsSiteHelper.ResultMessageView);
        }

        #endregion

        #region config-backup
        [LoginAndAuthorize]
        public ActionResult NippsConfigBackupConfirm()
        {
            return View();
        }

        [LoginAndAuthorize]
        public ActionResult NippsConfigBackup()
        {

            string backupServiceUrl = CommonHelper.DeployManagerServiceUrl + "BackupService/BackupConfiguration";
            BaseResponse response = RestHelper.RestGet<NippsPackageResponse>(backupServiceUrl);

            ViewBag.Result = response.Result;
            ViewBag.ResultMessages = response.ResultMessages;
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.BackupTitle;

            return View(NippsSiteHelper.ResultMessageView);
        }
        #endregion

        #region config-restore
        [LoginAndAuthorize]
        public ActionResult NippsConfigRestoreList()
        {
            string restoreServiceUrl = CommonHelper.DeployManagerServiceUrl + "BackupService/RestoreConfigurationList";
            BaseResponse response = RestHelper.RestGet<NippsPackageResponse>(restoreServiceUrl);

            ViewBag.Result = response.Result;
            ViewBag.ResultMessages = response.ResultMessages;
            ViewBag.Title = Resources.Global.BackupTitle;

            return View();
        }

        [LoginAndAuthorize]
        public ActionResult NippsConfigRestoreConfirm(string request)
        {
            ViewBag.RestoreItem = request;
            return View();
        }

        [LoginAndAuthorize]
        public ActionResult NippsConfigRestore(string request)
        {
            string restoreServiceUrl = CommonHelper.DeployManagerServiceUrl + "BackupService/RestoreConfiguration";
            BaseResponse response = RestHelper.RestPostObject<NippsPackageResponse, NippsPackageRequest>(
                restoreServiceUrl,
                new NippsPackageRequest { NippsPackages = new List<NippsPackage> { new NippsPackage{ PackageZIP = request } } }
                );

            ViewBag.Result = response.Result;
            ViewBag.ResultMessages = response.ResultMessages;
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.BackupTitle;

            return View(NippsSiteHelper.ResultMessageView);
        }

        #endregion

        #region server-list
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
            return Json(NippsSiteHelper.SelectListHost());
        }
        #endregion

    }
}