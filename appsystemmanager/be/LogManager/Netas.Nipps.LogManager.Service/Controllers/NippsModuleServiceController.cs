using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using NLog;
using NLog.Config;

using Autofac;

using Netas.Nipps.Aspect;
using Netas.Nipps.BaseDao;
using Netas.Nipps.BaseDao.V2;
using Netas.Nipps.BaseService;
using Netas.Nipps.BaseDao.Model.Response;

using Netas.Nipps.LogManager.Data.Model;
using Netas.Nipps.LogManager.Data.Model.Request;
using Netas.Nipps.LogManager.Data.Model.Response;

namespace Netas.Nipps.LogManager.Service.Controllers
{

    [RoutePrefix("api/NippsModuleService")]
    public class NippsModuleServiceController : BaseApiController
    {
        private static NLog.Logger mLogger = NLog.LogManager.GetCurrentClassLogger();

        [HttpGet]
        [HttpPost]
        [Route("List")]
        [PerformanceLoggingAdvice]
        public NippsModuleResponse List(NippsModuleRequest nippsModuleRequest)
        {
            //this operation returns only parent modules, not childs.
            
            NippsModuleResponse response = new NippsModuleResponse();
            response.ResultMessages = new List<string>();

            if (nippsModuleRequest == null)
                nippsModuleRequest = new NippsModuleRequest
                {
                    PageNo = 1,
                    PageSize = 1000,
                    NippsModules = new List<NippsModule> { new NippsModule { ParentId = 0 } }
                };

            if (nippsModuleRequest.PageNo < 1)
                nippsModuleRequest.PageNo = 1;

            if (nippsModuleRequest.PageSize == 0)
                nippsModuleRequest.PageSize = 1000;

            if (nippsModuleRequest.NippsModules == null || nippsModuleRequest.NippsModules.Count == 0)
                nippsModuleRequest.NippsModules = new List<NippsModule> { new NippsModule { ParentId = 0 } };

            try
            {
                using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<NippsModule> logModuleLogic = scope.Resolve<IGenericLogicV2<NippsModule>>();
                    logModuleLogic.PageSize = nippsModuleRequest.PageSize;

                    try
                    {
                        response.NippsModules = logModuleLogic.List(nippsModuleRequest.NippsModules[0], nippsModuleRequest.PageNo);
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
        public NippsModuleResponse GetByName(NippsModuleRequest nippsModuleRequest)
        {
            NippsModuleResponse response = new NippsModuleResponse();
            response.ResultMessages = new List<string>();

            if (nippsModuleRequest == null || nippsModuleRequest.NippsModules == null || nippsModuleRequest.NippsModules.Count() == 0 || string.IsNullOrEmpty(nippsModuleRequest.NippsModules[0].ModuleName) )
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add("NippsModules can not be null.");
                return response;
            }

            try
            {
                using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<NippsModule> logModuleLogic = scope.Resolve<IGenericLogicV2<NippsModule>>();
                    bool succeededOne = false;

                    response.NippsModules = new List<NippsModule>();
                    response.Result = Result.OK;

                    foreach (NippsModule logModule in nippsModuleRequest.NippsModules)
                    {
                        try
                        {

                            response.NippsModules.Add(logModuleLogic.GetByName(logModule.ModuleName));
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
        public NippsModuleResponse Add(NippsModuleRequest nippsModuleRequest)
        {
            NippsModuleResponse response = new NippsModuleResponse();
            response.ResultMessages = new List<string>();

            if (nippsModuleRequest == null || nippsModuleRequest.NippsModules == null || nippsModuleRequest.NippsModules.Count() == 0 || string.IsNullOrEmpty(nippsModuleRequest.NippsModules[0].ModuleName))
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add("NippsModules can not be null.");
                return response;
            }

            try
            {
                using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<NippsModule> logModuleLogic = scope.Resolve<IGenericLogicV2<NippsModule>>();
                    bool succeededOne = false;

                    response.Result = Result.OK;

                    foreach (NippsModule logModule in nippsModuleRequest.NippsModules)
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
        public NippsModuleResponse Remove(NippsModuleRequest nippsModuleRequest)
        {
            NippsModuleResponse response = new NippsModuleResponse();
            response.ResultMessages = new List<string>();

            if (nippsModuleRequest == null || nippsModuleRequest.NippsModules == null || nippsModuleRequest.NippsModules.Count() == 0 || string.IsNullOrEmpty(nippsModuleRequest.NippsModules[0].ModuleName))
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add("NippsModules can not be null.");
                return response;
            }

            try
            {
                using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<NippsModule> logModuleLogic = scope.Resolve<IGenericLogicV2<NippsModule>>();
                    bool succeededOne = false;

                    response.Result = Result.OK;

                    foreach (NippsModule logModule in nippsModuleRequest.NippsModules)
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
        public NippsModuleResponse Update(NippsModuleRequest nippsModuleRequest)
        {
            NippsModuleResponse response = new NippsModuleResponse();
            response.ResultMessages = new List<string>();

            if (nippsModuleRequest == null || nippsModuleRequest.NippsModules == null || nippsModuleRequest.NippsModules.Count() == 0 || string.IsNullOrEmpty(nippsModuleRequest.NippsModules[0].ModuleName))
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add("NippsModules can not be null.");
                return response;
            }

            try
            {
                using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IGenericLogicV2<NippsModule> logModuleLogic = scope.Resolve<IGenericLogicV2<NippsModule>>();
                    bool succeededOne = false;

                    response.Result = Result.OK;

                    foreach (NippsModule logModule in nippsModuleRequest.NippsModules)
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

        public override NippsModuleResponse LogSetLevel(NippsModuleRequest nippsModuleRequest)
        {

            NippsModuleResponse nippsModuleResponse = Update(nippsModuleRequest);

            if (nippsModuleResponse.Result == Result.OK)
            {
                LogLevel newLogLevel =
                    nippsModuleRequest.NippsModules[0].LogLevelId == NippsLogLevel.Trace ? LogLevel.Trace
                    : nippsModuleRequest.NippsModules[0].LogLevelId == NippsLogLevel.Debug ? LogLevel.Debug
                    : nippsModuleRequest.NippsModules[0].LogLevelId == NippsLogLevel.Info ? LogLevel.Info
                    : nippsModuleRequest.NippsModules[0].LogLevelId == NippsLogLevel.Warn ? LogLevel.Warn
                    : nippsModuleRequest.NippsModules[0].LogLevelId == NippsLogLevel.Error ? LogLevel.Error
                    : nippsModuleRequest.NippsModules[0].LogLevelId == NippsLogLevel.Fatal ? LogLevel.Fatal : LogLevel.Off;

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

            return nippsModuleResponse;
        }

    }
}
