// Admin - Course management (renamed from DersController)
using Homework_portal.Models;
using Homework_portal.Models.ViewModels;
using Homework_portal.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Homework_portal.Hubs;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;

namespace Homework_portal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Utility.AppRoles.Role_Admin + "," + Utility.AppRoles.Role_Ogretmen)]
    public class CourseController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _hub;

        public CourseController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IHubContext<NotificationHub> hub)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _hub = hub;
        }

        private bool IsAdmin => User.IsInRole(Utility.AppRoles.Role_Admin);
        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public IActionResult Index()
        {
            var courses = _unitOfWork.Course.GetAll(includeProperties: "Teacher");
            if (!IsAdmin && CurrentUserId != null)
            {
                courses = courses.Where(c => c.TeacherId == CurrentUserId);
            }
            return View(courses);
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            var teachers = await _userManager.GetUsersInRoleAsync(Utility.AppRoles.Role_Ogretmen);

            var vm = new CourseVM()
            {
                Course = new Models.Course { Name = string.Empty },
                TeacherList = new SelectList(teachers.Select(o => new { Id = o.Id, AdSoyad = o.Ad + " " + o.Soyad }), "Id", "AdSoyad")
            };

            if (id == null || id == 0)
            {
                // Öðretmen kendi adýna ders oluþturabilir, farklý öðretmen seçemesin
                if (!IsAdmin && CurrentUserId != null)
                {
                    vm.Course.TeacherId = CurrentUserId;
                    vm.TeacherList = new SelectList(teachers.Where(t => t.Id == CurrentUserId).Select(o => new { Id = o.Id, AdSoyad = o.Ad + " " + o.Soyad }), "Id", "AdSoyad");
                }
                return View(vm);
            }

            vm.Course = _unitOfWork.Course.Get(d => d.Id == id);
            if (vm.Course == null)
            {
                return NotFound();
            }

            if (!IsAdmin && CurrentUserId != null && vm.Course.TeacherId != CurrentUserId)
            {
                return Forbid();
            }

            // Düzenlemede de öðretmen baþka öðretmen seçemesin
            if (!IsAdmin && CurrentUserId != null)
            {
                vm.TeacherList = new SelectList(teachers.Where(t => t.Id == CurrentUserId).Select(o => new { Id = o.Id, AdSoyad = o.Ad + " " + o.Soyad }), "Id", "AdSoyad", CurrentUserId);
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(CourseVM vm)
        {
            if (!IsAdmin && CurrentUserId != null)
            {
                // Öðretmen sadece kendi adýna ders oluþturabilir/güncelleyebilir
                vm.Course.TeacherId = CurrentUserId;
            }

            if (ModelState.IsValid)
            {
                bool isNew = vm.Course.Id == 0;

                // Güncellemede yetki kontrolü
                if (!isNew && !IsAdmin && CurrentUserId != null)
                {
                    var existing = _unitOfWork.Course.Get(c => c.Id == vm.Course.Id);
                    if (existing == null || existing.TeacherId != CurrentUserId)
                    {
                        TempData["admin_error"] = "Bu dersi güncelleme yetkiniz yok.";
                        return RedirectToAction("Index");
                    }
                }

                if (isNew) _unitOfWork.Course.Add(vm.Course); else _unitOfWork.Course.Update(vm.Course);
                _unitOfWork.Save();

                var title = isNew ? "Yeni Ders" : "Ders Güncellendi";
                var message = isNew ? $"{vm.Course.Name} adlý yeni ders oluþturuldu." : $"{vm.Course.Name} adlý ders güncellendi.";
                await _hub.Clients.All.SendAsync("ReceiveNotification", title, message);

                TempData["admin_success"] = isNew ? "Ders eklendi." : "Ders güncellendi.";
                return RedirectToAction("Index");
            }

            var teachers = await _userManager.GetUsersInRoleAsync(Utility.AppRoles.Role_Ogretmen);
            vm.TeacherList = new SelectList(teachers.Select(o => new { Id = o.Id, AdSoyad = o.Ad + " " + o.Soyad }), "Id", "AdSoyad", vm.Course.TeacherId);
            return View(vm);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteById(int? id)
        {
            if (id == null || id == 0)
                return Json(new { success = false, message = "Geçersiz istek. Silinecek ders bulunamadý (id boþ)." });

            var course = _unitOfWork.Course.Get(d => d.Id == id, includeProperties: "Assignments");
            if (course == null)
                return Json(new { success = false, message = "Ders bulunamadý. Silme iþlemi iptal edildi." });

            if (!IsAdmin && CurrentUserId != null && course.TeacherId != CurrentUserId)
            {
                return Json(new { success = false, message = "Bu dersi silme yetkiniz yok." });
            }

            var odevSayisi = _unitOfWork.Assignment.GetAll(o => o.CourseId == id).Count();
            var kayitSayisi = _unitOfWork.CourseEnrollment.GetAll(k => k.CourseId == id).Count();

            if (odevSayisi > 0 || kayitSayisi > 0)
            {
                return Json(new
                {
                    success = false,
                    message = $"Ders silinemedi. Baðlý kayýtlar var. Ödev sayýsý: {odevSayisi}, Ders kaydý: {kayitSayisi}. Önce bu kayýtlarý silin veya devredin."
                });
            }

            try
            {
                _unitOfWork.Course.Remove(course);
                _unitOfWork.Save();
                await _hub.Clients.All.SendAsync("ReceiveNotification", "Ders Silindi", $"{course.Name} adlý ders silindi.");
                return Json(new { success = true, message = "Ders baþarýyla silindi." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Ders silinirken beklenmeyen bir hata oluþtu: " + ex.Message });
            }
        }
    }
}
