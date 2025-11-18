using System.ComponentModel.DataAnnotations;

namespace Homework_portal.Models.ViewModels
{
    public class ChangePasswordVM
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Þifre")]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Þifre")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Þifre (Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Þifreler uyuþmuyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
