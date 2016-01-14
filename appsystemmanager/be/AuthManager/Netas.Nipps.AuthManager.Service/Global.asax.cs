using System.Web.Http;

using Netas.Nipps.BaseService;

namespace Netas.Nipps.AuthManager.Service
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            NippsIoCHelper.RegisterComponents = AuthManagerServiceHelper.RegisterComponents;
            NippsIoCHelper.InitializeComponents = AuthManagerServiceHelper.InitializeComponents;

            NippsIoCHelper.BuildContainer();
            
        }
    }
}
