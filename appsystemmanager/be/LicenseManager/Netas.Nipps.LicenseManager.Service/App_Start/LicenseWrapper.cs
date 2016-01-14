using System;
using System.IO;
using System.Web;
using System.Text;
using System.Linq;
using System.Configuration;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.CSharp;

using Quartz;
using Quartz.Impl;

using Netas.Nipps.BaseService;

using Netas.Nipps.BaseDao.Model.Response;

using Netas.Nipps.ConfigManager.Data.Model;
using Netas.Nipps.ConfigManager.Data.Model.Request;
using Netas.Nipps.ConfigManager.Data.Model.Response;

using Netas.Nipps.LicenseManager.Data.Model;

namespace Netas.Nipps.LicenseManager.Service
{
    public sealed class LicenseWrapper
    {

        static readonly string LicenseBasePath = GetLicenseFilePath();
        static readonly string LicenseDllName = LicenseConfig.DllName;
        static readonly string LicenseFileName = LicenseBasePath + "client.license";

        static ISchedulerFactory _SchedulerFactory = new StdSchedulerFactory();
        static IScheduler _Scheduler = _SchedulerFactory.GetScheduler();

        static NippsLicense _License = null;
        public static NippsLicense License { get { return _License; } }

        [DllImport("licenselib.dll")]
        static extern int LicenseSha1(string pem_path, string source, string sha1a, string sha1b);

        public static void Start()
        {
            if (!_Scheduler.IsStarted)
            {
                try
                {
                    _Scheduler.Start();

                    IJobDetail licenseUpdateJob = JobBuilder.Create<LicenseUpdateJob>()
                        .WithIdentity("LicenseUpdateJob", "LicenseJobs")
                        .Build();

                    ITrigger licenseUpdateTrigger = TriggerBuilder.Create()
                        .StartAt(DateTime.Now.AddMinutes(2))
                        .WithIdentity("LicenseUpdateTrigger", "LicenseJobs")
                        .WithSimpleSchedule(x =>
                            x.WithIntervalInMinutes(30)
                            .RepeatForever())
                        .Build();

                    _Scheduler.ScheduleJob(licenseUpdateJob, licenseUpdateTrigger);

                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                }
                
            }
            
        }

        public static bool Valid()
        {

            LicenseDll();

            return true;
        }

        public static bool Valid(string content)
        {
            if (!File.Exists(LicenseFileName))
                using (FileStream fs = File.Create(LicenseFileName)) { fs.Close(); }

            File.WriteAllText(LicenseFileName, content);

            Validate();

            return true;
        }

        public static bool Valid(string phoneService, string version)
        {
            try
            {
                //compare name and the first 4 characters of the version
                //
                LicenseWrapper._License.Services
                    .Where(ps => ps.Name.Equals(phoneService) && ps.Version.Substring(0, 4).Equals(version.Substring(0, 4)))
                    .First();

            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                throw new AccessViolationException(phoneService + ", Version:" + version);
            }
            
            return true;
        }

        public static void Validate()
        {
            string sha1a = "";
            string sha1b = "";
            string license = "";
            string binPath = HttpContext.Current.Server.MapPath("~\\bin");

            if (!File.Exists(LicenseBasePath + "pri_client.pem"))
                if (File.Exists(binPath + "\\pri_client.pem"))
                    File.Copy(binPath + "\\pri_client.pem", LicenseBasePath + "pri_client.pem");

            if (!File.Exists(LicenseBasePath + "pub_netas.pem"))
                if (File.Exists(binPath + "\\pub_netas.pem"))
                    File.Copy(binPath + "\\pub_netas.pem", LicenseBasePath + "pub_netas.pem");

            using (StreamReader sr = File.OpenText(LicenseFileName))
            {
                string line;
                bool isSha1a = false;
                bool isSha1b = false;

                license = sr.ReadLine();

                _License = Newtonsoft.Json.JsonConvert.DeserializeObject<NippsLicense>(license);

                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (line.Equals("---BEGIN SHA1 A---"))
                    {
                        isSha1a = true;
                        isSha1b = false;
                        continue;
                    }
                    else if (line.Equals("---BEGIN SHA1 B---"))
                    {
                        isSha1b = true;
                        isSha1a = false;
                        continue;
                    }
                    else if (line.Equals("---END SHA1 A---") || line.Equals("---END SHA1 B---"))
                    {
                        isSha1a = false;
                        isSha1b = false;
                        continue;
                    }

                    if (isSha1a)
                        sha1a += line;
                    else if (isSha1b)
                        sha1b += line;
                }

            }

            int rc = LicenseSha1(LicenseBasePath, license, sha1a, sha1b);
            if (rc != 0)
            {
                File.Delete(LicenseFileName);
                _License = null;
                NLog.LogManager.GetCurrentClassLogger().Error("License is violated by SHA1.");
                throw new AccessViolationException(LicenseFileName);
            }

            LicenseDll();
        }

        private static void LicenseDll()
        {
            string binPath = HttpContext.Current.Server.MapPath("~\\bin");
            string dllName = LicenseBasePath + LicenseDllName;
            long nowTicks = DateTime.Now.Ticks;
            long _startOf = 0;
            long _validFor = 0;

            if (!File.Exists(dllName))
            {
                LicenseCompile();
                return;
            }

            if (_License.Type.Equals("LIFETIME"))
                return;

            lock (LicenseUpdateJob.LicenseUpdateJobLock)
            {
                AppDomainSetup ads = new AppDomainSetup();
                ads.ApplicationBase = binPath;

                AppDomain appDomain = AppDomain.CreateDomain("License Dll AppDomain", null, ads);

                try
                {
                    LicenseConfig licenseConfig = (LicenseConfig)appDomain
                        .CreateInstanceAndUnwrap(typeof(LicenseConfig).Assembly.FullName, typeof(LicenseConfig).FullName);

                    licenseConfig.LoadFrom(LicenseBasePath);
                    _startOf = licenseConfig.StartOf();
                    _validFor = licenseConfig.ValidFor();
                    licenseConfig.Ticks();
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                }
                finally
                {
                    AppDomain.Unload(appDomain);
                }
            }

            if (_startOf > nowTicks || _startOf != WebApiApplication.StartOf)
            {
                File.Delete(LicenseFileName);
                NLog.LogManager.GetCurrentClassLogger().Error("License is violated by StartOf.");
                throw new AccessViolationException(LicenseFileName);
            }

            if (nowTicks - _startOf > _validFor - _startOf)
            {
                File.Delete(LicenseFileName);
                NLog.LogManager.GetCurrentClassLogger().Error("License is violated by ValidFor.");
                throw new AccessViolationException(LicenseFileName);
            }
                
            return;
        }

        public static void LicenseCompile()
        {
            long _validFor = WebApiApplication.StartOf + long.Parse(_License.ValidFor) * 10000000 * 60 * 60 * 24;
            CSharpCodeProvider mscsProvider = new CSharpCodeProvider();
            CompilerParameters compParams = new CompilerParameters();

            compParams.GenerateExecutable = false;
            compParams.GenerateInMemory = false;
            compParams.IncludeDebugInformation = false;
            compParams.OutputAssembly = LicenseBasePath + LicenseDllName;

            CompilerResults compResults = mscsProvider
                .CompileAssemblyFromSource(compParams, LicenseConfig.Source(_validFor));

            if (compResults.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder("Compiler error(s):\n");
                foreach (CompilerError ce in compResults.Errors)
                    sb.AppendFormat("\t{0}: {1}\n", ce.ErrorNumber, ce.ErrorText);
                NLog.LogManager.GetCurrentClassLogger().Error(sb.ToString());
                throw new BadImageFormatException(LicenseDllName);
            }
        }

        public static string GetLicenseFilePath()
        {
            string svcUrl = ConfigurationManager.AppSettings["ConfigManagerServiceUrl"] + "Get";

            NippsParameterRequest paramRequest = new NippsParameterRequest
            {
                NippsParameters = new List<NippsParameter> { 
                    new NippsParameter{ CategoryName = "LICENSE", ParameterName = "LicenseFilePath" }
                }
            };
            NippsParameterResponse paramResponse = RestHelper.RestPostObject<NippsParameterResponse, NippsParameterRequest>(svcUrl, paramRequest);
            if (paramResponse.Result == Result.OK)
                return paramResponse.NippsParameters[0].ParameterValue;

            throw new Exception(paramResponse.ResultMessages[0]);
        }  

    }
}