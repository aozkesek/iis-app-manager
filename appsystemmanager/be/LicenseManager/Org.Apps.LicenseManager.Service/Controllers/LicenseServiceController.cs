using System;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Collections.Generic;

using Org.Apps.BaseService;

using Org.Apps.BaseDao.Model.Response;

using Org.Apps.LicenseManager.Data.Model;
using Org.Apps.LicenseManager.Data.Model.Request;
using Org.Apps.LicenseManager.Data.Model.Response;

namespace Org.Apps.LicenseManager.Service.Controllers
{
    [RoutePrefix("api/LicenseService")]
    public class LicenseServiceController : BaseApiController
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        [HttpPost]
        [Route("Load")]
        public BaseResponse Load(AppsLicenseRequest request)
        {
            AppsLicenseResponse response = new AppsLicenseResponse();
            response.Result = Result.OK;
            response.ResultMessages = new List<string>();

            try
            {
                if (string.IsNullOrEmpty(request.Content))
                {
                    response.Result = Result.FAIL;
                    response.ResultMessages.Add("Request.Content");
                    Logger.Error(response.ResultMessages[0]);
                }
                else
                {
                    if (!LicenseWrapper.Valid(request.Content))
                    {
                        response.Result = Result.FAIL;
                        response.ResultMessages.Add(string.Format("License is not VALID."));
                    }
                }
                    
            }
            catch(Exception ex) 
            {
                Logger.Error(ex.ToString());
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.ToString());
            }
            
            return response;
        }

        [HttpPost]
        [Route("Ipps")]
        public BaseResponse Ipps(AppsLicenseRequest ippsRequest)
        {
            AppsLicenseResponse response = new AppsLicenseResponse();
            response.Result = Result.OK;
            response.ResultMessages = new List<string>();

            try
            {
                if (ippsRequest == null || string.IsNullOrEmpty(ippsRequest.Version))
                    throw new ArgumentNullException("Request.Version.");

                string[] psParts = ippsRequest.Version.Split(':');
                if (psParts == null || psParts.Length != 2)
                    throw new ArgumentException("Request.Version");

                if (!LicenseWrapper.Valid())
                {
                    response.Result = Result.FAIL;
                    response.ResultMessages.Add(string.Format("License is not VALID."));
                }

                LicenseWrapper.Valid(psParts[0], psParts[1]);
                
            }
            catch (Exception ex)
            {
                Logger.Error("{0}> {1}", ippsRequest.ToString(), ex.ToString());
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.ToString());
            }
            return response;
        }

        [HttpGet]
        [Route("Get")]
        public AppsLicenseResponse Get()
        {
            AppsLicenseResponse response = new AppsLicenseResponse();
            response.Result = Result.OK;
            response.ResultMessages = new List<string>();

            try
            {
                if (!LicenseWrapper.Valid())
                {
                    response.Result = Result.FAIL;
                    response.ResultMessages.Add(string.Format("License is not VALID."));
                }

                response.License = LicenseWrapper.License;

            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.ToString());
            }
            return response;
        }

    }
}
