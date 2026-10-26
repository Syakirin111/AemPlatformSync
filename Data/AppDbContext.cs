using AemPlatformSync.Models;
using Microsoft.EntityFrameworkCore;

namespace AemPlatformSync.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Platform> Platforms => Set<Platform>();

    public DbSet<Well> Wells => Set<Well>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Platform>(entity =>
        {
            entity.ToTable("Platform");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id)
                .ValueGeneratedNever();

            entity.Property(p => p.UniqueName)
                .HasMaxLength(200);

            entity.HasMany(p => p.Wells)
                .WithOne(w => w.Platform)
                .HasForeignKey(w => w.PlatformId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Well>(entity =>
        {
            entity.ToTable("Well");
            entity.HasKey(w => w.Id);

            entity.Property(w => w.Id)
                .ValueGeneratedNever();

            entity.Property(w => w.UniqueName)
                .HasMaxLength(200);

            entity.HasIndex(w => w.PlatformId);
        });
    }
}