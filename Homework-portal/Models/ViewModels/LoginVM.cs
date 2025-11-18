using System.ComponentModel.DataAnnotations;

namespace Homework_portal.Models.ViewModels
{
    public class LoginVM
    {
        // Tek kutu: e‑posta veya öğrenci numarası
        [Required(ErrorMessage = "E-posta veya Öğrenci No gereklidir.")]
        [Display(Name = "E-posta veya Öğrenci No")]
        public string Identifier { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}