using Netas.Nipps.BaseService;
using System.Web.Http;

namespace Netas.Nipps.ConfigManager.Service
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            NippsIoCHelper.RegisterComponents = ConfigManagerServiceHelper.RegisterComponents;
            NippsIoCHelper.InitializeComponents = ConfigManagerServiceHelper.InitializeComponents;

            NippsIoCHelper.BuildContainer();
        }
    }
}