using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // 1. BU SATIRI EKLEDİK

namespace Homework_portal.Models
{
    public class Ders
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Ad { get; set; }

        [StringLength(500)]
        public string? Aciklama { get; set; }

        // Foreign Key
        [Required]
        public string OgretmenId { get; set; }

        [ForeignKey("OgretmenId")]
        [ValidateNever] // 2. BU SATIRI EKLEDİK
        public virtual ApplicationUser Ogretmen { get; set; }

        // İlişkili ödevler
        public virtual ICollection<Odev>? Odevler { get; set; }

        // Derse kayıtlı öğrenciler
        public virtual ICollection<DersKayit>? Kayitlar { get; set; }
    }
}