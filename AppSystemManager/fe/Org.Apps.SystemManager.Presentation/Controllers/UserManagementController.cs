using System;
using System.Configuration;
using System.ServiceModel;
using System.Web.Mvc;
using System.Collections.Generic;

using Org.Apps.BaseService;
using Org.Apps.BaseDao;
using Org.Apps.BaseDao.Model.Response;

using Org.Apps.AuthManager.Data.Model.Request;
using Org.Apps.AuthManager.Data.Model.Response;
using Org.Apps.AuthManager.Data.Model;

using Org.Apps.SystemManager.Presentation.Authorize;
using Org.Apps.SystemManager.Presentation.Base;
using Org.Apps.SystemManager.Presentation.Models;
using Org.Apps.SystemManager.Presentation.Helpers;


namespace Org.Apps.SystemManager.Presentation.Controllers
{
    public class UserManagementController : BaseController
    {
        static readonly string ReturnToAction = "UserList";
        static readonly string ReturnToController = "UserManagement";

        #region LOGIN/LOGOFF

        public ActionResult UserLogOff()
        {
            Session.Clear();
            Session.Add("IsUserAuthenticated", false);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult UserLoginConfirm(UserLogin userLogin)
        {
            Session.Add("IsUserAuthenticated", false);
            return View(userLogin);
        }

        [HttpPost]
        public ActionResult UserLogin(UserLogin userLogin)
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            if (ModelState.IsValid)
            {
                ViewBag.ForgottenPasswordLink = false;
                try
                {
                    UserRequest userRequest = new UserRequest
                    {
                        Users = new List<User> 
                        {
                            new User { 
                                UserName = userLogin.UserName, 
                                PasswordHash = userLogin.Password 
                            }
                        }
                    };

                    UserResponse userResponse = RestPostUserRequest("ValidatePassword", userRequest);
                    User user = userResponse.Users[0];

                    Session.Add("IsUserAuthenticated", true);
                    Session.Add("UserNameSurname", user.FirstName + " " + user.LastName);
                    Session.Add("User", user);

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());

                    if (CommonHelper.CheckInvalidPasswordException(ex))
                    {
                        ModelState.AddModelError("Password", Resources.Global.MessageInvalidPassword);
                        ViewBag.ForgottenPasswordLink = true;
                    }
                    else if (CommonHelper.CheckNoDataFoundException(ex))
                        ModelState.AddModelError("UserName", Resources.Global.MessageInvalidUsername);
                    else if (CommonHelper.CheckExpiredPasswordException(ex))
                        ModelState.AddModelError("Password", Resources.Global.MessageExpiredPassword);
                    else
                        ModelState.AddModelError("", Resources.Global.MessageUnknownError);
                }
            }
            else
                ModelState.AddModelError("", Resources.Global.MessageInvalidValues);

            return View("UserLoginConfirm", userLogin);
        }

        #endregion LOGIN/LOGOFF

        #region LIST USER

        [LoginAndAuthorize]
        public ActionResult UserList()
        {
            User sessionUser = (User)Session["User"];
            if (!sessionUser.UserName.Equals("ippsadmin"))
                return RedirectToAction("UserEditConfirm", "UserManagement");

            try
            {
                UserResponse userResponse = RestPostUserRequest("List", new UserRequest { PageNo = 1, PageSize = 1000 });
                ViewBag.ResultList = userResponse.Users;
                SetViewBagResult(userResponse, ViewBag);   
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                SetViewBagResult(new UserResponse { Result = Result.FAIL, ResultMessages = new List<string> { ex.ToString() } }, ViewBag);
            }
            return View();
        }

        #endregion LIST USER

        #region EDIT USER

        [LoginAndAuthorize]
        public ActionResult UserEditConfirm(string userName)
        {
            
            try
            {
                User user;

                if (CommonHelper.IsIppsAdmin(this)) {
                    UserRequest userRequest = new UserRequest { Users = new List<User> { new User { UserName = userName } } };

                    UserResponse userResponse = RestPostUserRequest("GetByName", userRequest);
                    user = userResponse.Users[0];
                }
                else
                    user = (User)Session["User"];

                return View(user);
            }
            catch (Exception ex)
            {
                Logger.Error("{0}\n{1}", userName, ex);
                //we do not expect to get an exception here whatever the reason is.
                //The only exception is logged in user deleted by the ippsadmin on the fly.
                //once again, we do not care about it, so just loggout
                return RedirectToAction("UserLogOff", "UserManagement");
            }
        }

        [HttpPost]
        [LoginAndAuthorize]
        public ActionResult UserEdit(User user)
        {

            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.UserEditTitle;
            ViewBag.Name = Resources.Global.UserEdit;

            try
            {
                string svcUri = CommonHelper.AuthManagerServiceUrl + "UserService/Update";
                UserRequest userRequest = new UserRequest { Users = new List<User> { user } };
                UserResponse userResponse = RestHelper.RestPostObject<UserResponse, UserRequest>(svcUri, userRequest);
                        
                if (!CommonHelper.IsIppsAdmin(this))
                    Session["User"] = user;

                if (userResponse.Result == Result.OK)
                    return RedirectToAction("UserList");

                SetViewBagResult(userResponse, ViewBag);

            }
            catch (Exception ex)
            {
                Logger.Error("{0}: {1}", user, ex.ToString());
                SetViewBagResult(new UserResponse { Result = Result.FAIL, ResultMessages = new List<string> { Resources.Global.MessageUnknownError } }, ViewBag);
            }

            return View(AppsSiteHelper.ResultMessageView);
        }

        #endregion EDIT USER

        #region ADD USER

        [LoginAndAuthorize]
        public ActionResult UserAddConfirm(User user)
        {
            return View(user == null ? new User() : user);
        }

        [HttpPost]
        [LoginAndAuthorize]
        public ActionResult UserAdd(User user)
        {

            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.UserAddTitle;
            ViewBag.Name = Resources.Global.UserAdd;
            
            try
            {
                string svcUri = CommonHelper.AuthManagerServiceUrl + "UserService/Add";
                UserRequest userRequest = new UserRequest { Users = new List<User> { user } };
                UserResponse userResponse = RestHelper.RestPostObject<UserResponse, UserRequest>(svcUri, userRequest);

                if (userResponse.Result == Result.OK)
                    return RedirectToAction("UserList");
                    
                SetViewBagResult(userResponse, ViewBag);

            }
            catch (Exception ex)
            {
                Logger.Error("{0}: {1}", user, ex.ToString());
                SetViewBagResult(new UserResponse { Result = Result.FAIL, ResultMessages = new List<string> { Resources.Global.MessageUnknownError } }, ViewBag);
            }

            return View(AppsSiteHelper.ResultMessageView);
        }

        #endregion ADD USER

        #region REMOVE USER

        [LoginAndAuthorize]
        public ActionResult UserRemoveConfirm(string userName)
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            try
            {
                string svcUrl = CommonHelper.AuthManagerServiceUrl + "UserService/GetByName";
                UserRequest userRequest = new UserRequest { Users = new List<User> { new User{ UserName = userName } } }; 
                UserResponse userResponse = RestHelper.RestPostObject<UserResponse, UserRequest>(svcUrl, userRequest);

                SetViewBagResult(userResponse, ViewBag);

                return View(userResponse.Users[0]);
            }
            catch (Exception ex)
            {
                logger.Error("{0}: {1}", userName, ex.ToString());
                ModelState.AddModelError("", Resources.Global.MessageUnknownError);
                SetViewBagResult(new UserResponse { Result = Result.FAIL, ResultMessages = new List<string> { ex.ToString() } }, ViewBag);
            }

            return View();
        }

        [HttpPost]
        [LoginAndAuthorize]
        public ActionResult UserRemove(User user)
        {
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.UserRemoveTitle;
            ViewBag.Name = Resources.Global.UserRemove;

            try
            {
                string svcUri = CommonHelper.AuthManagerServiceUrl + "UserService/Remove";
                UserRequest userRequest = new UserRequest { Users = new List<User> { user } };
                UserResponse userResponse = RestHelper.RestPostObject<UserResponse, UserRequest>(svcUri, userRequest);

                if (userResponse.Result == Result.OK)
                    return RedirectToAction("UserList");

                SetViewBagResult(userResponse, ViewBag);

            }
            catch (Exception ex)
            {
                Logger.Error("{0}: {1}", user, ex.ToString());
                SetViewBagResult(new UserResponse { Result = Result.FAIL, ResultMessages = new List<string> { ex.ToString() } }, ViewBag);
            }

            return View(AppsSiteHelper.ResultMessageView);
        }

        #endregion REMOVE USER

        #region PASSWORD SET/CHANGE/RENEW

        public ActionResult UserPasswordRenewConfirm(User user)
        {
            return View(user == null ? new User() : user);
        }

        [HttpPost]
        public ActionResult UserPasswordRenew(User userForm)
        {
            
            try
            {
                UserRequest userRequest = new UserRequest
                {
                    Users = new List<User> { new User { UserName = userForm.UserName, Email = userForm.Email } }
                };
                UserResponse userResponse;

                if (string.IsNullOrEmpty(userForm.FirstName) && string.IsNullOrEmpty(userForm.LastName))
                {
                    userResponse = RestPostUserRequest("GetByName", userRequest);
                    User user = userResponse.Users[0];
                    if (!user.Email.Equals(userForm.Email))
                    {
                        ModelState.AddModelError("", Resources.Global.MessageInvalidValues);
                        return View("UserPasswordRenewConfirm", userForm);
                    }
                }
                    
                userResponse = RestPostUserRequest("IssueNewPassword", userRequest);
                if (userResponse.Result == Result.OK)
                    userResponse.ResultMessages.Add(Resources.Global.MessageRenewPassword);

                ViewBag.Result = userResponse.Result;
                ViewBag.ResultMessages = userResponse.ResultMessages;
                ViewBag.ReturnToAction = ReturnToAction;
                ViewBag.ReturnToController = ReturnToController;
                ViewBag.Title = Resources.Global.PasswordRenew;

                return View(AppsSiteHelper.ResultMessageView);
            }
            catch (Exception ex)
            {
                Logger.Error("{0}: {1}", userForm, ex.ToString());
                if (CommonHelper.CheckNoDataFoundException(ex))
                    ModelState.AddModelError("", Resources.Global.MessageInvalidValues);
                else
                    ModelState.AddModelError("", Resources.Global.MessageUnknownError);
            }
            
            return View("UserPasswordRenewConfirm", userForm);
        }

        [LoginAndAuthorize]
        public ActionResult UserPasswordChangeConfirm()
        {
            return View();
        }

        [HttpPost]
        [LoginAndAuthorize]
        public ActionResult UserPasswordChange(UserPasswordChange userPasswordChange)
        {
            
            if (ModelState.IsValid)
            {
                try
                {
                    User user = (User)Session["User"];
                    
                    UserRequest userRequest = new UserRequest { 
                        NewPasswordHash = userPasswordChange.NewPassword,
                        Users = new List<User> { 
                            new User { 
                                UserName = user.UserName, 
                                PasswordHash = userPasswordChange.OldPassword 
                            } 
                        }
                    };

                    UserResponse userResponse = RestPostUserRequest("UpdatePassword", userRequest);
                    if (userResponse.Result == Result.OK)
                        userResponse.ResultMessages.Add(Resources.Global.MessagePasswordChanged);

                    ViewBag.Result = userResponse.Result;
                    ViewBag.ResultMessages = userResponse.ResultMessages;
                    ViewBag.ReturnToAction = "Index";
                    ViewBag.ReturnToController = "Home";
                    ViewBag.Title = Resources.Global.PasswordChange;

                    return View(AppsSiteHelper.ResultMessageView);
                    
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    if (CommonHelper.CheckNoDataFoundException(ex))
                        return RedirectToAction("UserLogOff", "UserManagement");
                    if (CommonHelper.CheckInvalidPasswordException(ex))
                        ModelState.AddModelError("", Resources.Global.MessageInvalidPasswordEntered);
                    else
                        ModelState.AddModelError("", Resources.Global.MessageUnknownError);
                }
            }
            else
                ModelState.AddModelError("", Resources.Global.MessageInvalidValues);

            return View("UserPasswordChangeConfirm");
        }

        #endregion PASSWORD SET/CHANGE/RENEW

        #region HELPERS

        private UserResponse RestPostUserRequest(string actionUri, UserRequest userRequest)
        {
            string userSvcUri = CommonHelper.AuthManagerServiceUrl + "UserService/" + actionUri;
            UserResponse userResponse = RestHelper.RestPostObject<UserResponse, UserRequest>(userSvcUri, userRequest);
            if (userResponse.Result == Result.OK)
                return userResponse;

            throw new Exception(userResponse.ResultMessages[0]);
        }

        #endregion HELPERS
    }
}
