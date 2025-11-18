using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Homework_portal.Data;
using Homework_portal.Models;
using Homework_portal.Repository;
using Homework_portal.Utility;
using Homework_portal.Hubs; // 1. YENÝ: Hub'ý tanýtmak için bu satýrý ekleyin
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Localization; // eklendi
using System.Globalization; // eklendi

var builder = WebApplication.CreateBuilder(args);

// 1. Connection string'i al
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. DbContext'i ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Identity (üyelik Sistemi) ekle
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    // E?posta benzersiz olsun
    options.User.RequireUniqueEmail = true;
    // Türkçe karakterler ve @ iþareti vb. kullanýcý adýna izin ver
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+þÞýÝöÖçÇðÐüÜ";
})
    .AddErrorDescriber<TurkishIdentityErrorDescriber>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 4. Identity Cookie ayarlarý
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.LogoutPath = "/Admin/Account/Logout";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied"; 
});


// 5. Repository Deseni (Unit of Work) ekle
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 6. DbInitializer (Veritabaný Baþlatýcý) ekle
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

// Localization: varsayýlan dili Türkçe yap
var supportedCultures = new[] { new CultureInfo("tr-TR") };

// Add services to the container. Tüm uygulama için varsayýlan olarak kimlik doðrulama iste
builder.Services
    .AddControllersWithViews(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
    })
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// 7. YENÝ: SignalR servisini ekle
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Ýstek yerelleþtirme (varsayýlan tr-TR)
var requestLocalizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};
app.UseRequestLocalization(requestLocalizationOptions);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 7. Authentication (Kimlik Doðrulama) ve Authorization (Yetkilendirme)
app.UseAuthentication();
app.UseAuthorization();

// 8. DbInitializer'ý çalýþtýr
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    dbInitializer.Initialize();
}

// 9. YENÝ: SignalR Hub Endpoint'ini (adresini) haritala
app.MapHub<NotificationHub>("/notificationHub");

// 10. ROTALAR

// 10a. Admin Area Rotasý
app.MapControllerRoute(
    name: "AdminArea",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// 10b. KÖK ("/") ADRESÝNÝ her zaman Giriþ sayfasýna yönlendir (oturum açýk olsa bile)
app.MapControllerRoute(
    name: "rootToLogin",
    pattern: string.Empty,
    defaults: new { area = "Admin", controller = "Account", action = "Login" });

// 10c. Varsayýlan Rota
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();