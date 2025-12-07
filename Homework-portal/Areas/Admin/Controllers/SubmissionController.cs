using Homework_portal.Models;
using Homework_portal.Repository;
using Homework_portal.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace Homework_portal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Role_Admin + "," + AppRoles.Role_Ogretmen)]
    public class SubmissionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubmissionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private bool IsAdmin => User.IsInRole(AppRoles.Role_Admin);
        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Kurs seçimi ekraný (kurs -> ödevler -> teslimler)
        [HttpGet]
        public IActionResult Index()
        {
            var courses = _unitOfWork.Course.GetAll(includeProperties: "Teacher").ToList();
            if (!IsAdmin && CurrentUserId != null)
            {
                courses = courses.Where(c => c.TeacherId == CurrentUserId).ToList();
            }
            return View("Courses", courses);
        }

        // Seçilen kursun tüm ödevleri
        [HttpGet]
        public IActionResult Course(int id)
        {
            var course = _unitOfWork.Course.Get(c => c.Id == id, tracked: true);
            if (course == null) return NotFound();
            if (!IsAdmin && CurrentUserId != null && course.TeacherId != CurrentUserId)
            {
                return Forbid();
            }

            var v = new Models.ViewModels.AdminCourseAssignmentsVM
            {
                Course = course,
                Assignments = _unitOfWork.Assignment.GetAll(a => a.CourseId == id, tracked: true).OrderByDescending(a => a.DueDate).ToList()
            };
            return View("CourseAssignments", v);
        }

        // Bir ödev için teslim edenler listesi + arama
        [HttpGet]
        public IActionResult ForAssignment(int id, string? q)
        {
            var assignment = _unitOfWork.Assignment.Get(a => a.Id == id, includeProperties: "Course", tracked: true);
            if (assignment == null) return NotFound();
            if (!IsAdmin && CurrentUserId != null && assignment.Course.TeacherId != CurrentUserId)
            {
                return Forbid();
            }

            var list = _unitOfWork.Submission
                .GetAll(s => s.AssignmentId == id, includeProperties: "Student", tracked: true)
                .OrderByDescending(s => s.SubmittedAt)
                .ToList();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLowerInvariant();
                list = list.Where(s =>
                        (!string.IsNullOrWhiteSpace(s.Student.FirstName) && s.Student.FirstName.ToLowerInvariant().Contains(term)) ||
                        (!string.IsNullOrWhiteSpace(s.Student.LastName) && s.Student.LastName.ToLowerInvariant().Contains(term)) ||
                        ($"{s.Student.FirstName} {s.Student.LastName}".ToLowerInvariant().Contains(term)) ||
                        (!string.IsNullOrWhiteSpace(s.Student.StudentNumber) && s.Student.StudentNumber.ToLowerInvariant().Contains(term))
                    ).ToList();
            }

            ViewBag.Query = q;
            var v = new Models.ViewModels.AdminAssignmentSubmissionsVM
            {
                Assignment = assignment,
                Submissions = list
            };
            return View("AssignmentSubmissions", v);
        }

        // Bir ödev için teslim etmeyenler listesi + arama
        [HttpGet]
        public IActionResult MissingForAssignment(int id, string? q)
        {
            var assignment = _unitOfWork.Assignment.Get(a => a.Id == id, includeProperties: "Course", tracked: true);
            if (assignment == null) return NotFound();
            if (!IsAdmin && CurrentUserId != null && assignment.Course.TeacherId != CurrentUserId)
            {
                return Forbid();
            }

            var enrolled = _unitOfWork.CourseEnrollment.GetAll(dk => dk.CourseId == assignment.CourseId, includeProperties: "Student", tracked: true)
                .Select(dk => dk.Student)
                .ToList();

            var submittedIds = _unitOfWork.Submission.GetAll(s => s.AssignmentId == id, tracked: true).Select(s => s.StudentId).ToHashSet();

            var notSubmitted = enrolled.Where(s => !submittedIds.Contains(s.Id)).ToList();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLowerInvariant();
                notSubmitted = notSubmitted.Where(s =>
                        (!string.IsNullOrWhiteSpace(s.FirstName) && s.FirstName.ToLowerInvariant().Contains(term)) ||
                        (!string.IsNullOrWhiteSpace(s.LastName) && s.LastName.ToLowerInvariant().Contains(term)) ||
                        ($"{s.FirstName} {s.LastName}".ToLowerInvariant().Contains(term)) ||
                        (!string.IsNullOrWhiteSpace(s.StudentNumber) && s.StudentNumber.ToLowerInvariant().Contains(term))
                    ).ToList();
            }

            ViewBag.Assignment = assignment;
            ViewBag.Query = q;
            return View("AssignmentMissing", notSubmitted);
        }

        [HttpGet]
        public IActionResult Detail(int id)
        {
            var sub = _unitOfWork.Submission.Get(t => t.Id == id, includeProperties: "Student,Assignment,Assignment.Course");
            if (sub == null)
                return NotFound();
            if (!IsAdmin && CurrentUserId != null && sub.Assignment.Course.TeacherId != CurrentUserId)
            {
                return Forbid();
            }
            return View(sub);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Detail(Submission model)
        {
            var subDb = _unitOfWork.Submission.Get(t => t.Id == model.Id);
            if (subDb == null)
                return NotFound();

            subDb.Grade = model.Grade;
            _unitOfWork.Submission.Update(subDb);
            _unitOfWork.Save();
            TempData["admin_success"] = "Not kaydedildi.";
            return RedirectToAction("ForAssignment", new { id = subDb.AssignmentId });
        }
    }
}
