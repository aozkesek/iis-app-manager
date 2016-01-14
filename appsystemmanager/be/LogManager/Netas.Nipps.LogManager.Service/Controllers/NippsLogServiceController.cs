using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using Autofac;

using Netas.Nipps.Aspect;
using Netas.Nipps.BaseDao;
using Netas.Nipps.BaseDao.V2;
using Netas.Nipps.BaseDao.Model.Response;
using Netas.Nipps.BaseService;

using Netas.Nipps.LogManager.Data.Model;
using Netas.Nipps.LogManager.Data.Model.Request;
using Netas.Nipps.LogManager.Data.Model.Response;

namespace Netas.Nipps.LogManager.Service.Controllers
{
    [RoutePrefix("api/NippsLogService")]
    public class NippsLogServiceController : ApiController
    {
        private static NLog.Logger mLogger = NLog.LogManager.GetCurrentClassLogger();

        [HttpGet]
        [HttpPost]
        [Route("List")]
        [PerformanceLoggingAdvice]
        public NippsLogResponse List([FromBody]NippsLogRequest request)
        {
            NippsLogResponse response = new NippsLogResponse();
            response.ResultMessages = new List<string>();

            if (request == null)
                request = new NippsLogRequest
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
                using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<NippsLog> NippsLogLogic = scope.Resolve<IGenericLogicV2<NippsLog>>();
                    NippsLogLogic.PageSize = request.PageSize;

                    try
                    {
                        if (request.NippsLogs != null && request.NippsLogs.Count > 0)
                            response.NippsLogs = NippsLogLogic.List(request.NippsLogs[0]);
                        else
                            response.NippsLogs = NippsLogLogic.List(request.PageNo);
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
        public NippsLogResponse Add([FromBody]NippsLogRequest request)
        {
            NippsLogResponse response = new NippsLogResponse();
            response.ResultMessages = new List<string>();

            if (request == null || request.NippsLogs == null || request.NippsLogs.Count() == 0 || string.IsNullOrEmpty(request.NippsLogs[0].LogModuleName))
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add("NippsLog can not be null.");
                return response;
            }

            try
            {
                using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<NippsLog> nippsLogLogic = scope.Resolve<IGenericLogicV2<NippsLog>>();
                    bool succeededOne = false;

                    response.Result = Result.OK;

                    foreach (NippsLog nippsLog in request.NippsLogs)
                    {
                        try
                        {

                            nippsLogLogic.Add(nippsLog);
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
        public NippsLogResponse Reset(NippsLogRequest request)
        {
            NippsLogResponse response = new NippsLogResponse();
            response.ResultMessages = new List<string>();

            if (request == null || request.NippsLogs == null || request.NippsLogs.Count() == 0 || string.IsNullOrEmpty(request.NippsLogs[0].LogModuleName))
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add("NippsLog can not be null.");
                return response;
            }

            try
            {
                using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<NippsLog> nippsLogLogic = scope.Resolve<IGenericLogicV2<NippsLog>>();
                    bool succeededOne = false;

                    response.Result = Result.OK;

                    foreach (NippsLog nippsLog in request.NippsLogs)
                    {
                        try
                        {
                            nippsLogLogic.Update(nippsLog);
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
        public NippsLogResponse ResetAll(NippsLogRequest request)
        {
            NippsLogResponse response = new NippsLogResponse();
            response.ResultMessages = new List<string>();

            if (request == null || request.NippsLogs == null || request.NippsLogs.Count() == 0 || string.IsNullOrEmpty(request.NippsLogs[0].LogModuleName))
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add("NippsLog can not be null.");
                return response;
            }

            try
            {
                using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<NippsLog> nippsLogLogic = scope.Resolve<IGenericLogicV2<NippsLog>>();
                    bool succeededOne = false;

                    response.Result = Result.OK;

                    List<NippsLog> nippsLogsToBeReset = nippsLogLogic.List(new NippsLog { LogModuleName = request.NippsLogs[0].LogModuleName });

                    foreach (NippsLog nippsLog in nippsLogsToBeReset)
                    {
                        try
                        {
                            nippsLog.CheckedBy = request.NippsLogs[0].CheckedBy;
                            nippsLogLogic.Update(nippsLog);
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
