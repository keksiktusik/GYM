using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApp.Models;
using MyApp.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
namespace MyApp.Controllers
{
    [Authorize]
    public class TrainingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public TrainingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // 📅 Kalendarz widoku
        public IActionResult Calendar()
        {
            return View();
        }

        // 📌 Pobranie wydarzeń użytkownika
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var events = _context.TrainingEvents
                .Where(e => e.UserId == user.Id)
                .Select(e => new 
                { 
                    e.Id, 
                    e.Title, 
                    start = e.Start, 
                    end = e.End 
                })
                .ToList();

            return Json(events);
        }

        // ➕ Dodanie wydarzenia
        [HttpPost]
public async Task<IActionResult> AddEvent([FromBody] TrainingEvent training)
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
        return Unauthorized();

    if (training == null)
        return BadRequest(new { success = false, error = "Brak danych wydarzenia." });

    // zabezpieczenie przed pustymi wartościami
    training.Title ??= "Trening";
    training.UserId = user.Id!;

    _context.TrainingEvents.Add(training);
    await _context.SaveChangesAsync();

    // 💌 POWIADOMIENIE E-MAIL O ZAPLANOWANYM TRENINGU
    if (!string.IsNullOrWhiteSpace(user.Email))
    {
        var startLocal = training.Start.ToLocalTime(); // jeśli przechowujesz w UTC
        string subject = "📅 Przypomnienie o zaplanowanym treningu – GYM";
        string body = $@"
            <p>Cześć,</p>
            <p>Twój trening <strong>{training.Title}</strong> został zaplanowany.</p>
            <p><strong>Data:</strong> {startLocal:dd.MM.yyyy}<br/>
               <strong>Godzina:</strong> {startLocal:HH\\:mm}</p>
            <p>Jeśli potrzebujesz, możesz edytować lub usunąć trening bezpośrednio w kalendarzu w aplikacji GYM.</p>
            <br/>
            <p>Pozdrawiamy,<br/><strong>Zespół GYM</strong></p>
        ";

        try
        {
            await _emailSender.SendEmailAsync(user.Email!, subject, body);
        }
        catch (Exception ex)
        {
            // opcjonalnie: logowanie błędu, na razie bez wywalania aplikacji
            Console.WriteLine($"Błąd wysyłki maila powiadomienia: {ex.Message}");
        }
    }

    return Ok(new { success = true });
}


        // ❌ Usunięcie wydarzenia
        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
                return Unauthorized();

            var training = _context.TrainingEvents
                .FirstOrDefault(e => e.Id == id && e.UserId == user.Id);

            if (training == null)
                return NotFound(new { success = false, error = "Nie znaleziono wydarzenia." });

            _context.TrainingEvents.Remove(training);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }
}
