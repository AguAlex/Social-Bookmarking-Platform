using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Social_Bookmarking_Platform.Data;
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

            var bookmarks = from bookmark in db.Bookmarks
                            select bookmark;

            ViewBag.FirstArticle = bookmarks.First();
            ViewBag.Bookmarks = bookmarks;

            return View();
        }

        public IActionResult OrderByDate()
        {
            var bookmarks = db.Bookmarks.OrderBy(o => o.Title);
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
