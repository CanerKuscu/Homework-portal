// ---- GÜNCEL VE TAM KOD ----
// Dosya Yolu: Homework-portal/Areas/Admin/Controllers/TeslimController.cs

using Homework_portal.Models;
using Homework_portal.Repository;
using Homework_portal.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homework_portal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Role_Admin + "," + AppRoles.Role_Ogretmen)]
    public class TeslimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public TeslimController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // --- TÜM TESLİMLERİ LİSTELE (INDEX) ---
        public IActionResult Index()
        {
            // Tüm teslimleri al.
            // İlişkili verileri (Öğrenci, Ödev ve Ödev'in Dersi) 'Include' et.
            var teslimler = _unitOfWork.Teslim.GetAll(
                includeProperties: "Ogrenci,Odev,Odev.Ders"
            );

            return View(teslimler);
        }

        // ----- YENİ EKLENEN KISIM (GET) -----

        // --- TESLİM DETAYINI GÖR VE NOT VERME FORMUNU GÖSTER (GET) ---
        [HttpGet]
        public IActionResult Detay(int id) // 'id' parametresi, TeslimId'dir
        {
            // Teslim kaydını, ilişkili tüm verilerle birlikte getir
            var teslim = _unitOfWork.Teslim.Get(
                t => t.Id == id,
                includeProperties: "Ogrenci,Odev,Odev.Ders"
            );

            if (teslim == null)
            {
                return NotFound();
            }

            return View(teslim);
        }

        // ----- YENİ EKLENEN KISIM (POST) -----

        // --- GİRİLEN NOTU KAYDET (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Detay(Teslim teslim)
        {
            // Formdan sadece 'teslim.Id' ve 'teslim.Not' bilgisi geliyor.
            // Güvenlik için, veritabanından orijinal kaydı bulmalıyız.
            var teslimDb = _unitOfWork.Teslim.Get(t => t.Id == teslim.Id);

            if (teslimDb == null)
            {
                return NotFound();
            }

            // Sadece 'Not' alanını güncelle
            teslimDb.Not = teslim.Not;

            _unitOfWork.Teslim.Update(teslimDb);
            _unitOfWork.Save();

            TempData["success"] = "Öğrenciye not başarıyla girildi.";

            // Listeleme (Index) sayfasına geri dön
            return RedirectToAction("Index");
        }
    }
}