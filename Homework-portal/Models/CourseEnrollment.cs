using System.ComponentModel.DataAnnotations;

namespace Homework_portal.Models
{
    public class CourseEnrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;
        public virtual ApplicationUser Student { get; set; } = null!;

        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;
    }
}
