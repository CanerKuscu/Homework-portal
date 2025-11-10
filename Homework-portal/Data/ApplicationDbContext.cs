using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Homework_portal.Models;

namespace Homework_portal.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ders> Dersler { get; set; }
        public DbSet<Odev> Odevler { get; set; }
        public DbSet<Teslim> Teslimler { get; set; }
        public DbSet<DersKayit> DersKayitlari { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Ders>()
                .HasOne(d => d.Ogretmen)
                .WithMany(u => u.VerdigiDersler)
                .HasForeignKey(d => d.OgretmenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DersKayit>()
                .HasOne(dk => dk.Ogrenci)
                .WithMany(u => u.AldigiDersler)
                .HasForeignKey(dk => dk.OgrenciId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DersKayit>()
                .HasOne(dk => dk.Ders)
                .WithMany(d => d.Kayitlar)
                .HasForeignKey(dk => dk.DersId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Teslim>()
                .HasOne(t => t.Ogrenci)
                .WithMany(u => u.Teslimler)
                .HasForeignKey(t => t.OgrenciId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Teslim>()
                .HasOne(t => t.Odev)
                .WithMany(o => o.Teslimler)
                .HasForeignKey(t => t.OdevId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}