using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using Autofac;

using Org.Apps.Aspect;
using Org.Apps.BaseDao;
using Org.Apps.BaseDao.V2;
using Org.Apps.BaseDao.Model.Response;
using Org.Apps.BaseService;

using Org.Apps.LogManager.Data.Model;
using Org.Apps.LogManager.Data.Model.Request;
using Org.Apps.LogManager.Data.Model.Response;

namespace Org.Apps.LogManager.Service.Controllers
{
    [RoutePrefix("api/AppsLogService")]
    public class AppsLogServiceController : ApiController
    {
        private static NLog.Logger mLogger = NLog.LogManager.GetCurrentClassLogger();

        [HttpGet]
        [HttpPost]
        [Route("List")]
        [PerformanceLoggingAdvice]
        public AppsLogResponse List([FromBody]AppsLogRequest request)
        {
            AppsLogResponse response = new AppsLogResponse();
            response.ResultMessages = new List<string>();

            if (request == null)
                request = new AppsLogRequest
                {
                    PageNo = 1,
                    PageSize = 1000
                };

            if (request.PageNo < 1)
                request.PageNo = 1;

            if (request.PageSize == 0)
                request.PageSize = 1000;

            try
            {
                using (ILifetimeScope scope = AppsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<AppsLog> AppsLogLogic = scope.Resolve<IGenericLogicV2<AppsLog>>();
                    AppsLogLogic.PageSize = request.PageSize;

                    try
                    {
                        if (request.AppsLogs != null && request.AppsLogs.Count > 0)
                            response.AppsLogs = AppsLogLogic.List(request.AppsLogs[0]);
                        else
                            response.AppsLogs = AppsLogLogic.List(request.PageNo);
                        response.Result = Result.OK;
                    }
                    catch (NoDataFoundException ex)
                    {
                        response.Result = Result.FAIL;
                        response.ResultMessages.Add(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        response.Result = Result.FAIL;
                        response.ResultMessages.Add(ex.ToString());
                    }

                }

            }
            catch (Exception ex)
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.Message);
                mLogger.Error(ex);
                RestHelper.ReportCriticalError("LogManager", ex.ToString());
            }

            
            return response;
        }

        [HttpPost]
        [Route("Add")]
        public AppsLogResponse Add([FromBody]AppsLogRequest request)
        {
            AppsLogResponse response = new AppsLogResponse();
            response.ResultMessages = new List<string>();

            if (request == null || request.AppsLogs == null || request.AppsLogs.Count() == 0 || string.IsNullOrEmpty(request.AppsLogs[0].LogModuleName))
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add("AppsLog can not be null.");
                return response;
            }

            try
            {
                using (ILifetimeScope scope = AppsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<AppsLog> AppsLogLogic = scope.Resolve<IGenericLogicV2<AppsLog>>();
                    bool succeededOne = false;

                    response.Result = Result.OK;

                    foreach (AppsLog AppsLog in request.AppsLogs)
                    {
                        try
                        {

                            AppsLogLogic.Add(AppsLog);
                            succeededOne = true;

                        }
                        catch (Exception ex)
                        {
                            if (succeededOne)
                                response.Result = Result.SUCCESSWITHWARN;
                            else
                                response.Result = Result.FAIL;
                            response.ResultMessages.Add(ex.ToString());
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.Message);
                mLogger.Error(ex);
                RestHelper.ReportCriticalError("LogManager", ex.ToString());
            }

            return response;
        }

        [HttpPost]
        [Route("Reset")]
        public AppsLogResponse Reset(AppsLogRequest request)
        {
            AppsLogResponse response = new AppsLogResponse();
            response.ResultMessages = new List<string>();

            if (request == null || request.AppsLogs == null || request.AppsLogs.Count() == 0 || string.IsNullOrEmpty(request.AppsLogs[0].LogModuleName))
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add("AppsLog can not be null.");
                return response;
            }

            try
            {
                using (ILifetimeScope scope = AppsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<AppsLog> AppsLogLogic = scope.Resolve<IGenericLogicV2<AppsLog>>();
                    bool succeededOne = false;

                    response.Result = Result.OK;

                    foreach (AppsLog AppsLog in request.AppsLogs)
                    {
                        try
                        {
                            AppsLogLogic.Update(AppsLog);
                            succeededOne = true;

                        }
                        catch (Exception ex)
                        {
                            if (succeededOne)
                                response.Result = Result.SUCCESSWITHWARN;
                            else
                                response.Result = Result.FAIL;
                            response.ResultMessages.Add(ex.ToString());
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.Message);
                mLogger.Error(ex);
                RestHelper.ReportCriticalError("LogManager", ex.ToString());
            }

            return response;
        }

        [HttpPost]
        [Route("ResetAll")]
        public AppsLogResponse ResetAll(AppsLogRequest request)
        {
            AppsLogResponse response = new AppsLogResponse();
            response.ResultMessages = new List<string>();

            if (request == null || request.AppsLogs == null || request.AppsLogs.Count() == 0 || string.IsNullOrEmpty(request.AppsLogs[0].LogModuleName))
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add("AppsLog can not be null.");
                return response;
            }

            try
            {
                using (ILifetimeScope scope = AppsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<AppsLog> AppsLogLogic = scope.Resolve<IGenericLogicV2<AppsLog>>();
                    bool succeededOne = false;

                    response.Result = Result.OK;

                    List<AppsLog> AppsLogsToBeReset = AppsLogLogic.List(new AppsLog { LogModuleName = request.AppsLogs[0].LogModuleName });

                    foreach (AppsLog AppsLog in AppsLogsToBeReset)
                    {
                        try
                        {
                            AppsLog.CheckedBy = request.AppsLogs[0].CheckedBy;
                            AppsLogLogic.Update(AppsLog);
                            succeededOne = true;

                        }
                        catch (Exception ex)
                        {
                            if (succeededOne)
                                response.Result = Result.SUCCESSWITHWARN;
                            else
                                response.Result = Result.FAIL;
                            response.ResultMessages.Add(ex.ToString());
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.Message);
                mLogger.Error(ex);
                RestHelper.ReportCriticalError("LogManager", ex.ToString());
            }
            

            return response;
        }

    }
}
