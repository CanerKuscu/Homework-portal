using Homework_portal.Models;
using Homework_portal.Models.ViewModels;
using Homework_portal.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

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
        [AllowAnonymous]
        public IActionResult Register()
        {
            // RegisterVM'i kullanıyoruz ama View'da Role alanını göstermeyeceğiz
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterVM model, string? Sinif, string? Sube, string? type)
        {
            // Modelden "Role" alanını manuel olarak temizleyip, doğrulama dışı bırakıyoruz.
            ModelState.Remove("Role");

            // type = student|teacher; default=student
            var normalizedType = (type ?? "student").ToLowerInvariant();
            bool isTeacher = normalizedType == "teacher" || normalizedType == "ogretmen";

            if (isTeacher)
            {
                // Öğretmen kaydında öğrenci alanları boşaltılır
                model.OgrenciNo = null;
                Sinif = null;
                Sube = null;
            }
            else
            {
                // Öğrenci kaydında öğrenci no benzersiz olsun
                if (!string.IsNullOrWhiteSpace(model.OgrenciNo))
                {
                    var existsStdNo = _userManager.Users.Any(u => u.OgrenciNo == model.OgrenciNo);
                    if (existsStdNo)
                    {
                        ModelState.AddModelError("OgrenciNo", "Bu öğrenci numarası zaten kayıtlı.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    Sinif = isTeacher ? null : Sinif,
                    Sube = isTeacher ? null : Sube,
                    OgrenciNo = isTeacher ? null : model.OgrenciNo
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (isTeacher)
                    {
                        // Öğretmen adayı rolü ver ve giriş yaptırma (admin onayı bekler)
                        await _userManager.AddToRoleAsync(user, AppRoles.Role_OgretmenAday);
                        TempData["success"] = "Öğretmen kaydınız alındı. Admin onayından sonra giriş yapabilirsiniz.";
                        return RedirectToAction("Login", "Account", new { area = "Admin" });
                    }
                    else
                    {
                        // Kayıt olan herkesi OTOMATİK OLARAK "Öğrenci" yap
                        await _userManager.AddToRoleAsync(user, AppRoles.Role_Ogrenci);

                        // Kayıttan sonra otomatik giriş yapsın
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        // Kullanıcıyı projenin ana sayfasına (Genel Arayüz) yönlendir
                        TempData["success"] = "Kayıt başarılı. Hoş geldiniz!"; // İSTENEN MESAJ
                        return RedirectToAction("Index", "Home");
                    }
                }

                // Hata oluştuysa hataları modele ekle (Türkçe Identity describer da devrede)
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // Model geçerli değilse formu tekrar göster
            return View(model);
        }

        // --- PROFİL GÖRÜNTÜLE/GÜNCELLE ---
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = new ProfileVM
            {
                Ad = user.Ad,
                Soyad = user.Soyad,
                Email = user.Email ?? string.Empty,
                OgrenciNo = user.OgrenciNo,
                Sinif = user.Sinif,
                Sube = user.Sube
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Lütfen zorunlu alanları doldurun.";
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Öğrenci No benzersizliği (başka kullanıcıda var mı?)
            var newStdNo = string.IsNullOrWhiteSpace(model.OgrenciNo) ? null : model.OgrenciNo.Trim();
            if (!string.Equals(newStdNo, user.OgrenciNo, System.StringComparison.Ordinal))
            {
                if (!string.IsNullOrWhiteSpace(newStdNo))
                {
                    var existsStdNo = _userManager.Users.Any(u => u.OgrenciNo == newStdNo && u.Id != user.Id);
                    if (existsStdNo)
                    {
                        ModelState.AddModelError("OgrenciNo", "Bu öğrenci numarası başka bir kullanıcıya ait.");
                        return View(model);
                    }
                }
            }

            // Koruyucu atamalar (boş gelirse mevcut değeri koru)
            user.Ad = string.IsNullOrWhiteSpace(model.Ad) ? user.Ad : model.Ad.Trim();
            user.Soyad = string.IsNullOrWhiteSpace(model.Soyad) ? user.Soyad : model.Soyad.Trim();
            user.Email = string.IsNullOrWhiteSpace(model.Email) ? user.Email : model.Email.Trim();
            user.UserName = user.Email; // login e‑posta ile
            user.Sinif = string.IsNullOrWhiteSpace(model.Sinif) ? null : model.Sinif.Trim();
            user.Sube = string.IsNullOrWhiteSpace(model.Sube) ? null : model.Sube.Trim();
            user.OgrenciNo = newStdNo;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["success"] = "Hesap bilgileriniz güncellendi.";
                return RedirectToAction(nameof(Profile));
            }
            TempData["error"] = string.Join("; ", result.Errors.Select(e => e.Description));
            return View(model);
        }

        // --- ŞİFRE DEĞİŞTİR ---
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["success"] = "Şifreniz güncellendi.";
                return RedirectToAction(nameof(ChangePassword));
            }

            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return View(model);
        }
    }
}