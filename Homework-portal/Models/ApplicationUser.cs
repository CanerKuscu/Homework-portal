using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Homework_portal.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Soyad { get; set; } = string.Empty;

        // Öğrenciler için sınıf ve şube (opsiyonel)
        [StringLength(10)]
        public string? Sinif { get; set; }

        [StringLength(10)]
        public string? Sube { get; set; }

        // Yeni: Öğrenci numarası (opsiyonel). Öğrenciler için doldurulur.
        [StringLength(30)]
        [Display(Name = "Öğrenci No")]
        public string? OgrenciNo { get; set; }

        public virtual ICollection<Course>? VerdigiDersler { get; set; }
        public virtual ICollection<CourseEnrollment>? AldigiDersler { get; set; }
        public virtual ICollection<Submission>? Teslimler { get; set; }
    }
}