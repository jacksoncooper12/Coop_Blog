using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Coop_Blog.Helpers;
using Coop_Blog.Models;
using PagedList;
using PagedList.Mvc;

namespace Coop_Blog.Controllers
{
    [RequireHttps]
    public class BlogPostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BlogPosts
        public ActionResult Index(int? page, string searchStr)
        {
            int pageSize = 6; //specifies number of posts per page
            int pageNumber = (page ?? 1); // ?? is a null coalescing operator (if left of ?? is null, use right of ??)
            ViewBag.Search = searchStr;
            var blogList = IndexSearch(searchStr);
            return View(blogList.ToPagedList(pageNumber, pageSize));
        }
        public IQueryable<BlogPost> IndexSearch(string searchStr)
        {
            IQueryable<BlogPost> result = null;
            if(searchStr != null)
            {
                result = db.BlogPosts.AsQueryable();
                result = result.Where(p => p.Title.Contains(searchStr) || p.Body.Contains(searchStr) || p.Comments.Any(c => c.Body.Contains(searchStr) || c.Author.FirstName.Contains(searchStr) || c.Author.LastName.Contains(searchStr) || c.Author.DisplayName.Contains(searchStr) || c.Author.Email.Contains(searchStr)));
            }
            else
            {
                result = db.BlogPosts.AsQueryable();
            }
            if (User.IsInRole("Admin"))
            {
                return result.OrderByDescending(p => p.Created);
            }
            else
            {
                return result.Where(p => p.Published).OrderByDescending(p => p.Created);
            }
            
        }

        // GET: BlogPosts/Details/5
        public ActionResult Details(string Slug)
        {
            if (String.IsNullOrWhiteSpace(Slug))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //var blogPost = db.BlogPosts.FirstOrDefault(p => p.Slug == Slug);
            var detailsCurrent = new DetailsModel();
            detailsCurrent.Post = db.BlogPosts.FirstOrDefault(b=>b.Slug == Slug);
            var detailsPrev = new DetailsModel();
            detailsPrev.Post = db.BlogPosts.OrderByDescending(b => b.Created).Where(b => b.Published).ToList().SkipWhile(b => b.Created != detailsCurrent.Post.Created).Skip(1).FirstOrDefault();
            var detailsNext = new DetailsModel();
            detailsNext.Post = db.BlogPosts.OrderBy(b => b.Created).Where(b => b.Published).ToList().SkipWhile(b => b.Created != detailsCurrent.Post.Created).Skip(1).FirstOrDefault();
            if (detailsPrev.Post == null)
            {
                ViewBag.PreviousPost = "This is the First Post";
            }
            else
            {
                ViewBag.PreviousPost = detailsPrev.Post.Title;
            }
            if (detailsNext.Post == null)
            {
                ViewBag.NextPost = "This is the Latest Post";
            }
            else
            {
                ViewBag.NextPost = detailsNext.Post.Title ?? "No Newer Post";
            }

            var detailsModel = new DetailsModel();
            detailsModel.Post = db.BlogPosts.FirstOrDefault(p => p.Slug == Slug);
            detailsModel.BlogPosts = db.BlogPosts.OrderByDescending(p => p.Created).Where(b=>b.Published).ToList();
            if (detailsModel == null)
            {
                return HttpNotFound();
            }
            return View(detailsModel);
        }
        public ActionResult PreviousPost( bool prev, int id)
        {
            if (prev)
            {
                var detailsCurrent = new DetailsModel();
                detailsCurrent.Post = db.BlogPosts.Find(id);
                var detailsPrev = new DetailsModel();
                detailsPrev.Post = db.BlogPosts.OrderByDescending(b => b.Created).Where(b => b.Published).ToList().SkipWhile(b => b.Created != detailsCurrent.Post.Created).Skip(1).FirstOrDefault();
                //BlogPost currentPost = db.BlogPosts.Find(id);
                //BlogPost previousPost = db.BlogPosts.OrderByDescending(b => b.Created).ToList().SkipWhile(b => b.Created != currentPost.Created).Skip(1).FirstOrDefault();
                if (detailsPrev.Post == null)
                {
                    return RedirectToAction(detailsCurrent.Post.Slug, "Blog/Details");
                }
                return RedirectToAction(detailsPrev.Post.Slug, "Blog/Details");
            }
            else
            {
                var detailsCurrent = new DetailsModel();
                detailsCurrent.Post = db.BlogPosts.Find(id);
                var detailsNext = new DetailsModel();
                detailsNext.Post = db.BlogPosts.OrderBy(b => b.Created).Where(b => b.Published).ToList().SkipWhile(b => b.Created != detailsCurrent.Post.Created).Skip(1).FirstOrDefault();
                //BlogPost currentPost = db.BlogPosts.Find(id);
                //BlogPost nextPost = db.BlogPosts.OrderBy(b => b.Created).ToList().SkipWhile(b => b.Created != currentPost.Created).Skip(1).FirstOrDefault();
                if (detailsNext.Post == null)
                {
                    return RedirectToAction(detailsCurrent.Post.Slug, "Blog/Details");
                }
                return RedirectToAction(detailsNext.Post.Slug, "Blog/Details");
            }
        }

        // GET: BlogPosts/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Abstract,Body,Published,MediaPath")] BlogPost blogPost, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                var Slug = StringUtilities.URLFriendly(blogPost.Title);
                if (String.IsNullOrWhiteSpace(Slug))
                {
                    ModelState.AddModelError("Title", "Invalid title");
                    return View(blogPost);
                }
                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    var justFileName = Path.GetFileNameWithoutExtension(fileName);
                    justFileName = StringUtilities.URLFriendly(justFileName);
                    fileName = $"{justFileName}_{DateTime.Now.Ticks}{Path.GetExtension(fileName)}";
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    blogPost.MediaPath = "/Uploads/" + fileName;
                }
                if (db.BlogPosts.Any(p => p.Slug == Slug))
                {
                    ModelState.AddModelError("Title", "The title must be unique");
                    return View(blogPost);
                }

                blogPost.Slug = Slug;
                blogPost.Created = DateTime.Now;
                db.BlogPosts.Add(blogPost);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(blogPost);
        }

        // GET: BlogPosts/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.BlogPosts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Abstract,Body,Slug,Created,Published,MediaPath")] BlogPost blogPost, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                var slug = StringUtilities.URLFriendly(blogPost.Title);
                if (slug != blogPost.Slug)
                {
                    if (String.IsNullOrWhiteSpace(slug))
                    {
                        ModelState.AddModelError("Title", "Invalid title");
                        return View(blogPost);
                    }
                    if (db.BlogPosts.Any(p => p.Slug == slug))
                    {
                        ModelState.AddModelError("Title", "The title must be unique");
                        return View(blogPost);
                    }
                    blogPost.Slug = slug;
                }
                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    blogPost.MediaPath = "/Uploads/" + fileName;
                }

                blogPost.Updated = DateTime.Now;
                db.Entry(blogPost).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(blogPost);
        }

        // GET: BlogPosts/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id) 
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.BlogPosts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BlogPost blogPost = db.BlogPosts.Find(id);
            db.BlogPosts.Remove(blogPost);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
