using System.Net.NetworkInformation;
using System.Security.Claims;
using Ganss.Xss;
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
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public BookmarksController(
        ApplicationDbContext context,
        IWebHostEnvironment env,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _env = env;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<IActionResult> New(Bookmark bookmark, IFormFile Image)
        {
            bookmark.Date = DateTime.Now;
            bookmark.UserId = _userManager.GetUserId(User);
            if (Image != null && Image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif",".mp4", ".mov" };
                var fileExtension = Path.GetExtension(Image.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("BookmarkImage", "Fisierul trebuie sa fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
                    return View(bookmark);
                }
                // Cale stocare
                var storagePath = Path.Combine(_env.WebRootPath, "images",
                Image.FileName);
                var databaseFileName = "/images/" + Image.FileName;
                // Salvare fisier
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(bookmark.Image));
                bookmark.Image = databaseFileName;
            }
            if (TryValidateModel(bookmark))
            {
                // Adăugare articol
                db.Bookmarks.Add(bookmark);
                await db.SaveChangesAsync();
                // Redirecționare după succes
                return RedirectToAction("Index", "Bookmarks");
            }
            bookmark.Categ = GetAllCategories();
            return View(bookmark);
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
                // fie in bookmark -> Title si Content
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
        // Adăugarea unui like pentru un bookmark
        [HttpPost]
        public async Task<IActionResult> Like(int bookmarkId)
        {
            var userId = _userManager.GetUserId(User);
            var user = await db.Users.FindAsync(userId);
            var bookmark = await db.Bookmarks.FindAsync(bookmarkId);

            if (user != null && bookmark != null)
            {
                // Verificam daca utilizatorul a dat deja like acestui bookmark
                var existingLike = await db.Likes
                    .FirstOrDefaultAsync(l => l.UserId == user.Id && l.BookmarkId == bookmark.Id);

                if (existingLike == null)
                {
                    var like = new Like
                    {
                        UserId = user.Id,
                        BookmarkId = bookmark.Id,
                        DateLiked = DateTime.Now
                    };
                    bookmark.LikesCnt++;
                    db.Likes.Add(like);
                    await db.SaveChangesAsync();
                }
                else
                {
                    bookmark.LikesCnt--;
                    db.Likes.Remove(existingLike);
                    await db.SaveChangesAsync();
                }
            }

            return RedirectToAction("Show", new { id = bookmarkId });
        }


        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            Bookmark bookmark = db.Bookmarks
                          .Include("Category")
                          .Include("Comments")
                          .Include("User")
                          .Include("Comments.User")
                          .FirstOrDefault(art => art.Id == id);

            var numberOfLikes = db.Likes
                         .Where(l => l.BookmarkId == id)
                         .Count();


            // Adaugam board-urile utilizatorului pentru dropdown
            ViewBag.UserBoards = db.Boards
                                      .Where(b => b.UserId == _userManager.GetUserId(User))
                                      .ToList();

            ViewBag.LikesCount = numberOfLikes;

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

            Bookmark bookmark = db.Bookmarks.Find(id);

            if (ModelState.IsValid)
            {
                if ((bookmark.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
                {
                    bookmark.Title = requestBookmark.Title;
                    bookmark.Date = DateTime.Now;
                    requestBookmark.Content = sanitizer.Sanitize(requestBookmark.Content);
                    bookmark.Content = requestBookmark.Content;

                    bookmark.CategoryId = requestBookmark.CategoryId;
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

            if (bookmark == null)
            {
                TempData["message"] = "Bookmark-ul nu a fost gasit";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var currentUserId = _userManager.GetUserId(User);
            if (bookmark.UserId == currentUserId || User.IsInRole("Admin"))
            {
                db.Comments.RemoveRange(bookmark.Comments); // Sterge comentariile asociate
                db.Bookmarks.Remove(bookmark);
                db.SaveChanges();

                TempData["message"] = "Bookmark-ul a fost sters cu succes";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti acest bookmark";
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
