using System.Web.Http;

using Org.Apps.BaseService;

namespace Org.Apps.LogManager.Service
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            AppsIoCHelper.RegisterComponents = LogManagerServiceHelper.RegisterComponents;
            AppsIoCHelper.InitializeComponents = LogManagerServiceHelper.InitializeComponents;

            AppsIoCHelper.BuildContainer();
            
            
        }
    }
}
