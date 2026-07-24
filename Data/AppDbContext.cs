using Microsoft.EntityFrameworkCore;
using test.Models;

namespace test.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMedia> ProjectMedia => Set<ProjectMedia>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<PortfolioMedia> PortfolioMedia => Set<PortfolioMedia>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.UserType).HasConversion<string>();
        });

        modelBuilder.Entity<Project>(e =>
        {
            e.Property(p => p.PropertyType).HasConversion<string>();
            e.HasOne(p => p.User)
             .WithMany(u => u.Projects)
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectMedia>(e =>
        {
            e.Property(m => m.MediaType).HasConversion<string>();
            e.HasOne(m => m.Project)
             .WithMany(p => p.Media)
             .HasForeignKey(m => m.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Portfolio>(e =>
        {
            e.HasOne(p => p.User)
             .WithOne(u => u.Portfolio)
             .HasForeignKey<Portfolio>(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PortfolioMedia>(e =>
        {
            e.Property(m => m.MediaType).HasConversion<string>();
            e.HasOne(m => m.Portfolio)
             .WithMany(p => p.Media)
             .HasForeignKey(m => m.PortfolioId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.HasOne(r => r.User)
             .WithMany(u => u.Reviews)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OtpCode>(e =>
        {
            e.HasOne(o => o.User)
             .WithMany(u => u.OtpCodes)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Favorite>(e =>
        {
            e.HasIndex(f => new { f.UserId, f.EngineerId }).IsUnique();

            e.HasOne(f => f.User)
             .WithMany(u => u.Favorites)
             .HasForeignKey(f => f.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(f => f.Engineer)
             .WithMany()
             .HasForeignKey(f => f.EngineerId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
