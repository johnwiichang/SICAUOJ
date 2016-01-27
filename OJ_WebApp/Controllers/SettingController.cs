using OJ_WebApp.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OJ_WebApp.Controllers
{
    [Authorize(Roles = "Admin, SAdmin")]
    public class SettingController : Controller
    {
        OJ_WebAppContext entity = new OJ_WebAppContext();
        // GET: Setting

        public ActionResult Compiler()
        {
            return View(entity.Compilers.ToList());
        }

        public ActionResult Node(Int32? page)
        {
            var standardTime = DateTime.Now.AddSeconds(-60);
            var games = from x in entity.Nodes where x.CompilerLastReport > standardTime && x.RunnerLastReport > standardTime orderby x.Name descending select x;
            var pageNumber = page ?? 1;
            var onePageOfProducts = games.ToPagedList(pageNumber, 25);
            return View(onePageOfProducts);
        }

        [Authorize(Roles = "SAdmin")]
        public ActionResult AddCompiler()
        {
            ViewBag.Title = Resources.Language.AddCompiler;
            return View("SaveCompiler", new Compiler());
        }

        public ActionResult EditCompiler(Int32 Id)
        {
            var c = entity.Compilers.Find(Id);
            ViewBag.Title = Resources.Language.Edit + " " + c.Name + " " + Resources.Language.Compiler;
            return View("SaveCompiler", c);
        }

        public ActionResult EditNode(String Id)
        {
            var c = entity.Nodes.Find(Id);
            ViewBag.Title = Resources.Language.Edit + " " + c.Name + " " + Resources.Language.Node;
            return View("SaveNode", c);
        }

        [Authorize(Roles = "SAdmin")]
        public ActionResult SaveCompiler(Compiler c)
        {
            try
            {
                ViewBag.Title = Resources.Language.Edit + " " + c.Name + " " + Resources.Language.Compiler;
                var compiler = entity.Compilers.Find(c.Id) ?? new Compiler();
                compiler.Name = c.Name;
                compiler.RunnerArgs = c.RunnerArgs;
                compiler.RunnerPath = c.RunnerPath;
                compiler.CompilerArgs = c.CompilerArgs;
                compiler.CompilerPath = c.CompilerPath;
                compiler.CodeFormat = c.CodeFormat;
                compiler.ExecutionFormat = c.ExecutionFormat;
                compiler.isScript = c.isScript;
                compiler.isForbidden = c.isForbidden;
                if (entity.Compilers.Find(c.Id) == null)
                {
                    entity.Compilers.Add(compiler);
                }
                entity.SaveChanges();
                return Content("<script>location.href='/Setting/Compiler'</script>");
            }
            catch (Exception ex)
            {
                return View("SaveCompiler", c);
            }
        }

        [Authorize(Roles = "SAdmin")]
        public ActionResult SaveNode(Node n)
        {
            try
            {
                ViewBag.Title = Resources.Language.Edit + " " + n.Name + " " + Resources.Language.Node;
                var node = entity.Nodes.Find(n.Name);
                node.inUse = n.inUse;
                node.MaxTask = n.MaxTask;
                node.WorkDir = n.WorkDir;
                node.Heartbeat = n.Heartbeat < 1000 ? 1000 : n.Heartbeat;
                entity.SaveChanges();
                return Content("<script>location.href='/Setting/Node'</script>");
            }
            catch (Exception ex)
            {
                return View("SaveNode", n);
            }
        }
    }
}