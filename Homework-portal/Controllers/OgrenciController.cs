// ---- GÜNCEL VE TAM KOD ----
// Dosya Yolu: Homework-portal/Controllers/OgrenciController.cs

using Homework_portal.Models;
using Homework_portal.Repository;
using Homework_portal.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Homework_portal.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.SignalR; // 1. YENİ: SignalR Hub Context için eklendi
using Homework_portal.Hubs;         // 2. YENİ: NotificationHub için eklendi

namespace Homework_portal.Controllers
{
    [Authorize(Roles = AppRoles.Role_Ogrenci)]
    public class OgrenciController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<NotificationHub> _hubContext; // 3. YENİ: Hub Context eklendi

        public OgrenciController(IUnitOfWork unitOfWork,
                                 UserManager<ApplicationUser> userManager,
                                 IWebHostEnvironment webHostEnvironment,
                                 IHubContext<NotificationHub> hubContext) // 4. YENİ: Parametre eklendi
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext; // 5. YENİ: Atama yapıldı
        }

        // --- Index() metodu (DEĞİŞİKLİK YOK) ---
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var dersKayitlari = _unitOfWork.DersKayit.GetAll(
                dk => dk.OgrenciId == userId,
                includeProperties: "Ders,Ders.Ogretmen"
            );

            return View(dersKayitlari);
        }


        // --- Odevler() metodu (DEĞİŞİKLİK YOK) ---
        [HttpGet]
        public IActionResult Odevler(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            OdevlerVM odevlerVM = new OdevlerVM();
            odevlerVM.Ders = _unitOfWork.Ders.Get(d => d.Id == id);
            if (odevlerVM.Ders == null)
            {
                return NotFound();
            }

            odevlerVM.OdevListesi = _unitOfWork.Odev.GetAll(o => o.DersId == id);
            var buDerstekiOdevIdleri = odevlerVM.OdevListesi.Select(o => o.Id);
            odevlerVM.OgrenciTeslimleri = _unitOfWork.Teslim.GetAll(
                t => t.OgrenciId == userId && buDerstekiOdevIdleri.Contains(t.OdevId)
            );

            return View(odevlerVM);
        }


        // --- TeslimEt (GET) metodu (DEĞİŞİKLİK YOK) ---
        [HttpGet]
        public IActionResult TeslimEt(int id)
        {
            Odev odev = _unitOfWork.Odev.Get(o => o.Id == id);
            if (odev == null)
            {
                return NotFound();
            }

            TeslimVM teslimVM = new TeslimVM()
            {
                Odev = odev,
                Teslim = new Teslim()
            };
            teslimVM.Teslim.OdevId = id;

            return View(teslimVM);
        }


        // --- ÖDEVİ TESLİM ET (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeslimEt(TeslimVM teslimVM) // 6. YENİ: 'async Task<IActionResult>' oldu
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                // ... (Dosya kaydetme kodları aynı, değişiklik yok) ...
                string rootPath = _webHostEnvironment.WebRootPath;
                string uploadsFolder = Path.Combine(rootPath, "teslimler");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var originalFileName = teslimVM.Dosya.FileName;
                var extension = Path.GetExtension(originalFileName);
                string uniqueFileName = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    teslimVM.Dosya.CopyTo(fileStream); // Not: Burası async olabilir ama şimdilik senkron bırakıyoruz.
                }

                // ... (Veritabanı kaydı aynı, değişiklik yok) ...
                teslimVM.Teslim.OgrenciId = userId;
                teslimVM.Teslim.TeslimTarihi = DateTime.Now;
                teslimVM.Teslim.Not = null;
                teslimVM.Teslim.DosyaYolu = @"/teslimler/" + uniqueFileName;

                _unitOfWork.Teslim.Add(teslimVM.Teslim);
                _unitOfWork.Save();

                // 7. YENİ: SignalR Bildirimini Gönder
                // Öğrencinin adını al
                var ogrenci = await _userManager.FindByIdAsync(userId);
                var ogrenciAdi = ogrenci.Ad + " " + ogrenci.Soyad;

                // Hub'a bağlı TÜM istemcilere (Admin/Öğretmen)
                // "ReceiveNotification" adında bir fonksiyonu tetiklemesi için mesaj yolla.
                await _hubContext.Clients.All.SendAsync(
                    "ReceiveNotification",
                    $"Yeni Teslim!",
                    $"{ogrenciAdi} adlı öğrenci az önce bir ödev teslim etti."
                );

                // ... (Yönlendirme kısmı aynı, değişiklik yok) ...
                TempData["success"] = "Ödev başarıyla teslim edildi.";

                var odev = _unitOfWork.Odev.Get(o => o.Id == teslimVM.Teslim.OdevId);
                return RedirectToAction("Odevler", new { id = odev.DersId });
            }
            else
            {
                // ... (Hata durumu aynı, değişiklik yok) ...
                teslimVM.Odev = _unitOfWork.Odev.Get(o => o.Id == teslimVM.Teslim.OdevId);
                return View(teslimVM);
            }
        }
    }
}