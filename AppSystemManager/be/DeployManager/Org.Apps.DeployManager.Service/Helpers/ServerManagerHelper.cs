using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Web.Administration;

namespace Org.Apps.DeployManager.Service.Helpers
{
    public sealed class ServerManagerHelper
    {

        public static string GetApplicationPhysicalPath(string siteName, string applicationName)
        {
            using (ServerManager sm = ServerManager.OpenRemote("localhost"))
            {
                return sm.Sites[siteName].Applications[String.IsNullOrEmpty(applicationName) ? "/" : applicationName].VirtualDirectories["/"].PhysicalPath;
            }
        }

        public static string PutEnvVarValue(string source)
        {
            Match m = Regex.Match(source, @"%\w+%");
            string ev;

            while (m.Success)
            {
                ev = System.Environment.GetEnvironmentVariable(m.Value.Replace("%", ""));
                source = source.Replace(m.Value, ev);
                m = m.NextMatch();
            }

            return source;
        }

    }
}