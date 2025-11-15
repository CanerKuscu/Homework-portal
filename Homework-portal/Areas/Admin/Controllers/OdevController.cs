// ---- GÜNCEL VE TAM KOD ----
// Dosya Yolu: Homework-portal/Areas/Admin/Controllers/OdevController.cs

using Homework_portal.Models;
using Homework_portal.Models.ViewModels; // OdevVM için
using Homework_portal.Repository;
using Homework_portal.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList için

namespace Homework_portal.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Sadece Admin veya Ogretmen rolleri erişebilsin
    [Authorize(Roles = AppRoles.Role_Admin + "," + AppRoles.Role_Ogretmen)]
    public class OdevController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OdevController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // --- ÖDEVLERİ LİSTELE (INDEX) ---
        public IActionResult Index()
        {
            // Verileri alırken 'Ders' bilgilerini de 'Include' ediyoruz.
            var odevler = _unitOfWork.Odev.GetAll(includeProperties: "Ders");
            return View(odevler);
        }


        // --- YENİ ÖDEV OLUŞTUR / GÜNCELLE (UPSERT - GET) ---
        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            // Tüm dersleri getir (açılır liste için)
            var dersler = _unitOfWork.Ders.GetAll();

            OdevVM odevVM = new OdevVM()
            {
                Odev = new Odev(),
                DersListesi = new SelectList(dersler.Select(d => new {
                    Id = d.Id,
                    Ad = d.Ad
                }), "Id", "Ad") // Açılır liste için ayarla
            };

            if (id == null || id == 0)
            {
                // id yoksa, bu bir 'Create' (Yeni Kayıt) işlemidir.
                // Varsayılan TeslimTarihi'ni 1 hafta sonrası yap
                odevVM.Odev.TeslimTarihi = DateTime.Now.AddDays(7);
                return View(odevVM);
            }
            else
            {
                // id varsa, bu bir 'Update' (Güncelleme) işlemidir.
                odevVM.Odev = _unitOfWork.Odev.Get(o => o.Id == id);
                if (odevVM.Odev == null)
                {
                    return NotFound();
                }
                return View(odevVM);
            }
        }

        // ----- YENİ EKLENEN KISIM (POST) -----

        // --- YENİ ÖDEV OLUŞTUR / GÜNCELLE (UPSERT - POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(OdevVM odevVM)
        {
            if (ModelState.IsValid)
            {
                if (odevVM.Odev.Id == 0)
                {
                    // ID = 0, yani YENİ KAYIT
                    _unitOfWork.Odev.Add(odevVM.Odev);
                    TempData["success"] = "Ödev başarıyla oluşturuldu.";
                }
                else
                {
                    // ID != 0, yani GÜNCELLEME
                    _unitOfWork.Odev.Update(odevVM.Odev);
                    TempData["success"] = "Ödev başarıyla güncellendi.";
                }
                _unitOfWork.Save(); // Değişiklikleri veritabanına kaydet
                return RedirectToAction("Index"); // Listeleme sayfasına geri dön
            }
            else
            {
                // Model geçerli değilse (hata varsa), formu tekrar doldur
                // (Açılır liste boş gelmesin diye)
                var dersler = _unitOfWork.Ders.GetAll();
                odevVM.DersListesi = new SelectList(dersler.Select(d => new {
                    Id = d.Id,
                    Ad = d.Ad
                }), "Id", "Ad", odevVM.Odev.DersId); // Seçili değeri de koru

                return View(odevVM);
            }
        }

        // ----- YENİ EKLENEN KISIM (DELETE AJAX) -----

        // --- AJAX İLE SİLME METODU ---
        [HttpDelete]
        public IActionResult DeleteById(int? id)
        {
            if (id == null || id == 0)
            {
                return Json(new { success = false, message = "Silinecek ödev bulunamadı." });
            }

            var odev = _unitOfWork.Odev.Get(d => d.Id == id);
            if (odev == null)
            {
                return Json(new { success = false, message = "Ödev bulunamadı." });
            }

            try
            {
                // Bu ödeve bağlı Teslim kayıtları varsa,
                // ApplicationDbContext'deki OnDelete(DeleteBehavior.Restrict) kuralı
                // nedeniyle burası HATA VERECEKTİR.
                _unitOfWork.Odev.Remove(odev);
                _unitOfWork.Save();

                // Başarı durumunda JSON ile cevap dön
                return Json(new { success = true, message = "Ödev başarıyla silindi." });
            }
            catch (Exception ex)
            {
                // Hata 'catch' bloğuna düşerse (ilişkili veri hatası)
                return Json(new { success = false, message = "Hata! Bu ödev silinemedi (bu ödeve yapılmış teslimler olabilir)." });
            }
        }
    }
}