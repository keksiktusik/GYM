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
    options.SignIn.RequireConfirmedAccount = true; 
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddRoles<IdentityRole>() // WAŻNE – role Admin/Klient
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// EMAIL
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

// -----------------------------------------------------------
// 3️⃣  MVC + RAZOR
// -----------------------------------------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// -----------------------------------------------------------
// 4️⃣  AUTOMATYCZNE MIGRACJE + TWORZENIE RÓL + ADMIN
// -----------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Migracje
    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // Role + pierwszy admin
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // ROLE
    string[] roles = { "Administrator", "Klient" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // ADMIN
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
// 5️⃣  ŚRODOWISKO
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

app.UseAuthentication(); 
app.UseAuthorization();

// -----------------------------------------------------------
// 6️⃣  ROUTING + STRONY
// -----------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
