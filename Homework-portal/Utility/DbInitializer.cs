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
            // 0a. AspNetUsers tablosu için Sinif/Sube/OgrenciNo sütunlarını garanti altına al
            try
            {
                _db.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('dbo.AspNetUsers','Sinif') IS NULL
    ALTER TABLE [dbo].[AspNetUsers] ADD [Sinif] NVARCHAR(10) NULL;
IF COL_LENGTH('dbo.AspNetUsers','Sube') IS NULL
    ALTER TABLE [dbo].[AspNetUsers] ADD [Sube] NVARCHAR(10) NULL;
IF COL_LENGTH('dbo.AspNetUsers','OgrenciNo') IS NULL
    ALTER TABLE [dbo].[AspNetUsers] ADD [OgrenciNo] NVARCHAR(30) NULL;
IF (EXISTS(SELECT 1 FROM sys.columns WHERE Name = 'Ad' AND Object_ID = Object_ID('dbo.AspNetUsers')))
BEGIN
    ALTER TABLE [dbo].[AspNetUsers] ALTER COLUMN [Ad] NVARCHAR(50) NOT NULL;
END
IF (EXISTS(SELECT 1 FROM sys.columns WHERE Name = 'Soyad' AND Object_ID = Object_ID('dbo.AspNetUsers')))
BEGIN
    ALTER TABLE [dbo].[AspNetUsers] ALTER COLUMN [Soyad] NVARCHAR(50) NOT NULL;
END
");
            }
            catch { }

            // 0b. Odevler tablosu için Sinif/Sube sütunlarını ve dosya alanlarını garanti altına al
            try
            {
                _db.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('dbo.Odevler','Sinif') IS NULL
    ALTER TABLE [dbo].[Odevler] ADD [Sinif] NVARCHAR(10) NULL;
IF COL_LENGTH('dbo.Odevler','Sube') IS NULL
    ALTER TABLE [dbo].[Odevler] ADD [Sube] NVARCHAR(10) NULL;
IF COL_LENGTH('dbo.Odevler','DosyaYolu') IS NULL
    ALTER TABLE [dbo].[Odevler] ADD [DosyaYolu] NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.Odevler','OrjinalDosyaAdi') IS NULL
    ALTER TABLE [dbo].[Odevler] ADD [OrjinalDosyaAdi] NVARCHAR(MAX) NULL;
");
            }
            catch { }

            // 0c. Teslimler tablosu için dosya alanlarını garanti altına al
            try
            {
                _db.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('dbo.Teslimler','DosyaYolu') IS NULL
    ALTER TABLE [dbo].[Teslimler] ADD [DosyaYolu] NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.Teslimler','OrjinalDosyaAdi') IS NULL
    ALTER TABLE [dbo].[Teslimler] ADD [OrjinalDosyaAdi] NVARCHAR(MAX) NULL;
");
            }
            catch { }

            // 1. Bekleyen migration'lar varsa çalıştır (Veritabanını güncelle)
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception)
            {
                // loglanabilir
            }

            // 2. Roller: her run'da eksik rolleri tamamla
            EnsureRole(AppRoles.Role_Admin);
            EnsureRole(AppRoles.Role_Ogretmen);
            EnsureRole(AppRoles.Role_Ogrenci);
            EnsureRole(AppRoles.Role_OgretmenAday);

            // admin@admin.com e-postası admin rolü için rezerve; başka kullanıcıda ise UserName değiştir.
            var nonAdminWithAdminEmail = _db.Users
                .AsEnumerable()
                .Where(u => string.Equals(u.Email, "admin@admin.com", StringComparison.OrdinalIgnoreCase)
                         && !_userManager.IsInRoleAsync(u, AppRoles.Role_Admin).GetAwaiter().GetResult())
                .ToList();
            foreach (var u in nonAdminWithAdminEmail)
            {
                if (u.UserName == u.Email)
                {
                    u.UserName = u.Email + ".user";
                    _db.Update(u);
                }
            }
            _db.SaveChanges();

            // 3. Varsayılan admin yoksa oluştur
            var adminExists = _db.Users
                .AsEnumerable()
                .Any(u => string.Equals(u.Email, "admin@admin.com", StringComparison.OrdinalIgnoreCase)
                       && _userManager.IsInRoleAsync(u, AppRoles.Role_Admin).GetAwaiter().GetResult());
            if (!adminExists)
            {
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    Ad = "Admin",
                    Soyad = "Kullanici",
                    EmailConfirmed = true
                }, "Admin123*").GetAwaiter().GetResult();

                var user = _db.Users.FirstOrDefault(u => u.Email == "admin@admin.com");
                if (user != null)
                {
                    _userManager.AddToRoleAsync(user, AppRoles.Role_Admin).GetAwaiter().GetResult();
                }
            }
        }

        private void EnsureRole(string role)
        {
            if (!_roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
            }
        }
    }
}