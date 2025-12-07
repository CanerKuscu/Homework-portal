using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Homework_portal.Utility;

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
        [FutureDate(ErrorMessage = "Son tarih bugünden sonra olmalýdýr.")]
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

        /// <summary>
        /// Birden fazla dosya yolu için virgülle ayrýlmýþ liste
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Birden fazla orijinal dosya adý için virgülle ayrýlmýþ liste
        /// </summary>
        public string? OriginalFileName { get; set; }

        public virtual ICollection<Submission>? Submissions { get; set; }

        // Helper properties for multiple files
        [ValidateNever]
        public string[] FilePaths => string.IsNullOrEmpty(FilePath) ? Array.Empty<string>() : FilePath.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        [ValidateNever]
        public string[] OriginalFileNames => string.IsNullOrEmpty(OriginalFileName) ? Array.Empty<string>() : OriginalFileName.Split(',', StringSplitOptions.RemoveEmptyEntries);
    }
}
