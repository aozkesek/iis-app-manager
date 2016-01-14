using System.Web.Http;

using Netas.Nipps.BaseService;

namespace Netas.Nipps.LogManager.Service
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            NippsIoCHelper.RegisterComponents = LogManagerServiceHelper.RegisterComponents;
            NippsIoCHelper.InitializeComponents = LogManagerServiceHelper.InitializeComponents;

            NippsIoCHelper.BuildContainer();
            
            
        }
    }
}
