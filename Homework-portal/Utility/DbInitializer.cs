using Homework_portal.Data;
using Homework_portal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Homework_portal.Utility
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        public DbInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db,
            IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
            _configuration = configuration;
        }

        public void Initialize()
        {
            try
            {
                // Do not drop DB. Apply migrations if any, else ensure created.
                var pending = _db.Database.GetPendingMigrations();
                if (pending.Any())
                {
                    _db.Database.Migrate();
                }
                else
                {
                    _db.Database.EnsureCreated();
                }
            }
            catch { }

            EnsureRole(AppRoles.Role_Admin);
            EnsureRole(AppRoles.Role_Ogretmen);
            EnsureRole(AppRoles.Role_Ogrenci);
            EnsureRole(AppRoles.Role_OgretmenAday);

            // Admin bilgilerini configuration'dan oku
            var adminEmail = _configuration["AdminSettings:Email"] ?? "admin@admin.com";
            var adminPassword = _configuration["AdminSettings:Password"] ?? "Admin123*";
            var adminFirstName = _configuration["AdminSettings:FirstName"] ?? "Admin";
            var adminLastName = _configuration["AdminSettings:LastName"] ?? "User";

            var adminExists = _db.Users
                .AsEnumerable()
                .Any(u => string.Equals(u.Email, adminEmail, StringComparison.OrdinalIgnoreCase)
                       && _userManager.IsInRoleAsync(u, AppRoles.Role_Admin).GetAwaiter().GetResult());
            if (!adminExists)
            {
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    EmailConfirmed = true
                }, adminPassword).GetAwaiter().GetResult();

                var user = _db.Users.FirstOrDefault(u => u.Email == adminEmail);
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