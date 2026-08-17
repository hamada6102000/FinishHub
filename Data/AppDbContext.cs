using Microsoft.EntityFrameworkCore;
using test.Helpers;
using test.Models;

namespace test.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserType> UserTypes => Set<UserType>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMedia> ProjectMedia => Set<ProjectMedia>();
    public DbSet<ProjectMaterial> ProjectMaterials => Set<ProjectMaterial>();
    public DbSet<ProjectRate> ProjectRates => Set<ProjectRate>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<PortfolioMedia> PortfolioMedia => Set<PortfolioMedia>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<DesignConversationRequest> DesignConversationRequests => Set<DesignConversationRequest>();
    public DbSet<WhatsAppNumber> WhatsAppNumbers => Set<WhatsAppNumber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.UserType).HasConversion<string>();
            e.HasOne(u => u.City)
             .WithMany(c => c.Users)
             .HasForeignKey(u => u.CityId)
             .OnDelete(DeleteBehavior.Restrict);

            // Restrict: a type that is still assigned to users can never be deleted out from
            // under them, which is what keeps existing users from losing their type.
            e.HasOne(u => u.Type)
             .WithMany(t => t.Users)
             .HasForeignKey(u => u.UserTypeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserType>(e =>
        {
            e.Property(t => t.NameEn).IsRequired().HasMaxLength(100);
            e.Property(t => t.NameAr).IsRequired().HasMaxLength(100);
            e.Property(t => t.Code).HasMaxLength(50);
            e.Property(t => t.Kind).HasConversion<string>();

            e.HasIndex(t => t.NameEn).IsUnique();
            e.HasIndex(t => t.NameAr).IsUnique();
            e.HasIndex(t => t.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
        });

        modelBuilder.Entity<City>(e =>
        {
            e.HasIndex(c => c.NameEn).IsUnique();
            e.HasIndex(c => c.NameAr).IsUnique();
        });

        modelBuilder.Entity<Project>(e =>
        {
            e.Property(p => p.PropertyType).HasConversion<string>();
            e.HasOne(p => p.User)
             .WithMany(u => u.Projects)
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.City)
             .WithMany()
             .HasForeignKey(p => p.CityId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectMedia>(e =>
        {
            e.Property(m => m.MediaType).HasConversion<string>();
            e.HasOne(m => m.Project)
             .WithMany(p => p.Media)
             .HasForeignKey(m => m.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectMaterial>(e =>
        {
            e.HasOne(m => m.Project)
             .WithMany(p => p.Materials)
             .HasForeignKey(m => m.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectRate>(e =>
        {
            e.HasOne(r => r.Project)
             .WithMany()
             .HasForeignKey(r => r.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.User)
             .WithMany()
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Restrict);
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

            e.HasOne(r => r.Reviewer)
             .WithMany()
             .HasForeignKey(r => r.ReviewerId)
             .OnDelete(DeleteBehavior.Restrict);
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

        modelBuilder.Entity<DesignConversationRequest>(e =>
        {
            e.Property(r => r.Service).HasConversion<string>();
            e.Property(r => r.FullName).HasMaxLength(200);
            e.Property(r => r.WhatsAppNumber).HasMaxLength(30);

            e.HasIndex(r => r.UserId);
            e.HasIndex(r => r.EngineerId);

            e.HasOne(r => r.User)
             .WithMany()
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.Engineer)
             .WithMany()
             .HasForeignKey(r => r.EngineerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.City)
             .WithMany()
             .HasForeignKey(r => r.CityId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Global, solution-level WhatsApp number: exactly zero or one row, ever.
        modelBuilder.Entity<WhatsAppNumber>(e =>
        {
            // The key is assigned by the application (always WhatsAppNumber.SingletonId),
            // not by an identity column, so the check constraint below can pin it to a single row.
            e.Property(w => w.Id).ValueGeneratedNever();

            e.Property(w => w.PhoneNumber)
             .IsRequired()
             .HasMaxLength(PhoneNumberHelper.MaxLength);

            e.HasIndex(w => w.PhoneNumber).IsUnique();

            e.ToTable(t => t.HasCheckConstraint(
                "CK_WhatsAppNumbers_SingleRow",
                $"[Id] = {WhatsAppNumber.SingletonId}"));
        });
    }
}
