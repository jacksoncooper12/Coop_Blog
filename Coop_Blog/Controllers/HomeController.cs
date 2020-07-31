using Coop_Blog.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Coop_Blog.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            var allBlogPosts = db.BlogPosts.Where(b => b.Published).ToList();
            return View(allBlogPosts);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            EmailModel model = new EmailModel();

            return View();
        }

        public ActionResult Confirmed()
        {
            return View();
        }

        public PartialViewResult _LoginPartial()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            return PartialView(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(EmailModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var body = "<p>New Message From: <bold>{0}<br><br></bold>Email: {1}</p><p>Message:</p><p>{2}</p>";
                    var from = "My Blog<example@email.com>";
                    var email = new MailMessage(from, ConfigurationManager.AppSettings["emailto"])
                    {
                        Subject = string.Format(model.Subject),
                        Body = string.Format(body, model.FromName, model.FromEmail, model.Body),
                        IsBodyHtml = true
                    };
                    var svc = new EmailService();
                    await svc.SendAsync(email);
                    return View("Confirmed");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.FromResult(0);
                }
            }
            return View(model);
        }
    }
}