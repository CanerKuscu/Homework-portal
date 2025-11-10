using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homework_portal.Models
{
    public class Teslim
    {
        [Key]
        public int Id { get; set; }

        public DateTime TeslimTarihi { get; set; } = DateTime.Now;

        public string? DosyaYolu { get; set; }

        public int? Not { get; set; }

        [Required]
        public int OdevId { get; set; }

        [ForeignKey("OdevId")]
        public virtual Odev Odev { get; set; }

        [Required]
        public string OgrenciId { get; set; }

        [ForeignKey("OgrenciId")]
        public virtual ApplicationUser Ogrenci { get; set; }
    }
}