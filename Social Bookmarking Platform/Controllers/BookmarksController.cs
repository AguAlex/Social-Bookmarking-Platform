﻿using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Social_Bookmarking_Platform.Data;
using Social_Bookmarking_Platform.Models;

namespace Social_Bookmarking_Platform.Controllers
{
    public class BookmarksController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public BookmarksController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            var bookmarks = db.Bookmarks.Include("Category")
                                      .Include("User")
                                      .OrderByDescending(a => a.Date);

            ViewBag.Bookmarks = bookmarks;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View();
        }

        public IActionResult Show(int id)
        {
            Bookmark bookmark = db.Bookmarks.Include("Category")
                                         .Include("Comments")
                                         .Include("User")
                                         .Include("Comments.User")
                              .Where(art => art.Id == id)
                              .First();
            /*
            // Adaugam bookmark-urile utilizatorului pentru dropdown
            ViewBag.UserBookmarks = db.Bookmarks
                                      .Where(b => b.UserId == _userManager.GetUserId(User))
                                      .ToList();
            */
            SetAccessRights();
            

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(bookmark);
        }

        public IActionResult New()
        {
            Bookmark bookmark = new Bookmark();

            bookmark.Categ = GetAllCategories();

            return View(bookmark);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;

            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Bookmarks/Show/" + comment.BookmarkId);
            }
            else
            {
                Bookmark bk = db.Bookmarks.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(bk => bk.Id == comment.BookmarkId)
                                         .First();

                //return Redirect("/Articles/Show/" + comm.ArticleId);
                /*
                ViewBag.UserBookmarks = db.Bookmarks
                                          .Where(b => b.UserId == _userManager.GetUserId(User))
                                          .ToList();
                */
                SetAccessRights();

                return View(bk);
            }
        }

        [HttpPost]
        public IActionResult New(Bookmark bookmark)
        {
            var sanitizer = new HtmlSanitizer();

            bookmark.Date = DateTime.Now;

            bookmark.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                //bookmark.Content = sanitizer.Sanitize(bookmark.Content);

                db.Bookmarks.Add(bookmark);
                db.SaveChanges();
                TempData["message"] = "Bookmark-ul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                bookmark.Categ = GetAllCategories();
                return View(bookmark);
            }
        }

        public IActionResult Edit(int id)
        {

            Bookmark Bookmark = db.Bookmarks.Include("Category")
                                         .Where(bk => bk.Id == id)
                                         .First();

            Bookmark.Categ = GetAllCategories();

            if ((Bookmark.UserId == _userManager.GetUserId(User)) ||
                User.IsInRole("Admin"))
            {
                return View(Bookmark);
            }
            else
            {

                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui bookmark care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Bookmark requestBookmark)
        {
            var sanitizer = new HtmlSanitizer();

            Bookmark Bookmark = db.Bookmarks.Find(id);

            if (ModelState.IsValid)
            {
                if ((Bookmark.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
                {
                    Bookmark.Title = requestBookmark.Title;
                    Bookmark.Date = DateTime.Now;
                    Bookmark.CategoryId = requestBookmark.CategoryId;
                    TempData["message"] = "Bookmark-ul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui bookmark care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                requestBookmark.Categ = GetAllCategories();
                return View(requestBookmark);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            Bookmark bookmark = db.Bookmarks.Include("Comments")
                                         .Where(bk => bk.Id == id)
                                         .First();

            if ((bookmark.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                db.Bookmarks.Remove(bookmark);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost sters";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un articol care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();

            var categories = from cat in db.Categories
                             select cat;

            foreach (var category in categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Title
                });
            }
            return selectList;
        }
    }
}