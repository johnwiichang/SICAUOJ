using OJ_WebApp.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using PagedList;

namespace OJ_WebApp.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        OJ_WebAppContext entity = new OJ_WebAppContext();
        // GET: Task
        public ActionResult My(Int32? page)
        {
            var Logined = entity.Users.Find(Int32.Parse(System.Web.Security.FormsAuthentication.Decrypt(Request.Cookies.Get(System.Web.Security.FormsAuthentication.FormsCookieName).Value).Name));
            var tasks = entity.Tasks.Where(x => x.Owner.Id == Logined.Id).OrderByDescending(x => x.Id);
            var pageNumber = page ?? 1;
            var onePageOfProducts = tasks.ToPagedList(pageNumber, 25);
            return View("Task", onePageOfProducts);
        }

        public ActionResult Select(Int32? page)
        {
            var Logined = entity.Users.Find(Int32.Parse(System.Web.Security.FormsAuthentication.Decrypt(Request.Cookies.Get(System.Web.Security.FormsAuthentication.FormsCookieName).Value).Name));
            var lib = entity.Libs.Where(x => !x.isPrivate || x.Groups.Where(y => y.Users.Where(z => z.Id == Logined.Id).Count() != 0).Count() != 0 && (x.Games.Count == 0 || x.Games.FirstOrDefault().BeginTime < DateTime.Now)).OrderBy(x => x.Id);
            var pageNumber = page ?? 1;
            var onePageOfProducts = lib.ToPagedList(pageNumber, 25);
            return View("Lib", onePageOfProducts);
        }

        public ActionResult Lib(Int32 libid, Int32? page)
        {
            var Logined = entity.Users.Find(Int32.Parse(System.Web.Security.FormsAuthentication.Decrypt(Request.Cookies.Get(System.Web.Security.FormsAuthentication.FormsCookieName).Value).Name));
            var lib = entity.Libs.Where(x => !x.isPrivate || x.Groups.Where(y => y.Users.Where(z => z.Id == Logined.Id).Count() != 0).Count() != 0 && (x.Games.Count == 0 || x.Games.FirstOrDefault().BeginTime < DateTime.Now)).Where(x => x.Id == libid).First();
            ViewBag.Title = lib.Name;
            ViewBag.Lib = lib.Id;
            var pageNumber = page ?? 1;
            var onePageOfProducts = lib.Issues.ToPagedList(pageNumber, 25);
            return View("Issue", onePageOfProducts);
        }

        public ActionResult Solve(Int32 libid, Int32 issueid)
        {
            try
            {
                var Logined = entity.Users.Find(Int32.Parse(System.Web.Security.FormsAuthentication.Decrypt(Request.Cookies.Get(System.Web.Security.FormsAuthentication.FormsCookieName).Value).Name));
                var lib = entity.Libs.Find(libid);
                if (lib.isPrivate && Logined.Groups.Where(x => x.Libs.Where(y => y.Id == libid).Count() > 0).Count() == 0)
                {
                    throw new Exception(Resources.Language.RequestRefused);
                }
                if (lib.isPrivate && lib.Games.First().BeginTime > DateTime.Now)
                {
                    throw new Exception(Resources.Language.RequestRefused);
                }
                Issue i = entity.Issues.Find(issueid);
                var issue = i.Libs.Where(x => x.Id == libid).Count() > 0;
                if (!issue)
                {
                    throw new Exception(Resources.Language.NotFund);
                }
                ViewData["issue"] = i;
                return View(new Models.Task());
            }
            catch (Exception)
            {
                return Content("<script>alert('" + Resources.Language.BeSerious + "');location.href='/'</script>");
            }
        }

        [HttpPost]
        public ActionResult Answer(Models.Task t)
        {
            t.Compiler = entity.Compilers.Find(t.Compiler.Id);
            if (t.Compiler.isForbidden)
            {
                return Content(Resources.Language.RequestRefused);
            }
            t.Owner = entity.Users.Find(Int32.Parse(System.Web.Security.FormsAuthentication.Decrypt(Request.Cookies.Get(System.Web.Security.FormsAuthentication.FormsCookieName).Value).Name));
            t.Issue = entity.Issues.Find(Int32.Parse(Request.UrlReferrer.Segments.Last()));
            t.CreateTime = DateTime.Now;
            entity.Tasks.Add(t);
            entity.SaveChanges();
            return Content("<script>location.href = '" + Url.Action("My", "Task") + "'</script>");
        }

        public ActionResult Result(Int32 Id)
        {
            Models.Task t = entity.Tasks.Find(Id);
            if (System.Web.Security.FormsAuthentication.Decrypt(Request.Cookies.Get(System.Web.Security.FormsAuthentication.FormsCookieName).Value).UserData != "Admin" && t.Owner.Id.ToString() != System.Web.Security.FormsAuthentication.Decrypt(Request.Cookies.Get(System.Web.Security.FormsAuthentication.FormsCookieName).Value).Name)
            {
                return Content("<script>location.href='/401'</script>");
            }
            return View("Taskdetail", t);
        }
    }
}