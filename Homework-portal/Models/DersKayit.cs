using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homework_portal.Models
{
    public class DersKayit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OgrenciId { get; set; }

        [ForeignKey("OgrenciId")]
        public virtual ApplicationUser Ogrenci { get; set; }

        [Required]
        public int DersId { get; set; }

        [ForeignKey("DersId")]
        public virtual Ders Ders { get; set; }
    }
}