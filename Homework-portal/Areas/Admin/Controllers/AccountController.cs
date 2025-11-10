using Homework_portal.Models;
using Homework_portal.Models.ViewModels;
using Homework_portal.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Güvenlik için eklendi

namespace Homework_portal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize] // 1. Controller'ın tamamını kilitle
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // --- GİRİŞ (LOGIN) ---

        [HttpGet]
        [AllowAnonymous] // 2. Kilidi sadece Login (GET) için aç
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous] // 3. Kilidi sadece Login (POST) için aç
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/Admin/Home/Index");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi. Lütfen e-posta ve şifrenizi kontrol edin.");
                    return View(model);
                }
            }
            return View(model);
        }

        // --- KAYIT (REGISTER) ---

        [HttpGet]
        [Authorize(Roles = AppRoles.Role_Admin)] // 4. SADECE ADMIN ERİŞEBİLSİN
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Role_Admin)] // 5. SADECE ADMIN ERİŞEBİLSİN
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Ad = model.Ad,
                    Soyad = model.Soyad
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Rol ataması
                    if (model.Role == AppRoles.Role_Ogretmen)
                    {
                        await _userManager.AddToRoleAsync(user, AppRoles.Role_Ogretmen);
                    }
                    else if (model.Role == AppRoles.Role_Ogrenci)
                    {
                        await _userManager.AddToRoleAsync(user, AppRoles.Role_Ogrenci);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, AppRoles.Role_Ogrenci);
                    }

                    // Admin yeni kullanıcı eklerken otomatik giriş yapmasına gerek yok.
                    // await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["success"] = "Yeni kullanıcı başarıyla oluşturuldu.";
                    // TODO: Burayı "Kullanıcı Listesi" sayfasına yönlendirmek daha iyi olacak
                    return RedirectToAction("Index", "Home", new { Area = "Admin" });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }


        // --- ÇIKIŞ (LOGOUT) ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        // [Authorize] olduğu için zaten sadece giriş yapanlar erişebilir
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account", new { Area = "Admin" });
        }
    }
}