using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Social_Bookmarking_Platform.Data;
using Social_Bookmarking_Platform.Data.Migrations;
using Social_Bookmarking_Platform.Models;
using System.Diagnostics;

namespace Social_Bookmarking_Platform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<HomeController> logger

            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;

        }

        public IActionResult Index()
        {

            var bookmarks = db.Bookmarks
                 .Include("Category")
                 .Include("User");

            ViewBag.FirstArticle = bookmarks.First();
            ViewBag.Bookmarks = bookmarks;

            // MOTOR DE CAUTARE
            var search = "";
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere 
                // Cautare in bookmark (Title)
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
                ViewBag.PaginationBaseUrl = "/Home/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Home/Index/?page";
            }

            return View();
        }

        public IActionResult OrderByDate()
        {
            var bookmarks = db.Bookmarks.Include("Category")
                                        .Include("User")
                                        .OrderByDescending(o => o.Date);
            ViewBag.Bookmarks = bookmarks;

            return View("Index");
        }
        public IActionResult OrderByLikes()
        {
            var bookmarks = db.Bookmarks.Include("Category")
                                        .Include("User")
                                        .OrderByDescending(o => o.Likes);
            ViewBag.Bookmarks = bookmarks;

            return View("Index");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
