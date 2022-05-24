using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Http;

using Org.Apps.Aspect;

using Org.Apps.BaseDao.Model.Response;

using Org.Apps.BaseService;

using Org.Apps.ConfigManager.Data.Model;
using Org.Apps.ConfigManager.Data.Model.Request;
using Org.Apps.ConfigManager.Data.Model.Response;

using Org.Apps.DeployManager.Data.Model;
using Org.Apps.DeployManager.Data.Model.Request;
using Org.Apps.DeployManager.Data.Model.Response;

using Org.Apps.DeployManager.Service.Helpers;

using NLog;


namespace Org.Apps.DeployManager.Service.Controllers
{
    [RoutePrefix("api/BackupService")]
    public class BackupServiceController : ApiController
    {
        static Logger _Logger = NLog.LogManager.GetCurrentClassLogger();

        #region upgrade
        [HttpPost]
        [Route("UpgradeApplication")]
        [PerformanceLoggingAdvice]
        public AppsPackageResponse UpgradeApplication(AppsPackageRequest request)
        {
            AppsPackageResponse response = new AppsPackageResponse();

            response.Result = Result.OK;
            response.ResultMessages = new List<string>();

            _Logger.Info("Checking for upgrade {0}", request);

            if (request == null || request.AppsPackages == null || request.AppsPackages.Count == 0)
            {
                response.ResultMessages.Add("ArgumentNullException: AppsPackages is null or empty.");
                return response;
            }
            
            //backup the current version,
            AppsPackageResponse backupResponse = BackupApplication(request);

            _Logger.Info("Starting to upgrade {0} with {1}", request, request.AppsPackages[0].PackageZIP);
            //restore the upgraded version
            AppsPackageResponse restoreResponse = RestoreApplication(request);

            _Logger.Info("Starting to execute upgrade.sql {0}", request);
            //execute upgrade.sql, if exist in the zip

            return response;
        }

        [HttpPost]
        [Route("UpgradeApplicationList")]
        [PerformanceLoggingAdvice]
        public AppsPackageResponse UpgradeApplicationList(AppsPackageRequest request)
        {
            AppsPackageResponse response = new AppsPackageResponse();
            string BackupTargetPath = GetBackupTargetPath();

            response.Result = Result.OK;
            response.ResultMessages = new List<string>();

            if (request != null && request.AppsPackages != null && request.AppsPackages.Count > 0)
                foreach (string fileName in Directory.GetFiles(BackupTargetPath, "Upgr_" + request.AppsPackages[0].ApplicationName.Replace("/", "") + "*.zip", SearchOption.TopDirectoryOnly))
                    response.ResultMessages.Add(fileName.Substring(BackupTargetPath.Length));
            else
                foreach (string fileName in Directory.GetFiles(BackupTargetPath, "Upgr_*.zip", SearchOption.TopDirectoryOnly))
                    response.ResultMessages.Add(fileName.Substring(BackupTargetPath.Length));

            return response;
        }
        #endregion

        #region config-backup
        [HttpGet]
        [Route("BackupConfiguration")]
        [PerformanceLoggingAdvice]
        public BaseResponse BackupConfiguration()
        {
            BaseResponse response = new AppsPackageResponse();
            response.ResultMessages = new List<string>(); 

            try
            {
                string targetPath = GetBackupTargetPath();
                string bcpExe = GetBcpExe();
                string connectionString = ConfigurationManager.ConnectionStrings["UpgradeDb"].ConnectionString;
                
                string dataBase = Regex.Match(connectionString, @"(Initial Catalog=)(\w+)").Value.Replace("Initial Catalog=", "");
                string userName = Regex.Match(connectionString, @"(User ID=)(\w+)").Value.Replace("User ID=", "");
                string passWord = Regex.Match(connectionString, @"(Password=)(\w+)").Value.Replace("Password=", "");
                string host = Regex.Match(connectionString, @"(Data Source=)(\w+)").Value.Replace("Data Source=", "");
                string sqlArgs = string.Format("-c -C RAW -S {0} -d {1} -U {2} -P {3}", host, dataBase, userName, passWord);

                //take the schema&table name from system parameter service, 
                response.ResultMessages.Add(string.Format("auth.Users exit code -> {0}", ConfigurationBackup(bcpExe, sqlArgs, "auth.Users", targetPath)));

                response.ResultMessages.Add(string.Format("log.AppsModules exit code -> {0}", ConfigurationBackup(bcpExe, sqlArgs, "log.AppsModules", targetPath)));

                response.ResultMessages.Add(string.Format("conf.ParameterCategories exit code -> {0}", ConfigurationBackup(bcpExe, sqlArgs, "conf.ParameterCategories", targetPath)));

                response.ResultMessages.Add(string.Format("conf.SystemParameters exit code -> {0}", ConfigurationBackup(bcpExe, sqlArgs, "conf.SystemParameters", targetPath)));

                response.Result = Result.OK;
                
            }
            catch (Exception ex)
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.ToString());
            }

            return response;
        }
        #endregion

        #region config-restore
        [HttpGet]
        [Route("RestoreConfigurationList")]
        [PerformanceLoggingAdvice]
        public BaseResponse RestoreConfigurationList()
        {
            BaseResponse response = new AppsPackageResponse();
            response.ResultMessages = new List<string>();

            try
            {
                string targetPath = GetBackupTargetPath();
                foreach (string fileName in Directory.GetFiles(targetPath, "DB_*.txt", SearchOption.TopDirectoryOnly))
                    response.ResultMessages.Add(fileName);

                response.Result = Result.OK;

            }
            catch (Exception ex)
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.ToString());
            }

            return response;
        }

        [HttpPost]
        [Route("RestoreConfiguration")]
        [PerformanceLoggingAdvice]
        public BaseResponse RestoreConfiguration(AppsPackageRequest request)
        {

            BaseResponse response = new AppsPackageResponse();
            response.ResultMessages = new List<string>();

            try
            {

                if (request != null && request.AppsPackages != null && request.AppsPackages.Count > 0)
                {
                    string bcpExe = GetBcpExe();
                    string connectionString = ConfigurationManager.ConnectionStrings["UpgradeDb"].ConnectionString;

                    string dataBase = Regex.Match(connectionString, @"(Initial Catalog=)(\w+)").Value.Replace("Initial Catalog=", "");
                    string userName = Regex.Match(connectionString, @"(User ID=)(\w+)").Value.Replace("User ID=", "");
                    string passWord = Regex.Match(connectionString, @"(Password=)(\w+)").Value.Replace("Password=", "");
                    string host = Regex.Match(connectionString, @"(Data Source=)(\w+)").Value.Replace("Data Source=", "");
                    string sqlArgs = string.Format("-c -C RAW -S {0} -d {1} -U {2} -P {3}", host, dataBase, userName, passWord);

                    foreach (AppsPackage np in request.AppsPackages)
                    {
                        response.ResultMessages.Add(
                            string.Format(
                                "restore {0} exit code -> {1}",
                                np.PackageZIP,
                                ConfigurationRestore(bcpExe, sqlArgs, Regex.Match(np.PackageZIP, "(_)(.*)(_)").Value.Replace("_", ""), np.PackageZIP)
                                )
                            );
                    }
                        
                }
                response.Result = Result.OK;

            }
            catch (Exception ex)
            {
                response.Result = Result.FAIL;
                response.ResultMessages.Add(ex.ToString());
            }

            return response;
        }
        #endregion

        #region backup
        [HttpPost]
        [Route("BackupApplication")]
        [PerformanceLoggingAdvice]
        public AppsPackageResponse BackupApplication(AppsPackageRequest request)
        {
            AppsPackageResponse response = new AppsPackageResponse();
            bool succeededOne = false;
            string BackupTargetPath = GetBackupTargetPath();

            response.ResultMessages = new List<string>();
            response.Result = Result.OK;

            _Logger.Debug(request.ToString());

            foreach (AppsPackage np in request.AppsPackages)
            {
                
                try
                {
                    if (String.IsNullOrEmpty(np.ApplicationName))
                        np.ApplicationName = "/"; //site backup

                    response.ResultMessages
                        .Add(ApplicationBackup(np));
                    
                    succeededOne = true;
                }
                catch (Exception ex)
                {
                    if (succeededOne)
                        response.Result = Result.SUCCESSWITHWARN;

                    response.ResultMessages.Add(ex.ToString());
                }
            }

            return response;
        }
        #endregion

        #region restore
        [HttpPost]
        [Route("RestoreApplication")]
        [PerformanceLoggingAdvice]
        public AppsPackageResponse RestoreApplication(AppsPackageRequest request)
        {
            AppsPackageResponse response = new AppsPackageResponse();

            response.Result = Result.OK;
            response.ResultMessages = new List<string>();

            _Logger.Debug(request);

            foreach (AppsPackage np in request.AppsPackages)
            {
                if (String.IsNullOrEmpty(np.ApplicationName))
                    np.ApplicationName = "/"; //site restore

                _Logger.Debug(np);

                response.ResultMessages
                    .Add(ApplicationRestore(np));
            }
            
            return response;
        }

        [HttpPost]
        [Route("RestoreApplicationList")]
        [PerformanceLoggingAdvice]
        public AppsPackageResponse RestoreApplicationList(AppsPackageRequest request)
        {
            AppsPackageResponse response = new AppsPackageResponse();
            string BackupTargetPath = GetBackupTargetPath();
            
            response.Result = Result.OK;
            response.ResultMessages = new List<string>();

            if (request != null && request.AppsPackages != null && request.AppsPackages.Count > 0)
                foreach (AppsPackage np in request.AppsPackages)
                    foreach (string fileName in Directory.GetFiles(BackupTargetPath, "Appl_" + ZipFileName(np, false).Replace(".zip", "*.zip"), SearchOption.TopDirectoryOnly))
                        response.ResultMessages.Add(fileName.Substring(BackupTargetPath.Length));
            else
                foreach (string fileName in Directory.GetFiles(BackupTargetPath, "Site_*.zip", SearchOption.TopDirectoryOnly))
                    response.ResultMessages.Add(fileName.Substring(BackupTargetPath.Length));

            return response;
        }
        #endregion

        #region helper

        static int ExecuteShellCommand(string shellCommand, string commandArgs) 
        {

            Process proc = Process.Start(shellCommand, commandArgs);
            proc.WaitForExit();
            if (proc.ExitCode != 0)
                _Logger.Error("{0} {1} returned exit code {2}.", shellCommand, commandArgs, proc.ExitCode);

            return proc.ExitCode;
        }

        static int ConfigurationBackup(string bcpExe, string sqlArgs, string table, string targetPath)
        {
            string outputFile = string.Format("{0}DB_{1}_{2:yyyyMMdd}", targetPath, table, DateTime.Now);
            string bcpFormatParams = string.Format(" {0} format nul -f \"{1}.fmt\" {2}", table, outputFile, sqlArgs);
            string bcpOutParams = string.Format(" {0} out \"{1}.txt\" {2}", table, outputFile, sqlArgs);

            if (File.Exists(outputFile + ".fmt"))
                File.Delete(outputFile + ".fmt");
            if (File.Exists(outputFile + ".txt"))
                File.Delete(outputFile + ".txt");

            int exitCode = ExecuteShellCommand(bcpExe, bcpFormatParams);
            if (exitCode != 0)
                return exitCode;
            return ExecuteShellCommand(bcpExe, bcpOutParams);
        }

        static int ConfigurationRestore(string bcpExe, string sqlArgs, string table, string sourceName)
        {
            string bcpInParams = string.Format(" {0} in \"{1}\" -f \"{2}\" {3}", table, sourceName, sourceName.Replace(".txt", ".fmt"), sqlArgs);

            return ExecuteShellCommand(bcpExe, bcpInParams);
        }

        private static string ApplicationRestore(AppsPackage np)
        {
            string currentAppFolder = AppDomain.CurrentDomain.BaseDirectory;
            string applicationFolder =
                ServerManagerHelper.PutEnvVarValue(
                    ServerManagerHelper.GetApplicationPhysicalPath(
                        np.SiteName,
                        np.ApplicationName
                        ));

            string BackupTargetPath = GetBackupTargetPath();
            string zipName = BackupTargetPath + np.PackageZIP;

            _Logger.Debug("{0}, {1}, {2}", currentAppFolder, applicationFolder, zipName);
            if (applicationFolder.StartsWith(currentAppFolder) || currentAppFolder.StartsWith(applicationFolder))
                return "[ErrorCode: RESTORE_COULD_NOT_EXTRACT_OVER_SELF]";

            applicationFolder = applicationFolder.EndsWith("\\") ? applicationFolder : applicationFolder + "\\";

            AppsZipHelper.Unzip(applicationFolder, zipName);

            return zipName.Substring(BackupTargetPath.Length);

        }

        private static string ApplicationBackup(AppsPackage np)
        {
            string applicationFolder = 
                ServerManagerHelper.PutEnvVarValue(
                    ServerManagerHelper.GetApplicationPhysicalPath(
                        np.SiteName, 
                        np.ApplicationName
                        ));
            string BackupTargetPath = GetBackupTargetPath();
            string zipName = BackupTargetPath + (np.ApplicationName.Equals("/") ? "Site_" : "Appl_") + ZipFileName(np);

            _Logger.Debug("{0}, {1}", applicationFolder, zipName);

            AppsZipHelper.ZipFolder(applicationFolder, zipName);
            return zipName.Substring(BackupTargetPath.Length);

        }

        private static string GetBackupTargetPath()
        {
            string ConfigManagerServiceUrl = ConfigurationManager.AppSettings["ConfigManagerServiceUrl"].ToString();
            
            AppsParameterRequest request = new AppsParameterRequest
            { AppsParameters = new List<AppsParameter>
                { new AppsParameter{ CategoryName = "BACKUP", ParameterName = "TargetPath" } } 
            };

            AppsParameterResponse response = RestHelper.RestPostObject<AppsParameterResponse, AppsParameterRequest>(ConfigManagerServiceUrl+"Get", request);
            if (response.Result == Result.OK)
                return response.AppsParameters[0].ParameterValue;
            //return default if not found/defined
            return @"C:\AppsPackages";
        }

        private static string GetBcpExe()
        {
            string ConfigManagerServiceUrl = ConfigurationManager.AppSettings["ConfigManagerServiceUrl"].ToString();

            AppsParameterRequest request = new AppsParameterRequest
            {
                AppsParameters = new List<AppsParameter> { new AppsParameter { CategoryName = "BACKUP", ParameterName = "BCP.EXE" } }
            };

            AppsParameterResponse response = RestHelper.RestPostObject<AppsParameterResponse, AppsParameterRequest>(ConfigManagerServiceUrl + "Get", request);
            if (response.Result == Result.OK)
                return response.AppsParameters[0].ParameterValue;
            //return default if not found/defined
            return @"C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\110\Tools\Binn\bcp.exe";
        }

        private static string ZipFileName(AppsPackage np, bool timeBased = true)
        {
            StringBuilder sb = new StringBuilder(np.HostName.Replace(" ", "").Replace(":", ""));
            DateTime backupTime = DateTime.Now;

            if (!String.IsNullOrEmpty(np.SiteName))
                sb.AppendFormat("_{0}",
                    np.SiteName.Replace(" ", ""));

            if (!String.IsNullOrEmpty(np.ApplicationName))
                sb.AppendFormat("_{0}",
                    np.ApplicationName.Replace(" ", "").Replace("/", ""));

            if (timeBased)
                sb.AppendFormat("_{0}",
                    backupTime.ToString("yyyyMMdd_HHmm"));

            sb.Append(".zip");

            return sb.ToString();
        }
        #endregion helper
    }
}