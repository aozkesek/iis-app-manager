using Org.Apps.BaseService;
using System.Web.Http;

namespace Org.Apps.ConfigManager.Service
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            AppsIoCHelper.RegisterComponents = ConfigManagerServiceHelper.RegisterComponents;
            AppsIoCHelper.InitializeComponents = ConfigManagerServiceHelper.InitializeComponents;

            AppsIoCHelper.BuildContainer();
        }
    }
}