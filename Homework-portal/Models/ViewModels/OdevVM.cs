// ---- YENİ DOSYA OLUŞTUR ----
// Dosya Yolu: Homework-portal/Models/ViewModels/OdevVM.cs

using Microsoft.AspNetCore.Mvc.Rendering; // SelectList için
using System.ComponentModel.DataAnnotations;

namespace Homework_portal.Models.ViewModels
{
    public class OdevVM
    {
        // Formda kullanacağımız Odev nesnesi
        public Odev Odev { get; set; }

        // Ödevi atayacağımız dersi seçmek için 
        // Derslerin listesini tutan açılır liste (Dropdown)
        public IEnumerable<SelectListItem>? DersListesi { get; set; }
    }
}