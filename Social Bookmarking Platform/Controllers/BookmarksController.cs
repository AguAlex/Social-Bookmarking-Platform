using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            return View();
        }
    }
}
