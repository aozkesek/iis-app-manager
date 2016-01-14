
﻿using System;
using System.Web.Mvc;
using System.ServiceModel;
using System.Configuration;
using System.Collections.Generic;

using Netas.Nipps.BaseService;

using Netas.Nipps.ConfigManager.Data.Model;
using Netas.Nipps.ConfigManager.Data.Model.Request;
using Netas.Nipps.ConfigManager.Data.Model.Response;

using Netas.Nipps.BaseDao.Model.Response;

using Netas.Nipps.SystemManager.Presentation.Models;
using Netas.Nipps.SystemManager.Presentation.Authorize;
using Netas.Nipps.SystemManager.Presentation.Base;
using Netas.Nipps.SystemManager.Presentation.Helpers;


namespace Netas.Nipps.SystemManager.Presentation.Controllers
{

    public class ConfigManagementController : BaseController
    {
        static readonly string ReturnToAction = "NippsParameterList";
        static readonly string ReturnToController = "ConfigManagement";

        #region EDIT PARAMETER

        [LoginAndAuthorize]
        public ActionResult NippsParameterEditConfirm(string categoryName, string parameterName)
        {
            try
            {
                string svcUrl = CommonHelper.ConfigManagerServiceUrl + "NippsParameterService/Get";
                NippsParameterRequest nippsParameterRequest = new NippsParameterRequest
                {
                    NippsParameters = new List<NippsParameter>
                    {
                        new NippsParameter { CategoryName = categoryName, ParameterName = parameterName }
                    }
                };
                NippsParameterResponse nippsParameterResponse = RestHelper.RestPostObject<NippsParameterResponse, NippsParameterRequest>(svcUrl, nippsParameterRequest);

                return View(nippsParameterResponse.NippsParameters[0]);
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
        public ActionResult NippsParameterEdit(NippsParameter parameterEdit)
        {
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.ParameterChange;
            ViewBag.Name = Resources.Global.ParameterChange;

            try
            {
                string svcUrl = CommonHelper.ConfigManagerServiceUrl + "NippsParameterService/Update";
                NippsParameterRequest request = new NippsParameterRequest { NippsParameters = new List<NippsParameter> { parameterEdit } };
                NippsParameterResponse response = RestHelper.RestPostObject<NippsParameterResponse, NippsParameterRequest>(svcUrl, request);

                if (response.Result == Result.OK)
                    return RedirectToAction("NippsParameterList");

                SetViewBagResult(response, ViewBag);

            }
            catch (Exception ex)
            {
                Logger.Error("{0}: {1}", parameterEdit, ex.ToString());
                SetViewBagResult(new NippsParameterResponse { Result = Result.FAIL, ResultMessages = new List<string> { Resources.Global.MessageUnknownError } }, ViewBag);
            }

            return View(NippsSiteHelper.ResultMessageView);
        }

        #endregion EDIT PARAMETER

        #region LIST PARAMATER

        [LoginAndAuthorize]
        public ActionResult NippsParameterList()
        {
            try
            {
                string svcUrl = CommonHelper.ConfigManagerServiceUrl + "NippsParameterService/List";
                NippsParameterResponse nippsParameterResponse = RestHelper.RestPostObject<NippsParameterResponse, NippsParameterRequest>(svcUrl, new NippsParameterRequest());
                ViewBag.ResultList = nippsParameterResponse.NippsParameters;
                SetViewBagResult(nippsParameterResponse, ViewBag);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                SetViewBagResult(new NippsParameterResponse { Result = Result.FAIL, ResultMessages = new List<string> { ex.ToString() } }, ViewBag);
            }
            return View();
        }

        #endregion LIST PARAMATER

    }
}
