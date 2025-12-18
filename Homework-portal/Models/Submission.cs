using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homework_portal.Models
{
    public class Submission
    {
        [Key]
        public int Id { get; set; }

        public DateTime SubmittedAt { get; set; }
        
        
        public string? FilePath { get; set; }
        
       
        public string? OriginalFileName { get; set; }
        
        public int? Grade { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [ValidateNever]
        public virtual Assignment Assignment { get; set; } = null!;

        [ValidateNever]
        public string StudentId { get; set; } = string.Empty;

        [ValidateNever]
        public virtual ApplicationUser Student { get; set; } = null!;

        
        [ValidateNever]
        public string[] FilePaths => string.IsNullOrEmpty(FilePath) ? Array.Empty<string>() : FilePath.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        [ValidateNever]
        public string[] OriginalFileNames => string.IsNullOrEmpty(OriginalFileName) ? Array.Empty<string>() : OriginalFileName.Split(',', StringSplitOptions.RemoveEmptyEntries);
    }
}
