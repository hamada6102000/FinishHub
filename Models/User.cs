namespace test.Models;

public class User
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int? CityId { get; set; }
    public string? Country { get; set; }
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public int? TotalExperience { get; set; }

    /// <summary>
    /// Behaviour bucket, kept in sync from <see cref="Type"/>.Kind whenever the type is assigned.
    /// Existing engineer/client rules read this, so they need no join and keep behaving exactly
    /// as before. The user-facing label is <see cref="Type"/>, not this.
    /// </summary>
    public UserTypeKind UserType { get; set; }

    /// <summary>The configurable user type this user belongs to.</summary>
    public int UserTypeId { get; set; } = Models.UserType.UserId;

    public string? Bio { get; set; }
    public string Position { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool IsFavourite { get; set; } = false;
    public bool IsTrusted { get; set; } = false;

    // Google Login
    public string? GoogleId { get; set; }

    // Navigation
    public City? City { get; set; }
    public UserType? Type { get; set; }
    public List<Project> Projects { get; set; } = new();
    public Portfolio? Portfolio { get; set; }
    public List<Review> Reviews { get; set; } = new();
    public List<OtpCode> OtpCodes { get; set; } = new();
    public List<Favorite> Favorites { get; set; } = new();
}
