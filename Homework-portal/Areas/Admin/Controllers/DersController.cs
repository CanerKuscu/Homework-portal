using Homework_portal.Models;
using Homework_portal.Models.ViewModels;
using Homework_portal.Repository;
using Homework_portal.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Homework_portal.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Sadece giriş yapmış ve rolü "Admin" VEYA "Ogretmen" olanlar erişebilsin
    [Authorize(Roles = AppRoles.Role_Admin + "," + AppRoles.Role_Ogretmen)]
    public class DersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public DersController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // --- DERSLERİ LİSTELE (INDEX) ---
        public IActionResult Index()
        {
            // Verileri alırken "Ogretmen" bilgilerini de 'Include' (Dahil Et) ediyoruz.
            var dersler = _unitOfWork.Ders.GetAll(includeProperties: "Ogretmen");
            return View(dersler);
        }

        // --- YENİ DERS OLUŞTUR (UPSERT - GET) ---
        // Hem 'Create' (Oluştur) hem de 'Update' (Güncelle) için aynı metodu kullanacağız (Upsert)
        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            // Öğretmen rolündeki tüm kullanıcıları getir
            var ogretmenler = await _userManager.GetUsersInRoleAsync(AppRoles.Role_Ogretmen);

            DersVM dersVM = new DersVM()
            {
                Ders = new Ders(),
                OgretmenListesi = new SelectList(ogretmenler.Select(o => new {
                    Id = o.Id,
                    AdSoyad = o.Ad + " " + o.Soyad
                }), "Id", "AdSoyad") // Açılır liste için ayarla
            };

            if (id == null || id == 0)
            {
                // id yoksa, bu bir 'Create' (Yeni Kayıt) işlemidir.
                return View(dersVM);
            }
            else
            {
                // id varsa, bu bir 'Update' (Güncelleme) işlemidir.
                // Veritabanından dersi bul
                dersVM.Ders = _unitOfWork.Ders.Get(d => d.Id == id);
                if (dersVM.Ders == null)
                {
                    return NotFound();
                }
                return View(dersVM);
            }
        }


        // --- YENİ DERS OLUŞTUR (UPSERT - POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(DersVM dersVM)
        {
            if (ModelState.IsValid)
            {
                if (dersVM.Ders.Id == 0)
                {
                    // ID = 0, yani YENİ KAYIT
                    _unitOfWork.Ders.Add(dersVM.Ders);
                    TempData["success"] = "Ders başarıyla oluşturuldu.";
                }
                else
                {
                    // ID != 0, yani GÜNCELLEME
                    _unitOfWork.Ders.Update(dersVM.Ders);
                    TempData["success"] = "Ders başarıyla güncellendi.";
                }
                _unitOfWork.Save(); // Değişiklikleri veritabanına kaydet
                return RedirectToAction("Index"); // Listeleme sayfasına geri dön
            }
            else
            {
                // Model geçerli değilse, formu (açılır liste boş gelmesin diye) tekrar doldur
                var ogretmenler = _userManager.GetUsersInRoleAsync(AppRoles.Role_Ogretmen).Result;
                dersVM.OgretmenListesi = new SelectList(ogretmenler.Select(o => new {
                    Id = o.Id,
                    AdSoyad = o.Ad + " " + o.Soyad
                }), "Id", "AdSoyad", dersVM.Ders.OgretmenId);

                return View(dersVM);
            }
        }


        // --- DERS SİL (DELETE - Geleneksel) ---
        // Bu metot artık AJAX kullandığımız için teknik olarak gerekli değil,
        // ama JavaScriptsiz tarayıcılar için bir yedek (fallback) olarak kalabilir.
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var ders = _unitOfWork.Ders.Get(d => d.Id == id, includeProperties: "Ogretmen");
            if (ders == null)
            {
                return NotFound();
            }
            return View(ders); // Delete.cshtml sayfasına yönlendirir
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var ders = _unitOfWork.Ders.Get(d => d.Id == id);
            if (ders == null)
            {
                return NotFound();
            }

            try
            {
                _unitOfWork.Ders.Remove(ders);
                _unitOfWork.Save();
                TempData["success"] = "Ders başarıyla silindi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Silme işlemi (ilişkili ödevler/kayıtlar varsa) patlayabilir
                TempData["error"] = "Hata! Bu dersi silemezsiniz (ilişkili ödevler veya kayıtlı öğrenciler olabilir).";
                return RedirectToAction("Index");
            }
        }

        // ----- GÜNCEL KISIM: YENİ AJAX METODU (ARA SINAV İÇİN) -----
        [HttpDelete]
        public IActionResult DeleteById(int? id)
        {
            if (id == null || id == 0)
            {
                // Hata durumunda JSON ile cevap dön
                return Json(new { success = false, message = "Silinecek ders bulunamadı." });
            }

            var ders = _unitOfWork.Ders.Get(d => d.Id == id);
            if (ders == null)
            {
                return Json(new { success = false, message = "Ders bulunamadı." });
            }

            try
            {
                _unitOfWork.Ders.Remove(ders);
                _unitOfWork.Save();
                // Başarı durumunda JSON ile cevap dön
                return Json(new { success = true, message = "Ders başarıyla silindi." });
            }
            catch (Exception ex)
            {
                // İlişkili veri (Foreign Key) hatası vb.
                return Json(new { success = false, message = "Hata! Bu ders silinemedi (ilişkili ödevler veya kayıtlı öğrenciler olabilir)." });
            }
        }
    }
}