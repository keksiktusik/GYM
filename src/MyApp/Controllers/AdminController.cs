using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApp.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using MyApp.Data;
namespace MyApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;


        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _context = context;
        }
        // 🌸 PANEL ADMINA (dashboard)
        public IActionResult Index()
        {
            return View("AIndex");
        }

        // 📋 LISTA UŻYTKOWNIKÓW + FILTROWANIE + WYSZUKIWANIE
        public async Task<IActionResult> Users(string searchString, string roleFilter)
        {
            var users = _userManager.Users.ToList();

            if (!string.IsNullOrEmpty(searchString))
                users = users.Where(u => u.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrEmpty(roleFilter))
            {
                var filtered = new List<ApplicationUser>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains(roleFilter))
                        filtered.Add(user);
                }
                users = filtered;
            }

            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.Roles = allRoles;
            ViewBag.SelectedRole = roleFilter;
            ViewBag.Search = searchString;

            // ⭐ NAJWAŻNIEJSZE — przekazujemy role KAŻDEGO użytkownika
            var userRolesDict = new Dictionary<string, string>();
            foreach (var u in users)
            {
                var r = await _userManager.GetRolesAsync(u);
                userRolesDict[u.Id] = r.FirstOrDefault() ?? "Brak roli";
            }

            ViewBag.UserRoles = userRolesDict;

            return View(users);
        }

        // 🔍 SZCZEGÓŁY UŻYTKOWNIKA
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;

            return View(user);
        }

        // 🏷️ ZMIANA ROLI
        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, roleName);

            // 🔄 Jeśli zmieniamy rolę aktualnie zalogowanemu użytkownikowi:
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == user.Id)
            {
                await _signInManager.RefreshSignInAsync(user);
            }

            TempData["msg"] = $"Rola użytkownika {user.Email} została zmieniona na {roleName}.";
            return RedirectToAction("Users");
        }


        // ✅ BLOKOWANIE / ODBLOKOWANIE KONTA
        [HttpPost]
        public async Task<IActionResult> ToggleLock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                user.LockoutEnd = null;
                TempData["msg"] = $"🔓 Użytkownik {user.Email} został odblokowany.";
            }
            else
            {
                user.LockoutEnd = DateTime.UtcNow.AddYears(100);
                TempData["msg"] = $"🚫 Użytkownik {user.Email} został zablokowany.";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction("Users");
        }

        // 🔁 RESET HASŁA (wysyłka nowego hasła tymczasowego)
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            string tempPassword = "Temp123!" + DateTime.Now.Millisecond; // przykładowe hasło tymczasowe
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);

            if (result.Succeeded)
                TempData["msg"] = $"🔑 Hasło użytkownika {user.Email} zostało zresetowane. Nowe: {tempPassword}";
            else
                TempData["msg"] = $"❌ Błąd resetowania hasła: {string.Join(", ", result.Errors.Select(e => e.Description))}";

            return RedirectToAction("Users");
        }

        // 💌 WYSYŁKA MAILA DO UŻYTKOWNIKA
        [HttpPost]
        public async Task<IActionResult> SendMessage(string userId, string message)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["msg"] = "❌ Nie znaleziono użytkownika.";
                return RedirectToAction("Users");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                TempData["msg"] = "❌ Ten użytkownik nie ma przypisanego adresu e-mail.";
                return RedirectToAction("Users");
            }

            string subject = "📩 Wiadomość od zespołu GYM";
            string body = $"<p><strong>Zespół GYM przesyła Ci wiadomość:</strong></p><blockquote>{message}</blockquote><br/><p>Pozdrawiamy,<br/><strong>Zespół GYM</strong></p>";

            try
            {
                await _emailSender.SendEmailAsync(user.Email!, subject, body);
                TempData["msg"] = $"✅ Wiadomość e-mail została wysłana do {user.Email}.";
            }
            catch (Exception ex)
            {
                TempData["msg"] = $"❌ Błąd wysyłki wiadomości: {ex.Message}";
            }

            return RedirectToAction("Users");
        }


        // 📊 STATYSTYKI SYSTEMU
        public async Task<IActionResult> Stats()
        {
            var users = _userManager.Users.ToList();

            int total = users.Count;
            int confirmed = users.Count(u => u.EmailConfirmed);
            int locked = users.Count(u => u.LockoutEnd != null && u.LockoutEnd > DateTime.UtcNow);

            var roles = _roleManager.Roles.ToList();
            var roleStats = new Dictionary<string, int>();

            foreach (var role in roles)
            {
                var roleName = role.Name ?? "Brak roli";
                int count = 0;

                foreach (var u in users)
                {
                    var r = await _userManager.GetRolesAsync(u) ?? new List<string>();

                    if (!string.IsNullOrEmpty(roleName) && r.Contains(roleName))
                        count++;
                }

                roleStats[roleName] = count;
            }

            ViewBag.Total = total;
            ViewBag.Confirmed = confirmed;
            ViewBag.Locked = locked;
            ViewBag.RoleStats = roleStats;

            return View();
        }
        // 📊 RAPORTY MIESIĘCZNE
        public async Task<IActionResult> Reports(int? month, int? year)
        {
            var selectedMonth = month ?? DateTime.Now.Month;
            var selectedYear = year ?? DateTime.Now.Year;

            // --- 1) Liczba nowych użytkowników ---
            var users = _userManager.Users.ToList();
            int newUsers = users.Count(u =>
                u.EmailConfirmed &&
                u.CreatedAt.Month == selectedMonth &&
                u.CreatedAt.Year == selectedYear
            );

            // --- 2) Ilość treningów wykonanych przez wszystkich ---
            int allTrainings = _context.TrainingEvents
                .Count(t =>
                    t.Start.Month == selectedMonth &&
                    t.Start.Year == selectedYear
                );

            // --- 3) Najpopularniejsze kategorie zajęć ---
            var popularCategories = _context.TrainingEvents
                .Where(t => t.Start.Month == selectedMonth && t.Start.Year == selectedYear)
                .GroupBy(t => t.Title)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToList();

            ViewBag.Month = selectedMonth;
            ViewBag.Year = selectedYear;
            ViewBag.NewUsers = newUsers;
            ViewBag.AllTrainings = allTrainings;
            ViewBag.PopularCategories = popularCategories;

            return View();
        }


    }
}