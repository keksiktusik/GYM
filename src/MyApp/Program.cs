using MyApp.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;


var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------
// 1️⃣  KONFIGURACJA BAZY DANYCH
// -----------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// -----------------------------------------------------------
// 2️⃣  KONFIGURACJA IDENTITY + ROLE
// -----------------------------------------------------------
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;  // wymaga potwierdzenia maila
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddRoles<IdentityRole>() // dodajemy role (Administrator, Klient)
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI(); // użycie gotowego UI do logowania/rejestracji

// -----------------------------------------------------------
// 3️⃣  KONTROLERY + RAZOR PAGES
// -----------------------------------------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

var app = builder.Build();

// -----------------------------------------------------------
// 4️⃣  MIGRACJE / TWORZENIE BAZY
// -----------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // Seed ról i administratora
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Role: Administrator i Klient
    string[] roles = { "Administrator", "Klient" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Konto administratora (tylko jedno)
    string adminEmail = "admin@myapp.local";
    string adminPass = "Admin123!";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(admin, adminPass);
        await userManager.AddToRoleAsync(admin, "Administrator");
    }
}

// -----------------------------------------------------------
// 5️⃣  KONFIGURACJA ŚRODOWISKA
// -----------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // musi być przed Authorization
app.UseAuthorization();

// -----------------------------------------------------------
// 6️⃣  ROUTING + STRONY
// -----------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // logowanie/rejestracja

app.Run();
