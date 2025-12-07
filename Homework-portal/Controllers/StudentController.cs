using System;
using System.Linq;
using System.Security.Claims;
using Homework_portal.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Homework_portal.Repository;
using Homework_portal.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Homework_portal.Controllers
{
    [Authorize(Roles = Utility.AppRoles.Role_Ogrenci)]
    public class StudentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("/Ogrenci/Derslerim")]
        [HttpGet("/Student/MyCourses")] // English alias
        public IActionResult MyCourses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var student = _unitOfWork.User.Get(u => u.Id == userId, tracked: true);

            var enrolledIds = _unitOfWork.CourseEnrollment
                .GetAll(e => e.StudentId == userId, tracked: true)
                .Select(e => e.CourseId)
                .Distinct()
                .ToList();

            List<Course> courses;
            if (enrolledIds.Any())
            {
                courses = _unitOfWork.Course.GetAll(c => enrolledIds.Contains(c.Id), includeProperties: "Teacher", tracked: true).ToList();
            }
            else
            {
                courses = _unitOfWork.Course.GetAll(includeProperties: "Teacher", tracked: true).ToList();
                if (courses.Any())
                {
                    ViewBag.ShowAllCoursesInfo = true;
                }
            }

            // Her ders için teslim edilmemiþ ödev sayýsýný hesapla
            var result = new List<CourseWithPendingVM>();
            
            foreach (var course in courses)
            {
                // Bu derse ait ve öðrencinin sýnýf/þubesine uygun ödevleri al
                var assignments = _unitOfWork.Assignment.GetAll(a => a.CourseId == course.Id
                    && (string.IsNullOrEmpty(a.Class) || a.Class == student!.Sinif)
                    && (string.IsNullOrEmpty(a.Branch) || a.Branch == student!.Sube),
                    tracked: true
                ).ToList();

                var assignmentIds = assignments.Select(a => a.Id).ToHashSet();

                // Öðrencinin bu dersteki teslimlerini al
                var submittedAssignmentIds = _unitOfWork.Submission
                    .GetAll(s => s.StudentId == userId && assignmentIds.Contains(s.AssignmentId), tracked: true)
                    .Select(s => s.AssignmentId)
                    .ToHashSet();

                // Teslim edilmemiþ ödev sayýsý
                var pendingCount = assignmentIds.Count - submittedAssignmentIds.Count;

                result.Add(new CourseWithPendingVM
                {
                    Course = course,
                    PendingAssignmentCount = pendingCount > 0 ? pendingCount : 0
                });
            }

            return View(result);
        }

        [HttpGet("/Ogrenci/Odevler/{id:int}")]
        [HttpGet("/Student/Assignments/{id:int}")] // English alias
        public IActionResult Assignments(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var course = _unitOfWork.Course.Get(d => d.Id == id, tracked: true);
            if (course == null)
            {
                return NotFound();
            }

            var student = _unitOfWork.User.Get(u => u.Id == userId, tracked: true);

            var vm = new AssignmentsVM { Course = course };

            vm.Assignments = _unitOfWork.Assignment.GetAll(o => o.CourseId == id
                && (string.IsNullOrEmpty(o.Class) || o.Class == student!.Sinif)
                && (string.IsNullOrEmpty(o.Branch) || o.Branch == student!.Sube),
                tracked: true
            ).ToList();

            var allAssignmentIds = vm.Assignments.Select(o => o.Id).ToHashSet();

            vm.StudentSubmissions = _unitOfWork.Submission.GetAll(
                t => t.StudentId == userId && allAssignmentIds.Contains(t.AssignmentId),
                tracked: true
            ).ToList();

            return View(vm);
        }

        // Submit GET
        [HttpGet("/Ogrenci/Submit/{id:int}")]
        [HttpGet("/Student/Submit/{id:int}")] // English alias
        public IActionResult Submit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var assignment = _unitOfWork.Assignment.Get(a => a.Id == id, includeProperties: "Course", tracked: true);
            if (assignment == null)
            {
                return NotFound();
            }

            var vm = new SubmissionVM
            {
                Assignment = assignment,
                Submission = new Submission { AssignmentId = assignment.Id }
            };

            return View(vm);
        }

        // Submit POST
        [HttpPost("/Ogrenci/Submit/{id:int}")]
        [HttpPost("/Student/Submit/{id:int}")] // English alias
        [ValidateAntiForgeryToken]
        public IActionResult Submit(int id, SubmissionVM vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var assignment = _unitOfWork.Assignment.Get(a => a.Id == vm.Submission.AssignmentId, includeProperties: "Course", tracked: true);
            if (assignment == null)
            {
                return NotFound();
            }

            if (vm.Files == null || vm.Files.Count == 0 || vm.Files.All(f => f.Length == 0))
            {
                ModelState.AddModelError("Files", "Lütfen en az bir dosya seçin.");
            }

            if (!ModelState.IsValid)
            {
                vm.Assignment = assignment;
                return View(vm);
            }

            // Save uploaded files
            var webRoot = _webHostEnvironment.WebRootPath;
            var uploadDir = Path.Combine(webRoot, "uploads", "submissions");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            var filePaths = new List<string>();
            var originalNames = new List<string>();

            foreach (var file in vm.Files!.Where(f => f.Length > 0))
            {
                var uniqueName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var physicalPath = Path.Combine(uploadDir, uniqueName);
                using (var fs = new FileStream(physicalPath, FileMode.Create))
                {
                    file.CopyTo(fs);
                }
                var relativePath = Path.Combine("/uploads", "submissions", uniqueName).Replace("\\", "/");
                filePaths.Add(relativePath);
                originalNames.Add(file.FileName);
            }

            // Create or update submission
            var existing = _unitOfWork.Submission.Get(s => s.StudentId == userId && s.AssignmentId == vm.Submission.AssignmentId, tracked: true);
            if (existing == null)
            {
                vm.Submission.StudentId = userId;
                vm.Submission.SubmittedAt = DateTime.Now;
                vm.Submission.FilePath = string.Join(",", filePaths);
                vm.Submission.OriginalFileName = string.Join(",", originalNames);
                _unitOfWork.Submission.Add(vm.Submission);
            }
            else
            {
                existing.SubmittedAt = DateTime.Now;
                existing.FilePath = string.Join(",", filePaths);
                existing.OriginalFileName = string.Join(",", originalNames);
                _unitOfWork.Submission.Update(existing);
            }

            _unitOfWork.Save();

            TempData["success"] = "Ödev baþarýlý þekilde teslim edildi.";
            return RedirectToAction(nameof(Assignments), new { id = assignment.CourseId });
        }
    }
}
