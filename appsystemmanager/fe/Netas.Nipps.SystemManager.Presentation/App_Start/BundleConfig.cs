using System.Web.Optimization;

namespace Netas.Nipps.SystemManager.Presentation
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/bundles/nipps").Include("~/Scripts/nipps.js"));

            bundles.Add(new ScriptBundle("~/bundles/intercooler").Include("~/Scripts/intercooler-0.4.10.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include("~/Scripts/jquery-ui-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/jquerydatatables").Include("~/Scripts/jquery.datatables.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/additional-methods",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryinputmask").Include("~/Scripts/jquery.inputmask.bundle.js"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/site.css", 
                "~/Content/jquery.datatables.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery-ui.css",
                        "~/Content/themes/base/jquery-ui.structure.css",
                        "~/Content/themes/base/jquery-ui.theme.css"
                        ));
        }
    }
}