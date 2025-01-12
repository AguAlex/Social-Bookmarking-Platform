using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Social_Bookmarking_Platform.Data;
using Social_Bookmarking_Platform.Models;

namespace Social_Bookmarking_Platform.Controllers
{

    [Authorize]
    public class BoardsController : Controller
    {
        
        
            private readonly ApplicationDbContext db;

            private readonly UserManager<ApplicationUser> _userManager;

            private readonly RoleManager<IdentityRole> _roleManager;

            public BoardsController(
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
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

            if (User.IsInRole("User"))
            {
                var boards = from board in db.Boards.Include("User")
                               .Where(b => b.UserId == _userManager.GetUserId(User))
                                select board;

                ViewBag.Boards = boards;

                return View();
            }
            else
            if (User.IsInRole("Admin"))
            {
                var boards = from board in db.Boards.Include("User")
                                select board;

                ViewBag.Boards = boards;

                return View();
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi asupra colectiei";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Bookmarks");
            }

        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            SetAccessRights();

            var board = db.Boards
                          .Include("BookmarkBoards.Bookmark.Category")
                          .Include("BookmarkBoards.Bookmark.User")
                          .Include("User")
                          .FirstOrDefault(b => b.Id == id);

            if (board == null)
            {
                TempData["message"] = "Resursa cautata nu poate fi gasita";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Bookmarks");
            }

            var currentUserId = _userManager.GetUserId(User);

            if (User.IsInRole("User"))
            {
                // Daca board-ul nu este privat sau utilizatorul este proprietarul
                if (!board.IsPrivate || board.UserId == currentUserId)
                {
                    return View(board);
                }
                else
                {
                    TempData["message"] = "Nu aveti drepturi pentru a accesa acest board";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Bookmarks");
                }
            }
            if (User.IsInRole("Admin"))
            {
                return View(board);
            }

            TempData["message"] = "Nu aveti drepturi";
            TempData["messageType"] = "alert-danger";
            return RedirectToAction("Index", "Bookmarks");
        }


        // Randarea formularului in care se completeaza datele unui bookmark
        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            return View();
        }

        // Adaugarea bookmark-ului in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult New(Board bd)
        {
            bd.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Boards.Add(bd);
                db.SaveChanges();
                TempData["message"] = "Board-ul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                return View(bd);
            }
        }


        // Conditiile de afisare a butoanelor de editare si stergere
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }
    }
}
