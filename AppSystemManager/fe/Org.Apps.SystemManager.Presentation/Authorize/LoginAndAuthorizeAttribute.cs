﻿using System;
using System.Web.Mvc;
using System.ServiceModel;
using System.Configuration;
using System.Collections.Generic;

using Org.Apps.BaseService;
using Org.Apps.BaseDao.Model.Response;

using Org.Apps.AuthManager.Data.Model;
using Org.Apps.AuthManager.Data.Model.Request;
using Org.Apps.AuthManager.Data.Model.Response;

using Org.Apps.SystemManager.Presentation.Base;
using Org.Apps.SystemManager.Presentation.Helpers;
using Org.Apps.SystemManager.Presentation.Controllers;

namespace Org.Apps.SystemManager.Presentation.Authorize
{
    public class LoginAndAuthorizeAttribute : AuthorizeAttribute
    {

        public String View { get; set; }
        public String Master { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {

            string isUserAuthenticated = filterContext.HttpContext.Session["IsUserAuthenticated"] == null ? "false" : filterContext.HttpContext.Session["IsUserAuthenticated"].ToString();
            User user = filterContext.HttpContext.Session["User"] == null ? null : (User)filterContext.HttpContext.Session["User"];

            if (isUserAuthenticated == null || !bool.Parse(isUserAuthenticated) || user == null || !GetUserByName(user.UserName))
            {
                filterContext.HttpContext.Session.Clear();
                filterContext.HttpContext.Session.Add("IsUserAuthenticated", false);
                if (filterContext.Controller is BaseController) {
                    BaseController requesterController = (BaseController)filterContext.Controller;
                    filterContext.Result = requesterController.RedirectToAction("Index","Home");
                }   
                else
                    filterContext.Result = new ViewResult { ViewName = "Index", MasterName = "Home" };
                return;

            }


            filterContext.Result = null;

        }

        private bool GetUserByName(string userName)
        {
            string svcUri = CommonHelper.AuthManagerServiceUrl + "UserService/GetByName";
            UserRequest userRequest = new UserRequest
            {
                Users = new List<User> { new User { UserName = userName } }
            };

            UserResponse userResponse = RestHelper.RestPostObject<UserResponse, UserRequest>(svcUri, userRequest);
            if (userResponse.Result == Result.OK)
                return true;

            return false;

            
        }

    }
}