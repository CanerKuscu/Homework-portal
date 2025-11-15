// ---- YENİ DOSYA OLUŞTUR ----
// Dosya Yolu: Homework-portal/Models/ViewModels/TeslimVM.cs

using System.ComponentModel.DataAnnotations;

namespace Homework_portal.Models.ViewModels
{
    public class TeslimVM
    {
        // Formda göstermek için Ödev detaylarını tutacağız
        public Odev Odev { get; set; }

        // Formdan yüklenecek dosyayı temsil eden nesne
        [Required(ErrorMessage = "Lütfen bir dosya seçin.")]
        [Display(Name = "Teslim Dosyası")]
        public IFormFile Dosya { get; set; }

        // Veritabanına kaydedilecek olan Teslim nesnesi
        // (Bu, POST işleminde doldurulacak)
        public Teslim Teslim { get; set; }
    }
}