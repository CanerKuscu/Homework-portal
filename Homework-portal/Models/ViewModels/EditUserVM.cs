using System.ComponentModel.DataAnnotations;

namespace Homework_portal.Models.ViewModels
{
    public class EditUserVM
    {
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [StringLength(30)]
        [Display(Name = "Öðrenci No")]
        public string? OgrenciNo { get; set; }

        [StringLength(10)]
        [Display(Name = "Sýnýf")]
        public string? Sinif { get; set; }

        [StringLength(10)]
        [Display(Name = "Þube")]
        public string? Sube { get; set; }

        public string CurrentRole { get; set; } = string.Empty;
    }
}
