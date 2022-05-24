using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Org.Apps.BaseService;

using Org.Apps.BaseDao.Model.Response;

using Org.Apps.DeployManager.Data.Model.Response;

using Org.Apps.ConfigManager.Data.Model;
using Org.Apps.ConfigManager.Data.Model.Request;
using Org.Apps.ConfigManager.Data.Model.Response;

using Org.Apps.SystemManager.Presentation.Helpers;
using Org.Apps.SystemManager.Presentation.Base;
using Org.Apps.SystemManager.Presentation.Models;
using Org.Apps.SystemManager.Presentation.Authorize;

namespace Org.Apps.SystemManager.Presentation.Controllers
{
    public class OperationalMetricsController : BaseController
    {

        [LoginAndAuthorize]
        public ActionResult OpMetricList()
        {
            ViewBag.ResultList = AppsSiteHelper.ListApps();

            if (ViewBag.ResultList != null && ViewBag.ResultList.Count > 0)
                SetViewBagResult(new AppsPackageResponse { Result = Result.OK }, ViewBag);
            else
                SetViewBagResult(new AppsPackageResponse { Result = Result.FAIL, ResultMessages = new List<string> { Resources.Global.MessageUnknownError } }, ViewBag);

            return View(); 
        }

        [LoginAndAuthorize]
        public ActionResult ServiceOpMetrics(Models.Apps Apps)
        {

            GetOperationalMetricsParam(Apps);

            return View(Apps);
        }

        [LoginAndAuthorize]
        public ActionResult ServiceOpMetricDetail(Models.Apps Apps, string name, string path, string headers)
        {
            ViewBag.Result = Result.FAIL;
            if (Apps == null || string.IsNullOrEmpty(Apps.HostName) || string.IsNullOrEmpty(Apps.SiteName) || string.IsNullOrEmpty(Apps.ApplicationName))
                return View(Apps);

            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(headers))
                return View(Apps);

            try
            {
                ViewBag.Headers = headers.Split('|');
                ViewBag.Name = name;

                string svcUrl = AppsSiteHelper.ServiceBaseUrl(Apps.HostName, Apps.SiteName, Apps.ApplicationName) + path;
                AppsOpMetricData omData = RestHelper.RestGet<AppsOpMetricData>(svcUrl);
                if (omData == null || omData.Data == null || omData.Data.Count == 0)
                    return View(Apps);

                ViewBag.Result = Result.OK;
                ViewBag.ResultList = omData;

            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }

            
            return View(Apps);
        }

        [LoginAndAuthorize]
        public ActionResult ServiceOpMetricEnable(Models.Apps Apps, string name, string path)
        {
            GetOperationalMetricsParam(Apps);

            if (ViewBag.Result == Result.OK)
            {
                List<AppsOperationalMetric> omMetrics = ViewBag.ResultList;
                
                //find requested op metric
                AppsOperationalMetric omMetric = omMetrics.Where(om => om.Name.Equals(name)).Single();
                
                //inverse the Active flag
                omMetric.Active = !omMetric.Active;
                if (omMetric.Active)
                {
                    //reset op metric values 
                    try
                    {
                        string svcUrl = AppsSiteHelper.ServiceBaseUrl(Apps.HostName, Apps.SiteName, Apps.ApplicationName) + path;
                        AppsOpMetricData omData = RestHelper.RestPostObject<AppsOpMetricData, string>(svcUrl, "");
                    }
                    catch (Exception ex) { Logger.Error(ex.ToString()); }

                }

                //save op metrics
                SaveOperationalMetricsParam(Apps, omMetrics);

                ViewBag.ResultList = omMetrics;

            }

            return View("ServiceOpMetrics", Apps);
        }

        void SaveOperationalMetricsParam(Models.Apps Apps, List<AppsOperationalMetric> opMetrics)
        {
            string paramName = Apps.ApplicationName.Replace("/Org.Apps.Service.", "").ToUpper();
            string svcUrl = CommonHelper.ConfigManagerServiceUrl + "AppsParameterService/Update";

            try
            {
                AppsParameterRequest omRequest = new AppsParameterRequest
                {
                    AppsParameters = new List<AppsParameter> 
                    { new AppsParameter { CategoryName = "OM", ParameterName = paramName, ParameterValue = Newtonsoft.Json.JsonConvert.SerializeObject(opMetrics) } }
                };
                AppsParameterResponse omResponse = RestHelper.RestPostObject<AppsParameterResponse, AppsParameterRequest>(svcUrl, omRequest);
                
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        void GetOperationalMetricsParam(Models.Apps Apps)
        {
            string paramName = Apps.ApplicationName.Replace("/Org.Apps.Service.", "").ToUpper();
            string svcUrl = CommonHelper.ConfigManagerServiceUrl + "AppsParameterService/Get";

            ViewBag.Result = Result.FAIL;
            ViewBag.ResultMessages = new List<string>();

            try
            {
                AppsParameterRequest omRequest = new AppsParameterRequest
                {
                    AppsParameters = new List<AppsParameter> { new AppsParameter { CategoryName = "OM", ParameterName = paramName } }
                };
                AppsParameterResponse omResponse = RestHelper.RestPostObject<AppsParameterResponse, AppsParameterRequest>(svcUrl, omRequest);
                if (omResponse.Result != Result.OK)
                    return;

                if (omResponse.AppsParameters == null || omResponse.AppsParameters.Count != 1)
                    return;

                if (string.IsNullOrEmpty(omResponse.AppsParameters[0].ParameterValue))
                    return;

                List<AppsOperationalMetric> omMetrics = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AppsOperationalMetric>>(omResponse.AppsParameters[0].ParameterValue);
                if (omMetrics == null || omMetrics.Count == 0)
                    return;

                ViewBag.Result = Result.OK;
                ViewBag.ResultList = omMetrics;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }

        }
    }
}