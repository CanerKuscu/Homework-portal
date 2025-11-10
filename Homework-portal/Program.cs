using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Homework_portal.Data;
using Homework_portal.Models;
using Homework_portal.Repository;
using Homework_portal.Utility;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection string'i al
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. DbContext'i ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Identity (Üyelik Sistemi) ekle
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 4. GÜNCEL KISIM: Identity Cookie ayarlarý
// Sisteme Giriþ (Login) sayfasýnýn yerini bildirir
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.LogoutPath = "/Admin/Account/Logout"; // Logout adresini de belirtelim
    options.AccessDeniedPath = "/Admin/Home/Index"; // Yetkisi yetmeyince Admin anasayfaya yönlendir
});


// 5. Repository Deseni (Unit of Work) ekle
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 6. DbInitializer (Veritabaný Baþlatýcý) ekle
builder.Services.AddScoped<IDbInitializer, DbInitializer>();


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 7. Authentication (Kimlik Doðrulama) ve Authorization (Yetkilendirme)
app.UseAuthentication();
app.UseAuthorization();

// 8. DbInitializer'ý Çalýþtýr
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    dbInitializer.Initialize();
}

// 9. ROTALAR (Sýralama çok önemli)

// 9a. Admin Area Rotasý (Önce bu gelmeli)
app.MapControllerRoute(
    name: "AdminArea",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// 9b. Varsayýlan Rota (Sonra bu gelmeli)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();