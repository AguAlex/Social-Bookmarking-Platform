using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Social_Bookmarking_Platform.Data;
using Social_Bookmarking_Platform.Data.Migrations;
using Social_Bookmarking_Platform.Models;
using System.Net.NetworkInformation;

namespace Social_Bookmarking_Platform.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UsersController(
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
        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            var users = from user in db.Users
                        orderby user.UserName
                        select user;

            ViewBag.UsersList = users;
            ViewBag.currentUserId = _userManager.GetUserId(User);

            return View();
        }
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult> Show(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var user = await db.Users
                               .Include(u => u.Boards)
                               .FirstOrDefaultAsync(u => u.Id == id);

            if (currentUserId != id)
            {
                user.Boards = user.Boards.Where(b => !b.IsPrivate).ToList();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;

            ViewBag.UserCurent = await _userManager.GetUserAsync(User);
            ViewBag.currentUserId = currentUserId;
            ViewBag.UserBoards = user.Boards;

            return View(user);
        }



        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult> Edit(string id)
        {
            string currentUserId = _userManager.GetUserId(User);

            if (currentUserId != id && !User.IsInRole("Admin"))
            {
                TempData["message"] = "Nu aveti permisiunea de a edita acest utilizator.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Home");
            }

            ApplicationUser user = db.Users.Find(id);

            ViewBag.AllRoles = GetAllRoles();

            var roleNames = await _userManager.GetRolesAsync(user);
            ViewBag.UserRole = _roleManager.Roles
                                           .Where(r => roleNames.Contains(r.Name))
                                           .Select(r => r.Id)
                                           .FirstOrDefault();

            return View(user);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public async Task<ActionResult> Edit(string id, ApplicationUser newData, [FromForm] string newRole)
        {
            ApplicationUser user = db.Users.Find(id);

            user.AllRoles = GetAllRoles();


            if (ModelState.IsValid)
            {
                user.UserName = newData.UserName;
                user.Email = newData.Email;
                user.FirstName = newData.FirstName;
                user.LastName = newData.LastName;
                user.PhoneNumber = newData.PhoneNumber;
                
                // Cautam toate rolurile din baza de date
                var roles = db.Roles.ToList();

                foreach (var role in roles)
                {
                    // Scoatem userul din rolurile anterioare
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                // Adaugam noul rol selectat
                var roleName = await _roleManager.FindByIdAsync(newRole);
                await _userManager.AddToRoleAsync(user, roleName.ToString());

                db.SaveChanges();

            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Delete(string id)
        {
            var user = db.Users
                         .Include("Bookmarks")
                         .Include("Comments")
                         //.Include("Boards")
                         .Where(u => u.Id == id)
                         .First();

            // Delete user comments
            if (user.Comments.Count > 0)
            {
                foreach (var comment in user.Comments)
                {
                    db.Comments.Remove(comment);
                }
            }

            // Delete user bookmarks
            if (user.Bookmarks.Count > 0)
            {
                foreach (var bookmark in user.Bookmarks)
                {
                    db.Bookmarks.Remove(bookmark);
                }
            }
            
            // Delete user boards
            if (user.Boards.Count > 0)
            {
                foreach (var board in user.Boards)
                {
                    db.Boards.Remove(board);
                }
            }
            

            db.ApplicationUsers.Remove(user);

            db.SaveChanges();

            return RedirectToAction("Index");
        }
        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles
                        select role;

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }
    }
}
