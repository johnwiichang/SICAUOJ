using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OJ_WebApp.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        [Authorize]
        public RedirectToRouteResult Index()
        {
            return RedirectToAction("Dashboard", new { });
        }

        [Authorize]
        public ActionResult Dashboard()
        {
            var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            ViewBag.Version = v.Major + "." + v.Minor + "." + v.Build;
            return View("Index");
        }

        public ActionResult PageNotFund()
        {
            return View();
        }

        public ActionResult ValidPermission()
        {
            return View();
        }
    }
}