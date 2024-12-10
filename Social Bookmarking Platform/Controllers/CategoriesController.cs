
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Social_Bookmarking_Platform.Data;
using Social_Bookmarking_Platform.Models;

namespace Social_Bookmarking_Platform.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CategoriesController(
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
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var categories = from category in db.Category
                             orderby category.Title
                             select category;
            ViewBag.Categories = categories;
            return View();
        }
        public ActionResult Show(int id)
        {
            Category category = db.Category.Find(id);
            return View(category);
        }
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(Category cat)
        {
            if (ModelState.IsValid)
            {
                db.Category.Add(cat);
                db.SaveChanges();
                TempData["message"] = "Categoria a fost adaugata";
                return RedirectToAction("Index");
            }

            else
            {
                return View(cat);
            }
        }
        public ActionResult Edit(int id)
        {
            Category category = db.Category.Find(id);
            return View(category);
        }

        [HttpPost]
        public ActionResult Edit(int id, Category requestCategory)
        {
            Category category = db.Category.Find(id);

            if (ModelState.IsValid)
            {
                category.Title = requestCategory.Title;
                db.SaveChanges();
                TempData["message"] = "Categoria a fost modificata!";
                return RedirectToAction("Index");
            }
            else
            {
                return View(requestCategory);
            }
        }


        [HttpPost]
        public ActionResult Delete(int id)
        {
            Category category = db.Category.Find(id);

            db.Category.Remove(category);

            TempData["message"] = "Categoria a fost stearsa";
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}