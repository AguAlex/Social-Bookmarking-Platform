﻿using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Social_Bookmarking_Platform.Data;
using Social_Bookmarking_Platform.Data.Migrations;
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
        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var bookmarks = db.Bookmarks
                 .Include("Category")
                 .Include("User")
                 .Where(a => a.UserId == userId)
                 .OrderByDescending(a => a.Date);

            ViewBag.Bookmarks = bookmarks;



            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            // MOTOR DE CAUTARE
            var search = "";
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere 
                // Cautare in articol (Title)
                List<int> bookmarkIds = db.Bookmarks.Where
                                        (
                                         bk => bk.Title.Contains(search)
                                         //|| bk.Content.Contains(search)
                                        ).Select(a => a.Id).ToList();
                
                // Cautare in comentarii (Content)
                List<int> bookmarkIdsOfCommentsWithSearchString = db.Comments
                                        .Where
                                        (
                                         c => c.Content.Contains(search)
                                        ).Select(c => (int)c.BookmarkId).ToList();
                

                // Se formeaza o singura lista formata din toate id-urile selectate anterior
                List<int> mergedIds = bookmarkIds.Union(bookmarkIdsOfCommentsWithSearchString).ToList();


                // Lista bookmark-urilor care contin cuvantul cautat
                // fie in articol -> Title si Content
                // fie in comentarii -> Content
                bookmarks = db.Bookmarks.Where(bookmark => mergedIds.Contains(bookmark.Id))
                                      .Include("Category")
                                      .Include("User")
                                      .OrderByDescending(a => a.Date);

            }

            ViewBag.SearchString = search;

            // AFISARE PAGINATA

            // Alegem sa afisam 3 articole pe pagina
            int _perPage = 6;

            // Fiind un numar variabil de bookmarks, verificam de fiecare data utilizand 
            // metoda Count()

            int totalItems = bookmarks.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3 
            // Asadar offsetul este egal cu numarul de bookmarks care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedBookmarks = bookmarks.Skip(offset).Take(_perPage);

            // Preluam numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem bookmarks cu ajutorul unui ViewBag catre View-ul corespunzator
            ViewBag.Bookmarks = paginatedBookmarks;


            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Bookmarks/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Bookmarks/Index/?page";
            }

            return View();
        }

        [HttpPost]
        public IActionResult IncrementLike(int bookmarkId)
        {
            var bookmark = db.Bookmarks.Where(bk => bk.Id == bookmarkId).FirstOrDefault();

            if (bookmark == null)
            {
                TempData["message"] = "Bookmark not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            // Verifică dacă like-ul este mai mare de 0
            if (bookmark.Likes == null)
            {
                bookmark.Likes = 0;  // Setează la 0 dacă este null
            }

            // Alternarea între incrementare și decrementare
            if (bookmark.Likes > 0)
            {
                bookmark.Likes--;  // Scade un like
            }
            else
            {
                bookmark.Likes++;  // Dacă likes sunt 0, le crește
            }

            db.SaveChanges(); // Salvează schimbările

            // După modificare, redirecționează înapoi la pagina Show
            return RedirectToAction("Show", new { id = bookmarkId });
        }



        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            Bookmark bookmark = db.Bookmarks.Include("Category")
                                         .Include("Comments")
                                         .Include("User")
                                         .Include("Comments.User")
                              .Where(art => art.Id == id)
                              .First();
            
            // Adaugam board-urile utilizatorului pentru dropdown
            ViewBag.UserBoards = db.Boards
                                      .Where(b => b.UserId == _userManager.GetUserId(User))
                                      .ToList();
          
            SetAccessRights();
            

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(bookmark);
        }

        [Authorize(Roles = "User,Admin")]
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
                
                ViewBag.UserBoards = db.Boards
                                          .Where(b => b.UserId == _userManager.GetUserId(User))
                                          .ToList();
                
                SetAccessRights();

                return View(bk);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
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

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult AddBoard([FromForm] BookmarkBoard bookmarkBoard)
        {
            if (ModelState.IsValid)
            {
                // Verificam daca avem deja bookmark-ul in colectie
                if (db.BookmarkBoards
                    .Where(ab => ab.BookmarkId == bookmarkBoard.BookmarkId)
                    .Where(ab => ab.BoardId == bookmarkBoard.BoardId)
                    .Count() > 0)
                {
                    TempData["message"] = "Acest bookmark este deja adaugat in colectie";
                    TempData["messageType"] = "alert-danger";
                }
                else
                {
                    db.BookmarkBoards.Add(bookmarkBoard);

                    db.SaveChanges();

                    TempData["message"] = "Bookmark-ul a fost adaugat in colectia selectata";
                    TempData["messageType"] = "alert-success";
                }
            }
            else
            {
                TempData["message"] = "Nu s-a putut adauga bookmark-ul in colectie";
                TempData["messageType"] = "alert-danger";
            }

            return Redirect("/Bookmarks/Show/" + bookmarkBoard.BookmarkId);
        }
        [Authorize(Roles = "User,Admin")]
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
