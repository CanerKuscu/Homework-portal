using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homework_portal.Models
{
    [Table("Teslimler")]
    public class Submission
    {
        [Key]
        public int Id { get; set; }

        [Column("TeslimTarihi")]
        public DateTime SubmittedAt { get; set; }

        [Column("DosyaYolu")]
        public string? FilePath { get; set; }

        [Column("OrjinalDosyaAdi")]
        public string? OriginalFileName { get; set; }

        [Column("Not")]
        public int? Grade { get; set; }

        [Required]
        [Column("OdevId")]
        public int AssignmentId { get; set; }

        [ForeignKey(nameof(AssignmentId))]
        [ValidateNever]
        public virtual Assignment Assignment { get; set; } = null!;

        [Required]
        [Column("OgrenciId")]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        [ValidateNever]
        public virtual ApplicationUser Student { get; set; } = null!;
    }
}
