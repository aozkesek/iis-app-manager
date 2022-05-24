using System.Web.Http;

using Org.Apps.BaseService;

namespace Org.Apps.AuthManager.Service
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            AppsIoCHelper.RegisterComponents = AuthManagerServiceHelper.RegisterComponents;
            AppsIoCHelper.InitializeComponents = AuthManagerServiceHelper.InitializeComponents;

            AppsIoCHelper.BuildContainer();
            
        }
    }
}
