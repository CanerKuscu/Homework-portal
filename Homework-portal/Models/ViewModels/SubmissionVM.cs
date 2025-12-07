using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Http;

namespace Homework_portal.Models.ViewModels
{
    public class SubmissionVM
    {
        [ValidateNever]
        public Assignment? Assignment { get; set; }
        public Submission Submission { get; set; } = new Submission();

        [Required(ErrorMessage = "Lütfen en az bir dosya seçin.")]
        [Display(Name = "Dosyalar")]
        public List<IFormFile>? Files { get; set; }
    }
}
