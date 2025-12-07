using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Homework_portal.Models;
using Homework_portal.Repository;
using Homework_portal.Utility;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace Homework_portal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Role_Admin + "," + AppRoles.Role_Ogretmen)]
    public class BadgeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public BadgeController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        private bool IsAdmin => User.IsInRole(AppRoles.Role_Admin);
        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetCounts()
        {
            // Yeni teslimler sayýsý (notlandýrýlmamýþ)
            int pendingSubmissions = 0;
            
            if (IsAdmin)
            {
                // Admin tüm notlandýrýlmamýþ teslimleri görür
                pendingSubmissions = _unitOfWork.Submission.GetAll(s => s.Grade == null).Count();
            }
            else if (CurrentUserId != null)
            {
                // Öðretmen sadece kendi derslerindeki teslimleri görür
                var myCourseIds = _unitOfWork.Course
                    .GetAll(c => c.TeacherId == CurrentUserId)
                    .Select(c => c.Id)
                    .ToHashSet();

                var myAssignmentIds = _unitOfWork.Assignment
                    .GetAll(a => myCourseIds.Contains(a.CourseId))
                    .Select(a => a.Id)
                    .ToHashSet();

                pendingSubmissions = _unitOfWork.Submission
                    .GetAll(s => s.Grade == null && myAssignmentIds.Contains(s.AssignmentId))
                    .Count();
            }

            // Bekleyen öðretmen onaylarý (sadece admin görür)
            // OgretmenAday rolündeki kullanýcýlarý say
            int pendingTeachers = 0;
            if (IsAdmin)
            {
                var pendingTeachersList = await _userManager.GetUsersInRoleAsync(AppRoles.Role_OgretmenAday);
                pendingTeachers = pendingTeachersList.Count;
            }

            return Json(new { pendingSubmissions, pendingTeachers });
        }
    }
}
