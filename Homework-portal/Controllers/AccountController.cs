using Homework_portal.Models;
using Homework_portal.Models.ViewModels;
using Homework_portal.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Homework_portal.Controllers
{
    // Bu controller "Genel Arayüz" içindir, [Area] etiketi yok.
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // --- GENEL (PUBLIC) KAYIT ---

        [HttpGet]
        public IActionResult Register()
        {
            // RegisterVM'i kullanıyoruz ama View'da Role alanını göstermeyeceğiz
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            // Modelden "Role" alanını manuel olarak temizleyip, doğrulama dışı bırakıyoruz.
            ModelState.Remove("Role");

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
                    // Kayıt olan herkesi OTOMATİK OLARAK "Öğrenci" yap
                    await _userManager.AddToRoleAsync(user, AppRoles.Role_Ogrenci);

                    // Kayıttan sonra otomatik giriş yapsın
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Kullanıcıyı projenin ana sayfasına (Genel Arayüz) yönlendir
                    return RedirectToAction("Index", "Home");
                }

                // Hata oluştuysa hataları modele ekle
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // Model geçerli değilse formu tekrar göster
            return View(model);
        }
    }
}