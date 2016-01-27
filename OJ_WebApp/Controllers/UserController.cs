using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OJ_WebApp.Models;
using System.Web.Security;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Configuration;
using OJ_WebApp.Service;

namespace OJ_WebApp.Controllers
{
    [AllowAnonymous]
    public class UserController : Controller
    {
        OJ_WebAppContext entity = new OJ_WebAppContext();

        public ActionResult GenMail(String email, String code, String IP, bool isReg, string lang)
        {
            ViewBag.Email = email;
            ViewBag.Code = code;
            ViewBag.IP = IP;
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);
            return View(isReg ? "RegMail" : "FindPasswordMail");
        }

        [HttpGet]
        public ActionResult Reg(String verify, String email)
        {
            var databasemail = entity.Mails.Find(verify);
            if (databasemail == null || email != databasemail.Email || databasemail.CreateTime.AddMinutes(30) < DateTime.Now || entity.Users.Where(x => x.Email == email).Count() != 0)
            {
                return Content("<script>alert('" + Resources.Language.RequestRefused + "');location.href='/';</script>");
            }
            ViewBag.Verify = verify;
            User u = new User();
            u.Email = databasemail.Email;
            return View("Register", u);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reg(Mail mail)
        {
            mail.Id = String.Join("", Guid.NewGuid().ToString().Split('-'));
            mail.IPAdd = Request.UserHostAddress;
            try
            {
                var now = DateTime.Now;
                var before10min = now.AddMinutes(-10);
                var before12h = now.AddHours(-12);
                if ((from x in entity.Users where x.Email == mail.Email select x).Count() != 0)
                {
                    return Content("<script>alert('" + Resources.Language.EmailTaken + "');location.href='" + Url.Action("Register", "User") + "';</script>");
                }
                else if ((from x in entity.Mails where x.Email == mail.Email && x.CreateTime < now && x.CreateTime > before10min select x).Count() > 1)
                {
                    return Content("<script>alert('" + Resources.Language.RequestTooMuch + "');location.href='" + Url.Action("Register", "User") + "';</script>");
                }
                else if ((from x in entity.Mails where x.IPAdd == mail.IPAdd && x.CreateTime < now && x.CreateTime > before12h select x).Count() > 4)
                {
                    return Content("<script>alert('" + Resources.Language.RequestTooMuch + "');location.href='" + Url.Action("Register", "User") + "';</script>");
                }
                entity.Mails.Add(mail);
                entity.SaveChanges();
                SendMail(mail.Id, true, System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                return Content("<script>alert('" + Resources.Language.Success + "');location.href='/';</script>");
            }
            catch (Exception ex)
            {
                mail.Id = "";
                return View("New", mail);
            }
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View("New", new Mail());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User u)
        {
            if (entity.Mails.Find(Request["very_"])?.Email != u.Email)
            {
                return Content("<script>alert('" + Resources.Language.RequestRefused + "');location.href='/';</script>");
            }
            if (u.Password != Request["re-password"])
            {
                return Content("<script>alert('" + Resources.Language.InformationNotMatch + "');location.href='/'</script>");
            }
            if (u.Password.Length < 6)
            {
                return Content("<script>alert('" + Resources.Language.PasswordTooShort + "');location.href='/'</script>");
            }
            u.Password = BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(u.Password + "SICAUOJ"))).Replace("-", "");
            User user = new Models.User();
            user.Email = u.Email;
            user.Name = u.Name;
            user.Password = u.Password;
            user.CreateTime = DateTime.Now;
            entity.Users.Add(user);
            try
            {
                entity.SaveChanges();
                return Content("<script>alert('" + Resources.Language.Success + "');window.location.href='/User/Login'</script>");
            }
            catch (Exception ex)
            {
                u.Password = "";
                return View(u);
            }
        }

        public ActionResult Logoff()
        {
            FormsAuthentication.SignOut();
            return Content("<script>window.location.href='/User/Login'</script>");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User u)
        {
            u.Password = BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(u.Password + "SICAUOJ"))).Replace("-", "");
            var logined = (from x in entity.Users where x.Email == u.Email && x.Password == u.Password && !x.isLost select x).ToList();
            if (logined.Count() == 1)
            {
                String role = "User";
                if ((from x in logined.First().Groups where x.isAdmin select x).Count() > 0)
                {
                    role = "Admin";
                }
                var eftempTime = logined.FirstOrDefault().CreateTime;
                if (entity.Users.Where(x => x.CreateTime < eftempTime).Count() == 0)
                {
                    role = "SAdmin";
                }
                FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                    1,
                    logined.First().Id.ToString(),
                    DateTime.Now,
                    DateTime.Now.AddMinutes(120),
                    false,
                    role
                   );
                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                System.Web.HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);
                return Content("<script>location.href = '/Home/Dashboard/'</script>");
            }
            u.Password = "";
            ViewBag.JS = "<script>alert('" + Resources.Language.AccountFrozen + "')</script>";
            return View(u);
        }

        public ActionResult My()
        {
            var Logined = entity.Users.Find(Int32.Parse(User.Identity.Name));
            Logined.Password = "";
            ViewBag.Self = "YES";
            return View("SaveUser", Logined);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Find(Mail mail)
        {
            mail.Id = String.Join("", Guid.NewGuid().ToString().Split('-'));
            mail.IPAdd = Request.UserHostAddress;
            try
            {
                var now = DateTime.Now;
                var before10min = now.AddMinutes(-10);
                var before12h = now.AddHours(-12);
                if ((from x in entity.Users where x.Email == mail.Email select x).Count() == 0)
                {
                    return Content("<script>alert('" + Resources.Language.Done + "');location.href='/';</script>");
                }
                else if ((from x in entity.Mails where x.Email == mail.Email && x.CreateTime < now && x.CreateTime > before10min select x).Count() > 2)
                {
                    return Content("<script>alert('" + Resources.Language.RequestTooMuch + "');location.href='" + Url.Action("Register", "User") + "';</script>");
                }
                else if ((from x in entity.Mails where x.IPAdd == mail.IPAdd && x.CreateTime < now && x.CreateTime > before12h select x).Count() > 6)
                {
                    return Content("<script>alert('" + Resources.Language.RequestTooMuch + "');location.href='" + Url.Action("Register", "User") + "';</script>");
                }
                entity.Users.Where(x => x.Email == mail.Email).First().Verification = mail.Id;
                entity.Mails.Add(mail);
                entity.SaveChanges();
                SendMail(mail.Id, false, System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                return Content("<script>alert('" + Resources.Language.Success + "');location.href='/';</script>");
            }
            catch (Exception ex)
            {
                mail.Id = "";
                return View(mail);
            }
        }

        public ActionResult ResetPassword()
        {
            return View("Find", new Mail());
        }

        public ActionResult Find()
        {
            return View("FindPassword", new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(User u)
        {
            var us = (from x in entity.Users where x.Verification == u.Verification && x.Email == u.Email select x).FirstOrDefault();
            if (us != null)
            {
                if (us.Password.Length < 6)
                {
                    ViewBag.JS = "<script>alert('" + Resources.Language.PasswordTooShort + "')</script>";
                    u.Password = "";
                    return View("FindPassword", u);
                }
                var ru = entity.Users.Find(us.Id);
                ru.Password = BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(u.Password + "SICAUOJ"))).Replace("-", "");
                ru.isLost = false;
                ru.Verification = null;
                entity.SaveChanges();
                return Content("<script>alert('" + Resources.Language.Success + "');location.href='" + Url.Action("Login", "User") + "'</script>");
            }
            else
            {
                ViewBag.JS = "<script>alert('" + Resources.Language.InformationNotMatch + "')</script>";
                return View("FindPassword", u);
            }
        }

        [HttpPost]
        public ActionResult SaveUser(User u)
        {
            u.Password = BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(u.Password + "SICAUOJ"))).Replace("-", "");
            if ((from x in entity.Users where x.Id == u.Id && x.Password == u.Password select x).Count() == 1)
            {
                try
                {
                    ViewBag.Title = Resources.Language.Edit + " " + u.Name;
                    var user = entity.Users.Find(u.Id);
                    user.Name = u.Name;
                    user.Compiler = entity.Compilers.Find(u.Compiler.Id);
                    var newpass = Request["newPassword"] ?? "";
                    if (newpass != "")
                    {
                        user.Password = BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(newpass + "SICAUOJ"))).Replace("-", "");
                    }
                    entity.SaveChanges();
                    return Content("<script>alert('" + Resources.Language.Success + "');location.href='/'</script>");
                }
                catch (Exception ex)
                {
                    ViewBag.JS = "<script>alert('" + ex.Message + "')</script>";
                    return View("SaveUser", u);
                }
            }
            else
            {
                return Content("<script>alert('" + Resources.Language.WrongPassword + "');location.href='/User/Logoff'</script>");
            }
        }

        private Task<bool> SendMail(String id, bool isRegRequest, String lang)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                var mailconfig = (EmailConfigurationProvider)ConfigurationManager.GetSection("EmailConfigurationProvider");
                try
                {
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);
                    var mail = entity.Mails.Find(id);
                    MailAddress from = new MailAddress(mailconfig.Account, mailconfig.Name);
                    MailAddress to = new MailAddress(mail.Email, mail.Email);
                    MailMessage message = new MailMessage(from, to);
                    message.Subject = "SICAU OJ " + (isRegRequest ? Resources.Language.Register : Resources.Language.FindPassword);
                    message.IsBodyHtml = true;
                    message.Body = Encoding.UTF8.GetString((new System.Net.WebClient()).DownloadData("http://" + Request.Url.Host.ToString() + ":" + Request.Url.Port + "/User/GenMail?email=" + mail.Email + "&code=" + mail.Id + "&IP=" + mail.IPAdd + "&isReg=" + isRegRequest.ToString() + "&lang=" + lang));
                    SmtpClient client = new SmtpClient(mailconfig.Server, mailconfig.Port);
                    client.EnableSsl = mailconfig.IsSSL;
                    client.Credentials = new System.Net.NetworkCredential(mailconfig.Account, mailconfig.Password);
                    client.Send(message);
                }
                catch (Exception ex)
                {
                    ;
                }
                return true;
            });
        }
    }
}