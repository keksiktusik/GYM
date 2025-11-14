using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyApp.Data;
using MyApp.Models;

public class TrainingReminderService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<TrainingReminderService> _logger;

    public TrainingReminderService(IServiceProvider services, ILogger<TrainingReminderService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                    var tomorrow = DateTime.UtcNow.Date.AddDays(1);

                    var trainings = await context.TrainingEvents
                        .Where(t =>
                            !t.ReminderSent &&
                            t.Start.Date == tomorrow)
                        .ToListAsync(stoppingToken);

                    foreach (var training in trainings)
                    {
                        var user = await userManager.FindByIdAsync(training.UserId);
                        if (user == null || string.IsNullOrWhiteSpace(user.Email))
                            continue;

                        var localStart = training.Start.ToLocalTime();

                        string subject = "⏰ Przypomnienie: Twój trening już jutro! – GYM";
                        string body = $@"
                            <p>Cześć,</p>
                            <p>Przypominamy o Twoim jutrzejszym treningu <strong>{training.Title}</strong>.</p>
                            <p>
                                <strong>Data:</strong> {localStart:dd.MM.yyyy}<br/>
                                <strong>Godzina:</strong> {localStart:HH\\:mm}
                            </p>
                            <p>Przygotuj bidon z wodą, ulubioną muzykę i dobry nastrój. 💪</p>
                            <br/>
                            <p>Trzymamy kciuki za Twój trening! 💗<br/><strong>Zespół GYM</strong></p>
                        ";

                        try
                        {
                            await emailSender.SendEmailAsync(user.Email!, subject, body);
                            training.ReminderSent = true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Błąd wysyłki maila przypominającego o treningu (TrainingEventId: {Id})", training.Id);
                        }
                    }

                    if (trainings.Any())
                    {
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd w TrainingReminderService");
            }

            // 🔁 Sprawdzaj raz na godzinę
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
