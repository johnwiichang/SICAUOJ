using OJ_WebApp.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OJ_WebApp.Controllers
{
    [Authorize(Roles = "Admin, SAdmin")]
    public class AdminController : Controller
    {
        OJ_WebAppContext entity = new OJ_WebAppContext();
        // GET: Admin
        public ActionResult Lib(Int32? page)
        {
            var lib = from x in entity.Libs orderby x.Id descending select x;
            var pageNumber = page ?? 1;
            var onePageOfProducts = lib.ToPagedList(pageNumber, 25);
            return View(onePageOfProducts);
        }

        public ActionResult AddLib()
        {
            ViewBag.Title = Resources.Language.AddLib;
            return View("SaveLib", new Lib());
        }

        public ActionResult AddIssue()
        {
            try
            {
                var d = entity.Libs.Find(Int32.Parse(Request["LibId"]));
                d.Issues.Clear();
                foreach (var item in Request["issues"].Split('#'))
                {
                    if (item != null && item != "")
                    {
                        d.Issues.Add(entity.Issues.Find(Int32.Parse(item)));
                    }
                }
                entity.SaveChanges();
                return Content("<script>location.href='" + Url.Action("LibOverView", "Admin") + @"/" + Request["LibId"] + "'</script>");
            }
            catch (Exception)
            {
                return Content("<script>alert('" + Resources.Language.ContentError + "');location.href='" + Url.Action("LibOverView", "Admin") + @"/" + Request["LibId"] + "'</script>");
            }
        }

        public ActionResult LibOverView(Int32 Id, Int32? page)
        {
            var lib = entity.Libs.Find(Id);
            ViewBag.Title = lib.Name;
            ViewBag.Lib = lib.Id;
            var pageNumber = page ?? 1;
            var onePageOfProducts = lib.Issues.ToPagedList(pageNumber, 25);
            return View("Issue", onePageOfProducts);
        }

        public ActionResult EditLib(Int32 Id)
        {
            var l = entity.Libs.Find(Id);
            ViewBag.Title = Resources.Language.Edit + " " + l.Name;
            return View("SaveLib", l);
        }

        public ActionResult SaveLib(Lib l)
        {
            try
            {
                ViewBag.Title = Resources.Language.Edit + " " + l.Name;
                var lib = entity.Libs.Find(l.Id) ?? new Lib();
                lib.Name = l.Name;
                lib.Intro = l.Intro;
                lib.isPrivate = l.isPrivate;
                if (lib.Groups != null)
                {
                    lib.Groups.Clear();
                }
                foreach (var item in Request["group-set"].Split('#'))
                {
                    if (item != "" && item != null)
                    {
                        lib.Groups.Add(entity.Groups.Find(Int32.Parse(item)));
                    }
                }
                if (entity.Libs.Find(l.Id) == null)
                {
                    entity.Libs.Add(lib);
                }
                entity.SaveChanges();
                return Content("<script>location.href='" + Url.Action("Lib", "Admin") + "'</script>");
            }
            catch (Exception ex)
            {
                ViewBag.JS = "<script>alert('" + ex.Message + "')</script>";
                return View("SaveLib", l);
            }
        }

        public ActionResult Problem(Int32? page)
        {
            var issues = from x in entity.Issues orderby x.Id descending select x;
            var pageNumber = page ?? 1;
            var onePageOfProducts = issues.ToPagedList(pageNumber, 25);
            return View(onePageOfProducts);
        }

        public ActionResult AddProblem()
        {
            ViewBag.Title = Resources.Language.AddQuestion;
            return View("SaveProblem", new Issue());
        }

        public ActionResult EditProblem(Int32 Id)
        {
            var issue = entity.Issues.Find(Id);
            ViewBag.Title = Resources.Language.Edit + " " + issue.Title;
            return View("SaveProblem", issue);
        }

        [HttpPost]
        public ActionResult SaveProblem(Issue i)
        {
            try
            {
                ViewBag.Title = Resources.Language.Edit + " " + i.Title;
                var issue = entity.Issues.Find(i.Id) ?? new Issue();
                issue.Title = i.Title;
                issue.Content = i.Content;
                issue.ComplieTime = i.ComplieTime;
                issue.Input = i.Input;
                issue.Output = i.Output;
                issue.RunTime = i.RunTime;
                issue.PrivateMemorySize = i.PrivateMemorySize;
                if (entity.Issues.Find(i.Id) == null)
                {
                    issue.CreateTime = DateTime.Now;
                    entity.Issues.Add(issue);
                }
                entity.SaveChanges();
                return Content("<script>location.href='" + Url.Action("Problem", "Admin") + "'</script>");
            }
            catch (Exception ex)
            {
                ViewBag.JS = "<script>alert('" + ex.Message + "')</script>";
                return View("SaveProblem", i);
            }
        }

        public ActionResult Task(Int32? page)
        {
            var tasks = from x in entity.Tasks orderby x.Id descending select x;
            var pageNumber = page ?? 1;
            var onePageOfProducts = tasks.ToPagedList(pageNumber, 25);
            return View(onePageOfProducts);
        }

        public ActionResult TaskDetail(Int32 Id)
        {
            var task = entity.Tasks.Find(Id);
            return View(task);
        }

        public ActionResult UserDetail(Int32 Id)
        {
            return View(entity.Users.Find(Id));
        }

        public ActionResult ProblemDetail(Int32 Id)
        {
            return View(entity.Issues.Find(Id));
        }

        public ActionResult Game(Int32? page)
        {
            var games = from x in entity.Games orderby x.Id descending select x;
            var pageNumber = page ?? 1;
            var onePageOfProducts = games.ToPagedList(pageNumber, 25);
            return View(onePageOfProducts);
        }

        public ActionResult GameDetail(Int32 Id)
        {
            var game = entity.Games.Find(Id);
            var lib = game.GameLib.Issues.Select(x => x.Id);
            foreach (var item in game.GameLib.Groups.LastOrDefault().Users)
            {
                var solved1st = entity.Tasks.Where(x => x.Owner.Id == item.Id && x.isPass && x.CreateTime < game.EndTime && x.CreateTime > game.BeginTime).Select(x => x.Issue.Id).Where(x => lib.Contains(x));
                List<int> solved = new List<int>();
                foreach (var iss in solved1st)
                {
                    if (solved.IndexOf(iss) == -1)
                    {
                        solved.Add(iss);
                    }
                }
                TimeSpan pendingTime = new TimeSpan();
                DateTime tempTime = game.BeginTime;
                for (int i = 0; i < solved.Count; i++)
                {
                    var createtime = entity.Issues.Find(solved[i]).Tasks.Last(x => x.Owner.Id == item.Id && x.isPass && x.CreateTime < game.EndTime && x.CreateTime > game.BeginTime).CreateTime;
                    pendingTime += createtime.Subtract(tempTime);
                    tempTime = createtime;
                }
                item.Solved = solved.Count;
                item.SpanOfSolved = pendingTime.TotalSeconds;
                var unpass = new List<int>();
                foreach (var unpassitem in solved)
                {
                    unpass.AddRange((from x in entity.Tasks where x.Owner.Id == item.Id && !x.isPass && x.Issue.Id == unpassitem && x.CreateTime < game.EndTime && x.CreateTime > game.BeginTime select x.Id).ToList());
                }
                item.Err = unpass.Count;
                item.Total = item.Err * 1200 + pendingTime.TotalSeconds;
            }
            return View(game);
        }

        public ActionResult AddGame()
        {
            ViewBag.Title = Resources.Language.AddCompetition;
            return View("SaveGame", new Game());
        }

        public ActionResult EditGame(Int32 Id)
        {
            var game = entity.Games.Find(Id);
            ViewBag.Title = Resources.Language.Edit + " " + game.Name;
            return View("SaveGame", game);
        }

        [HttpPost]
        public ActionResult SaveGame(Game g)
        {
            try
            {
                ViewBag.Title = Resources.Language.Edit + " " + g.Name;
                var game = entity.Games.Find(g.Id) ?? new Game();
                game.Name = g.Name;
                game.BeginTime = g.BeginTime;
                game.EndTime = g.EndTime;
                game.GameLib = entity.Libs.Find(g.GameLib.Id);
                if (entity.Games.Find(g.Id) == null)
                {
                    entity.Games.Add(game);
                }
                entity.SaveChanges();
                return Content("<script>location.href='" + Url.Action("Game", "Admin") + "'</script>");
            }
            catch (Exception ex)
            {
                ViewBag.JS = "<script>alert('" + ex.Message + "')</script>";
                return View("SaveGame", g);
            }
        }

        public ActionResult Group(Int32? page)
        {
            var games = from x in entity.Groups orderby x.Id descending select x;
            var pageNumber = page ?? 1;
            var onePageOfProducts = games.ToPagedList(pageNumber, 25);
            return View(onePageOfProducts);
        }

        public ActionResult AddGroup()
        {
            ViewBag.Title = Resources.Language.AddGroup;
            ViewBag.New = "New";
            return View("SaveGroup", new Group());
        }

        public ActionResult EditGroup(Int32 Id)
        {
            var l = entity.Groups.Find(Id);
            ViewBag.Title = Resources.Language.Edit + " " + l.Name;
            return View("SaveGroup", l);
        }

        [HttpPost]
        public ActionResult SaveGroup(Group g)
        {
            try
            {
                ViewBag.Title = Resources.Language.Edit + " " + g.Name;
                var group = entity.Groups.Find(g.Id) ?? new Group();
                group.Name = g.Name;
                if ((g.isAdmin || group.isAdmin) && !User.IsInRole("SAdmin"))
                {
                    return Content("<script>location.href='" + Url.Action("ValidPermission", "Home", new { }) + "'</script>");
                }
                group.isAdmin = g.isAdmin;
                if (group.Users != null)
                {
                    group.Users.Clear();
                }
                foreach (var item in Request["user-set"].Split('#'))
                {
                    if (item != "" && item != null)
                    {
                        group.Users.Add(entity.Users.Find(Int32.Parse(item)));
                    }
                }
                if (entity.Groups.Find(g.Id) == null)
                {
                    entity.Groups.Add(group);
                }
                entity.SaveChanges();
                return Content("<script>location.href='" + Url.Action("Group", "Admin") + "'</script>");
            }
            catch (Exception ex)
            {
                if (Request["new_"] == "New")
                {
                    ViewBag.New = "New";
                }
                ViewBag.JS = "<script>alert('" + Resources.Language.InformationNotComplete + "')</script>";
                return View("SaveGroup", g);
            }
        }

        public ActionResult GroupOverView(Int32 Id, Int32? page)
        {
            var g = entity.Groups.Find(Id);
            var users = from x in g.Users orderby x.Id descending select x;
            var pageNumber = page ?? 1;
            var onePageOfProducts = users.ToPagedList(pageNumber, 25);
            ViewBag.URL = Url.Action("EditGroup", "Admin", new { Id = Id });
            ViewBag.Title = g.Name + " " + Resources.Language.Member;
            ViewBag.Id = Id;
            return View("UserList", onePageOfProducts);
        }

        public ActionResult UserList(Int32? page)
        {
            var users = from x in entity.Users orderby x.Id descending select x;
            var pageNumber = page ?? 1;
            var onePageOfProducts = users.ToPagedList(pageNumber, 25);
            ViewBag.URL = Url.Action("AddUser", "Admin");
            ViewBag.Title = Resources.Language.User;
            return View(onePageOfProducts);
        }

        public ActionResult AddUser()
        {
            ViewBag.Title = Resources.Language.AddUser;
            ViewBag.New = "New";
            return View("SaveUser", new User());
        }

        public ActionResult EditUser(Int32 Id)
        {
            var l = entity.Users.Find(Id);
            ViewBag.Title = Resources.Language.Edit + " " + l.Name;
            return View("SaveUser", l);
        }

        [HttpPost]
        public ActionResult SaveUser(User u)
        {
            try
            {
                ViewBag.Title = Resources.Language.Edit + " " + u.Name;
                var user = entity.Users.Find(u.Id) ?? new User();
                user.Name = u.Name;
                user.isLost = u.isLost;
                if (user.isLost)
                {
                    user.Verification = Guid.NewGuid().ToString();
                    ViewBag.JS = "<script>alert('" + user.Verification + "')</script>";
                }
                else
                {
                    user.Verification = null;
                }
                if (entity.Users.Find(u.Id) == null)
                {
                    user.Email = u.Email;
                    user.Password = BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(u.Password + "SICAUOJ"))).Replace("-", "");
                    user.CreateTime = DateTime.Now;
                    entity.Users.Add(user);
                }
                entity.SaveChanges();
                user.Password = "";
                return Content(ViewBag.JS + "<script>location.href='" + Url.Action("UserList", "Admin") + "'</script>");
            }
            catch (Exception ex)
            {
                if (Request["new_"] == "New")
                {
                    ViewBag.New = "New";
                }
                ViewBag.Title = Resources.Language.AddUser;
                ViewBag.JS = "<script>alert('" + Resources.Language.InformationNotComplete + "')</script>";
                return View("SaveUser", u);
            }
        }

        [HttpPost]
        public RedirectResult Quick()
        {
            String method = String.Empty;
            String controller = String.Empty;
            Int32 Id = 0;
            String RequestStr = Request["keyword"] ?? "";
            if (RequestStr == "")
            {
                return new RedirectResult(Url.Action("Dashboard", "Home"));
            }
            switch (RequestStr.Substring(0, 2))
            {
                case "#T":
                    method = "TaskDetail";
                    controller = "Admin";
                    Id = Int32.Parse(RequestStr.Substring(2));
                    break;
                case "#P":
                    method = "ProblemDetail";
                    controller = "Admin";
                    Id = Int32.Parse(RequestStr.Substring(2));
                    break;
                case "#U":
                    method = "UserDetail";
                    controller = "Admin";
                    Id = Int32.Parse(RequestStr.Substring(2));
                    break;
                case "#L":
                    method = "LibOverView";
                    controller = "Admin";
                    Id = Int32.Parse(RequestStr.Substring(2));
                    break;
                default:
                    method = "SearchWith";
                    controller = "Admin";
                    return new RedirectResult(Url.Action(method, controller, new { key = RequestStr }));
            }
            return new RedirectResult(Url.Action(method, controller, new { Id = Id }));
        }

        public ActionResult RefreshTask(Int32 Id)
        {
            try
            {
                var t = entity.Tasks.Find(Id);
                t.isPass = false;
                t.Owner = t.Owner;
                t.Issue = t.Issue;
                t.Runtime = null;
                t.Compiletime = null;
                t.Compiler = t.Compiler;
                t.status = "WC";
                entity.SaveChanges();
            }
            catch (Exception ex)
            {
                return Content("<script>alert(\"" + Resources.Language.ReJudgeRefused + ex.Message + "。\");location.href='" + Url.Action("TaskDetail", "Admin", new { Id = Id }) + "'</script>");
            }
            return Content("<script>alert('" + Resources.Language.Success + "');location.href='" + Url.Action("TaskDetail", "Admin", new { Id = Id }) + "'</script>");
        }

        public ActionResult SearchWith(String key, Int32? page)
        {
            ViewBag.Title = Resources.Language.Result + " - " + key;
            ViewBag.Key = key;
            var res = new List<Dictionary<String, String>>();
            var userres = from x in entity.Users where x.Name.Contains(key) || x.Email.Contains(key) select x;
            var tasks = from x in entity.Tasks where x.Answer.Contains(key) || x.Owner.Name.Contains(key) select x;
            var issues = from x in entity.Issues where x.Title.Contains(key) || x.Content.Contains(key) select x;
            foreach (var item in issues)
            {
                var dic = new Dictionary<String, String>();
                dic.Add("Name", Resources.Language.Question + ": #" + item.Id + " (" + item.Title + ")");
                String tempStr = Resources.Language.Libraries;
                foreach (var lib in item.Libs)
                {
                    tempStr += "<br />#" + lib.Id;
                }
                dic.Add("Detail", Resources.Language.Title + ": " + item.Title + "<br />" + Resources.Language.Summary + ": " + item.Content.Substring(0, (item.Content.Length > 500) ? 500 : item.Content.Length).Replace("\r\n", "").Replace("[title]", "").Replace("[/title]", "").Replace("[code]", "").Replace("[/code]", "").Replace("[math]", "").Replace("[/math]", "") + "<br />" + tempStr);
                dic.Add("Url", Url.Action("ProblemDetail", "Admin", new { item.Id }));
                res.Add(dic);
            }
            foreach (var item in userres)
            {
                var dic = new Dictionary<String, String>();
                dic.Add("Name", Resources.Language.User + ": #" + item.Id + " (" + item.Name + ")");
                dic.Add("Detail", "E-mai：" + item.Email + "<br />" + Resources.Language.UserName + ": " + item.Name);
                dic.Add("Url", Url.Action("UserDetail", "Admin", new { item.Id }));
                res.Add(dic);
            }
            foreach (var item in tasks)
            {
                var dic = new Dictionary<String, String>();
                dic.Add("Name", Resources.Language.Tasks + ": #" + item.Id);
                dic.Add("Detail", Resources.Language.User + ": #" + item.Owner.Id + "<br />" + Resources.Language.Status + ": " + item.status + "<br />" + Resources.Language.Question + ": #" + item.Issue.Id);
                dic.Add("Url", Url.Action("TaskDetail", "Admin", new { item.Id }));
                res.Add(dic);
            }
            var pageNumber = page ?? 1;
            var onePageOfProducts = res.ToPagedList(pageNumber, 25);
            return View(onePageOfProducts);
        }
    }
}