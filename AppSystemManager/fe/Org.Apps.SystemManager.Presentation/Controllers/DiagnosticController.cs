using System;
using Org.Apps.SystemManager.Presentation.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Text;
using System.Web.Mvc;

using Org.Apps.SystemManager.Presentation.Base;
using Org.Apps.SystemManager.Presentation.Authorize;

namespace Org.Apps.SystemManager.Presentation.Controllers
{
    public class DiagnosticController : BaseController
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private static string _diagnosticServiceUrl = ConfigurationManager.AppSettings["DiagnosticsServiceUrl"];

        [LoginAndAuthorize]
        public ActionResult Index()
        {
            return View();
        }

        [LoginAndAuthorize]
        public ActionResult ProductList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            try
            {
                byte[] bytes;
                using (WebClient client = new WebClient())
                {
                    bytes = client.DownloadData(_diagnosticServiceUrl);
                }

                var data = Encoding.ASCII.GetString(bytes);
                var products = JsonConvert.DeserializeObject<IEnumerable<Product>>(data);

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return View();
            }
            
        }

    }
}