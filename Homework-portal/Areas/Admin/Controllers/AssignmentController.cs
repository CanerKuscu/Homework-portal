using Homework_portal.Models;
using Homework_portal.Models.ViewModels;
using Homework_portal.Repository;
using Homework_portal.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Homework_portal.Hubs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting; // eklendi
using System.Security.Claims;

namespace Homework_portal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Role_Admin + "," + AppRoles.Role_Ogretmen)]
    public class AssignmentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IWebHostEnvironment _env; // eklendi

        private static readonly string[] CLASS_LIST = new[] { "1","2","3","4","5","6","7","8","9","10","11","12" };
        private static readonly string[] BRANCH_LIST = new[] { "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","R","S","T","U","V","Y","Z" };

        public AssignmentController(IUnitOfWork unitOfWork, IHubContext<NotificationHub> hub, IWebHostEnvironment env) // param eklendi
        {
            _unitOfWork = unitOfWork;
            _hub = hub;
            _env = env; // set
        }

        private bool IsAdmin => User.IsInRole(AppRoles.Role_Admin);
        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public IActionResult Index()
        {
            var items = _unitOfWork.Assignment.GetAll(includeProperties: "Course");
            if (!IsAdmin && CurrentUserId != null)
            {
                var myCourseIds = _unitOfWork.Course.GetAll(c => c.TeacherId == CurrentUserId).Select(c => c.Id).ToHashSet();
                items = items.Where(a => myCourseIds.Contains(a.CourseId));
            }
            return View(items);
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            var allCourses = _unitOfWork.Course.GetAll();
            var courses = allCourses;
            if (!IsAdmin && CurrentUserId != null)
            {
                courses = allCourses.Where(c => c.TeacherId == CurrentUserId);
            }

            var vm = new AssignmentVM
            {
                Assignment = new Assignment
                {
                    Title = string.Empty,
                    Description = string.Empty,
                    DueDate = DateTime.Now.AddDays(7)
                },
                CourseList = courses.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }),
                AllClasses = CLASS_LIST,
                AllBranches = BRANCH_LIST,
                SelectedClasses = Array.Empty<string>(),
                SelectedBranches = Array.Empty<string>()
            };

            if (id == null || id == 0) return View(vm);

            var existing = _unitOfWork.Assignment.Get(o => o.Id == id, includeProperties: "Course");
            if (existing == null) return NotFound();
            if (!IsAdmin && CurrentUserId != null && existing.Course.TeacherId != CurrentUserId)
                return Forbid();

            vm.Assignment = existing;
            vm.CourseList = courses.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name,
                Selected = d.Id == vm.Assignment.CourseId
            });
            vm.SelectedClasses = string.IsNullOrWhiteSpace(existing.Class) ? Array.Empty<string>() : new[] { existing.Class! };
            vm.SelectedBranches = string.IsNullOrWhiteSpace(existing.Branch) ? Array.Empty<string>() : new[] { existing.Branch! };
            vm.AllClasses = CLASS_LIST;
            vm.AllBranches = BRANCH_LIST;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(AssignmentVM vm)
        {
            // Yetki: Öðretmen yalnýzca kendi derslerine ödev verebilir
            if (!IsAdmin && CurrentUserId != null)
            {
                var ownsCourse = _unitOfWork.Course.Get(c => c.Id == vm.Assignment.CourseId)?.TeacherId == CurrentUserId;
                if (!ownsCourse)
                {
                    ModelState.AddModelError("Assignment.CourseId", "Sadece size ait derslere ödev verebilirsiniz.");
                }
            }

            // Tek seçimleri modele yaz
            if (vm.SelectedClasses != null && vm.SelectedClasses.Any()) vm.Assignment.Class = vm.SelectedClasses.First(); else vm.Assignment.Class = null;
            if (vm.SelectedBranches != null && vm.SelectedBranches.Any()) vm.Assignment.Branch = vm.SelectedBranches.First(); else vm.Assignment.Branch = null;

            // Baþlýðý normalize et ve mükerrer kontrol yap
            vm.Assignment.Title = (vm.Assignment.Title ?? string.Empty).Trim();
            var keyClass = vm.Assignment.Class ?? string.Empty;
            var keyBranch = vm.Assignment.Branch ?? string.Empty;
            var normalized = vm.Assignment.Title.ToLowerInvariant();
            var duplicates = _unitOfWork.Assignment
                .GetAll(a => a.Id != vm.Assignment.Id
                          && a.CourseId == vm.Assignment.CourseId
                          && (a.Class ?? "") == keyClass
                          && (a.Branch ?? "") == keyBranch,
                        tracked: true)
                .Any(a => (a.Title ?? string.Empty).Trim().ToLowerInvariant() == normalized);
            if (duplicates)
            {
                ModelState.AddModelError("Assignment.Title", "Ayný ders, sýnýf ve þube için bu baþlýkta bir ödev zaten var.");
            }

            if (!ModelState.IsValid)
            {
                var allCourses = _unitOfWork.Course.GetAll();
                var courses = (!IsAdmin && CurrentUserId != null) ? allCourses.Where(c => c.TeacherId == CurrentUserId) : allCourses;
                vm.CourseList = courses.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(), Text = d.Name, Selected = d.Id == vm.Assignment.CourseId
                });
                vm.AllClasses = CLASS_LIST;
                vm.AllBranches = BRANCH_LIST;
                return View(vm);
            }

            // Öðretmenin yüklediði dosyalarý ModelState geçtikten sonra kaydet (çoklu dosya)
            var files = Request.Form.Files.GetFiles("AssignmentFiles");
            if (files != null && files.Count > 0)
            {
                var webRoot = _env.WebRootPath;
                var uploadDir = Path.Combine(webRoot, "uploads", "assignments");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                var filePaths = new System.Collections.Generic.List<string>();
                var originalNames = new System.Collections.Generic.List<string>();

                foreach (var file in files.Where(f => f.Length > 0))
                {
                    var uniqueName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var physicalPath = Path.Combine(uploadDir, uniqueName);
                    using (var fs = new FileStream(physicalPath, FileMode.Create))
                    {
                        await file.CopyToAsync(fs);
                    }
                    filePaths.Add(Path.Combine("/uploads", "assignments", uniqueName).Replace("\\", "/"));
                    originalNames.Add(file.FileName);
                }

                if (filePaths.Count > 0)
                {
                    vm.Assignment.FilePath = string.Join(",", filePaths);
                    vm.Assignment.OriginalFileName = string.Join(",", originalNames);
                }
            }

            var isNew = vm.Assignment.Id == 0;
            if (isNew) _unitOfWork.Assignment.Add(vm.Assignment); else _unitOfWork.Assignment.Update(vm.Assignment);
            _unitOfWork.Save();

            var title = isNew ? "Yeni Ödev" : "Ödev Güncellendi";
            var message = isNew ? $"{vm.Assignment.Title} adlý ödev oluþturuldu." : $"{vm.Assignment.Title} adlý ödev güncellendi.";
            await _hub.Clients.All.SendAsync("ReceiveNotification", title, message);

            TempData["admin_success"] = isNew ? "Ödev oluþturuldu." : "Ödev güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteById(int id)
        {
            var item = _unitOfWork.Assignment.Get(o => o.Id == id, includeProperties: "Course,Submissions");
            if (item == null)
            {
                return Json(new { success = false, message = "Ödev bulunamadý." });
            }

            if (!IsAdmin && CurrentUserId != null && item.Course.TeacherId != CurrentUserId)
            {
                return Json(new { success = false, message = "Bu ödevi silme yetkiniz yok." });
            }

            if (item.Submissions != null && item.Submissions.Any())
            {
                foreach (var s in item.Submissions.ToList())
                {
                    _unitOfWork.Submission.Remove(s);
                }
            }

            _unitOfWork.Assignment.Remove(item);
            try
            {
                _unitOfWork.Save();
                await _hub.Clients.All.SendAsync("ReceiveNotification", "Ödev Silindi", $"{item.Title} adlý ödev silindi.");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Silme iþlemi baþarýsýz: " + ex.Message });
            }

            return Json(new { success = true, message = "Ödev baþarýyla silindi." });
        }
    }
}
