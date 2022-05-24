using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Configuration;
using System.Text.RegularExpressions;

using Org.Apps.BaseService;

using Org.Apps.BaseDao.Model.Response;

using Org.Apps.DeployManager.Data.Model;
using Org.Apps.DeployManager.Data.Model.Request;
using Org.Apps.DeployManager.Data.Model.Response;

using Org.Apps.SystemManager.Presentation.Authorize;
using Org.Apps.SystemManager.Presentation.Helpers;
using Org.Apps.SystemManager.Presentation.Base;

namespace Org.Apps.SystemManager.Presentation.Controllers
{
    public class BackupManagementController : BaseController
    {
        static readonly string ReturnToAction = "AppsServiceList";
        static readonly string ReturnToController = "DeployManagement";

        #region backup
        [LoginAndAuthorize]
        public ActionResult AppsBackup(Models.Apps Apps)
        {

            ViewBag.Title = Resources.Global.BackupTitle;

            string backupServiceUrl = AppsSiteHelper.BuildDeploymentServiceUrl(Apps.HostName, "BackupService/BackupApplication");
            AppsPackageRequest backupRequest = new AppsPackageRequest 
            {
                AppsPackages = new List<AppsPackage>
                {
                    new AppsPackage
                    {
                        ApplicationName = Apps.ApplicationName,
                        SiteName = Apps.SiteName,
                        HostName = Apps.HostName
                    }
                }
            };

            AppsPackageResponse backupResponse = RestHelper.RestPostObject<AppsPackageResponse, AppsPackageRequest>(backupServiceUrl, backupRequest);

            ViewBag.Result = backupResponse.Result;
            ViewBag.ResultMessages = backupResponse.ResultMessages;
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.BackupTitle;

            return View(AppsSiteHelper.ResultMessageView);
        }

        [LoginAndAuthorize]
        public ActionResult AppsBackupConfirm(Models.Apps Apps)
        {
            return View(Apps);
        }
        #endregion

        #region restore
        [LoginAndAuthorize]
        public ActionResult AppsRestore(Models.Apps Apps, string restore)
        {

            ViewBag.Title = Resources.Global.RestoreTitle;

            string restoreServiceUrl = AppsSiteHelper.BuildDeploymentServiceUrl(Apps.HostName, "BackupService/RestoreApplication");
            AppsPackageRequest request = new AppsPackageRequest
            {
                AppsPackages = new List<AppsPackage>
                {
                    new AppsPackage
                    {
                        HostName = Apps.HostName,
                        SiteName = Apps.SiteName,
                        ApplicationName = Apps.ApplicationName,
                        ApplicationPoolName = Apps.ApplicationPoolName,
                        PackageZIP = restore
                    }
                }
            };

            AppsPackageResponse response = RestHelper.RestPostObject<AppsPackageResponse, AppsPackageRequest>(restoreServiceUrl, request);

            ViewBag.Result = response.Result;
            ViewBag.ResultMessages = response.ResultMessages;
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.RestoreTitle;

            return View(AppsSiteHelper.ResultMessageView);
        }

        [LoginAndAuthorize]
        public ActionResult AppsRestoreConfirm(Models.Apps Apps, String restoreItem)
        {
            ViewBag.RestoreItem = restoreItem;

            return View(Apps);
        }

        [LoginAndAuthorize]
        public ActionResult AppsRestoreList(Models.Apps Apps)
        {
            string restoreServiceUrl = AppsSiteHelper.BuildDeploymentServiceUrl(Apps.HostName, "BackupService/RestoreApplicationList");
            AppsPackageRequest restoreRequest = new AppsPackageRequest { AppsPackages = new List<AppsPackage>() };

            if (!String.IsNullOrEmpty(Apps.HostName))
                restoreRequest.AppsPackages.Add(new AppsPackage { 
                    HostName = Apps.HostName,
                    SiteName = Apps.SiteName,
                    ApplicationName = Apps.ApplicationName
                });

            AppsPackageResponse restoreResponse = RestHelper.RestPostObject<AppsPackageResponse, AppsPackageRequest>(restoreServiceUrl, restoreRequest);
            ViewBag.ResultList = restoreResponse.ResultMessages;
            SetViewBagResult(restoreResponse, ViewBag);

            return View(Apps);
        }
        #endregion

        #region upgrade
        [LoginAndAuthorize]
        public ActionResult AppsUpgradeList(Models.Apps Apps)
        {
            string upgradeServiceUrl = AppsSiteHelper.BuildDeploymentServiceUrl(Apps.HostName, "BackupService/UpgradeApplicationList");
            AppsPackageRequest request = new AppsPackageRequest
            {
                AppsPackages = new List<AppsPackage>()
            };

            if (!String.IsNullOrEmpty(Apps.HostName))
                request.AppsPackages.Add(new AppsPackage
                {
                    HostName = Apps.HostName,
                    SiteName = Apps.SiteName,
                    ApplicationName = Apps.ApplicationName
                });

            AppsPackageResponse response = RestHelper.RestPostObject<AppsPackageResponse, AppsPackageRequest>(upgradeServiceUrl, request);
            ViewBag.ResultList = response.ResultMessages;
            SetViewBagResult(response, ViewBag);

            return View(Apps);
        }

        [LoginAndAuthorize]
        public ActionResult AppsUpgradeConfirm(Models.Apps Apps, string upgradeItem)
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            logger.Debug("{0}, {1}", Apps, upgradeItem);
            ViewBag.UpgradeItem = upgradeItem;

            return View(Apps);
        }

        [LoginAndAuthorize]
        public ActionResult AppsUpgrade(Models.Apps Apps, string upgrade)
        {
            ViewBag.Title = Resources.Global.UpgradeTitle;

            string restoreServiceUrl = AppsSiteHelper.BuildDeploymentServiceUrl(Apps.HostName, "BackupService/UpgradeApplication");
            AppsPackageRequest request = new AppsPackageRequest
            {
                AppsPackages = new List<AppsPackage>
                {
                    new AppsPackage
                    {
                        HostName = Apps.HostName,
                        SiteName = Apps.SiteName,
                        ApplicationName = Apps.ApplicationName,
                        ApplicationPoolName = Apps.ApplicationPoolName,
                        PackageZIP = upgrade
                    }
                }
            };

            AppsPackageResponse response = RestHelper.RestPostObject<AppsPackageResponse, AppsPackageRequest>(restoreServiceUrl, request);

            ViewBag.Result = response.Result;
            ViewBag.ResultMessages = response.ResultMessages;
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.UpgradeTitle;

            return View(AppsSiteHelper.ResultMessageView);
        }

        #endregion

        #region config-backup
        [LoginAndAuthorize]
        public ActionResult AppsConfigBackupConfirm()
        {
            return View();
        }

        [LoginAndAuthorize]
        public ActionResult AppsConfigBackup()
        {

            string backupServiceUrl = CommonHelper.DeployManagerServiceUrl + "BackupService/BackupConfiguration";
            BaseResponse response = RestHelper.RestGet<AppsPackageResponse>(backupServiceUrl);

            ViewBag.Result = response.Result;
            ViewBag.ResultMessages = response.ResultMessages;
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.BackupTitle;

            return View(AppsSiteHelper.ResultMessageView);
        }
        #endregion

        #region config-restore
        [LoginAndAuthorize]
        public ActionResult AppsConfigRestoreList()
        {
            string restoreServiceUrl = CommonHelper.DeployManagerServiceUrl + "BackupService/RestoreConfigurationList";
            BaseResponse response = RestHelper.RestGet<AppsPackageResponse>(restoreServiceUrl);

            ViewBag.Result = response.Result;
            ViewBag.ResultMessages = response.ResultMessages;
            ViewBag.Title = Resources.Global.BackupTitle;

            return View();
        }

        [LoginAndAuthorize]
        public ActionResult AppsConfigRestoreConfirm(string request)
        {
            ViewBag.RestoreItem = request;
            return View();
        }

        [LoginAndAuthorize]
        public ActionResult AppsConfigRestore(string request)
        {
            string restoreServiceUrl = CommonHelper.DeployManagerServiceUrl + "BackupService/RestoreConfiguration";
            BaseResponse response = RestHelper.RestPostObject<AppsPackageResponse, AppsPackageRequest>(
                restoreServiceUrl,
                new AppsPackageRequest { AppsPackages = new List<AppsPackage> { new AppsPackage{ PackageZIP = request } } }
                );

            ViewBag.Result = response.Result;
            ViewBag.ResultMessages = response.ResultMessages;
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.BackupTitle;

            return View(AppsSiteHelper.ResultMessageView);
        }

        #endregion

        #region server-list
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
            return Json(AppsSiteHelper.SelectListHost());
        }
        #endregion

    }
}