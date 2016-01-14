using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Http;

using Netas.Nipps.Aspect;

using Netas.Nipps.BaseDao.Model.Response;

using Netas.Nipps.BaseService;

using Netas.Nipps.ConfigManager.Data.Model;
using Netas.Nipps.ConfigManager.Data.Model.Request;
using Netas.Nipps.ConfigManager.Data.Model.Response;

using Netas.Nipps.DeployManager.Data.Model;
using Netas.Nipps.DeployManager.Data.Model.Request;
using Netas.Nipps.DeployManager.Data.Model.Response;

using Netas.Nipps.DeployManager.Service.Helpers;

using NLog;


namespace Netas.Nipps.DeployManager.Service.Controllers
{
    [RoutePrefix("api/BackupService")]
    public class BackupServiceController : ApiController
    {
        static Logger _Logger = NLog.LogManager.GetCurrentClassLogger();

        #region upgrade
        [HttpPost]
        [Route("UpgradeApplication")]
        [PerformanceLoggingAdvice]
        public NippsPackageResponse UpgradeApplication(NippsPackageRequest request)
        {
            NippsPackageResponse response = new NippsPackageResponse();

            response.Result = Result.OK;
            response.ResultMessages = new List<string>();

            _Logger.Info("Checking for upgrade {0}", request);

            if (request == null || request.NippsPackages == null || request.NippsPackages.Count == 0)
            {
                response.ResultMessages.Add("ArgumentNullException: NippsPackages is null or empty.");
                return response;
            }
            
            //backup the current version,
            NippsPackageResponse backupResponse = BackupApplication(request);

            _Logger.Info("Starting to upgrade {0} with {1}", request, request.NippsPackages[0].PackageZIP);
            //restore the upgraded version
            NippsPackageResponse restoreResponse = RestoreApplication(request);

            _Logger.Info("Starting to execute upgrade.sql {0}", request);
            //execute upgrade.sql, if exist in the zip

            return response;
        }

        [HttpPost]
        [Route("UpgradeApplicationList")]
        [PerformanceLoggingAdvice]
        public NippsPackageResponse UpgradeApplicationList(NippsPackageRequest request)
        {
            NippsPackageResponse response = new NippsPackageResponse();
            string BackupTargetPath = GetBackupTargetPath();

            response.Result = Result.OK;
            response.ResultMessages = new List<string>();

            if (request != null && request.NippsPackages != null && request.NippsPackages.Count > 0)
                foreach (string fileName in Directory.GetFiles(BackupTargetPath, "Upgr_" + request.NippsPackages[0].ApplicationName.Replace("/", "") + "*.zip", SearchOption.TopDirectoryOnly))
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
            BaseResponse response = new NippsPackageResponse();
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

                response.ResultMessages.Add(string.Format("log.NippsModules exit code -> {0}", ConfigurationBackup(bcpExe, sqlArgs, "log.NippsModules", targetPath)));

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
            BaseResponse response = new NippsPackageResponse();
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
        public BaseResponse RestoreConfiguration(NippsPackageRequest request)
        {

            BaseResponse response = new NippsPackageResponse();
            response.ResultMessages = new List<string>();

            try
            {

                if (request != null && request.NippsPackages != null && request.NippsPackages.Count > 0)
                {
                    string bcpExe = GetBcpExe();
                    string connectionString = ConfigurationManager.ConnectionStrings["UpgradeDb"].ConnectionString;

                    string dataBase = Regex.Match(connectionString, @"(Initial Catalog=)(\w+)").Value.Replace("Initial Catalog=", "");
                    string userName = Regex.Match(connectionString, @"(User ID=)(\w+)").Value.Replace("User ID=", "");
                    string passWord = Regex.Match(connectionString, @"(Password=)(\w+)").Value.Replace("Password=", "");
                    string host = Regex.Match(connectionString, @"(Data Source=)(\w+)").Value.Replace("Data Source=", "");
                    string sqlArgs = string.Format("-c -C RAW -S {0} -d {1} -U {2} -P {3}", host, dataBase, userName, passWord);

                    foreach (NippsPackage np in request.NippsPackages)
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
        public NippsPackageResponse BackupApplication(NippsPackageRequest request)
        {
            NippsPackageResponse response = new NippsPackageResponse();
            bool succeededOne = false;
            string BackupTargetPath = GetBackupTargetPath();

            response.ResultMessages = new List<string>();
            response.Result = Result.OK;

            _Logger.Debug(request.ToString());

            foreach (NippsPackage np in request.NippsPackages)
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
        public NippsPackageResponse RestoreApplication(NippsPackageRequest request)
        {
            NippsPackageResponse response = new NippsPackageResponse();

            response.Result = Result.OK;
            response.ResultMessages = new List<string>();

            _Logger.Debug(request);

            foreach (NippsPackage np in request.NippsPackages)
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
        public NippsPackageResponse RestoreApplicationList(NippsPackageRequest request)
        {
            NippsPackageResponse response = new NippsPackageResponse();
            string BackupTargetPath = GetBackupTargetPath();
            
            response.Result = Result.OK;
            response.ResultMessages = new List<string>();

            if (request != null && request.NippsPackages != null && request.NippsPackages.Count > 0)
                foreach (NippsPackage np in request.NippsPackages)
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

        private static string ApplicationRestore(NippsPackage np)
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

            NippsZipHelper.Unzip(applicationFolder, zipName);

            return zipName.Substring(BackupTargetPath.Length);

        }

        private static string ApplicationBackup(NippsPackage np)
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

            NippsZipHelper.ZipFolder(applicationFolder, zipName);
            return zipName.Substring(BackupTargetPath.Length);

        }

        private static string GetBackupTargetPath()
        {
            string ConfigManagerServiceUrl = ConfigurationManager.AppSettings["ConfigManagerServiceUrl"].ToString();
            
            NippsParameterRequest request = new NippsParameterRequest
            { NippsParameters = new List<NippsParameter>
                { new NippsParameter{ CategoryName = "BACKUP", ParameterName = "TargetPath" } } 
            };

            NippsParameterResponse response = RestHelper.RestPostObject<NippsParameterResponse, NippsParameterRequest>(ConfigManagerServiceUrl+"Get", request);
            if (response.Result == Result.OK)
                return response.NippsParameters[0].ParameterValue;
            //return default if not found/defined
            return @"C:\NIPPSPackages";
        }

        private static string GetBcpExe()
        {
            string ConfigManagerServiceUrl = ConfigurationManager.AppSettings["ConfigManagerServiceUrl"].ToString();

            NippsParameterRequest request = new NippsParameterRequest
            {
                NippsParameters = new List<NippsParameter> { new NippsParameter { CategoryName = "BACKUP", ParameterName = "BCP.EXE" } }
            };

            NippsParameterResponse response = RestHelper.RestPostObject<NippsParameterResponse, NippsParameterRequest>(ConfigManagerServiceUrl + "Get", request);
            if (response.Result == Result.OK)
                return response.NippsParameters[0].ParameterValue;
            //return default if not found/defined
            return @"C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\110\Tools\Binn\bcp.exe";
        }

        private static string ZipFileName(NippsPackage np, bool timeBased = true)
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