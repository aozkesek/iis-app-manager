using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Netas.Nipps.BaseService;

using Netas.Nipps.BaseDao.Model.Response;

using Netas.Nipps.DeployManager.Data.Model.Response;

using Netas.Nipps.ConfigManager.Data.Model;
using Netas.Nipps.ConfigManager.Data.Model.Request;
using Netas.Nipps.ConfigManager.Data.Model.Response;

using Netas.Nipps.SystemManager.Presentation.Helpers;
using Netas.Nipps.SystemManager.Presentation.Base;
using Netas.Nipps.SystemManager.Presentation.Models;
using Netas.Nipps.SystemManager.Presentation.Authorize;

namespace Netas.Nipps.SystemManager.Presentation.Controllers
{
    public class OperationalMetricsController : BaseController
    {

        [LoginAndAuthorize]
        public ActionResult OpMetricList()
        {
            ViewBag.ResultList = NippsSiteHelper.ListNipps();

            if (ViewBag.ResultList != null && ViewBag.ResultList.Count > 0)
                SetViewBagResult(new NippsPackageResponse { Result = Result.OK }, ViewBag);
            else
                SetViewBagResult(new NippsPackageResponse { Result = Result.FAIL, ResultMessages = new List<string> { Resources.Global.MessageUnknownError } }, ViewBag);

            return View(); 
        }

        [LoginAndAuthorize]
        public ActionResult ServiceOpMetrics(Models.Nipps nipps)
        {

            GetOperationalMetricsParam(nipps);

            return View(nipps);
        }

        [LoginAndAuthorize]
        public ActionResult ServiceOpMetricDetail(Models.Nipps nipps, string name, string path, string headers)
        {
            ViewBag.Result = Result.FAIL;
            if (nipps == null || string.IsNullOrEmpty(nipps.HostName) || string.IsNullOrEmpty(nipps.SiteName) || string.IsNullOrEmpty(nipps.ApplicationName))
                return View(nipps);

            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(headers))
                return View(nipps);

            try
            {
                ViewBag.Headers = headers.Split('|');
                ViewBag.Name = name;

                string svcUrl = NippsSiteHelper.ServiceBaseUrl(nipps.HostName, nipps.SiteName, nipps.ApplicationName) + path;
                NippsOpMetricData omData = RestHelper.RestGet<NippsOpMetricData>(svcUrl);
                if (omData == null || omData.Data == null || omData.Data.Count == 0)
                    return View(nipps);

                ViewBag.Result = Result.OK;
                ViewBag.ResultList = omData;

            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }

            
            return View(nipps);
        }

        [LoginAndAuthorize]
        public ActionResult ServiceOpMetricEnable(Models.Nipps nipps, string name, string path)
        {
            GetOperationalMetricsParam(nipps);

            if (ViewBag.Result == Result.OK)
            {
                List<NippsOperationalMetric> omMetrics = ViewBag.ResultList;
                
                //find requested op metric
                NippsOperationalMetric omMetric = omMetrics.Where(om => om.Name.Equals(name)).Single();
                
                //inverse the Active flag
                omMetric.Active = !omMetric.Active;
                if (omMetric.Active)
                {
                    //reset op metric values 
                    try
                    {
                        string svcUrl = NippsSiteHelper.ServiceBaseUrl(nipps.HostName, nipps.SiteName, nipps.ApplicationName) + path;
                        NippsOpMetricData omData = RestHelper.RestPostObject<NippsOpMetricData, string>(svcUrl, "");
                    }
                    catch (Exception ex) { Logger.Error(ex.ToString()); }

                }

                //save op metrics
                SaveOperationalMetricsParam(nipps, omMetrics);

                ViewBag.ResultList = omMetrics;

            }

            return View("ServiceOpMetrics", nipps);
        }

        void SaveOperationalMetricsParam(Models.Nipps nipps, List<NippsOperationalMetric> opMetrics)
        {
            string paramName = nipps.ApplicationName.Replace("/Netas.Nipps.Service.", "").ToUpper();
            string svcUrl = CommonHelper.ConfigManagerServiceUrl + "NippsParameterService/Update";

            try
            {
                NippsParameterRequest omRequest = new NippsParameterRequest
                {
                    NippsParameters = new List<NippsParameter> 
                    { new NippsParameter { CategoryName = "OM", ParameterName = paramName, ParameterValue = Newtonsoft.Json.JsonConvert.SerializeObject(opMetrics) } }
                };
                NippsParameterResponse omResponse = RestHelper.RestPostObject<NippsParameterResponse, NippsParameterRequest>(svcUrl, omRequest);
                
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        void GetOperationalMetricsParam(Models.Nipps nipps)
        {
            string paramName = nipps.ApplicationName.Replace("/Netas.Nipps.Service.", "").ToUpper();
            string svcUrl = CommonHelper.ConfigManagerServiceUrl + "NippsParameterService/Get";

            ViewBag.Result = Result.FAIL;
            ViewBag.ResultMessages = new List<string>();

            try
            {
                NippsParameterRequest omRequest = new NippsParameterRequest
                {
                    NippsParameters = new List<NippsParameter> { new NippsParameter { CategoryName = "OM", ParameterName = paramName } }
                };
                NippsParameterResponse omResponse = RestHelper.RestPostObject<NippsParameterResponse, NippsParameterRequest>(svcUrl, omRequest);
                if (omResponse.Result != Result.OK)
                    return;

                if (omResponse.NippsParameters == null || omResponse.NippsParameters.Count != 1)
                    return;

                if (string.IsNullOrEmpty(omResponse.NippsParameters[0].ParameterValue))
                    return;

                List<NippsOperationalMetric> omMetrics = Newtonsoft.Json.JsonConvert.DeserializeObject<List<NippsOperationalMetric>>(omResponse.NippsParameters[0].ParameterValue);
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