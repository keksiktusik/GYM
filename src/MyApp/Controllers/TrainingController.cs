using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApp.Models;
using MyApp.Data;

namespace MyApp.Controllers
{
    [Authorize]
    public class TrainingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TrainingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 📅 Kalendarz
        public IActionResult Calendar()
        {
            return View();
        }

        // 🔹 Pobranie wydarzeń użytkownika (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var user = await _userManager.GetUserAsync(User);
            var events = _context.TrainingEvents
                .Where(e => e.UserId == user.Id)
                .Select(e => new { e.Id, e.Title, start = e.Start, end = e.End })
                .ToList();

            return Json(events);
        }

        // 🔹 Dodanie nowego wydarzenia
        [HttpPost]
        public async Task<IActionResult> AddEvent([FromBody] TrainingEvent training)
        {
            var user = await _userManager.GetUserAsync(User);
            training.UserId = user.Id;

            _context.TrainingEvents.Add(training);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // 🔹 Usunięcie wydarzenia
        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var training = _context.TrainingEvents.FirstOrDefault(e => e.Id == id && e.UserId == user.Id);
            if (training == null) return NotFound();

            _context.TrainingEvents.Remove(training);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
