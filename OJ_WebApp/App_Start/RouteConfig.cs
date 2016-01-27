using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OJ_WebApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "404",
                url: "404",
                defaults: new { controller = "Home", action = "PageNotFund" }
            );

            routes.MapRoute(
                name: "401",
                url: "401",
                defaults: new { controller = "Home", action = "ValidPermission" }
            );

            routes.MapRoute(
                name: "issue",
                url: "Task/Solve/{libid}/{issueid}",
                defaults: new { controller = "Task", action = "Solve" }
            );

            routes.MapRoute(
                name: "lib",
                url: "Task/Lib/{libid}",
                defaults: new { controller = "Task", action = "Lib" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
