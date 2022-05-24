using System;
using System.Reflection;
using System.Diagnostics;
using System.Configuration;
using Org.Apps.LicenseManager.Data.Model;
using Org.Apps.LicenseManager.Data.Model.Request;
using Org.Apps.LicenseManager.Data.Model.Response;

namespace Org.Apps.BaseService
{
    public static class AppsLicenseHelper
    {
        public static Assembly AssemblyLicense(Assembly ipps)
        {
            try
            {
                string serviceUrl = ConfigurationManager.AppSettings["LicenseManagerServiceUrl"] + "LicenseService/Ipps";
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(ipps.Location);
                string fName = fvi.InternalName.Replace("Org.Apps.Service.", "").Replace(".dll", "");
                AppsLicenseRequest licReq = new AppsLicenseRequest { Version = string.Format("{0}:{1}", fName, fvi.FileVersion) };
                AppsLicenseResponse licRes = RestHelper.RestPostObject<AppsLicenseResponse, AppsLicenseRequest>(serviceUrl, licReq);
                return licRes.Result == BaseDao.Model.Response.Result.OK ? ipps : null;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return null;

        }
    }
}
