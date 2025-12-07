using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homework_portal.Models
{
    public class Assignment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Lütfen bir baþlýk girin.")]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Lütfen bir açýklama girin.")]
        public string Description { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Lütfen bir teslim tarihi seçin.")]
        [Display(Name = "Teslim Tarihi")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Lütfen geçerli bir ders seçin.")]
        [Display(Name = "Ders")]
        public int CourseId { get; set; }

        [ValidateNever]
        public virtual Course Course { get; set; } = null!;

        [StringLength(10)]
        public string? Class { get; set; }

        [StringLength(10)]
        public string? Branch { get; set; }

        public string? FilePath { get; set; }

        public string? OriginalFileName { get; set; }

        public virtual ICollection<Submission>? Submissions { get; set; }
    }
}
