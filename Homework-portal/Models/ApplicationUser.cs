using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Homework_portal.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string Ad { get; set; }

        [Required]
        [StringLength(50)]
        public string Soyad { get; set; }

        public virtual ICollection<Ders>? VerdigiDersler { get; set; }
        public virtual ICollection<DersKayit>? AldigiDersler { get; set; }
        public virtual ICollection<Teslim>? Teslimler { get; set; }
    }
}