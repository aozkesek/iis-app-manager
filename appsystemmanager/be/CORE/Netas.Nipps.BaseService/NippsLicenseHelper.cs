using System;
using System.Reflection;
using System.Diagnostics;
using System.Configuration;
using Netas.Nipps.LicenseManager.Data.Model;
using Netas.Nipps.LicenseManager.Data.Model.Request;
using Netas.Nipps.LicenseManager.Data.Model.Response;

namespace Netas.Nipps.BaseService
{
    public static class NippsLicenseHelper
    {
        public static Assembly AssemblyLicense(Assembly ipps)
        {
            try
            {
                string serviceUrl = ConfigurationManager.AppSettings["LicenseManagerServiceUrl"] + "LicenseService/Ipps";
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(ipps.Location);
                string fName = fvi.InternalName.Replace("Netas.Nipps.Service.", "").Replace(".dll", "");
                NippsLicenseRequest licReq = new NippsLicenseRequest { Version = string.Format("{0}:{1}", fName, fvi.FileVersion) };
                NippsLicenseResponse licRes = RestHelper.RestPostObject<NippsLicenseResponse, NippsLicenseRequest>(serviceUrl, licReq);
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
