using System;
using System.Web;
using System.Web.Http;

namespace Org.Apps.LicenseManager.Service
{
    public class WebApiApplication : HttpApplication
    {
        public static readonly long StartOf = DateTime.Now.Ticks;
        
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            LicenseWrapper.Start();

        }

        
    }
}
