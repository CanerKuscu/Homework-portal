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
                    FirstName = "Admin",
                    LastName = "User",
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