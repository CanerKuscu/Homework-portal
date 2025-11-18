using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homework_portal.Models
{
    [Table("Dersler")]
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Column("Ad")]
        public string Name { get; set; } = null!;

        [StringLength(30)]
        [Column("Kod")]
        public string? Code { get; set; }

        [StringLength(500)]
        [Column("Aciklama")]
        public string? Description { get; set; }

        [Required]
        [Column("OgretmenId")]
        public string TeacherId { get; set; } = null!;

        [ForeignKey(nameof(TeacherId))]
        [ValidateNever]
        public ApplicationUser Teacher { get; set; } = null!;

        public ICollection<Assignment>? Assignments { get; set; }
        public ICollection<CourseEnrollment>? Enrollments { get; set; }
    }
}
