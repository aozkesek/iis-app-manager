using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using NLog;
using NLog.Config;

using Autofac;

using Org.Apps.Aspect;
using Org.Apps.BaseDao;
using Org.Apps.BaseDao.V2;
using Org.Apps.BaseService;
using Org.Apps.BaseDao.Model.Response;

using Org.Apps.LogManager.Data.Model;
using Org.Apps.LogManager.Data.Model.Request;
using Org.Apps.LogManager.Data.Model.Response;

namespace Org.Apps.LogManager.Service.Controllers
{

    [RoutePrefix("api/AppsModuleService")]
    public class AppsModuleServiceController : BaseApiController
    {
        private static NLog.Logger mLogger = NLog.LogManager.GetCurrentClassLogger();

        [HttpGet]
        [HttpPost]
        [Route("List")]
        [PerformanceLoggingAdvice]
        public AppsModuleResponse List(AppsModuleRequest AppsModuleRequest)
        {
            //this operation returns only parent modules, not childs.
            
            AppsModuleResponse response = new AppsModuleResponse();
            response.ResultMessages = new List<string>();

            if (AppsModuleRequest == null)
                AppsModuleRequest = new AppsModuleRequest
                {
                    PageNo = 1,
                    PageSize = 1000,
                    AppsModules = new List<AppsModule> { new AppsModule { ParentId = 0 } }
                };

            if (AppsModuleRequest.PageNo < 1)
                AppsModuleRequest.PageNo = 1;

            if (AppsModuleRequest.PageSize == 0)
                AppsModuleRequest.PageSize = 1000;

            if (AppsModuleRequest.AppsModules == null || AppsModuleRequest.AppsModules.Count == 0)
                AppsModuleRequest.AppsModules = new List<AppsModule> { new AppsModule { ParentId = 0 } };

            try
            {
                using (ILifetimeScope scope = AppsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<AppsModule> logModuleLogic = scope.Resolve<IGenericLogicV2<AppsModule>>();
                    logModuleLogic.PageSize = AppsModuleRequest.PageSize;

                    try
                    {
                        response.AppsModules = logModuleLogic.List(AppsModuleRequest.AppsModules[0], AppsModuleRequest.PageNo);
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
            }
            

            return response;
        }

        [HttpPost]
        [Route("GetByName")]
        [PerformanceLoggingAdvice]
        public AppsModuleResponse GetByName(AppsModuleRequest AppsModuleRequest)
        {
            AppsModuleResponse response = new AppsModuleResponse();
            response.ResultMessages = new List<string>();

            if (AppsModuleRequest == null || AppsModuleRequest.AppsModules == null || AppsModuleRequest.AppsModules.Count() == 0 || string.IsNullOrEmpty(AppsModuleRequest.AppsModules[0].ModuleName) )
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add("AppsModules can not be null.");
                return response;
            }

            try
            {
                using (ILifetimeScope scope = AppsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<AppsModule> logModuleLogic = scope.Resolve<IGenericLogicV2<AppsModule>>();
                    bool succeededOne = false;

                    response.AppsModules = new List<AppsModule>();
                    response.Result = Result.OK;

                    foreach (AppsModule logModule in AppsModuleRequest.AppsModules)
                    {
                        try
                        {

                            response.AppsModules.Add(logModuleLogic.GetByName(logModule.ModuleName));
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
            } 
            

            return response;
        }

        [HttpPost]
        [Route("Add")]
        public AppsModuleResponse Add(AppsModuleRequest AppsModuleRequest)
        {
            AppsModuleResponse response = new AppsModuleResponse();
            response.ResultMessages = new List<string>();

            if (AppsModuleRequest == null || AppsModuleRequest.AppsModules == null || AppsModuleRequest.AppsModules.Count() == 0 || string.IsNullOrEmpty(AppsModuleRequest.AppsModules[0].ModuleName))
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add("AppsModules can not be null.");
                return response;
            }

            try
            {
                using (ILifetimeScope scope = AppsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<AppsModule> logModuleLogic = scope.Resolve<IGenericLogicV2<AppsModule>>();
                    bool succeededOne = false;

                    response.Result = Result.OK;

                    foreach (AppsModule logModule in AppsModuleRequest.AppsModules)
                    {
                        try
                        {

                            logModuleLogic.Add(logModule);
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
            } 
            

            return response;
        }

        [HttpPost]
        [Route("Remove")]
        public AppsModuleResponse Remove(AppsModuleRequest AppsModuleRequest)
        {
            AppsModuleResponse response = new AppsModuleResponse();
            response.ResultMessages = new List<string>();

            if (AppsModuleRequest == null || AppsModuleRequest.AppsModules == null || AppsModuleRequest.AppsModules.Count() == 0 || string.IsNullOrEmpty(AppsModuleRequest.AppsModules[0].ModuleName))
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add("AppsModules can not be null.");
                return response;
            }

            try
            {
                using (ILifetimeScope scope = AppsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<AppsModule> logModuleLogic = scope.Resolve<IGenericLogicV2<AppsModule>>();
                    bool succeededOne = false;

                    response.Result = Result.OK;

                    foreach (AppsModule logModule in AppsModuleRequest.AppsModules)
                    {
                        try
                        {

                            logModuleLogic.Remove(logModule);
                            succeededOne = true;
                            if (response.Result != Result.OK)
                                response.Result = Result.SUCCESSWITHWARN;

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
            } 
            

            return response;
        }

        [HttpPost]
        [Route("Update")]
        public AppsModuleResponse Update(AppsModuleRequest AppsModuleRequest)
        {
            AppsModuleResponse response = new AppsModuleResponse();
            response.ResultMessages = new List<string>();

            if (AppsModuleRequest == null || AppsModuleRequest.AppsModules == null || AppsModuleRequest.AppsModules.Count() == 0 || string.IsNullOrEmpty(AppsModuleRequest.AppsModules[0].ModuleName))
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add("AppsModules can not be null.");
                return response;
            }

            try
            {
                using (ILifetimeScope scope = AppsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<AppsModule> logModuleLogic = scope.Resolve<IGenericLogicV2<AppsModule>>();
                    bool succeededOne = false;

                    response.Result = Result.OK;

                    foreach (AppsModule logModule in AppsModuleRequest.AppsModules)
                    {
                        try
                        {

                            logModuleLogic.Update(logModule);
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
            }
            

            return response;
        }

        public override AppsModuleResponse LogSetLevel(AppsModuleRequest AppsModuleRequest)
        {

            AppsModuleResponse AppsModuleResponse = Update(AppsModuleRequest);

            if (AppsModuleResponse.Result == Result.OK)
            {
                LogLevel newLogLevel =
                    AppsModuleRequest.AppsModules[0].LogLevelId == AppsLogLevel.Trace ? LogLevel.Trace
                    : AppsModuleRequest.AppsModules[0].LogLevelId == AppsLogLevel.Debug ? LogLevel.Debug
                    : AppsModuleRequest.AppsModules[0].LogLevelId == AppsLogLevel.Info ? LogLevel.Info
                    : AppsModuleRequest.AppsModules[0].LogLevelId == AppsLogLevel.Warn ? LogLevel.Warn
                    : AppsModuleRequest.AppsModules[0].LogLevelId == AppsLogLevel.Error ? LogLevel.Error
                    : AppsModuleRequest.AppsModules[0].LogLevelId == AppsLogLevel.Fatal ? LogLevel.Fatal : LogLevel.Off;

                foreach (LoggingRule rule in NLog.LogManager.Configuration.LoggingRules)
                {
                    rule.DisableLoggingForLevel(LogLevel.Trace);
                    rule.DisableLoggingForLevel(LogLevel.Debug);
                    rule.DisableLoggingForLevel(LogLevel.Info);
                    rule.DisableLoggingForLevel(LogLevel.Warn);
                    rule.DisableLoggingForLevel(LogLevel.Error);
                    rule.DisableLoggingForLevel(LogLevel.Fatal);
                    if (newLogLevel != LogLevel.Off)
                        rule.EnableLoggingForLevel(newLogLevel);
                    
                }
                NLog.LogManager.ReconfigExistingLoggers();
            }

            return AppsModuleResponse;
        }

    }
}
