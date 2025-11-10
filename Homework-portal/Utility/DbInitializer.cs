using Homework_portal.Data;
using Homework_portal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Homework_portal.Utility
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }

        public void Initialize()
        {
            // 1. Bekleyen migration'lar varsa çalıştır (Veritabanını güncelle)
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // Hata günlüğü (loglama) eklenebilir
            }

            // 2. Rolleri oluştur (Admin, Ogretmen, Ogrenci)
            // Eğer roller veritabanında yoksa, GetAwaiter().GetResult() ile senkron olarak oluştur
            if (!_roleManager.RoleExistsAsync(AppRoles.Role_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(AppRoles.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(AppRoles.Role_Ogretmen)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(AppRoles.Role_Ogrenci)).GetAwaiter().GetResult();

                // 3. Roller oluşturulduysa, ilk Admin kullanıcısını oluştur
                // admin@admin.com / Admin123*
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    Ad = "Admin",
                    Soyad = "Kullanici",
                    EmailConfirmed = true
                }, "Admin123*").GetAwaiter().GetResult(); // Varsayılan şifre: Admin123*

                // Admin kullanıcısını bul ve "Admin" rolüne ata
                // ----- HATA BURADAYDI, DÜZELTİLDİ -----
                ApplicationUser user = _db.Users.FirstOrDefault(u => u.Email == "admin@admin.com");
                // ----- ----------------------------- -----
                if (user != null)
                {
                    _userManager.AddToRoleAsync(user, AppRoles.Role_Admin).GetAwaiter().GetResult();
                }
            }
        }
    }
}