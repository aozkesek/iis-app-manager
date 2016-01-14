using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using System.IO.Compression;
using System.Collections.Generic;

using NLog.Targets;

using Netas.Nipps.BaseService;
using Netas.Nipps.BaseDao;
using Netas.Nipps.BaseDao.Model.Response;

using Netas.Nipps.LogManager.Data.Model;
using Netas.Nipps.LogManager.Data.Model.Request;
using Netas.Nipps.LogManager.Data.Model.Response;

using Netas.Nipps.ConfigManager.Data.Model;
using Netas.Nipps.ConfigManager.Data.Model.Request;
using Netas.Nipps.ConfigManager.Data.Model.Response;

using Netas.Nipps.AuthManager.Data.Model;
using Netas.Nipps.DeployManager.Data.Model;

using Netas.Nipps.SystemManager.Presentation.Base;
using Netas.Nipps.SystemManager.Presentation.Authorize;
using Netas.Nipps.SystemManager.Presentation.Models;
using Netas.Nipps.SystemManager.Presentation.Helpers;

namespace Netas.Nipps.SystemManager.Presentation.Controllers
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
                List<NippsParameter> nippsHosts = NippsSiteHelper.ListNippsHost();
                NippsModuleRequest nippsModuleRequest = new NippsModuleRequest();
                nippsModuleRequest.NippsModules = new List<NippsModule>();

                foreach (NippsParameter nippsHost in nippsHosts)
                    foreach (string value in nippsHost.ParameterValue.Split('|'))
                        NippsSiteHelper.ListNippsSite(value)
                            .ForEach(delegate(NippsSite nippsSite) 
                            {
                                nippsSite.NippsApplications
                                    .ForEach(delegate(NippsApplication nippsApplication) 
                                    {
                                        if (nippsApplication.Path.StartsWith("/Netas.Nipps.Service."))
                                            nippsModuleRequest.NippsModules.Add(
                                                new NippsModule { 
                                                    ModuleName = value + ">" + nippsSite.Name + ">" + nippsApplication.Path,
                                                    ParentId = 0
                                                });        
                                    });
                            });

                NippsModuleResponse nippsModuleResponse;

                if (nippsModuleRequest.NippsModules.Count > 0)
                    try
                    {
                        RestPostNippsModuleRequest("Add", nippsModuleRequest);
                    }
                    catch (Exception ex) { }
                    
                nippsModuleResponse = RestPostNippsModuleRequest("List", new NippsModuleRequest());
                ViewBag.ResultList = nippsModuleResponse.NippsModules;
                SetViewBagResult(nippsModuleResponse, ViewBag);

            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                SetViewBagResult(new NippsModuleResponse { Result = Result.FAIL, ResultMessages = new List<string> { ex.ToString() } }, ViewBag);
            }

            return View();

        }
        #endregion

        #region log-download
        [LoginAndAuthorize]
        public ActionResult LogDownload(NippsModule nippsModule)
        {
            List<NippsLogFile> logFiles = new List<NippsLogFile>();
            try
            {
                ModuleNameParser mnp = new ModuleNameParser(nippsModule.ModuleName);
                string svcLogUrl = NippsSiteHelper.ServiceLogUrl(mnp);
                Logger.Debug("MODULE={0}, URL={1}", mnp.ToString(), svcLogUrl);

                logFiles = LogGetFileList(svcLogUrl, logFiles);
                ViewBag.ResultList = logFiles;
                SetViewBagResult(new NippsLogResponse { Result = logFiles.Count > 0 ? Result.OK : Result.SUCCESSWITHWARN, ResultMessages = new List<string>() }, ViewBag);
            }
            catch (Exception ex)
            {
                Logger.Error("{0}\n{1}", nippsModule, ex.ToString());
                SetViewBagResult(new NippsLogResponse { Result = Result.FAIL, ResultMessages = new List<string> { ex.ToString() } }, ViewBag);
            }

            return PartialView("LogDownload", nippsModule);
        }

        [LoginAndAuthorize]
        public ActionResult LogDownloadAll(NippsLogFileRequest logStartFinish)
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
                
            List<NippsLogFile> logFiles = new List<NippsLogFile>();

            ViewBag.Title = string.Format(Resources.Global.MessageDownloadAllLogs
                , logStartFinish.LogStartDate.ToShortDateString()
                , logStartFinish.LogFinishDate.ToShortDateString());
                
            if (isSystemManager)
                logFiles = SMNippsLogZipFileList(new List<NippsLogFile>(), logStartFinish);
            else
            {
                ModuleNameParser mnp;
                string uri = CommonHelper.LogManagerServiceUrl + "NippsModuleService/List";
                NippsModuleResponse moduleResponse = RestHelper.RestPostObject<NippsModuleResponse, NippsModuleRequest>(uri, new NippsModuleRequest());
                foreach (NippsModule nippsModule in moduleResponse.NippsModules)
                {
                    try
                    {
                        mnp = new ModuleNameParser(nippsModule.ModuleName);
                        uri = NippsSiteHelper.ServiceLogUrl(mnp);

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
        public ActionResult LogParameterChange(NippsModule nippsModule)
        {
            try
            {
                ModuleNameParser mnp = new ModuleNameParser(nippsModule.ModuleName);
                string uri = CommonHelper.ConfigManagerServiceUrl + "NippsParameterService/List";

                NippsParameterRequest listRequest = new NippsParameterRequest { Category = mnp.Service.ToUpper() };
                NippsParameterResponse listResponse = RestHelper.RestPostObject<NippsParameterResponse, NippsParameterRequest>(uri, listRequest);

                if (listResponse.Result == Result.OK)
                {
                    nippsModule.LogLevelId = (NippsLogLevel) Enum.Parse(typeof(NippsLogLevel), 
                        listResponse.NippsParameters.Where(p => p.ParameterName.Equals("MinLevel")).Single().ParameterValue, false);
                    
                    nippsModule.ArchiveEvery = (FileArchivePeriod)Enum.Parse(typeof(FileArchivePeriod), 
                        listResponse.NippsParameters.Where(p => p.ParameterName.Equals("ArchiveEvery")).Single().ParameterValue, false);
                    
                    nippsModule.ArchiveAboveSize = int.Parse(
                        listResponse.NippsParameters.Where(p => p.ParameterName.Equals("ArchiveAboveSize")).Single().ParameterValue) / 1000000;
                    
                    nippsModule.MaxArchiveFiles = int.Parse(
                        listResponse.NippsParameters.Where(p => p.ParameterName.Equals("MaxArchiveFiles")).Single().ParameterValue);

                }
                else //could not get, so set defaults
                {
                    nippsModule.LogLevelId = NippsLogLevel.Warn;
                    nippsModule.ArchiveEvery = FileArchivePeriod.Day;
                    nippsModule.ArchiveAboveSize = 10 * 1000000;
                    nippsModule.MaxArchiveFiles = 10;
                }
               
            }
            catch (Exception ex)
            {
                Logger.Error("{0}: {1}", nippsModule, ex);

                nippsModule.LogLevelId = NippsLogLevel.Warn;
                nippsModule.ArchiveEvery = FileArchivePeriod.Day;
                nippsModule.ArchiveAboveSize = 10 * 1000000;
                nippsModule.MaxArchiveFiles = 10;
            }

            return View(nippsModule);
        }

        [LoginAndAuthorize]
        public ActionResult LogParameterSave(NippsModule nippsModule)
        {
            ModuleNameParser mnp = new ModuleNameParser(nippsModule.ModuleName);

            try
            {
                UpdateLogParameter(nippsModule, mnp, ViewBag);
                
                UpdateNippsModule(nippsModule, ViewBag);

                SetLogLevel(nippsModule, mnp, ViewBag);
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

            return View(NippsSiteHelper.ResultMessageView);
        }
        #endregion

        #region log-detail
        [LoginAndAuthorize]
        public ActionResult LogDetail(NippsModule nippsModule)
        {
            List<NippsLog> nippsLog = new List<NippsLog>();
            
            ViewBag.ModuleName = nippsModule.ModuleName;
            ViewBag.ModuleId = nippsModule.ModuleId;
            ViewBag.CheckedBy = ((User)Session["User"]).UserName;
            
            try
            {
                string svcUrl = CommonHelper.LogManagerServiceUrl + "NippsLogService/List";
                NippsLogRequest logRequest = new NippsLogRequest { NippsLogs = new List<NippsLog> { new NippsLog { LogModuleName = nippsModule.ModuleName } } };
                NippsLogResponse logResponse = RestHelper.RestPostObject<NippsLogResponse, NippsLogRequest>(svcUrl, logRequest);
                ViewBag.ResultList = logResponse.NippsLogs;
                SetViewBagResult(logResponse, ViewBag);

            }
            catch (Exception ex) 
            { 
                Logger.Error("{0}\n{1}", nippsModule, ex.ToString());
 
            }

            return View(nippsModule);

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
                NippsLogRequest logRequest = new NippsLogRequest 
                { 
                    NippsLogs = new List<NippsLog> 
                    { 
                        new NippsLog { LogModuleName = moduleName, CheckedBy = checkedBy } 
                    } 
                };
                string svcUri = CommonHelper.LogManagerServiceUrl + "NippsLogService/ResetAll";
                NippsLogResponse logResponse = RestHelper.RestPostObject<NippsLogResponse, NippsLogRequest>(svcUri, logRequest);
                SetViewBagResult(logResponse, ViewBag);

            }
            catch (Exception ex)
            {
                string em = string.Format("{0}: {1}", moduleName, ex.ToString());
                Logger.Error(em);
                SetViewBagResult(new NippsLogResponse { Result = Result.FAIL, ResultMessages = new List<string> { em} }, ViewBag);
            }

            return View(NippsSiteHelper.ResultMessageView);

        }
        #endregion

        #region HELPER

        private static void SetLogLevel(NippsModule nippsModule, ModuleNameParser mnp, dynamic ViewBag)
        {
            string uri = NippsSiteHelper.ServiceLogUrl(mnp) + "LogSetLevel";
            NippsModuleRequest logRequest = new NippsModuleRequest { NippsModules = new List<NippsModule> { nippsModule } };
            NippsModuleResponse logResponse = RestHelper.RestPostObject<NippsModuleResponse, NippsModuleRequest>(uri, logRequest);
            ViewBag.Name = Resources.Global.LogLevelChange;
            SetViewBagResult(logResponse, ViewBag);
        }

        private static void UpdateLogParameter(NippsModule nippsModule, ModuleNameParser mnp, dynamic ViewBag)
        {
            string uri = CommonHelper.ConfigManagerServiceUrl + "NippsParameterService/Update";
            string newLogLevel = Enum.GetName(typeof(NippsLogLevel), nippsModule.LogLevelId);
            string newArchiveEvery = Enum.GetName(typeof(FileArchivePeriod), nippsModule.ArchiveEvery);
            string categoryName = mnp.Service.ToUpper();
                
            NippsParameterRequest parameterRequest = new NippsParameterRequest
            {
                NippsParameters = new List<NippsParameter> { 
                        new NippsParameter{ CategoryName = categoryName, ParameterName = "MinLevel", ParameterValue = newLogLevel }
                        , new NippsParameter{ CategoryName = categoryName, ParameterName = "ArchiveEvery", ParameterValue = newArchiveEvery }
                        , new NippsParameter{ CategoryName = categoryName, ParameterName = "ArchiveAboveSize", ParameterValue = (nippsModule.ArchiveAboveSize * 1000000).ToString() }
                        , new NippsParameter{ CategoryName = categoryName, ParameterName = "MaxArchiveFiles", ParameterValue = nippsModule.MaxArchiveFiles.ToString() }
                    }
            };
            NippsParameterResponse parameterResponse = RestHelper.RestPostObject<NippsParameterResponse, NippsParameterRequest>(uri, parameterRequest);
            ViewBag.Name = Resources.Global.LogParametersChange;
            SetViewBagResult(parameterResponse, ViewBag);

        }

        private static void UpdateNippsModule(NippsModule nippsModule, dynamic ViewBag)
        {
            string uri = CommonHelper.LogManagerServiceUrl + "NippsModuleService/Update";

            NippsModuleRequest moduleUpdateRequest = new NippsModuleRequest { NippsModules = new List<NippsModule> { nippsModule } };
            NippsModuleResponse moduleUpdateResponse = RestHelper.RestPostObject<NippsModuleResponse, NippsModuleRequest>(uri, moduleUpdateRequest);
            ViewBag.Name = Resources.Global.LogParametersChange;
            SetViewBagResult(moduleUpdateResponse, ViewBag);
        }

        private static string GetConfigServiceLogUrl(string serviceName)
        {
            return ConfigurationManager.AppSettings[serviceName + "ServiceUrl"].ToString()
                + ConfigurationManager.AppSettings[serviceName + "ServiceLogUrl"].ToString(); 
        }

        private List<NippsModule> ListSMModules()
        {
            NippsModuleRequest request = new NippsModuleRequest
            {
                NippsModules = new List<NippsModule> { new NippsModule { ParentId = 1 } }
            };

            NippsModuleResponse response = RestHelper.RestPostObject<NippsModuleResponse, NippsModuleRequest>(
                CommonHelper.LogManagerServiceUrl + "NippsModuleService/List"
                , request);

            return response.NippsModules;
        }

        private List<NippsLogFile> SMNippsLogZipFileList(List<NippsLogFile> logFiles, NippsLogFileRequest logStartFinish)
        {

            List<String> nlogFiles = NippsLogHelper.ListLogFileNames();

            string zipFileName = Request.ApplicationPath.Replace("/", "");
            string sourcePath = nlogFiles[0].Substring(0, nlogFiles[0].IndexOf("\\logs\\") + 6);

            if (NippsLogHelper.ZipFiles(zipFileName, sourcePath, ".log", logStartFinish.LogStartDate, logStartFinish.LogFinishDate))
                logFiles.Add(new NippsLogFile
                {
                    LogFileContent = Request.Url.AbsoluteUri.Substring(0, Request.Url.AbsoluteUri.IndexOf(Request.ApplicationPath)) + Request.ApplicationPath + "/logs/",
                    LogFileName = NippsLogHelper.TranslateZipFileName(zipFileName, ".log", logStartFinish.LogStartDate, logStartFinish.LogFinishDate)
                });

            List<NippsModule> smModules = ListSMModules();
            foreach (NippsModule smModule in smModules)
                logFiles = LogGetZipFileList(
                    GetConfigServiceLogUrl(smModule.ModuleName), 
                    logFiles, 
                    logStartFinish);

            return logFiles;
        }

        private List<NippsLogFile> LogGetFileList(string uri, List<NippsLogFile> logFiles)
        {
            try
            {
                NippsLogFileResponse nippsLogFileResponse = RestLogGetFileList(uri + "/LogGetFileList");
                Logger.Debug("{0}, {1}", uri, nippsLogFileResponse.NippsLogFiles.Count);
                foreach (NippsLogFile nippsLogFile in nippsLogFileResponse.NippsLogFiles)
                    logFiles.Add(nippsLogFile);

            }
            catch (Exception ex) { Logger.Error(ex); }

            return logFiles;
        }

        private List<NippsLogFile> LogGetZipFileList(string uri, List<NippsLogFile> logFiles, NippsLogFileRequest logStartFinish)
        {
            try
            {
                List<NippsLogFile> zipLogs = RestHelper.RestPostObject<List<NippsLogFile>, NippsLogFileRequest>(uri + "/LogGetZipFileList", logStartFinish);
                
                foreach (NippsLogFile zipLog in zipLogs)
                    logFiles.Add(zipLog);

            }
            catch (Exception ex) { Logger.Error(ex); }
            
            return logFiles;
        }

        private NippsModuleResponse RestPostNippsModuleRequest(string actionUri, NippsModuleRequest nippsModuleRequest)
        {
            string svcUri = CommonHelper.LogManagerServiceUrl + "NippsModuleService/" + actionUri;
            NippsModuleResponse nippsModuleResponse = RestHelper.RestPostObject<NippsModuleResponse, NippsModuleRequest>(svcUri, nippsModuleRequest);
            if (nippsModuleResponse.Result == Result.OK)
                return nippsModuleResponse;

            throw new Exception(nippsModuleResponse.ResultMessages[0]);
        }

        private NippsLogResponse RestPostNippsLogRequest(string actionUri, NippsLogRequest nippsLogRequest)
        {
            string svcUri = CommonHelper.LogManagerServiceUrl + "NippsLogService/" + actionUri;
            NippsLogResponse nippsLogResponse = RestHelper.RestPostObject<NippsLogResponse, NippsLogRequest>(svcUri, nippsLogRequest);
            if (nippsLogResponse.Result == Result.OK)
                return nippsLogResponse;

            throw new Exception(nippsLogResponse.ResultMessages[0]);
        }

        private NippsLogFileResponse RestLogGetFileList(string uri)
        {
            NippsLogFileResponse logFileResponse = new NippsLogFileResponse 
            { 
                NippsLogFiles = RestHelper.RestGet<List<NippsLogFile>>(uri)
            };

            return logFileResponse;
        }

        #endregion
    }
}
