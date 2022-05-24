using Autofac;
using Org.Apps.Aspect;
using Org.Apps.BaseDao.Model.Response;
using Org.Apps.BaseService;
using Org.Apps.ConfigManager.Data.Model;
using Org.Apps.ConfigManager.Data.Model.Request;
using Org.Apps.ConfigManager.Data.Model.Response;
using Org.Apps.ConfigManager.Logic.Intf.V2;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Org.Apps.ConfigManager.Service.Controllers
{
    internal enum AppsParameterOperation
    {
        ADD = 0,
        UPDATE = 1,
        REMOVE = 2
    }

    [RoutePrefix("api/AppsParameterService")]
    public class AppsParameterServiceController : BaseApiController
    {
        private static NLog.Logger mLogger = NLog.LogManager.GetCurrentClassLogger();

        [HttpPost]
        [Route("Add")]
        [PerformanceLoggingAdvice]
        public AppsParameterResponse Add(AppsParameterRequest AppsParameterRequest)
        {
            return AddUpdateRemoveParameter(AppsParameterRequest);
        }

        [HttpPost]
        [Route("Update")]
        [PerformanceLoggingAdvice]
        public AppsParameterResponse Update(AppsParameterRequest AppsParameterRequest)
        {
            return AddUpdateRemoveParameter(AppsParameterRequest, AppsParameterOperation.UPDATE);
        }

        [HttpPost]
        [Route("Remove")]
        [PerformanceLoggingAdvice]
        public AppsParameterResponse Remove(AppsParameterRequest AppsParameterRequest)
        {
            return AddUpdateRemoveParameter(AppsParameterRequest, AppsParameterOperation.REMOVE);
        }

        [HttpPost]
        [Route("Get")]
        [PerformanceLoggingAdvice]
        public AppsParameterResponse Get(AppsParameterRequest AppsParameterRequest)
        {
            AppsParameterResponse parameterResponse = new AppsParameterResponse();
            parameterResponse.ResultMessages = new List<String>();
            if (AppsParameterRequest != null && AppsParameterRequest.AppsParameters != null && AppsParameterRequest.AppsParameters.Count > 0)
            {
                Boolean succeededOne = false;

                try
                {
                    parameterResponse.AppsParameters = new List<AppsParameter>();
                    using (ILifetimeScope scope = AppsIoCHelper.IoCContainer.BeginLifetimeScope())
                    {
                        IAppsParameterLogicV2 AppsParameterLogic = scope.Resolve<IAppsParameterLogicV2>();
                        foreach (AppsParameter AppsParameter in AppsParameterRequest.AppsParameters)
                        {
                            try
                            {
                                parameterResponse.AppsParameters.Add(
                                    AppsParameterLogic.GetParameter(AppsParameter.CategoryName, AppsParameter.ParameterName)
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
                                mLogger.Error("{0}: {1}", AppsParameter, ex.ToString());
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
        public AppsParameterResponse List()
        {
            return List(new AppsParameterRequest() { PageNo = 1, PageSize = 1000, Version = "" });
        }

        [HttpPost]
        [Route("List")]
        [PerformanceLoggingAdvice]
        public AppsParameterResponse List(AppsParameterRequest AppsParameterRequest)
        {
            AppsParameterResponse parameterResponse = new AppsParameterResponse();
            parameterResponse.ResultMessages = new List<String>();
            if (AppsParameterRequest == null)
                AppsParameterRequest = new AppsParameterRequest() { PageNo = 1, PageSize = 1000, Version = "" };
            else
            {
                if (AppsParameterRequest.PageSize < 1)
                    AppsParameterRequest.PageSize = 1000;
                if (AppsParameterRequest.PageNo < 1)
                    AppsParameterRequest.PageNo = 1;
            }

            try
            {
                parameterResponse.AppsParameters = new List<AppsParameter>();
                using (ILifetimeScope scope = AppsIoCHelper.IoCContainer.BeginLifetimeScope())
                {
                    IAppsParameterLogicV2 AppsParameterLogic = scope.Resolve<IAppsParameterLogicV2>();
                    try
                    {
                        if (!String.IsNullOrEmpty(AppsParameterRequest.Category))
                        {
                            parameterResponse.AppsParameters = AppsParameterLogic.ListParameterByCategory(AppsParameterRequest.Category);
                        }
                        else
                        {
                            parameterResponse.AppsParameters = AppsParameterLogic.ListParameter(AppsParameterRequest.PageNo, AppsParameterRequest.PageSize);
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

        private AppsParameterResponse AddUpdateRemoveParameter(AppsParameterRequest AppsParameterRequest, AppsParameterOperation operation = AppsParameterOperation.ADD)
        {
            AppsParameterResponse parameterResponse = new AppsParameterResponse();
            parameterResponse.ResultMessages = new List<String>();
            if (AppsParameterRequest != null && AppsParameterRequest.AppsParameters != null && AppsParameterRequest.AppsParameters.Count > 0)
            {
                Boolean succeededOne = false;

                try
                {
                    parameterResponse.AppsParameters = new List<AppsParameter>();
                    using (ILifetimeScope scope = AppsIoCHelper.IoCContainer.BeginLifetimeScope())
                    {
                        IAppsParameterLogicV2 AppsParameterLogic = scope.Resolve<IAppsParameterLogicV2>();
                        foreach (AppsParameter AppsParameter in AppsParameterRequest.AppsParameters)
                        {
                            try
                            {
                                if (operation == AppsParameterOperation.UPDATE)
                                    AppsParameterLogic.UpdateParameter(AppsParameter.CategoryName, AppsParameter.ParameterName, AppsParameter.ParameterValue);
                                else if (operation == AppsParameterOperation.REMOVE)
                                    AppsParameterLogic.RemoveParameter(AppsParameter.CategoryName, AppsParameter.ParameterName);
                                else
                                    AppsParameterLogic.AddParameter(AppsParameter.CategoryName, AppsParameter.ParameterName, AppsParameter.ParameterValue);
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