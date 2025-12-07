using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Homework_portal.Models
{
    public class ApplicationUser : IdentityUser
    {
        // English properties persisted to DB
        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Class { get; set; }

        [StringLength(10)]
        public string? Branch { get; set; }

        [StringLength(30)]
        public string? StudentNumber { get; set; }

        public virtual ICollection<Course>? TeachingCourses { get; set; }
        public virtual ICollection<CourseEnrollment>? EnrolledCourses { get; set; }
        public virtual ICollection<Submission>? Submissions { get; set; }

        // Turkish alias properties (not mapped) to keep existing code compiling
        [NotMapped]
        public string Ad { get => FirstName; set => FirstName = value; }

        [NotMapped]
        public string Soyad { get => LastName; set => LastName = value; }

        [NotMapped]
        public string? Sinif { get => Class; set => Class = value; }

        [NotMapped]
        public string? Sube { get => Branch; set => Branch = value; }

        [NotMapped]
        public string? OgrenciNo { get => StudentNumber; set => StudentNumber = value; }

        [NotMapped]
        public virtual ICollection<Course>? VerdigiDersler { get => TeachingCourses; set => TeachingCourses = value; }

        [NotMapped]
        public virtual ICollection<CourseEnrollment>? AldigiDersler { get => EnrolledCourses; set => EnrolledCourses = value; }

        [NotMapped]
        public virtual ICollection<Submission>? Teslimler { get => Submissions; set => Submissions = value; }
    }
}