using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Collections.Generic;

using Netas.Nipps.SystemManager.Presentation.Authorize;
using Netas.Nipps.SystemManager.Presentation.Base;
using Netas.Nipps.SystemManager.Presentation.Models;
using Netas.Nipps.SystemManager.Presentation.Helpers;

using Netas.Nipps.BaseDao.Model.Response;

using Netas.Nipps.LicenseManager.Data.Model;
using Netas.Nipps.LicenseManager.Data.Model.Request;
using Netas.Nipps.LicenseManager.Data.Model.Response;

using Netas.Nipps.ConfigManager.Data.Model;
using Netas.Nipps.ConfigManager.Data.Model.Request;
using Netas.Nipps.ConfigManager.Data.Model.Response;

using Netas.Nipps.BaseService;

namespace Netas.Nipps.SystemManager.Presentation.Controllers
{
    public class LicenseManagementController : BaseController
    {
        static string ReturnToAction { get { return "License"; } }
        static string ReturnToController { get { return "LicenseManagement"; } }

        [LoginAndAuthorize]
        public ActionResult License()
        {
            NippsLicense nippsLicense = new NippsLicense { 
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
                NippsLicenseResponse licenseResponse = RestHelper.RestGet<NippsLicenseResponse>(svcUrl);
                if (licenseResponse.Result == Result.OK)
                    nippsLicense = licenseResponse.License;

            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            
            return View(nippsLicense);
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
                NippsLicenseRequest licenseRequest = new NippsLicenseRequest { Content = licenseString };
                NippsLicenseResponse licenseResponse = RestHelper.RestPostObject<NippsLicenseResponse, NippsLicenseRequest>(svcUrl, licenseRequest);
                
                if (licenseResponse.Result != Result.OK)
                {
                    Logger.Error(licenseResponse.ResultMessages[0]);
                    ViewBag.Result = Result.FAIL;
                    ViewBag.ResultMessages = new List<string> { Resources.Global.MessageUnknownError };
                    ViewBag.ReturnToAction = ReturnToAction;
                    ViewBag.ReturnToController = ReturnToController;
                    ViewBag.Title = Resources.Global.LicenseAddTitle;

                    return View(NippsSiteHelper.ResultMessageView);
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

                return View(NippsSiteHelper.ResultMessageView);
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