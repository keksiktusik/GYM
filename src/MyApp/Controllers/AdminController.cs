using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApp.Data;
using MyApp.Models;
using System.Linq;

namespace MyApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // 🏠 Panel główny
        public IActionResult Index()
        {
            return View();
        }

        // 👥 Lista użytkowników
        public IActionResult Users()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // 💪 Lista zajęć
        public IActionResult Classes()
        {
            var zajecia = _context.TypyZajec.ToList();
            return View(zajecia);
        }

        // ➕ Dodawanie zajęć
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddClass(TypZajec model)
        {
            if (ModelState.IsValid)
            {
                model.DataUtworzenia = DateTime.Now;
                _context.TypyZajec.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Classes");
            }

            // Jeśli coś poszło nie tak – wróć do widoku zajęć
            var zajecia = _context.TypyZajec.ToList();
            return View("Classes", zajecia);
        }

        // ✏️ Edycja (GET)
        [HttpGet]
        public IActionResult EditClass(int id)
        {
            var zajecie = _context.TypyZajec.FirstOrDefault(z => z.Id == id);
            if (zajecie == null)
                return NotFound();

            return View(zajecie); // Widok: Views/Admin/EditClass.cshtml
        }

        // ✏️ Edycja (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditClass(int id, TypZajec updated)
        {
            if (id != updated.Id)
                return BadRequest();

            var zajecie = _context.TypyZajec.FirstOrDefault(z => z.Id == id);
            if (zajecie == null)
                return NotFound();

            zajecie.Nazwa = updated.Nazwa;
            zajecie.Opis = updated.Opis;
            zajecie.Cena = updated.Cena;
            zajecie.CzasTrwania = updated.CzasTrwania;
            zajecie.PoziomTrudnosci = updated.PoziomTrudnosci;

            _context.SaveChanges();
            return RedirectToAction("Classes");
        }

        // ❌ Usuwanie zajęć
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteClass(int id)
        {
            var zajecie = _context.TypyZajec.FirstOrDefault(z => z.Id == id);
            if (zajecie == null)
                return NotFound();

            _context.TypyZajec.Remove(zajecie);
            _context.SaveChanges();
            return RedirectToAction("Classes");
        }
    }
}
