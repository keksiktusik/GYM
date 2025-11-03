using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace MyApp.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        public SmtpEmailSender(IConfiguration config) => _config = config;

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtp = _config.GetSection("Smtp");

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("MY GYM", smtp["From"]));
            msg.To.Add(MailboxAddress.Parse(email));
            msg.Subject = subject;
            msg.Body = new TextPart("html") { Text = htmlMessage };

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(smtp["Host"], int.Parse(smtp["Port"]), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtp["User"], smtp["Pass"]);
                await client.SendAsync(msg);
                await client.DisconnectAsync(true);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ E-mail wysłany do: {email}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Błąd wysyłania e-maila:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }
    }
}
