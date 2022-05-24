using System;
using System.IO;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Data.Entity;
using SharpCompress.Reader;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Web.Administration;
using System.Security.AccessControl;
using System.Security.Principal;
using System.DirectoryServices.AccountManagement;

namespace AppsInstall
{
    class AppsInstall
    {
        #region attributes
        static readonly string TempFolder = @"temp\";

        static readonly Regex RegexIdentity = new Regex("(<identity impersonate=)(.*)(/>)");
        static readonly string IdentityFormat = "<identity impersonate=\"true\" password=\"{1}\" userName=\"{0}\"/>";

        static readonly Regex RegexConnectionStringUserID = new Regex("(User ID=Appsuser)");
        static readonly string ConnectionStringUserIDFormat = "User ID={0}";

        static readonly Regex RegexConnectionStringPassword = new Regex("(Password=Appspwd)");
        static readonly string ConnectionStringPasswordFormat = "Password={0}";

        static readonly Regex RegexConnectionStringDataSource = new Regex("(Data Source=tge36dbsrv)");
        static readonly string ConnectionStringDataSourceFormat = "Data Source={0}";

        static readonly Regex RegexConnectionStringInitialCatalog = new Regex("(Initial Catalog=Apps_SM)");
        static readonly string ConnectionStringInitialCatalogFormat = "Initial Catalog={0}";

        static readonly Regex RegexServiceUrls = new Regex(@"(ServiceUrl"" value=""http://(.*)/Org\.Apps\.)");
        static readonly string ServiceUrlFormat = @"ServiceUrl"" value=""http://{0}/Org.Apps.";

        static readonly Regex RegexSql = new Regex("\n[Gg][Oo]");

        static DirectorySecurity IISW3RootDirectorySecurity;
        static AppsJson AppsConfig;

        static string[] CommandLineArgs;

        static bool ServiceOnlyInstall = false;
        #endregion

        static void Main(string[] args)
        {
            CommandLineArgs = args;

            try
            {
                if (!ArgExist("path="))
                {
                    Console.WriteLine("<path=...> parameter must be entered.");

                    Console.WriteLine("press enter to finish.");
                    Console.ReadKey(true);
                    return;
                }

                bool noSql = ArgExist("nosql");
                bool noIis = ArgExist("noiis");
                bool noZip = ArgExist("nozip");
                bool genLic = ArgExist("genlic");

                AppsConfig = LoadAppsJson();

                CheckVersionInfo();

                CheckInstallType();

                if (genLic || (!noIis && !noSql && !ServiceOnlyInstall))
                    GenerateLicense();

                if (!noZip)
                {
                    UnzipPackage();

                    UpdateConfig();

                    CopyLicenseLibrary();

                    if (!noIis)
                    {
                        MoveFolder();
                        SetupIIS();
                    }

                    if (!noSql && !ServiceOnlyInstall)
                        ExecuteSqlScript();

                }
                
                Console.WriteLine("finished.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (Directory.Exists(TempFolder))
                Directory.Delete(TempFolder, true);

            Console.WriteLine("press enter to finish.");
            Console.ReadKey(true);
            
        }

        #region methods
        static void CreateDirectory(string directory, DirectorySecurity ds)
        {
            if (ArgExist("nosecurity"))
                Directory.CreateDirectory(directory);
            else
                Directory.CreateDirectory(directory, ds);
        }

        static void CreatePathFolder(string path)
        {
            StringBuilder folder = new StringBuilder();
            string[] folders = path.Split('\\');
            Array.Resize(ref folders, folders.Length - 1);

            foreach (string folderPart in folders)
                if (!Directory.Exists(folder.Append(folderPart + "\\").ToString()))
                    CreateDirectory(folder.ToString(), IISW3RootDirectorySecurity);
        }

        static AppsJson LoadAppsJson()
        {
            Console.WriteLine("reading Apps.json...");
            using (StreamReader AppsJsonReader = File.OpenText(@"Apps.json"))
            {
                string Appsjson = AppsJsonReader.ReadToEnd();
                Console.WriteLine("deserializing Apps.json...");
                return JsonConvert.DeserializeObject<AppsJson>(Appsjson);
            }
            throw new InvalidDataException("AppsJson could not deserialized.");
        }

        static bool Match(string name)
        {

            try
            {
                AppsModule module = AppsConfig.Apps.IPPhoneServices
                    .Where(m => ArgExist(m.Name))
                    .SingleOrDefault();

                return name.StartsWith(module.ApplicationName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

        }

        static void UnzipPackage()
        {

            IISW3RootDirectorySecurity = ArgExist("nosecurity") ? null : new DirectorySecurity(AppsConfig.Environment.IISwwwroot, AccessControlSections.All);

            Console.WriteLine("creating temp folder...");
            CreateDirectory(TempFolder, IISW3RootDirectorySecurity);

            Console.WriteLine("loading Apps.zip into memory...");
            using (Stream AppsZipStream = new MemoryStream(File.ReadAllBytes(@"Apps.zip")))
            {
                Console.WriteLine("opening zip...");
                using (var zipReader = ReaderFactory.Open(AppsZipStream))
                {
                    string key;

                    while (zipReader.MoveToNextEntry())
                    {
                        key = zipReader.Entry.Key.Replace("/", "\\");

                        if (ServiceOnlyInstall)
                            //extract only given ip phone services and deploy manager if ServiceOnlyInstall
                            if (!Match(key) && !key.StartsWith("Org.Apps.DeployManager.Service"))
                                continue;

                        if (!zipReader.Entry.IsDirectory)
                        {
                            Console.WriteLine("checking and deleting if exist file... -> " + TempFolder + key);
                            if (File.Exists(TempFolder + key))
                                File.Delete(TempFolder + key);

                            Console.WriteLine("checking and creating if not exist folder... -> " + TempFolder + key);
                            CreatePathFolder(TempFolder + key);

                            Console.WriteLine("creating file... -> " + TempFolder + key);
                            using (Stream entryStream = File.Create(TempFolder + key))
                            {
                                Console.WriteLine("writing file...");
                                zipReader.WriteEntryTo(entryStream);
                                entryStream.Close();
                            }
                        }
                        else
                        {
                            Console.WriteLine("checking and creating if not exist folder... -> " + TempFolder + key);
                            if (!Directory.Exists(TempFolder + key))
                                CreateDirectory(TempFolder + key, IISW3RootDirectorySecurity);
                        }

                    }
                }
            }

        }

        static void UpdateWebConfigs(AppsModule AppsModule)
        {
            string configName;
            string configText;
            string[] iisLocalAdminParts = AppsConfig.Environment.IISLocalAdmin.Split('/');
            string[] sqlLocalAdminParts = AppsConfig.Environment.SqlLocalAdmin.Split('/');
            string[] sqlUserParts = AppsConfig.Environment.SqlUser.Split('/');
            bool isDeployManager = AppsModule.ApplicationName.Equals("Org.Apps.DeployManager.Service");

            configName = TempFolder + AppsModule.ApplicationName + @"\web.config";
            if (!File.Exists(configName))
                return;

            Console.WriteLine("opening " + configName);

            configText = File.ReadAllText(configName);

            //system.web>identity only one identity must be defined in there
            Console.WriteLine("checking and updating if exist system.web>indendity");
            if (RegexIdentity.Match(configText).Success)
                configText = RegexIdentity.Replace(configText,
                    string.Format(IdentityFormat, iisLocalAdminParts[0], iisLocalAdminParts[1]));

            //connectionstrings: only one identity must be defined in there 
            Console.WriteLine("checking and updating if exist connectionstrings");
            if (RegexConnectionStringUserID.Match(configText).Success)
                configText = RegexConnectionStringUserID.Replace(configText, string.Format(ConnectionStringUserIDFormat, isDeployManager ? sqlLocalAdminParts[0] : sqlUserParts[0]));

            if (RegexConnectionStringPassword.Match(configText).Success)
                configText = RegexConnectionStringPassword.Replace(configText, string.Format(ConnectionStringPasswordFormat, isDeployManager ? sqlLocalAdminParts[1] : sqlUserParts[1]));

            if (RegexConnectionStringDataSource.Match(configText).Success)
                configText = RegexConnectionStringDataSource.Replace(configText, string.Format(ConnectionStringDataSourceFormat, AppsConfig.Environment.SqlHost));

            if (RegexConnectionStringInitialCatalog.Match(configText).Success)
                configText = RegexConnectionStringInitialCatalog.Replace(configText, string.Format(ConnectionStringInitialCatalogFormat, AppsConfig.Environment.SqlDatabase));

            //appsettings>may be more than one in there
            Console.WriteLine("checking and updating if exist applicationsettings>***serviceurls");
            if (RegexServiceUrls.Matches(configText).Count > 0)
                configText = RegexServiceUrls.Replace(configText, string.Format(ServiceUrlFormat, AppsConfig.Environment.IISSMHostPort));


            File.WriteAllText(configName, configText);

        }

        static void UpdateConfig()
        {
            foreach (AppsModule AppsModule in AppsConfig.Apps.SMModules)
                UpdateWebConfigs(AppsModule);

            foreach (AppsModule AppsModule in AppsConfig.Apps.IPPhoneServices)
                UpdateWebConfigs(AppsModule);
        }

        static void MoveFolder()
        {

            Console.WriteLine("moving or copying files and folders into " + AppsConfig.Environment.IISwwwroot);

            foreach (string sourceDirectory in Directory.GetDirectories(TempFolder, "Org.Apps.*", SearchOption.TopDirectoryOnly))
            {
                string appName = sourceDirectory.Replace(TempFolder, "");
                string targetDirectory = AppsConfig.Environment.IISwwwroot + "\\" + appName;
                AppsModule module = appName.StartsWith("Org.Apps.Service")
                    ?
                    AppsConfig.Apps.IPPhoneServices.Where(m => m.ApplicationName.Equals(appName)).Single()
                    :
                    AppsConfig.Apps.SMModules.Where(m => m.ApplicationName.Equals(appName)).Single();
                string minVersion = module != null ? module.MinVersion : "";

                //check if first installation or an upgrade
                if (string.IsNullOrEmpty(minVersion))
                {
                    if (Directory.Exists(targetDirectory))
                        Directory.Delete(targetDirectory, true);
                    Directory.Move(sourceDirectory, targetDirectory);
                    CreateDirectory(targetDirectory + "\\Logs", IISW3RootDirectorySecurity);
                    GiveFullControlRight(targetDirectory + "\\Logs");
                    GiveFullControlRight(targetDirectory + "\\Tmp");
                    GiveFullControlRight(targetDirectory + "\\nlog.config");
                }
                else
                {

                    foreach (string subDirectory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
                    {
                        string subTargetDirectory = targetDirectory + subDirectory.Replace(sourceDirectory, "");
                        Console.WriteLine(subTargetDirectory + " checking and creating if necessary...");
                        if (!Directory.Exists(subTargetDirectory))
                            CreateDirectory(subTargetDirectory, IISW3RootDirectorySecurity);
                    }

                    foreach (string file in Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories))
                    {
                        string targetFile = targetDirectory + file.Replace(sourceDirectory, "");
                        Console.WriteLine(targetFile + " copying...");
                        File.Copy(file, targetFile, true);
                    }
                }

            }

        }

        static void GiveFullControlRight(string name)
        {
            if (Directory.Exists(name))
            {
                try
                {
                    DirectorySecurity ds = Directory.GetAccessControl(name, AccessControlSections.All);
                    ds.AddAccessRule(new FileSystemAccessRule(AppsConfig.Environment.MachineName + @"\IIS_IUSRS", FileSystemRights.FullControl, AccessControlType.Allow));
                    Directory.SetAccessControl(name, ds);
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
            else if (File.Exists(name))
            {
                try
                {
                    FileSecurity fs = File.GetAccessControl(name, AccessControlSections.All);
                    fs.AddAccessRule(new FileSystemAccessRule(AppsConfig.Environment.MachineName + @"\IIS_IUSRS", FileSystemRights.FullControl, AccessControlType.Allow));
                    File.SetAccessControl(name, fs);
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }

        }

        static void ExecuteSqlScript()
        {
            string[] sqlLocalAdminParts = AppsConfig.Environment.SqlLocalAdmin.Split('/');
            string connectionString = string.Format(
                "Data Source={0};Integrated Security=False;User ID={1};Password={2};Initial Catalog={3};Connection Timeout=5",
                AppsConfig.Environment.SqlHost, sqlLocalAdminParts[0], sqlLocalAdminParts[1], AppsConfig.Environment.SqlDatabase);

            Console.WriteLine("checking & opening Apps.sql...");
            if (!File.Exists("Apps.sql"))
                return;

            string sqlScript = File.ReadAllText("Apps.sql");

            Console.WriteLine("opening db...");
            using (DbContext dbContext = new DbContext(connectionString))
            {
                Console.WriteLine("deleting database if exist...");
                if (dbContext.Database.Exists())
                    dbContext.Database.Delete();

                Console.WriteLine("creating database...");
                dbContext.Database.Create();

                DbContextTransaction transaction = dbContext.Database.BeginTransaction();

                try
                {
                    int startIndex = 0;
                    foreach (Match match in RegexSql.Matches(sqlScript))
                    {
                        string sqlCommand = sqlScript.Substring(startIndex, match.Index - startIndex);

                        Console.WriteLine("executing sql ...");
                        Console.WriteLine(sqlCommand);
                        dbContext.Database.ExecuteSqlCommand(sqlCommand);

                        startIndex = match.Index + match.Length;

                    }

                    transaction.Commit();
                    Console.WriteLine("database committed.");

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(ex);
                    Console.WriteLine("database rolled back.");
                }
            }

        }

        static void AddApplication(Site site, AppsModule AppsModule)
        {
            string physicalPath = AppsConfig.Environment.IISwwwroot;

            if (ServiceOnlyInstall)
                if (!Match(AppsModule.ApplicationName) && !AppsModule.ApplicationName.StartsWith("Org.Apps.DeployManager.Service"))
                    return;

            if (physicalPath.EndsWith("\\"))
                physicalPath += AppsModule.ApplicationName;
            else
                physicalPath += "\\" + AppsModule.ApplicationName;

            AppsModule.ApplicationName = "/" + AppsModule.ApplicationName;

            Console.WriteLine("adding " + AppsModule.ApplicationName + " from path " + physicalPath);
            Application application = site.Applications.Add(AppsModule.ApplicationName, physicalPath);
            application.ApplicationPoolName = string.IsNullOrEmpty(AppsConfig.Environment.IISApplicationPoolName) ? AppsModule.ApplicationPoolName : AppsConfig.Environment.IISApplicationPoolName;
        }

        static void SetupIIS()
        {

            Console.WriteLine("searching for iis site " + AppsConfig.Environment.IISSiteName);
            using (ServerManager iisServer = new ServerManager())
            {

                foreach (Site site in iisServer.Sites)
                {
                    if (site.Name.Equals(AppsConfig.Environment.IISSiteName))
                    {
                        foreach (AppsModule AppsModule in AppsConfig.Apps.SMModules)
                            AddApplication(site, AppsModule);

                        foreach (AppsModule AppsModule in AppsConfig.Apps.IPPhoneServices)
                            AddApplication(site, AppsModule);

                        Console.WriteLine("commiting changes...");
                        iisServer.CommitChanges();

                        return;
                    }
                }

                throw new KeyNotFoundException(AppsConfig.Environment.IISSiteName + " site not found.");

            }
        }

        static bool ArgExist(string arg)
        {
            if (CommandLineArgs == null || CommandLineArgs.Length == 0)
                return false;

            if (!arg.EndsWith("="))
                if (string.IsNullOrEmpty(CommandLineArgs.Where<string>(o => o.Equals(arg)).SingleOrDefault()))
                    return false;
                else
                    return true;

            if (string.IsNullOrEmpty(CommandLineArgs.Where<string>(o => o.StartsWith(arg)).SingleOrDefault()))
                return false;

            return true;
        }

        static void CheckVersionInfo()
        {

            Console.WriteLine("checking version info...");

            foreach (AppsModule module in AppsConfig.Apps.SMModules)
            {
                if (!string.IsNullOrEmpty(module.MinVersion))
                {
                    string moduleDll = AppsConfig.Environment.IISwwwroot + "\\" + module.ApplicationName + "\\bin\\" + module.ApplicationName + ".dll";
                    string version = FileVersionInfo.GetVersionInfo(moduleDll).FileVersion;

                    Console.WriteLine("checking " + module.ApplicationName);
                    if (module.MinVersion.CompareTo(version) == 1)
                        throw new InvalidOperationException(
                            string.Format("Invalid version exception.  {0} is required to {1}+.  The current is {2}.", module.ApplicationName, module.MinVersion, version));

                }
            }

        }

        static void CheckInstallType()
        {
            foreach (AppsModule module in AppsConfig.Apps.IPPhoneServices)
            {
                if (ArgExist(module.Name))
                {
                    ServiceOnlyInstall = true;
                    return;
                }
            }
        }

        static SecurityIdentifier FindGroupIdentity(string name)
        {
            using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
            {
                using (GroupPrincipal group = GroupPrincipal.FindByIdentity(context, name))
                {
                    if (group != null)
                        return group.Sid;
                }
            }
            return null;
        }

        static void GenerateLicense()
        {
            StringBuilder commandArgs = new StringBuilder();
            string path = CommandLineArgs.Where(c => c.StartsWith("path=")).Single().Replace("path=", "");

            commandArgs.AppendFormat(" {0} {1} ", path, AppsConfig.Apps.Version);

            foreach (AppsModule m in AppsConfig.Apps.IPPhoneServices)
                commandArgs.AppendFormat(" \"{0}:{1}\" ", m.Name, m.Version);

            Console.WriteLine("license_public.exe is running now with these command line params -> ");
            Console.WriteLine(commandArgs.ToString());

            Process proc = Process.Start("license_public.exe", commandArgs.ToString());
            proc.WaitForExit();

            if (proc.ExitCode != 0)
                throw new Exception("license_public.exe return an error.");
        }
        
        private static void CopyLicenseLibrary()
        {
            File.Copy("licenselib.dll", TempFolder + "Org.Apps.LicenseManager.Service\\bin\\licenselib.dll");
            File.Copy("libeay32.dll", TempFolder + "Org.Apps.LicenseManager.Service\\bin\\libeay32.dll");
            File.Copy("pri_client.pem", TempFolder + "Org.Apps.LicenseManager.Service\\bin\\pri_client.pem");
            File.Copy("pub_Org.pem", TempFolder + "Org.Apps.LicenseManager.Service\\bin\\pub_Org.pem");
        }

        #endregion

    }
}
