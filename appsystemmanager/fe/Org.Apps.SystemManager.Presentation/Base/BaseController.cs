using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Generic;
using Org.Apps.BaseDao.Model.Response;

namespace Org.Apps.SystemManager.Presentation.Base
{
    public class BaseController : Controller
    {
        public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public RedirectToRouteResult RedirectToAction(string actionName, string controllerName)
        {
            return base.RedirectToAction(actionName, controllerName);
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
        }

        public static void SetViewBagResult(BaseResponse response, dynamic ViewBag)
        {

            if (ViewBag.Name == null)
                ViewBag.Name = "";

            if (ViewBag.Result == null)
                ViewBag.Result = Result.OK;

            if (ViewBag.ResultMessages == null)
                ViewBag.ResultMessages = new List<string>();

            if (response == null)
                return;

            if (response.Result == Result.OK)
            {
                ViewBag.ResultMessages.Add(string.Format(Resources.Global.MessageOk, ViewBag.Name));
                if (ViewBag.Result == Result.FAIL)
                    ViewBag.Result = Result.SUCCESSWITHWARN;
            }
            else if (response.Result == Result.FAIL)
            {
                ViewBag.ResultMessages.Add(string.Format(Resources.Global.MessageFail, ViewBag.Name));
                if (ViewBag.Result == Result.OK)
                    ViewBag.Result = Result.SUCCESSWITHWARN;
            }
            else
            {
                ViewBag.ResultMessages.Add(string.Format(Resources.Global.MessageWarn, ViewBag.Name));
                ViewBag.Result = Result.SUCCESSWITHWARN;
            }

            if (response.ResultMessages != null && response.ResultMessages.Count > 0)
                foreach (string m in response.ResultMessages)
                    if (!string.IsNullOrEmpty(m))
                        ViewBag.ResultMessages.Add(m);

        }
    }
}