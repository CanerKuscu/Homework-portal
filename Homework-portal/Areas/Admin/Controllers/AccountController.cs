using Homework_portal.Models;
using Homework_portal.Models.ViewModels;
using Homework_portal.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq; // eklendi LINQ için
using Homework_portal.Repository; // eklendi: UnitOfWork kullanımı için

namespace Homework_portal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork; // eklendi

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork) // eklendi
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork; // eklendi
        }

        // --- GİRİŞ (LOGIN) - GET ---

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Tek alan: Identifier (e‑posta veya öğrenci no)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            ApplicationUser? user = null;
            var id = (model.Identifier ?? string.Empty).Trim();

            // Basit email kontrolü
            bool looksLikeEmail = id.Contains('@');
            if (looksLikeEmail)
                user = await _userManager.FindByEmailAsync(id);
            else
                user = _userManager.Users.FirstOrDefault(u => u.StudentNumber == id);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı bulunamadı.");
                return View(model);
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
                return View(model);
            }

            var roles = await _userManager.GetRolesAsync(user);
            // Öğretmen adayı ise girişe izin verme
            if (roles.Contains(AppRoles.Role_OgretmenAday))
            {
                await _signInManager.SignOutAsync();
                TempData["info"] = "Öğretmen kaydınız admin onayındadır. Onaylanana kadar giriş yapamazsınız.";
                return RedirectToAction(nameof(Login));
            }

            if (roles.Contains(AppRoles.Role_Admin) || roles.Contains(AppRoles.Role_Ogretmen))
                return RedirectToAction("Index", "Home", new { Area = "Admin" });
            if (roles.Contains(AppRoles.Role_Ogrenci))
                return RedirectToAction("Derslerim", "Ogrenci", new { Area = "" });

            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        // --- YETKİ YOK SAYFASI ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // --- YENİ: KULLANICILARI LİSTELE ---
        [HttpGet]
        [Authorize(Roles = AppRoles.Role_Admin)]
        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserRoleVM>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                model.Add(new UserRoleVM
                {
                    UserId = u.Id,
                    Ad = u.Ad,
                    Soyad = u.Soyad,
                    Email = u.Email!,
                    CurrentRole = roles.FirstOrDefault() ?? string.Empty,
                    AvailableRoles = new List<string> { AppRoles.Role_Admin, AppRoles.Role_Ogretmen, AppRoles.Role_Ogrenci },
                    Sinif = u.Sinif,
                    Sube = u.Sube,
                    OgrenciNo = u.OgrenciNo
                });
            }
            return View(model);
        }

        // --- YENİ: ROL GÜNCELLE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Role_Admin)]
        public async Task<IActionResult> UpdateRole(string userId, string newRole)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(newRole))
            {
                TempData["admin_error"] = "Geçersiz rol güncelleme isteği.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["admin_error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Users));
            }

            var oldRoles = await _userManager.GetRolesAsync(user);
            if (oldRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, oldRoles);
            }
            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                TempData["admin_error"] = "Seçilen rol sistemde tanımlı değil.";
                return RedirectToAction(nameof(Users));
            }
            await _userManager.AddToRoleAsync(user, newRole);

            // Eğer öğrenci rolünden çıkıyorsa Sınıf/Şube'yi temizle
            if (newRole != AppRoles.Role_Ogrenci)
            {
                user.Sinif = null;
                user.Sube = null;
                await _userManager.UpdateAsync(user);
            }

            TempData["admin_success"] = "Rol başarıyla güncellendi.";
            return RedirectToAction(nameof(Users));
        }

        // --- KAYIT (REGISTER) ---

        [HttpGet]
        [Authorize(Roles = AppRoles.Role_Admin)]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Role_Admin)]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    // Email login için dursun
                    UserName = model.Email,
                    Email = model.Email,
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    // Öğrenci seçilirse Sınıf/Şube ve OgrenciNo kaydedilsin, değilse null
                    Sinif = model.Role == AppRoles.Role_Ogrenci ? (Request.Form["Sinif"].FirstOrDefault() ?? null) : null,
                    Sube = model.Role == AppRoles.Role_Ogrenci ? (Request.Form["Sube"].FirstOrDefault() ?? null) : null,
                    OgrenciNo = model.Role == AppRoles.Role_Ogrenci ? (model.OgrenciNo ?? Request.Form["OgrenciNo"].FirstOrDefault()) : null
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var targetRole = string.IsNullOrWhiteSpace(model.Role) ? AppRoles.Role_Ogrenci : model.Role;
                    if (!await _roleManager.RoleExistsAsync(targetRole))
                    {
                        TempData["admin_error"] = "Seçilen rol sistemde tanımlı değil.";
                        return View(model);
                    }
                    await _userManager.AddToRoleAsync(user, targetRole);

                    TempData["admin_success"] = "Yeni kullanıcı başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Register));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }


        // --- ÇIKIŞ (LOGOUT) [DÜZELTİLDİ] ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            // DÜZELTME: Admin giriş sayfasına değil,
            // projenin genel ana sayfasına yönlendir.
            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        // --- YENİ: KULLANICI SİL ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Role_Admin)]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["admin_error"] = "Geçersiz kullanıcı.";
                return RedirectToAction(nameof(Users));
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["admin_error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Users));
            }

            // Updated: use English repositories
            var dersSayisi = _unitOfWork.Course.GetAll(d => d.TeacherId == user.Id).Count();
            var kayitSayisi = _unitOfWork.CourseEnrollment.GetAll(dk => dk.StudentId == user.Id).Count();
            var teslimSayisi = _unitOfWork.Submission.GetAll(t => t.StudentId == user.Id).Count();

            if (dersSayisi > 0 || kayitSayisi > 0 || teslimSayisi > 0)
            {
                TempData["admin_error"] = $"Kullanıcı silinemedi. Bağlı kayıtlar var. Ders: {dersSayisi}, Ders kaydı: {kayitSayisi}, Teslim: {teslimSayisi}. Önce bu kayıtları silin veya devredin.";
                return RedirectToAction(nameof(Users));
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(AppRoles.Role_Admin) && (await _userManager.GetUsersInRoleAsync(AppRoles.Role_Admin)).Count == 1)
            {
                TempData["admin_error"] = "Son admin silinemez.";
                return RedirectToAction(nameof(Users));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["admin_success"] = "Kullanıcı silindi.";
            }
            else
            {
                TempData["admin_error"] = string.Join("; ", result.Errors.Select(e => e.Description));
            }
            return RedirectToAction(nameof(Users));
        }
    }
}