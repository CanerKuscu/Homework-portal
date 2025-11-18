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
    [Route("Ogrenci")]
    [Route("Student")] // English alias
    public class StudentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("Derslerim")]
        [HttpGet("MyCourses")]
        public IActionResult MyCourses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            // Önce kayýt olunan ders Id'lerini al (Course navigasyonuna güvenmeden)
            var enrolledIds = _unitOfWork.CourseEnrollment
                .GetAll(e => e.StudentId == userId, tracked: true)
                .Select(e => e.CourseId)
                .Distinct()
                .ToList();

            List<Course> myCourses;
            if (enrolledIds.Any())
            {
                myCourses = _unitOfWork.Course.GetAll(c => enrolledIds.Contains(c.Id), tracked: true).ToList();
            }
            else
            {
                // Öðrencinin kaydý yoksa tüm mevcut dersleri göster ama sayfa içi bilgi ver
                myCourses = _unitOfWork.Course.GetAll(tracked: true).ToList();
                if (myCourses.Any())
                {
                    ViewBag.ShowAllCoursesInfo = true;
                }
            }

            return View(myCourses);
        }

        [HttpGet("Odevler/{id:int}")]
        [HttpGet("Assignments/{id:int}")]
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

        // Submit GET/POST: unchanged
    }
}
