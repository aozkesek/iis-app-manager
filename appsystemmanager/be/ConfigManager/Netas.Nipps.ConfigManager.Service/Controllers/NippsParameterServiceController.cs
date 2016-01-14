using Autofac;
using Netas.Nipps.Aspect;
using Netas.Nipps.BaseDao.Model.Response;
using Netas.Nipps.BaseService;
using Netas.Nipps.ConfigManager.Data.Model;
using Netas.Nipps.ConfigManager.Data.Model.Request;
using Netas.Nipps.ConfigManager.Data.Model.Response;
using Netas.Nipps.ConfigManager.Logic.Intf.V2;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Netas.Nipps.ConfigManager.Service.Controllers
{
    internal enum NippsParameterOperation
    {
        ADD = 0,
        UPDATE = 1,
        REMOVE = 2
    }

    [RoutePrefix("api/NippsParameterService")]
    public class NippsParameterServiceController : BaseApiController
    {
        private static NLog.Logger mLogger = NLog.LogManager.GetCurrentClassLogger();

        [HttpPost]
        [Route("Add")]
        [PerformanceLoggingAdvice]
        public NippsParameterResponse Add(NippsParameterRequest nippsParameterRequest)
        {
            return AddUpdateRemoveParameter(nippsParameterRequest);
        }

        [HttpPost]
        [Route("Update")]
        [PerformanceLoggingAdvice]
        public NippsParameterResponse Update(NippsParameterRequest nippsParameterRequest)
        {
            return AddUpdateRemoveParameter(nippsParameterRequest, NippsParameterOperation.UPDATE);
        }

        [HttpPost]
        [Route("Remove")]
        [PerformanceLoggingAdvice]
        public NippsParameterResponse Remove(NippsParameterRequest nippsParameterRequest)
        {
            return AddUpdateRemoveParameter(nippsParameterRequest, NippsParameterOperation.REMOVE);
        }

        [HttpPost]
        [Route("Get")]
        [PerformanceLoggingAdvice]
        public NippsParameterResponse Get(NippsParameterRequest nippsParameterRequest)
        {
            NippsParameterResponse parameterResponse = new NippsParameterResponse();
            parameterResponse.ResultMessages = new List<String>();
            if (nippsParameterRequest != null && nippsParameterRequest.NippsParameters != null && nippsParameterRequest.NippsParameters.Count > 0)
            {
                Boolean succeededOne = false;

                try
                {
                    parameterResponse.NippsParameters = new List<NippsParameter>();
                    using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                    {
                        INippsParameterLogicV2 nippsParameterLogic = scope.Resolve<INippsParameterLogicV2>();
                        foreach (NippsParameter nippsParameter in nippsParameterRequest.NippsParameters)
                        {
                            try
                            {
                                parameterResponse.NippsParameters.Add(
                                    nippsParameterLogic.GetParameter(nippsParameter.CategoryName, nippsParameter.ParameterName)
                                    );
                                succeededOne = true;
                            }
                            catch (Exception ex)
                            {
                                if (succeededOne)
                                    parameterResponse.Result = Result.SUCCESSWITHWARN;
                                else
                                    parameterResponse.Result = Result.FAIL;
                                parameterResponse.ResultMessages.Add(ex.ToString());
                                mLogger.Error("{0}: {1}", nippsParameter, ex.ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    parameterResponse.Result = Result.FAIL;
                    parameterResponse.ResultMessages.Add(ex.ToString());
                    mLogger.Error(ex.ToString());
                    
                }
            }
            else
            {
                parameterResponse.Result = Result.FAIL;
                parameterResponse.ResultMessages.Add(ResultMessagesHelper.ToString(ResultMessages.REQUEST_INVALID_PARAMETER));
            }
            return parameterResponse;
        }

        [HttpGet]
        [Route("List")]
        [PerformanceLoggingAdvice]
        public NippsParameterResponse List()
        {
            return List(new NippsParameterRequest() { PageNo = 1, PageSize = 1000, Version = "" });
        }

        [HttpPost]
        [Route("List")]
        [PerformanceLoggingAdvice]
        public NippsParameterResponse List(NippsParameterRequest nippsParameterRequest)
        {
            NippsParameterResponse parameterResponse = new NippsParameterResponse();
            parameterResponse.ResultMessages = new List<String>();
            if (nippsParameterRequest == null)
                nippsParameterRequest = new NippsParameterRequest() { PageNo = 1, PageSize = 1000, Version = "" };
            else
            {
                if (nippsParameterRequest.PageSize < 1)
                    nippsParameterRequest.PageSize = 1000;
                if (nippsParameterRequest.PageNo < 1)
                    nippsParameterRequest.PageNo = 1;
            }

            try
            {
                parameterResponse.NippsParameters = new List<NippsParameter>();
                using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    INippsParameterLogicV2 nippsParameterLogic = scope.Resolve<INippsParameterLogicV2>();
                    try
                    {
                        if (!String.IsNullOrEmpty(nippsParameterRequest.Category))
                        {
                            parameterResponse.NippsParameters = nippsParameterLogic.ListParameterByCategory(nippsParameterRequest.Category);
                        }
                        else
                        {
                            parameterResponse.NippsParameters = nippsParameterLogic.ListParameter(nippsParameterRequest.PageNo, nippsParameterRequest.PageSize);
                        }
                    }
                    catch (Exception ex)
                    {
                        parameterResponse.Result = Result.FAIL;
                        parameterResponse.ResultMessages.Add(ex.ToString());
                        mLogger.Error(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                parameterResponse.Result = Result.FAIL;
                parameterResponse.ResultMessages.Add(ex.ToString());
                mLogger.Error(ex.ToString());
            }
            return parameterResponse;
        }

        private NippsParameterResponse AddUpdateRemoveParameter(NippsParameterRequest nippsParameterRequest, NippsParameterOperation operation = NippsParameterOperation.ADD)
        {
            NippsParameterResponse parameterResponse = new NippsParameterResponse();
            parameterResponse.ResultMessages = new List<String>();
            if (nippsParameterRequest != null && nippsParameterRequest.NippsParameters != null && nippsParameterRequest.NippsParameters.Count > 0)
            {
                Boolean succeededOne = false;

                try
                {
                    parameterResponse.NippsParameters = new List<NippsParameter>();
                    using (ILifetimeScope scope = NippsIoCHelper.IoCContainer.BeginLifetimeScope())
                    {
                        INippsParameterLogicV2 nippsParameterLogic = scope.Resolve<INippsParameterLogicV2>();
                        foreach (NippsParameter nippsParameter in nippsParameterRequest.NippsParameters)
                        {
                            try
                            {
                                if (operation == NippsParameterOperation.UPDATE)
                                    nippsParameterLogic.UpdateParameter(nippsParameter.CategoryName, nippsParameter.ParameterName, nippsParameter.ParameterValue);
                                else if (operation == NippsParameterOperation.REMOVE)
                                    nippsParameterLogic.RemoveParameter(nippsParameter.CategoryName, nippsParameter.ParameterName);
                                else
                                    nippsParameterLogic.AddParameter(nippsParameter.CategoryName, nippsParameter.ParameterName, nippsParameter.ParameterValue);
                                succeededOne = true;
                            }
                            catch (Exception ex)
                            {
                                if (succeededOne)
                                    parameterResponse.Result = Result.SUCCESSWITHWARN;
                                else
                                    parameterResponse.Result = Result.FAIL;
                                parameterResponse.ResultMessages.Add(ex.ToString());
                                mLogger.Error(ex.ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    parameterResponse.Result = Result.FAIL;
                    parameterResponse.ResultMessages.Add(ex.ToString());
                    mLogger.Error(ex.ToString());
                }
            }
            else
            {
                parameterResponse.Result = Result.FAIL;
                parameterResponse.ResultMessages.Add(ResultMessagesHelper.ToString(ResultMessages.REQUEST_INVALID_PARAMETER));
            }
            return parameterResponse;
        }
    }
}