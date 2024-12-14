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

            ViewBag.UserBoards = user.Boards;

            return View(user);
        }


        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(string id)
        {
            ApplicationUser user = db.Users.Find(id);

            ViewBag.AllRoles = GetAllRoles();

            var roleNames = await _userManager.GetRolesAsync(user);

            ViewBag.UserRole = _roleManager.Roles
                                              .Where(r => roleNames.Contains(r.Name))
                                              .Select(r => r.Id)
                                              .First();
            return View(user);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> Edit(string id, ApplicationUser newData, [FromForm] string newRole, IFormFile Image)
        {
            // Găsirea utilizatorului în baza de date
            ApplicationUser user = db.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            // Asigurăm lista de roluri
            user.AllRoles = GetAllRoles();

            if (ModelState.IsValid)
            {
                // Actualizăm informațiile de bază ale utilizatorului
                user.UserName = newData.UserName;
                user.Email = newData.Email;
                user.FirstName = newData.FirstName;
                user.LastName = newData.LastName;
                user.PhoneNumber = newData.PhoneNumber;

                // Gestiunea rolurilor utilizatorului
                var roles = db.Roles.ToList();
                foreach (var role in roles)
                {
                    // Eliminăm utilizatorul din rolurile anterioare
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }

                // Adăugăm noul rol selectat
                var roleName = await _roleManager.FindByIdAsync(newRole);
                if (roleName != null)
                {
                    await _userManager.AddToRoleAsync(user, roleName.Name);
                }

                // Gestionăm imaginea de profil dacă este încărcată
                if (Image != null && Image.Length > 0)
                {
                    // Verificăm extensia fișierului
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                    var fileExtension = Path.GetExtension(Image.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("UserProfileImage", "Fișierul trebuie să fie o imagine (jpg, jpeg, png, gif) sau un video (mp4, mov).");
                        return View(user);
                    }

                    // Cale stocare imagine
                    var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
                    var databaseFileName = "/images/" + Image.FileName;

                    // Salvăm fișierul pe server
                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(fileStream);
                    }

                    // Actualizăm calea imaginii în baza de date
                    user.ProfileImage = databaseFileName;
                }

                // Salvăm modificările în baza de date
                db.SaveChanges();
            }

            // Redirecționăm către acțiunea Show cu id-ul utilizatorului
            return RedirectToAction("Show", new { id });
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
