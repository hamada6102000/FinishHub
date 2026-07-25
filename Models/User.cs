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
    public string? ProfileImageUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public int? TotalExperience { get; set; }
    public UserType UserType { get; set; }
    public string? Bio { get; set; }
    public string? Position { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool IsFavourite { get; set; } = false;

    // Google Login
    public string? GoogleId { get; set; }

    // Navigation
    public City? City { get; set; }
    public List<Project> Projects { get; set; } = new();
    public Portfolio? Portfolio { get; set; }
    public List<Review> Reviews { get; set; } = new();
    public List<OtpCode> OtpCodes { get; set; } = new();
    public List<Favorite> Favorites { get; set; } = new();
}
