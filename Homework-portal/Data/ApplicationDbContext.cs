using Homework_portal.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Homework_portal.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Dersler { get; set; } = null!;
        public DbSet<Assignment> Odevler { get; set; } = null!;
        public DbSet<Submission> Teslimler { get; set; } = null!;
        public DbSet<CourseEnrollment> DersKayitlari { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Course>()
                .HasOne(d => d.Teacher)
                .WithMany(u => u.VerdigiDersler!)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Assignment>()
                .HasOne(o => o.Course)
                .WithMany(d => d.Assignments!)
                .HasForeignKey(o => o.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseEnrollment>()
                .HasOne(dk => dk.Course)
                .WithMany(d => d.Enrollments)
                .HasForeignKey(dk => dk.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Submission>()
                .HasOne(t => t.Student)
                .WithMany(u => u.Teslimler)
                .HasForeignKey(t => t.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Submission>()
                .HasOne(t => t.Assignment)
                .WithMany(o => o.Submissions)
                .HasForeignKey(t => t.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}