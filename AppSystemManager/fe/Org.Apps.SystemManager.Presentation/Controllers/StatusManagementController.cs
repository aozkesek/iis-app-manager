using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using System.IO.Compression;
using System.Collections.Generic;

using NLog.Targets;

using Org.Apps.BaseService;
using Org.Apps.BaseDao;
using Org.Apps.BaseDao.Model.Response;

using Org.Apps.LogManager.Data.Model;
using Org.Apps.LogManager.Data.Model.Request;
using Org.Apps.LogManager.Data.Model.Response;

using Org.Apps.ConfigManager.Data.Model;
using Org.Apps.ConfigManager.Data.Model.Request;
using Org.Apps.ConfigManager.Data.Model.Response;

using Org.Apps.AuthManager.Data.Model;
using Org.Apps.DeployManager.Data.Model;

using Org.Apps.SystemManager.Presentation.Base;
using Org.Apps.SystemManager.Presentation.Authorize;
using Org.Apps.SystemManager.Presentation.Models;
using Org.Apps.SystemManager.Presentation.Helpers;

namespace Org.Apps.SystemManager.Presentation.Controllers
{
    public class StatusManagementController : BaseController
    {
        static readonly string ReturnToAction = "StatusList";
        static readonly string ReturnToController = "StatusManagement";

        #region list
        [LoginAndAuthorize]
        public ActionResult StatusList()
        {
            try
            {
                List<AppsParameter> AppsHosts = AppsSiteHelper.ListAppsHost();
                AppsModuleRequest AppsModuleRequest = new AppsModuleRequest();
                AppsModuleRequest.AppsModules = new List<AppsModule>();

                foreach (AppsParameter AppsHost in AppsHosts)
                    foreach (string value in AppsHost.ParameterValue.Split('|'))
                        AppsSiteHelper.ListAppsSite(value)
                            .ForEach(delegate(AppsSite AppsSite) 
                            {
                                AppsSite.AppsApplications
                                    .ForEach(delegate(AppsApplication AppsApplication) 
                                    {
                                        if (AppsApplication.Path.StartsWith("/Org.Apps.Service."))
                                            AppsModuleRequest.AppsModules.Add(
                                                new AppsModule { 
                                                    ModuleName = value + ">" + AppsSite.Name + ">" + AppsApplication.Path,
                                                    ParentId = 0
                                                });        
                                    });
                            });

                AppsModuleResponse AppsModuleResponse;

                if (AppsModuleRequest.AppsModules.Count > 0)
                    try
                    {
                        RestPostAppsModuleRequest("Add", AppsModuleRequest);
                    }
                    catch (Exception ex) { }
                    
                AppsModuleResponse = RestPostAppsModuleRequest("List", new AppsModuleRequest());
                ViewBag.ResultList = AppsModuleResponse.AppsModules;
                SetViewBagResult(AppsModuleResponse, ViewBag);

            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                SetViewBagResult(new AppsModuleResponse { Result = Result.FAIL, ResultMessages = new List<string> { ex.ToString() } }, ViewBag);
            }

            return View();

        }
        #endregion

        #region log-download
        [LoginAndAuthorize]
        public ActionResult LogDownload(AppsModule AppsModule)
        {
            List<AppsLogFile> logFiles = new List<AppsLogFile>();
            try
            {
                ModuleNameParser mnp = new ModuleNameParser(AppsModule.ModuleName);
                string svcLogUrl = AppsSiteHelper.ServiceLogUrl(mnp);
                Logger.Debug("MODULE={0}, URL={1}", mnp.ToString(), svcLogUrl);

                logFiles = LogGetFileList(svcLogUrl, logFiles);
                ViewBag.ResultList = logFiles;
                SetViewBagResult(new AppsLogResponse { Result = logFiles.Count > 0 ? Result.OK : Result.SUCCESSWITHWARN, ResultMessages = new List<string>() }, ViewBag);
            }
            catch (Exception ex)
            {
                Logger.Error("{0}\n{1}", AppsModule, ex.ToString());
                SetViewBagResult(new AppsLogResponse { Result = Result.FAIL, ResultMessages = new List<string> { ex.ToString() } }, ViewBag);
            }

            return PartialView("LogDownload", AppsModule);
        }

        [LoginAndAuthorize]
        public ActionResult LogDownloadAll(AppsLogFileRequest logStartFinish)
        {
            string systemManager = Request.Params["IsSystemManager"];
            bool isSystemManager = !string.IsNullOrEmpty(systemManager) && systemManager.Equals("true");

            if (logStartFinish.LogFinishDate < logStartFinish.LogStartDate || logStartFinish.LogFinishDate > DateTime.Now)
            {
                ModelState.AddModelError("LogFinishDate", Resources.Global.MessageInvalidFinishDate);
                ViewBag.IsSystemManager = isSystemManager;
                ViewBag.ModelState = ModelState;
                return View("LogDownloadConfirm");
            }
                
            List<AppsLogFile> logFiles = new List<AppsLogFile>();

            ViewBag.Title = string.Format(Resources.Global.MessageDownloadAllLogs
                , logStartFinish.LogStartDate.ToShortDateString()
                , logStartFinish.LogFinishDate.ToShortDateString());
                
            if (isSystemManager)
                logFiles = SMAppsLogZipFileList(new List<AppsLogFile>(), logStartFinish);
            else
            {
                ModuleNameParser mnp;
                string uri = CommonHelper.LogManagerServiceUrl + "AppsModuleService/List";
                AppsModuleResponse moduleResponse = RestHelper.RestPostObject<AppsModuleResponse, AppsModuleRequest>(uri, new AppsModuleRequest());
                foreach (AppsModule AppsModule in moduleResponse.AppsModules)
                {
                    try
                    {
                        mnp = new ModuleNameParser(AppsModule.ModuleName);
                        uri = AppsSiteHelper.ServiceLogUrl(mnp);

                        logFiles = LogGetZipFileList(uri, logFiles, logStartFinish);
                    }
                    catch (Exception ex) { Logger.Error(ex.ToString()); }
                }
                        
            }

            if (logFiles.Count > 0)
                ViewBag.Result = Result.OK;
            else
                ViewBag.Result = Result.SUCCESSWITHWARN;

            ViewBag.ResultList = logFiles;
            
            return View();

        }

        public ActionResult SMLogDownloadConfirm()
        {
            ViewBag.IsSystemManager = true;
            return View("LogDownloadConfirm");
        }

        public ActionResult LogDownloadConfirm()
        {
            ViewBag.IsSystemManager = false;
            return View();
        }
        #endregion

        #region log-parameters
        [LoginAndAuthorize]
        public ActionResult LogParameterChange(AppsModule AppsModule)
        {
            try
            {
                ModuleNameParser mnp = new ModuleNameParser(AppsModule.ModuleName);
                string uri = CommonHelper.ConfigManagerServiceUrl + "AppsParameterService/List";

                AppsParameterRequest listRequest = new AppsParameterRequest { Category = mnp.Service.ToUpper() };
                AppsParameterResponse listResponse = RestHelper.RestPostObject<AppsParameterResponse, AppsParameterRequest>(uri, listRequest);

                if (listResponse.Result == Result.OK)
                {
                    AppsModule.LogLevelId = (AppsLogLevel) Enum.Parse(typeof(AppsLogLevel), 
                        listResponse.AppsParameters.Where(p => p.ParameterName.Equals("MinLevel")).Single().ParameterValue, false);
                    
                    AppsModule.ArchiveEvery = (FileArchivePeriod)Enum.Parse(typeof(FileArchivePeriod), 
                        listResponse.AppsParameters.Where(p => p.ParameterName.Equals("ArchiveEvery")).Single().ParameterValue, false);
                    
                    AppsModule.ArchiveAboveSize = int.Parse(
                        listResponse.AppsParameters.Where(p => p.ParameterName.Equals("ArchiveAboveSize")).Single().ParameterValue) / 1000000;
                    
                    AppsModule.MaxArchiveFiles = int.Parse(
                        listResponse.AppsParameters.Where(p => p.ParameterName.Equals("MaxArchiveFiles")).Single().ParameterValue);

                }
                else //could not get, so set defaults
                {
                    AppsModule.LogLevelId = AppsLogLevel.Warn;
                    AppsModule.ArchiveEvery = FileArchivePeriod.Day;
                    AppsModule.ArchiveAboveSize = 10 * 1000000;
                    AppsModule.MaxArchiveFiles = 10;
                }
               
            }
            catch (Exception ex)
            {
                Logger.Error("{0}: {1}", AppsModule, ex);

                AppsModule.LogLevelId = AppsLogLevel.Warn;
                AppsModule.ArchiveEvery = FileArchivePeriod.Day;
                AppsModule.ArchiveAboveSize = 10 * 1000000;
                AppsModule.MaxArchiveFiles = 10;
            }

            return View(AppsModule);
        }

        [LoginAndAuthorize]
        public ActionResult LogParameterSave(AppsModule AppsModule)
        {
            ModuleNameParser mnp = new ModuleNameParser(AppsModule.ModuleName);

            try
            {
                UpdateLogParameter(AppsModule, mnp, ViewBag);
                
                UpdateAppsModule(AppsModule, ViewBag);

                SetLogLevel(AppsModule, mnp, ViewBag);
            }
            catch (Exception ex)
            {
                if (ViewBag.Result == Result.OK)
                    ViewBag.Result = Result.SUCCESSWITHWARN;
                ViewBag.ResultMessages.Add(ex.ToString());
            }

            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.LogParametersChangeTitle;

            if (ViewBag.Result == Result.OK)
                return RedirectToAction("StatusList");

            return View(AppsSiteHelper.ResultMessageView);
        }
        #endregion

        #region log-detail
        [LoginAndAuthorize]
        public ActionResult LogDetail(AppsModule AppsModule)
        {
            List<AppsLog> AppsLog = new List<AppsLog>();
            
            ViewBag.ModuleName = AppsModule.ModuleName;
            ViewBag.ModuleId = AppsModule.ModuleId;
            ViewBag.CheckedBy = ((User)Session["User"]).UserName;
            
            try
            {
                string svcUrl = CommonHelper.LogManagerServiceUrl + "AppsLogService/List";
                AppsLogRequest logRequest = new AppsLogRequest { AppsLogs = new List<AppsLog> { new AppsLog { LogModuleName = AppsModule.ModuleName } } };
                AppsLogResponse logResponse = RestHelper.RestPostObject<AppsLogResponse, AppsLogRequest>(svcUrl, logRequest);
                ViewBag.ResultList = logResponse.AppsLogs;
                SetViewBagResult(logResponse, ViewBag);

            }
            catch (Exception ex) 
            { 
                Logger.Error("{0}\n{1}", AppsModule, ex.ToString());
 
            }

            return View(AppsModule);

        }
        #endregion

        #region log-reset
        [LoginAndAuthorize]
        public ActionResult LogReset(int moduleId, string moduleName, string checkedBy)
        {
            
            ViewBag.ReturnToAction = ReturnToAction;
            ViewBag.ReturnToController = ReturnToController;
            ViewBag.Title = Resources.Global.LogResetTitle;
            ViewBag.Name = Resources.Global.LogReset;

            try
            {
                AppsLogRequest logRequest = new AppsLogRequest 
                { 
                    AppsLogs = new List<AppsLog> 
                    { 
                        new AppsLog { LogModuleName = moduleName, CheckedBy = checkedBy } 
                    } 
                };
                string svcUri = CommonHelper.LogManagerServiceUrl + "AppsLogService/ResetAll";
                AppsLogResponse logResponse = RestHelper.RestPostObject<AppsLogResponse, AppsLogRequest>(svcUri, logRequest);
                SetViewBagResult(logResponse, ViewBag);

            }
            catch (Exception ex)
            {
                string em = string.Format("{0}: {1}", moduleName, ex.ToString());
                Logger.Error(em);
                SetViewBagResult(new AppsLogResponse { Result = Result.FAIL, ResultMessages = new List<string> { em} }, ViewBag);
            }

            return View(AppsSiteHelper.ResultMessageView);

        }
        #endregion

        #region HELPER

        private static void SetLogLevel(AppsModule AppsModule, ModuleNameParser mnp, dynamic ViewBag)
        {
            string uri = AppsSiteHelper.ServiceLogUrl(mnp) + "LogSetLevel";
            AppsModuleRequest logRequest = new AppsModuleRequest { AppsModules = new List<AppsModule> { AppsModule } };
            AppsModuleResponse logResponse = RestHelper.RestPostObject<AppsModuleResponse, AppsModuleRequest>(uri, logRequest);
            ViewBag.Name = Resources.Global.LogLevelChange;
            SetViewBagResult(logResponse, ViewBag);
        }

        private static void UpdateLogParameter(AppsModule AppsModule, ModuleNameParser mnp, dynamic ViewBag)
        {
            string uri = CommonHelper.ConfigManagerServiceUrl + "AppsParameterService/Update";
            string newLogLevel = Enum.GetName(typeof(AppsLogLevel), AppsModule.LogLevelId);
            string newArchiveEvery = Enum.GetName(typeof(FileArchivePeriod), AppsModule.ArchiveEvery);
            string categoryName = mnp.Service.ToUpper();
                
            AppsParameterRequest parameterRequest = new AppsParameterRequest
            {
                AppsParameters = new List<AppsParameter> { 
                        new AppsParameter{ CategoryName = categoryName, ParameterName = "MinLevel", ParameterValue = newLogLevel }
                        , new AppsParameter{ CategoryName = categoryName, ParameterName = "ArchiveEvery", ParameterValue = newArchiveEvery }
                        , new AppsParameter{ CategoryName = categoryName, ParameterName = "ArchiveAboveSize", ParameterValue = (AppsModule.ArchiveAboveSize * 1000000).ToString() }
                        , new AppsParameter{ CategoryName = categoryName, ParameterName = "MaxArchiveFiles", ParameterValue = AppsModule.MaxArchiveFiles.ToString() }
                    }
            };
            AppsParameterResponse parameterResponse = RestHelper.RestPostObject<AppsParameterResponse, AppsParameterRequest>(uri, parameterRequest);
            ViewBag.Name = Resources.Global.LogParametersChange;
            SetViewBagResult(parameterResponse, ViewBag);

        }

        private static void UpdateAppsModule(AppsModule AppsModule, dynamic ViewBag)
        {
            string uri = CommonHelper.LogManagerServiceUrl + "AppsModuleService/Update";

            AppsModuleRequest moduleUpdateRequest = new AppsModuleRequest { AppsModules = new List<AppsModule> { AppsModule } };
            AppsModuleResponse moduleUpdateResponse = RestHelper.RestPostObject<AppsModuleResponse, AppsModuleRequest>(uri, moduleUpdateRequest);
            ViewBag.Name = Resources.Global.LogParametersChange;
            SetViewBagResult(moduleUpdateResponse, ViewBag);
        }

        private static string GetConfigServiceLogUrl(string serviceName)
        {
            return ConfigurationManager.AppSettings[serviceName + "ServiceUrl"].ToString()
                + ConfigurationManager.AppSettings[serviceName + "ServiceLogUrl"].ToString(); 
        }

        private List<AppsModule> ListSMModules()
        {
            AppsModuleRequest request = new AppsModuleRequest
            {
                AppsModules = new List<AppsModule> { new AppsModule { ParentId = 1 } }
            };

            AppsModuleResponse response = RestHelper.RestPostObject<AppsModuleResponse, AppsModuleRequest>(
                CommonHelper.LogManagerServiceUrl + "AppsModuleService/List"
                , request);

            return response.AppsModules;
        }

        private List<AppsLogFile> SMAppsLogZipFileList(List<AppsLogFile> logFiles, AppsLogFileRequest logStartFinish)
        {

            List<String> nlogFiles = AppsLogHelper.ListLogFileNames();

            string zipFileName = Request.ApplicationPath.Replace("/", "");
            string sourcePath = nlogFiles[0].Substring(0, nlogFiles[0].IndexOf("\\logs\\") + 6);

            if (AppsLogHelper.ZipFiles(zipFileName, sourcePath, ".log", logStartFinish.LogStartDate, logStartFinish.LogFinishDate))
                logFiles.Add(new AppsLogFile
                {
                    LogFileContent = Request.Url.AbsoluteUri.Substring(0, Request.Url.AbsoluteUri.IndexOf(Request.ApplicationPath)) + Request.ApplicationPath + "/logs/",
                    LogFileName = AppsLogHelper.TranslateZipFileName(zipFileName, ".log", logStartFinish.LogStartDate, logStartFinish.LogFinishDate)
                });

            List<AppsModule> smModules = ListSMModules();
            foreach (AppsModule smModule in smModules)
                logFiles = LogGetZipFileList(
                    GetConfigServiceLogUrl(smModule.ModuleName), 
                    logFiles, 
                    logStartFinish);

            return logFiles;
        }

        private List<AppsLogFile> LogGetFileList(string uri, List<AppsLogFile> logFiles)
        {
            try
            {
                AppsLogFileResponse AppsLogFileResponse = RestLogGetFileList(uri + "/LogGetFileList");
                Logger.Debug("{0}, {1}", uri, AppsLogFileResponse.AppsLogFiles.Count);
                foreach (AppsLogFile AppsLogFile in AppsLogFileResponse.AppsLogFiles)
                    logFiles.Add(AppsLogFile);

            }
            catch (Exception ex) { Logger.Error(ex); }

            return logFiles;
        }

        private List<AppsLogFile> LogGetZipFileList(string uri, List<AppsLogFile> logFiles, AppsLogFileRequest logStartFinish)
        {
            try
            {
                List<AppsLogFile> zipLogs = RestHelper.RestPostObject<List<AppsLogFile>, AppsLogFileRequest>(uri + "/LogGetZipFileList", logStartFinish);
                
                foreach (AppsLogFile zipLog in zipLogs)
                    logFiles.Add(zipLog);

            }
            catch (Exception ex) { Logger.Error(ex); }
            
            return logFiles;
        }

        private AppsModuleResponse RestPostAppsModuleRequest(string actionUri, AppsModuleRequest AppsModuleRequest)
        {
            string svcUri = CommonHelper.LogManagerServiceUrl + "AppsModuleService/" + actionUri;
            AppsModuleResponse AppsModuleResponse = RestHelper.RestPostObject<AppsModuleResponse, AppsModuleRequest>(svcUri, AppsModuleRequest);
            if (AppsModuleResponse.Result == Result.OK)
                return AppsModuleResponse;

            throw new Exception(AppsModuleResponse.ResultMessages[0]);
        }

        private AppsLogResponse RestPostAppsLogRequest(string actionUri, AppsLogRequest AppsLogRequest)
        {
            string svcUri = CommonHelper.LogManagerServiceUrl + "AppsLogService/" + actionUri;
            AppsLogResponse AppsLogResponse = RestHelper.RestPostObject<AppsLogResponse, AppsLogRequest>(svcUri, AppsLogRequest);
            if (AppsLogResponse.Result == Result.OK)
                return AppsLogResponse;

            throw new Exception(AppsLogResponse.ResultMessages[0]);
        }

        private AppsLogFileResponse RestLogGetFileList(string uri)
        {
            AppsLogFileResponse logFileResponse = new AppsLogFileResponse 
            { 
                AppsLogFiles = RestHelper.RestGet<List<AppsLogFile>>(uri)
            };

            return logFileResponse;
        }

        #endregion
    }
}
