using System.ComponentModel.DataAnnotations;

namespace Homework_portal.Models.ViewModels
{
    public class ProfileVM
    {
        [Required(ErrorMessage = "Ad gereklidir.")]
        [StringLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad gereklidir.")]
        [StringLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Öðrenci No")]
        public string? OgrenciNo { get; set; }

        [Display(Name = "Sýnýf")]
        public string? Sinif { get; set; }

        [Display(Name = "Þube")]
        public string? Sube { get; set; }
    }
}
