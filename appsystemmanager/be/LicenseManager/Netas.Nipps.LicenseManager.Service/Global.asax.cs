using System;
using System.Web;
using System.Web.Http;

namespace Netas.Nipps.LicenseManager.Service
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
