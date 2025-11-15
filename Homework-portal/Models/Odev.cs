// ---- GÜNCEL VE TAM KOD ----
// Dosya Yolu: Homework-portal/Models/Odev.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homework_portal.Models
{
    public class Odev
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Lütfen bir başlık girin.")]
        [StringLength(200)]
        public string Baslik { get; set; }

        [Required(ErrorMessage = "Lütfen bir açıklama girin.")]
        public string Aciklama { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        // Tarih formatı (localization) hatasını çözmek için eklendi
        [Required(ErrorMessage = "Lütfen bir teslim tarihi seçin.")]
        [Display(Name = "Teslim Tarihi")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime TeslimTarihi { get; set; }


        // "The Ders field is required" hatasını çözmek için [Required] yerine [Range] eklendi
        [Range(1, int.MaxValue, ErrorMessage = "Lütfen geçerli bir ders seçin.")]
        [Display(Name = "Ders")]
        public int DersId { get; set; }


        [ForeignKey("DersId")]
        public virtual Ders Ders { get; set; }

        public virtual ICollection<Teslim>
    ? Teslimler
        { get; set; }
    }
}
