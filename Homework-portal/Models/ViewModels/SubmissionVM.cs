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

        [Required(ErrorMessage = "Please select a file.")]
        [Display(Name = "Submission File")]
        public IFormFile? File { get; set; }
    }
}
