using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApp.Models;
using MyApp.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Linq;

namespace MyApp.Controllers
{
    [Authorize]
    public class TrainingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public TrainingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // 📅 Widok kalendarza
        public IActionResult Calendar()
        {
            return View();
        }

        // 📌 Pobranie wydarzeń użytkownika (również info o cykliczności)
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
                    end = e.End,
                    isRecurring = e.IsRecurring
                })
                .ToList();

            return Json(events);
        }

       // ➕ Dodanie wydarzenia (pojedynczego lub cyklicznego)
[HttpPost]
[Route("Training/AddEvent")]
public async Task<IActionResult> AddEvent([FromBody] TrainingEvent training)
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
        return Unauthorized(new { success = false, error = "Użytkownik niezalogowany." });

    if (training == null)
        return BadRequest(new { success = false, error = "Brak danych wydarzenia." });

    training.UserId = user.Id!;
    training.Title ??= "Trening";

    // ===============================
    // 1️⃣ ZWYKŁY TRENING (NIECykliczny)
    // ===============================
    if (!training.IsRecurring)
    {
        try
        {
            _context.TrainingEvents.Add(training);
            await _context.SaveChangesAsync();

            await SendImmediateEmail(user, training);

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    // ===============================
    // 2️⃣ CYKLICZNY TRENING
    // ===============================
   // 🔧 Naprawa: upewniamy się że dane cyklu nie są nullem
if (training.IsRecurring)
{
    if (string.IsNullOrWhiteSpace(training.RecurrenceInterval))
        training.RecurrenceInterval = Request.Form["RecurrenceInterval"];

    if (string.IsNullOrWhiteSpace(training.RecurrenceDays))
        training.RecurrenceDays = Request.Form["RecurrenceDays"];

    if (training.RecurrenceEndDate == null && !string.IsNullOrWhiteSpace(Request.Form["RecurrenceEndDate"]))
        training.RecurrenceEndDate = DateTime.Parse(Request.Form["RecurrenceEndDate"]);
}

  
   if (training.RecurrenceEndDate == null)
        return BadRequest(new { success = false, error = "Brak daty zakończenia cyklu." });

    var groupId = Guid.NewGuid();
    training.RecurrenceGroupId = groupId;
    training.IsRecurring = true;

    // zapisujemy bazowy opis cyklu
    _context.TrainingEvents.Add(training);

    DateTime date = training.Start;
    DateTime end = training.RecurrenceEndDate.Value;

    List<TrainingEvent> generatedEvents = new();

    while (date <= end)
    {
        if (training.RecurrenceInterval == "Weekly")
        {
            var days = training.RecurrenceDays?.Split(',') ?? Array.Empty<string>();
            var weekdayCode = date.DayOfWeek.ToString().Substring(0, 3);

            if (!days.Contains(weekdayCode))
            {
                date = date.AddDays(1);
                continue;
            }
        }

        generatedEvents.Add(new TrainingEvent
        {
            UserId = user.Id!,
            Title = training.Title,
            Description = training.Description,
            Category = training.Category,
            Start = date,
            End = date.Add(training.End - training.Start),
            IsRecurring = false,
            RecurrenceGroupId = groupId
        });

        date = training.RecurrenceInterval switch
        {
            "Daily" => date.AddDays(1),
            "Weekly" => date.AddDays(1),
            "Monthly" => date.AddMonths(1),
            _ => date.AddDays(1)
        };
    }

    try
    {
        _context.TrainingEvents.AddRange(generatedEvents);
        await _context.SaveChangesAsync();

        await SendImmediateEmail(user, training);

        return Ok(new { success = true });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}

        // ❌ Usunięcie pojedynczego wydarzenia
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

        // 💌 Wysyłka maila po utworzeniu treningu
        private async Task SendImmediateEmail(ApplicationUser user, TrainingEvent training)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                return;

            var startLocal = training.Start.ToLocalTime();
            string subject = "📆 Zaplanowano trening – GYM";

            string body = $@"
                <p>Cześć!</p>
                <p>Zaplanowano nowy <strong>{training.Title}</strong>.</p>
                <p>
                    <strong>Data:</strong> {startLocal:dd.MM.yyyy} <br/>
                    <strong>Godzina:</strong> {startLocal:HH\\:mm}
                </p>
                <br/>
                <p>Powodzenia! 💪<br/>Zespół GYM</p>
            ";

            await _emailSender.SendEmailAsync(user.Email!, subject, body);
        }

        // 🔁 Lista cyklicznych planów użytkownika
        [HttpGet]
        public async Task<IActionResult> GetRecurringPlans()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var recurring = _context.TrainingEvents
                .Where(e => e.UserId == user.Id && e.IsRecurring)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.RecurrenceInterval,
                    e.RecurrenceDays,
                    e.RecurrenceEndDate,
                    e.RecurrenceGroupId
                })
                .ToList();

            return Json(recurring);
        }

        // ❌ Usuwanie całego cyklu
        [HttpPost]
        public async Task<IActionResult> DeleteRecurrence([FromBody] Guid groupId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var events = _context.TrainingEvents
                .Where(e => e.UserId == user.Id && e.RecurrenceGroupId == groupId)
                .ToList();

            _context.TrainingEvents.RemoveRange(events);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // ✏ Edytowanie cyklu
        [HttpPost]
        public async Task<IActionResult> EditRecurrence([FromBody] TrainingEvent updated)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var original = _context.TrainingEvents
                .FirstOrDefault(e => e.UserId == user.Id &&
                                     e.Id == updated.Id &&
                                     e.IsRecurring);

            if (original == null)
                return NotFound();

            original.Title = updated.Title;
            original.Description = updated.Description;
            original.RecurrenceInterval = updated.RecurrenceInterval;
            original.RecurrenceDays = updated.RecurrenceDays;
            original.RecurrenceEndDate = updated.RecurrenceEndDate;

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        [HttpPost]
public async Task<IActionResult> UpdateEvent([FromBody] TrainingEvent updated)
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return Unauthorized();

    var existing = _context.TrainingEvents
        .FirstOrDefault(e => e.Id == updated.Id && e.UserId == user.Id);

    if (existing == null)
        return NotFound();

    existing.Title = updated.Title;
    existing.Description = updated.Description;
    existing.Start = updated.Start;
    existing.End = updated.End;

    await _context.SaveChangesAsync();
    return Ok(new { success = true });
}
[HttpGet]
public async Task<IActionResult> GetNextTraining()
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return Unauthorized();

    var now = DateTime.UtcNow;

    var next = _context.TrainingEvents
        .Where(e => e.UserId == user.Id && e.Start > now)
        .OrderBy(e => e.Start)
        .FirstOrDefault();

    if (next == null)
        return Json(new { exists = false });

    return Json(new
    {
        exists = true,
        title = next.Title,
        start = next.Start
    });
}
[HttpGet]
public async Task<IActionResult> GetMonthlyCount(int year, int month)
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return Unauthorized();

    var start = new DateTime(year, month, 1);
    var end = start.AddMonths(1);

    int count = _context.TrainingEvents
        .Where(e => e.UserId == user.Id && e.Start >= start && e.Start < end)
        .Count();

    return Json(new { count });
}


    }
}
