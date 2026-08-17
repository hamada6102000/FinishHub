namespace test.Models;

/// <summary>
/// A user type an administrator can manage from the Dashboard (User, Engineer, Carpenter,
/// Plumber, Electrician, ...). Adding a type is data entry, not a deployment.
/// </summary>
public class UserType
{
    /// <summary>Seeded row id for the built-in "User" type. Matches UserTypeKind.User.</summary>
    public const int UserId = 1;

    /// <summary>Seeded row id for the built-in "Engineer" type. Matches UserTypeKind.Engineer.</summary>
    public const int EngineerId = 2;

    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// Stable, immutable key for the two built-in types ("USER", "ENGINEER"). Null for every
    /// administrator-created type. Code looks types up by this instead of by display name,
    /// which administrators are free to rename.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Which behaviour bucket users of this type belong to. Administrator-created types are
    /// always <see cref="UserTypeKind.User"/>, so only the built-in Engineer type is treated as
    /// an engineer by /api/engineers, favourites and design conversations.
    /// </summary>
    public UserTypeKind Kind { get; set; } = UserTypeKind.User;

    /// <summary>
    /// True for the two built-in types. System types cannot be renamed, deactivated or deleted
    /// because application logic depends on them existing.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Inactive types stay on the users already assigned to them but disappear from signup and
    /// cannot be assigned to anyone new.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public List<User> Users { get; set; } = new();
}
