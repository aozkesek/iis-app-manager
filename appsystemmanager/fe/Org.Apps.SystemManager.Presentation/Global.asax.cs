using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web;
using System;
using System.Threading;
using System.Globalization;

namespace Org.Apps.SystemManager.Presentation
{

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
            
        }

        protected void Application_AcquireRequestState(object sender, EventArgs evArgs)
        {
            HttpCookie langCookie = Request.Cookies.Get("Language");
            if (langCookie == null) 
            {
                string lang = Request.UserLanguages[0];
                if (string.IsNullOrEmpty(lang))
                    lang = "tr";
                else
                    lang = lang.Substring(0, 2);
                langCookie = new HttpCookie("Language", lang);
                langCookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Add(langCookie);
            }

            Thread.CurrentThread.CurrentCulture = new CultureInfo(langCookie.Value);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
        }

        protected void Application_Error(object sender, EventArgs evArgs)
        {
            var ex = Server.GetLastError().GetBaseException();
            NLog.LogManager.GetCurrentClassLogger().Error("Application_Error handler cought an error:\n{0}", ex.ToString());
            if (ex.GetType() == typeof(HttpException))
            {
                var httpEx = (HttpException)ex;
                if (httpEx.GetHttpCode() == 404)
                {
                    Response.Redirect(Request.ApplicationPath);
                }

            }
            else
                Response.Redirect(Request.ApplicationPath);

        }
    }
}