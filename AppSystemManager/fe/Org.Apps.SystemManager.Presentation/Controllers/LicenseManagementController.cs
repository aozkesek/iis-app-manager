﻿using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Collections.Generic;

using Org.Apps.SystemManager.Presentation.Authorize;
using Org.Apps.SystemManager.Presentation.Base;
using Org.Apps.SystemManager.Presentation.Models;
using Org.Apps.SystemManager.Presentation.Helpers;

using Org.Apps.BaseDao.Model.Response;

using Org.Apps.LicenseManager.Data.Model;
using Org.Apps.LicenseManager.Data.Model.Request;
using Org.Apps.LicenseManager.Data.Model.Response;

using Org.Apps.ConfigManager.Data.Model;
using Org.Apps.ConfigManager.Data.Model.Request;
using Org.Apps.ConfigManager.Data.Model.Response;

using Org.Apps.BaseService;

namespace Org.Apps.SystemManager.Presentation.Controllers
{
    public class LicenseManagementController : BaseController
    {
        static string ReturnToAction { get { return "License"; } }
        static string ReturnToController { get { return "LicenseManagement"; } }

        [LoginAndAuthorize]
        public ActionResult License()
        {
            AppsLicense AppsLicense = new AppsLicense { 
                    Type = "", 
                    ValidFor = "",
                    IssuedBy = "",
                    LicensedTo = "",
                    Version = "",
                    Services = null 
                };

            try
            {
                string svcUrl = CommonHelper.LicenseManagerServiceUrl + "LicenseService/Get";
                AppsLicenseResponse licenseResponse = RestHelper.RestGet<AppsLicenseResponse>(svcUrl);
                if (licenseResponse.Result == Result.OK)
                    AppsLicense = licenseResponse.License;

            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            
            return View(AppsLicense);
        }

        [LoginAndAuthorize]
        public ActionResult LicenseAdd(HttpPostedFileBase license)
        {
            string sha1a = "---BEGIN SHA1 A---";
            string sha1b = "---BEGIN SHA1 B---";
            
            try
            {
                byte[] licenseBuffer = new byte[license.ContentLength];
                license.InputStream.Read(licenseBuffer, 0, license.ContentLength);
                string licenseString = System.Text.Encoding.Default.GetString(licenseBuffer);

                if (string.IsNullOrEmpty(licenseString) || !licenseString.Contains(sha1a) || !licenseString.Contains(sha1b))
                    throw new InvalidDataException("SHA1 parts are not found in the license file.");

                string svcUrl = CommonHelper.LicenseManagerServiceUrl + "LicenseService/Load";
                AppsLicenseRequest licenseRequest = new AppsLicenseRequest { Content = licenseString };
                AppsLicenseResponse licenseResponse = RestHelper.RestPostObject<AppsLicenseResponse, AppsLicenseRequest>(svcUrl, licenseRequest);
                
                if (licenseResponse.Result != Result.OK)
                {
                    Logger.Error(licenseResponse.ResultMessages[0]);
                    ViewBag.Result = Result.FAIL;
                    ViewBag.ResultMessages = new List<string> { Resources.Global.MessageUnknownError };
                    ViewBag.ReturnToAction = ReturnToAction;
                    ViewBag.ReturnToController = ReturnToController;
                    ViewBag.Title = Resources.Global.LicenseAddTitle;

                    return View(AppsSiteHelper.ResultMessageView);
                }

            }
            catch(Exception ex) 
            {
                Logger.Error(ex.ToString());
                ViewBag.Result = Result.FAIL;
                ViewBag.ResultMessages = new List<string> { Resources.Global.MessageUnknownError };
                ViewBag.ReturnToAction = ReturnToAction;
                ViewBag.ReturnToController = ReturnToController;
                ViewBag.Title = Resources.Global.LicenseAddTitle;

                return View(AppsSiteHelper.ResultMessageView);
            }
                        
            return RedirectToAction("License");
        }

        [LoginAndAuthorize]
        public ActionResult LicenseAddConfirm()
        {
            return View();
        }

    }
}