using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homework_portal.Models
{
    public class Odev
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Baslik { get; set; }

        [Required]
        public string Aciklama { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        [Required]
        public DateTime TeslimTarihi { get; set; }

        [Required]
        public int DersId { get; set; }

        [ForeignKey("DersId")]
        public virtual Ders Ders { get; set; }

        public virtual ICollection<Teslim>? Teslimler { get; set; }
    }
}