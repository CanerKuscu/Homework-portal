// ---- YENİ DOSYA OLUŞTUR ----
// Dosya Yolu: Homework-portal/Models/ViewModels/OdevlerVM.cs

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Homework_portal.Models.ViewModels
{
    public class OdevlerVM
    {
        // Hangi dersin ödevlerini listeleyeceğimizi bilmek için
        // Dersin kendisini (veya en azından adını) tutalım.
        public Ders Ders { get; set; }

        // O derse ait tüm ödevlerin listesi
        public IEnumerable<Odev> OdevListesi { get; set; }

        // Giriş yapmış öğrencinin o dersteki ödevlere yaptığı teslimlerin listesi
        [ValidateNever]
        public IEnumerable<Teslim> OgrenciTeslimleri { get; set; }
    }
}