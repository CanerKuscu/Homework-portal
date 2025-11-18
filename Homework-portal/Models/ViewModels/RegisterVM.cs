using System.ComponentModel.DataAnnotations;

namespace Homework_portal.Models.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Ad gereklidir.")]
        [StringLength(50)]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Soyad gereklidir.")]
        [StringLength(50)]
        public string Soyad { get; set; }

        // Opsiyonel: Öğrenci seçilirse zorunlu olacak
        [Display(Name = "Öğrenci No")]
        public string? OgrenciNo { get; set; }

        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter uzunluğunda olmalı.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Şifre (Tekrar)")]
        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Rol seçimi gereklidir.")]
        public string Role { get; set; } // Kullanıcı rolünü (Ogretmen, Ogrenci) tutacağız
    }
}