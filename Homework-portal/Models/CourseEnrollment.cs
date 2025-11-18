using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homework_portal.Models
{
    [Table("DersKayitlari")]
    public class CourseEnrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("OgrenciId")]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public virtual ApplicationUser Student { get; set; } = null!;

        [Required]
        [Column("DersId")]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; } = null!;
    }
}
