using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homework_portal.Models
{
    [Table("Odevler")]
    public class Assignment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Lütfen bir baþlýk girin.")]
        [StringLength(200)]
        [Column("Baslik")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Lütfen bir açýklama girin.")]
        [Column("Aciklama")]
        public string Description { get; set; } = null!;

        [Column("OlusturmaTarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Lütfen bir teslim tarihi seçin.")]
        [Display(Name = "Teslim Tarihi")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        [Column("TeslimTarihi")]
        public DateTime DueDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Lütfen geçerli bir ders seçin.")]
        [Display(Name = "Ders")]
        [Column("DersId")]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        [ValidateNever]
        public virtual Course Course { get; set; } = null!;

        [StringLength(10)]
        [Column("Sinif")]
        public string? Class { get; set; }

        [StringLength(10)]
        [Column("Sube")]
        public string? Branch { get; set; }

        [Column("DosyaYolu")]
        public string? FilePath { get; set; }

        [Column("OrjinalDosyaAdi")]
        public string? OriginalFileName { get; set; }

        public virtual ICollection<Submission>? Submissions { get; set; }
    }
}
