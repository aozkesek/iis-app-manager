
﻿using System;
using System.Web.Mvc;
using System.ServiceModel;
using System.Configuration;
using System.Collections.Generic;

using Org.Apps.BaseService;

using Org.Apps.ConfigManager.Data.Model;
using Org.Apps.ConfigManager.Data.Model.Request;
using Org.Apps.ConfigManager.Data.Model.Response;

using Org.Apps.BaseDao.Model.Response;

using Org.Apps.SystemManager.Presentation.Models;
using Org.Apps.SystemManager.Presentation.Authorize;
using Org.Apps.SystemManager.Presentation.Base;
using Org.Apps.SystemManager.Presentation.Helpers;


namespace Org.Apps.SystemManager.Presentation.Controllers
{

    public class ConfigManagementController : BaseController
    {
        static readonly string ReturnToAction = "AppsParameterList";
        static readonly string ReturnToController = "ConfigManagement";

        #region EDIT PARAMETER

        [LoginAndAuthorize]
        public ActionResult AppsParameterEditConfirm(string categoryName, string parameterName)
        {
            try
            {
                string svcUrl = CommonHelper.ConfigManagerServiceUrl + "AppsParameterService/Get";
                AppsParameterRequest AppsParameterRequest = new AppsParameterRequest
                {
                    AppsParameters = new List<AppsParameter>
                    {
                        new AppsParameter { CategoryName = categoryName, ParameterName = parameterName }
                    }
                };
                AppsParameterResponse AppsParameterResponse = RestHelper.RestPostObject<AppsParameterResponse, AppsParameterRequest>(svcUrl, AppsParameterRequest);

                return View(AppsParameterResponse.AppsParameters[0]);
            }
            catch (Exception ex)
            {
                Logger.Error("{0}-{1}: {2}", categoryName, parameterName, ex.ToString());
                ModelState.AddModelError("", Resources.Global.MessageUnknownError);
            }
            return View();
        }

        [HttpPost]
        [LoginAndAuthorize]
        public ActionResult AppsParameterEdit(AppsParameter parameterEdit)
        {
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.ParameterChange;
            ViewBag.Name = Resources.Global.ParameterChange;

            try
            {
                string svcUrl = CommonHelper.ConfigManagerServiceUrl + "AppsParameterService/Update";
                AppsParameterRequest request = new AppsParameterRequest { AppsParameters = new List<AppsParameter> { parameterEdit } };
                AppsParameterResponse response = RestHelper.RestPostObject<AppsParameterResponse, AppsParameterRequest>(svcUrl, request);

                if (response.Result == Result.OK)
                    return RedirectToAction("AppsParameterList");

                SetViewBagResult(response, ViewBag);

            }
            catch (Exception ex)
            {
                Logger.Error("{0}: {1}", parameterEdit, ex.ToString());
                SetViewBagResult(new AppsParameterResponse { Result = Result.FAIL, ResultMessages = new List<string> { Resources.Global.MessageUnknownError } }, ViewBag);
            }

            return View(AppsSiteHelper.ResultMessageView);
        }

        #endregion EDIT PARAMETER

        #region LIST PARAMATER

        [LoginAndAuthorize]
        public ActionResult AppsParameterList()
        {
            try
            {
                string svcUrl = CommonHelper.ConfigManagerServiceUrl + "AppsParameterService/List";
                AppsParameterResponse AppsParameterResponse = RestHelper.RestPostObject<AppsParameterResponse, AppsParameterRequest>(svcUrl, new AppsParameterRequest());
                ViewBag.ResultList = AppsParameterResponse.AppsParameters;
                SetViewBagResult(AppsParameterResponse, ViewBag);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                SetViewBagResult(new AppsParameterResponse { Result = Result.FAIL, ResultMessages = new List<string> { ex.ToString() } }, ViewBag);
            }
            return View();
        }

        #endregion LIST PARAMATER

    }
}
