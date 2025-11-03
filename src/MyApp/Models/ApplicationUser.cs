using Microsoft.AspNetCore.Identity;

namespace MyApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Możesz dodać własne pola, jeśli chcesz (np. imię i nazwisko):
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
