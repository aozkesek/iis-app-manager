using System;
using System.Collections.Generic;
using System.Web.Http;

using Autofac;

using Netas.Nipps.Aspect;
using Netas.Nipps.BaseService;
using Netas.Nipps.BaseDao.Model.Response;
using Netas.Nipps.AuthManager.Data.Model.Response;
using Netas.Nipps.AuthManager.Data.Model.Request;
using Netas.Nipps.AuthManager.Logic.Intf;
using Netas.Nipps.AuthManager.Data.Model;

namespace Netas.Nipps.AuthManager.Service.Controllers
{
    [RoutePrefix("api/UserService")]
    public class UserServiceController : BaseApiController
    {
        private static NLog.Logger mLogger = NLog.LogManager.GetCurrentClassLogger();

        #region PRIVATE_METHODS
        private UserResponse GetUser(Boolean byName, UserRequest userRequest)
        {

            UserResponse userResponse = new UserResponse();
            userResponse.ResultMessages = new List<String>();
            if (userRequest != null && userRequest.Users != null && userRequest.Users.Count > 0)
            {
                Boolean succeededOne = false;
                try
                {
                    userResponse.Users = new List<User>();
                    using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                    {
                        IUserLogic userLogic = scope.Resolve<IUserLogic>();
                        foreach (User user in userRequest.Users)
                        {
                            try
                            {
                                if (byName)
                                    userResponse.Users.Add(userLogic.GetByName(user.UserName));
                                else
                                    userResponse.Users.Add(userLogic.Get(user.UserId));
                                succeededOne = true;
                            }
                            catch (Exception ex)
                            {
                                if (succeededOne)
                                    userResponse.Result = Result.SUCCESSWITHWARN;
                                else
                                    userResponse.Result = Result.FAIL;

                                userResponse.ResultMessages.Add(ex.ToString());
                                mLogger.Error("{0}\n{1}", user, ex);
                            }

                        }

                    }
                }
                catch (Exception ex)
                {
                    userResponse.Result = Result.FAIL;
                    userResponse.ResultMessages.Add(ex.ToString());
                    mLogger.Error("{0}", ex);
                }
                
            }
            else
            {
                userResponse.Result = Result.FAIL;
                userResponse.ResultMessages.Add(ResultMessagesHelper.ToString(ResultMessages.REQUEST_INVALID_PARAMETER));

            }
            return userResponse;
        }

        private UserResponse AddOrUpdateUser(Boolean update, UserRequest userRequest)
        {
            UserResponse userResponse = new UserResponse();
            userResponse.ResultMessages = new List<String>();
            if (userRequest != null && userRequest.Users != null && userRequest.Users.Count > 0)
            {
                Boolean succeededOne = false;

                try
                {
                    userResponse.Users = new List<User>();
                    using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                    {
                        IUserLogic userLogic = scope.Resolve<IUserLogic>();
                        foreach (User user in userRequest.Users)
                        {
                            try
                            {
                                if (update)
                                    userLogic.Update(user);
                                else
                                    userLogic.Add(user);
                                succeededOne = true;
                            }
                            catch (Exception ex)
                            {
                                if (succeededOne)
                                    userResponse.Result = Result.SUCCESSWITHWARN;
                                else
                                    userResponse.Result = Result.FAIL;

                                userResponse.ResultMessages.Add(ex.ToString());
                                mLogger.Error("{0}\n{1}", user, ex);
                            }

                        }

                    }
                }
                catch (Exception ex)
                {
                    userResponse.Result = Result.FAIL;
                    userResponse.ResultMessages.Add(ex.ToString());
                    mLogger.Error("{0}", ex);
                }
                
            }
            else
            {
                userResponse.Result = Result.FAIL;
                userResponse.ResultMessages.Add(ResultMessagesHelper.ToString(ResultMessages.REQUEST_INVALID_PARAMETER));

            }
            return userResponse;
        }

        private UserResponse SetUserPassword(UserRequest userRequest, bool isIssue)
        {
            UserResponse userResponse = new UserResponse();
            userResponse.ResultMessages = new List<String>();
            if (userRequest != null && userRequest.Users != null && userRequest.Users.Count > 0)
            {
                try
                {
                    using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                    {
                        IUserLogic userLogic = scope.Resolve<IUserLogic>();

                        if (isIssue)
                            userLogic.IssueNewPassword(userRequest.Users[0].UserName);
                        else
                            userLogic.SetPassword(userRequest.Users[0].UserName, userRequest.Users[0].PasswordHash);

                        userResponse.Result = Result.OK;

                    }

                }
                catch (Exception ex)
                {
                    userResponse.Result = Result.FAIL;
                    userResponse.ResultMessages.Add(ex.ToString());
                    mLogger.Error("{0}\n{1}", userRequest.Users[0], ex);
                }
            }
            else
            {
                userResponse.Result = Result.FAIL;
                userResponse.ResultMessages.Add(ResultMessagesHelper.ToString(ResultMessages.REQUEST_INVALID_PARAMETER));

            }
            return userResponse;
        }

        #endregion

        #region SERVICE_OPERATIONS
        
        [HttpPost]
        [Route("Get")]
        [PerformanceLoggingAdvice]
        public UserResponse Get(UserRequest request)
        {
            return GetUser(false, request);
        }

        [HttpPost]
        [Route("GetByName")]
        [PerformanceLoggingAdvice]
        public UserResponse GetByName(UserRequest request)
        {
            return GetUser(true, request);
        }

        [HttpPost]
        [Route("Add")]
        [PerformanceLoggingAdvice]
        public UserResponse Add(UserRequest request)
        {
            return AddOrUpdateUser(false, request);
        }

        [HttpPost]
        [Route("Update")]
        [PerformanceLoggingAdvice]
        public UserResponse Update(UserRequest request)
        {
            return AddOrUpdateUser(true, request);
        }

        [HttpPost]
        [Route("Remove")]
        [PerformanceLoggingAdvice]
        public UserResponse Remove(UserRequest request)
        {
            UserResponse userResponse = new UserResponse();
            userResponse.ResultMessages = new List<String>();
            if (request != null && request.Users != null && request.Users.Count > 0)
            {
                try
                {
                    using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                    {
                        bool succeededOne = false;
                        IUserLogic userLogic = scope.Resolve<IUserLogic>();
                        foreach (User user in request.Users)
                        {
                            try
                            {
                                userLogic.Remove(user);
                                succeededOne = true;
                            }
                            catch (Exception ex)
                            {
                                if (succeededOne)
                                    userResponse.Result = Result.SUCCESSWITHWARN;
                                else
                                    userResponse.Result = Result.FAIL;
                                userResponse.ResultMessages.Add(ex.ToString());
                                mLogger.Error("{0}\n{1}", user, ex);
                            }

                        }

                    }

                }
                catch (Exception ex)
                {
                    userResponse.Result = Result.FAIL;
                    userResponse.ResultMessages.Add(ex.ToString());
                    mLogger.Error("{0}", ex);
                }
            }
            else
            {
                userResponse.Result = Result.FAIL;
                userResponse.ResultMessages.Add(ResultMessagesHelper.ToString(ResultMessages.REQUEST_INVALID_PARAMETER));

            }
            return userResponse;
        }

        [HttpGet]
        [HttpPost]
        [Route("List")]
        [PerformanceLoggingAdvice]
        public UserResponse List(UserRequest request)
        {
            UserResponse userResponse = new UserResponse();
            userResponse.ResultMessages = new List<String>();

            if (request == null)
                request = new UserRequest();

            try
            {
                using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IUserLogic userLogic = scope.Resolve<IUserLogic>();

                    if (request.PageSize > 0)
                        userLogic.PageSize = request.PageSize;

                    if (request.PageNo > 0)
                        userResponse.Users = userLogic.List(request.PageNo);
                    else
                        userResponse.Users = userLogic.List();

                    userResponse.Result = Result.OK;

                }

            }
            catch (Exception ex)
            {
                userResponse.Result = Result.FAIL;
                userResponse.ResultMessages.Add(ex.ToString());
                mLogger.Error("{0}", ex);
            }

            return userResponse;
        }

        [HttpPost]
        [Route("ValidatePassword")]
        [PerformanceLoggingAdvice]
        public UserResponse ValidatePassword(UserRequest userRequest)
        {
            UserResponse userResponse = new UserResponse();
            userResponse.ResultMessages = new List<String>();

            if (userRequest != null && userRequest.Users != null && userRequest.Users.Count > 0)
            {
                try
                {
                    using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                    {
                        IUserLogic userLogic = scope.Resolve<IUserLogic>();

                        if (userLogic.ValidatePassword(userRequest.Users[0].UserName, userRequest.Users[0].PasswordHash)) {
                            userResponse.Result = Result.OK;
                            userResponse.Users = new List<User>();
                            userResponse.Users.Add(userLogic.GetByName(userRequest.Users[0].UserName));
                        }   
                        else
                        {
                            userResponse.Result = Result.FAIL;
                            userResponse.ResultMessages.Add(ResultMessagesHelper.ToString(ResultMessages.RESPONSE_INVALID_PASSWORD));
                        }

                    }

                }
                catch (Exception ex)
                {
                    userResponse.Result = Result.FAIL;
                    userResponse.ResultMessages.Add(ex.ToString());
                    mLogger.Error("{0}", ex);
                }
            }
            else
            {
                userResponse.Result = Result.FAIL;
                userResponse.ResultMessages.Add(ResultMessagesHelper.ToString(ResultMessages.REQUEST_INVALID_PARAMETER));

            }
            return userResponse;
        }

        [HttpPost]
        [Route("UpdatePassword")]
        [PerformanceLoggingAdvice]
        public UserResponse UpdatePassword(UserRequest userRequest)
        {
            UserResponse userResponse = new UserResponse();
            userResponse.ResultMessages = new List<String>();
            if (userRequest != null && userRequest.Users != null && userRequest.Users.Count > 0)
            {
                try
                {
                    using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                    {
                        IUserLogic userLogic = scope.Resolve<IUserLogic>();
                        userLogic.UpdatePassword(userRequest.Users[0].UserName, userRequest.Users[0].PasswordHash, userRequest.NewPasswordHash);
                        userResponse.Result = Result.OK;

                    }

                }
                catch (Exception ex)
                {
                    userResponse.Result = Result.FAIL;
                    userResponse.ResultMessages.Add(ex.ToString());
                    mLogger.Error("{0}", ex);
                }
            }
            else
            {
                userResponse.Result = Result.FAIL;
                userResponse.ResultMessages.Add(ResultMessagesHelper.ToString(ResultMessages.REQUEST_INVALID_PARAMETER));

            }
            return userResponse;
        }

        [HttpPost]
        [Route("SetPassword")]
        [PerformanceLoggingAdvice]
        public UserResponse SetPassword(UserRequest userRequest)
        {
            return SetUserPassword(userRequest, false);
        }

        [HttpPost]
        [Route("IssueNewPassword")]
        [PerformanceLoggingAdvice]
        public UserResponse IssueNewPassword(UserRequest userRequest)
        {
            return SetUserPassword(userRequest, true);
        }

        [PerformanceLoggingAdvice]
        public override System.Net.Http.HttpResponseMessage LogGetFileList()
        {
            return base.LogGetFileList();
        }

        [PerformanceLoggingAdvice]
        public override System.Net.Http.HttpResponseMessage LogGetZipFileList(NippsLogFileRequest logFileRequest)
        {
            return base.LogGetZipFileList(logFileRequest);
        }

        #endregion

    }
}
