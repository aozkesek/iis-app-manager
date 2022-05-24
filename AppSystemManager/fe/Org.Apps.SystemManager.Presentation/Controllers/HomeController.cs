using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Threading;
using System.Globalization;

namespace Org.Apps.SystemManager.Presentation.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["IsUserAuthenticated"] == null)
                Session["IsUserAuthenticated"] = false;

            if ((bool)Session["IsUserAuthenticated"] == true)
                return View();

            return RedirectToAction("UserLoginConfirm","UserManagement");

        }

        public ActionResult About()
        {
            string fileName = Request.PhysicalPath.Replace("Home\\About", "bin\\Org.Apps.SystemManager.Presentation.dll");
            ViewBag.Version = FileVersionInfo.GetVersionInfo(fileName).FileVersion;

            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Apps()
        {
            return View();
        }

        public ActionResult SetLanguage(string lang, string returnUrl)
        {
            HttpCookie langCookie = Request.Cookies.Get("Language");
            if (langCookie == null)
                langCookie = new HttpCookie("Language", lang);
            else
                langCookie.Value = lang;
            
            langCookie.Expires.AddDays(30);
            Response.Cookies.Add(langCookie);
                
            Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            return Redirect(returnUrl);
        }
    }
}