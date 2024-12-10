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

            // Toti utilizatorii pot vedea Bookmark-urile existente in platforma
            // Fiecare utilizator vede bookmark-urile pe care le-a creat
            // Userii cu rolul de Admin pot sa vizualizeze toate bookmark-urile existente
            // HttpGet - implicit
<<<<<<< Updated upstream
            [Authorize(Roles = "User,Admin")]
=======
        [Authorize(Roles = "User,Admin")]
>>>>>>> Stashed changes
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

            if (User.IsInRole("User"))
            {
                var boards = db.Boards
                                  .Include("BookmarkBoards.Bookmark.Category")
                                  .Include("BookmarkBoards.Bookmark.User")
                .Include("User")
                                  .Where(b => b.Id == id)
                                  .Where(b => b.UserId == _userManager.GetUserId(User))
                                  .FirstOrDefault();

                if (boards == null)
                {
                    TempData["message"] = "Resursa cautata nu poate fi gasita";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Bookmarks");
                }

                return View(boards);
            }

            else
            if (User.IsInRole("Admin"))
            {
                var boards = db.Boards
                                  .Include("BookmarkBoards.Bookmark.Category")
                                  .Include("BookmarkBoards.Bookmark.User")
                                  .Include("User")
                                  .Where(b => b.Id == id)
                                  .FirstOrDefault();


                if (boards == null)
                {
                    TempData["message"] = "Resursa cautata nu poate fi gasita";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Bookmark");
                }


                return View(boards);
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Bookmark");
            }
        }

        // Randarea formularului in care se completeaza datele unui bookmark
        // [HttpGet] - se executa implicit
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
                TempData["message"] = "Colectia a fost adaugata";
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
