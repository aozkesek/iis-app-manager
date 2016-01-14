using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Text.RegularExpressions;

using Netas.Nipps.BaseDao;
using Netas.Nipps.BaseDao.Model.Response;

using Netas.Nipps.LogManager.Data.Model;
using Netas.Nipps.LogManager.Data.Model.Request;
using Netas.Nipps.LogManager.Data.Model.Response;

using Netas.Nipps.DeployManager.Data.Model;
using Netas.Nipps.DeployManager.Data.Model.Request;
using Netas.Nipps.DeployManager.Data.Model.Response;

using Netas.Nipps.ConfigManager.Data.Model;
using Netas.Nipps.ConfigManager.Data.Model.Request;
using Netas.Nipps.ConfigManager.Data.Model.Response;

using NLog;
using NLog.Config;

namespace Netas.Nipps.BaseService
{

    public abstract class BaseApiController : ApiController
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
        }

        #region logsetlevel
        [HttpPost]
        [Route("LogSetLevel")]
        public virtual NippsModuleResponse LogSetLevel(NippsModuleRequest nippsModuleRequest)
        {
            
            if (nippsModuleRequest == null || nippsModuleRequest.NippsModules == null || nippsModuleRequest.NippsModules.Count == 0) 
            {
                return new NippsModuleResponse { Result = Result.FAIL };
            }

            NippsModule nippsModule = nippsModuleRequest.NippsModules[0];
            string newLogLevel = Enum.GetName(typeof(NippsLogLevel), nippsModule.LogLevelId);
            
            foreach (LoggingRule rule in NLog.LogManager.Configuration.LoggingRules)
            {
                //first disable all
                rule.DisableLoggingForLevel(LogLevel.Trace);
                rule.DisableLoggingForLevel(LogLevel.Debug);
                rule.DisableLoggingForLevel(LogLevel.Info);
                rule.DisableLoggingForLevel(LogLevel.Warn);
                rule.DisableLoggingForLevel(LogLevel.Error);
                rule.DisableLoggingForLevel(LogLevel.Fatal);
                //then enable the new one
                rule.EnableLoggingForLevel(LogLevel.FromString(newLogLevel));
            }
            NLog.LogManager.ReconfigExistingLoggers();

            return new NippsModuleResponse { Result = Result.OK };
        }
        #endregion

        #region loggetfilelist
        [HttpGet]
        [Route("LogGetFileList")]
        public virtual HttpResponseMessage LogGetFileList()
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            List<NippsLogFile> logFiles = new List<NippsLogFile>();
            int apiPos = this.Request.RequestUri.AbsoluteUri.IndexOf("/api/");

            //run only with Web API -rest- service
            if (apiPos > -1)
            {
                string baseUri = this.Request.RequestUri.AbsoluteUri.Substring(0, apiPos);

                logger.Debug(baseUri);

                foreach (NLog.Targets.Target target in NLog.LogManager.Configuration.ConfiguredNamedTargets)
                {
                    if (target.GetType().Name.Equals("FileTarget"))
                    {
                        NLog.Targets.FileTarget fileTarget = target as NLog.Targets.FileTarget;
                        string aLogFilePath = fileTarget.FileName.Render(new LogEventInfo() { TimeStamp = DateTime.Now }).Replace("\\\\", "\\");
                        int logPos = aLogFilePath.IndexOf("\\logs\\");

                        logger.Debug(aLogFilePath);

                        if (logPos > -1)
                        {
                            try
                            {
                                foreach (string logFile in Directory.GetFiles(aLogFilePath.Substring(0, logPos + 5)))
                                {
                                    logFiles.Add(new NippsLogFile
                                    {
                                        LogFileContent = baseUri + logFile.Substring(logPos).Replace('\\', '/'),
                                        LogFileName = logFile.Substring(logPos + 6).Replace('\\', '/')
                                    });

                                    logger.Debug(logFiles[logFiles.Count - 1]);
                                }

                                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(logFiles);
                                responseMessage.Content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
                                responseMessage.StatusCode = System.Net.HttpStatusCode.OK;
                            }
                            catch (Exception ex)
                            {
                                responseMessage.Content = new StringContent(
                                    "{ \"ExceptionMessage\": \"" + ex.Message + "\"}",
                                    System.Text.Encoding.UTF8,
                                    "application/json");
                                responseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                            }
                        }
                        else
                        {
                            responseMessage.Content = new StringContent(
                                    "{ \"ExceptionMessage\": \"NLog->targets->target->fileName attribute should include backslash character instead of slash character.\"}",
                                    System.Text.Encoding.UTF8,
                                    "application/json");
                            responseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                        }
                    }
                }
            }
            else
                responseMessage.StatusCode = System.Net.HttpStatusCode.NotFound;

            return responseMessage;
        }
        #endregion

        #region loggetzipfilelist
        [HttpPost]
        [Route("LogGetZipFileList")]
        public virtual HttpResponseMessage LogGetZipFileList(NippsLogFileRequest logFileRequest)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage();

            string absoluteUri = Request.RequestUri.AbsoluteUri;
            int apiPos = absoluteUri.IndexOf("/api/");

            //is this a webapi request?
            if (apiPos < 0)
            {
                responseMessage.StatusCode = System.Net.HttpStatusCode.NotFound;
                return responseMessage;
            }

            //get all names of NLog.FileTarget
            List<string> logFilePaths = NippsLogHelper.ListLogFileNames();

            if (logFilePaths.Count == 0)
            {
                responseMessage.Content = new StringContent(
                        "{ \"FileNotFound\": \"NLog->targets does not include an entry type is FileTarget.\"}",
                        System.Text.Encoding.UTF8,
                        "application/json");
                responseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                return responseMessage;
            }

            int logPos = logFilePaths[0].IndexOf("\\logs\\");
            //is name in valid form ?
            if (logPos < 0)
            {
                responseMessage.Content = new StringContent(
                        "{ \"ExceptionMessage\": \"NLog->targets->target->fileName attribute should include backslash character instead of slash character.\"}",
                        System.Text.Encoding.UTF8,
                        "application/json");
                responseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                return responseMessage;
            }

            string baseLogPath = logFilePaths[0].Substring(0, logPos + 6);
            string zipFileName = Request.GetConfiguration().VirtualPathRoot.Replace("/", "");
            bool isZipped = NippsLogHelper.ZipFiles(zipFileName, baseLogPath, ".log", logFileRequest.LogStartDate, logFileRequest.LogFinishDate);

            if (!isZipped)
            {
                responseMessage.Content = new StringContent(
                        "{ \"ExceptionMessage\": \"Logs can not ZIPped.  See the logs for more detail.\"}",
                        System.Text.Encoding.UTF8,
                        "application/json");
                responseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                return responseMessage;
            }

            List<NippsLogFile> logFiles = new List<NippsLogFile> {
                new NippsLogFile {
                    LogFileContent = absoluteUri.Substring(0, apiPos + 1) + "logs/",
                    LogFileName = NippsLogHelper.TranslateZipFileName(zipFileName, ".log", logFileRequest.LogStartDate, logFileRequest.LogFinishDate)
                }};

            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(logFiles);
            responseMessage.Content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
            responseMessage.StatusCode = System.Net.HttpStatusCode.OK;

            return responseMessage;
        }
        #endregion

        #region logcriticalerror
        public virtual void LogCriticalError(string logMessage)
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            try
            {
                Regex hostRegex = new Regex("://(.*)/Netas\\.Nipps\\.");
                Regex appRegex = new Regex("(/Netas\\.Nipps\\.)([\\.\\w]*)(/)");
                string absoluteUri = Request.RequestUri.AbsoluteUri;
                string application = appRegex.Match(absoluteUri).Value;
                string host = hostRegex.Match(absoluteUri).Value.Replace("://", "").Replace("/Netas.Nipps.", "");
                string port = Regex.IsMatch(host, ":[0-9]*") ? Regex.Match(host, ":[0-9]*").Value.Replace(":", "") : "80";

                host = host.Replace(":" + port, "");

                string uri = ConfigurationManager.AppSettings["DeployManagerServiceUrl"];
                if (string.IsNullOrEmpty(uri))
                {
                    logger.Error("DeployManagerServiceUrl is not defined in the config.");
                    return;
                }

                NippsSiteResponse siteResponse = RestHelper.RestGet<NippsSiteResponse>(uri + "DeploymentService/ListNippsSite");
                if (siteResponse.Result != Result.OK)
                {
                    foreach (string m in siteResponse.ResultMessages)
                        logger.Error(m);
                    return;
                }

                uri = ConfigurationManager.AppSettings["LogManagerServiceUrl"];
                if (string.IsNullOrEmpty(uri))
                {
                    logger.Error("LogManagerServiceUrl is not defined in the config.");
                    return;
                }

                NippsSite nippsSite = siteResponse.NippsSites.Where(ns => ns.Port.Equals(port)).Single();
                string moduleName = host + ">" + nippsSite.Name + ">/" + application.Replace("/", "");

                NippsLogRequest logRequest = new NippsLogRequest
                {
                    NippsLogs = new List<NippsLog> { new NippsLog { LogLevelId = NippsLogLevel.Fatal, LogModuleName = moduleName, LogMessage = logMessage } }
                };
                NippsLogResponse logResponse = RestHelper.RestPostObject<NippsLogResponse, NippsLogRequest>(uri + "NippsLogService/Add", logRequest);
                if (logResponse.Result != Result.OK)
                    foreach (string m in logResponse.ResultMessages)
                        logger.Error(m);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
        #endregion

        #region om usagemetrics
        [HttpGet]
        [HttpPost]
        public virtual HttpResponseMessage UsageMetrics()
        {
            try
            {
                //check method, if it's POST reset metrics
                if (Request.Method.Equals(HttpMethod.Post))
                    ResetUsageMetrics();

                //find parameter name from request url
                Regex appNameReqex = new Regex("(/Netas\\.Nipps\\.Service)(.*)(/api/)");
                string absUri = this.Request.RequestUri.AbsoluteUri;
                if (!appNameReqex.Match(absUri).Success)
                    return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NoContent };
                string paramName = appNameReqex.Match(absUri).Value.Replace("/Netas.Nipps.Service.", "").Replace("/api/", "").ToUpper();

                //call parameter service, get value
                NippsParameterRequest omRequest = new NippsParameterRequest
                {
                    NippsParameters = new List<NippsParameter> { new NippsParameter { CategoryName = "OM", ParameterName = paramName } }
                };
                string svcUrl = ConfigurationManager.AppSettings["ConfigServiceUrl"].Replace("Service/List", "Service/Get");
                NippsParameterResponse omResponse = RestHelper.RestPostObject<NippsParameterResponse, NippsParameterRequest>(svcUrl, omRequest);
                if (omResponse.Result != Result.OK)
                    return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NoContent };
                if (omResponse.NippsParameters == null || omResponse.NippsParameters.Count != 1)
                    return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NoContent };
                if (string.IsNullOrEmpty(omResponse.NippsParameters[0].ParameterValue))
                    return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NoContent };

                //deserialize value  
                List<NippsOperationalMetric> omMetrics = Newtonsoft.Json.JsonConvert.DeserializeObject<List<NippsOperationalMetric>>(omResponse.NippsParameters[0].ParameterValue);
                if (omMetrics == null || omMetrics.Count == 0)
                    return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NoContent };
                NippsOperationalMetric omUsage = omMetrics.Where(om => om.Name.Equals("UsageMetrics")).Single();
                
                //is om metric active ?
                if (!omUsage.Active)
                    return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NoContent };

            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NoContent };
            }

            return new HttpResponseMessage
            {
                Content = new StringContent(
                    string.Format("{{\"data\": [[{0}, {1}]]}}", TotalRequestCount, TotalUnhandledErrorCount),
                    Encoding.UTF8,
                    "application/json")
            };
        }

        //do not forget these two re-write to return actual value in the derived class  
        public virtual int TotalRequestCount { get { return 0; } }
        public virtual int TotalUnhandledErrorCount { get { return 0; } }
        public virtual void ResetUsageMetrics() { }

        #endregion
    }
}