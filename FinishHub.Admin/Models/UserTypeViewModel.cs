using System.ComponentModel.DataAnnotations;

namespace FinishHub.Admin.Models;

public class UserTypeViewModel
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    /// <summary>Stable key for the built-in types ("USER", "ENGINEER"); null for custom types.</summary>
    public string? Code { get; set; }

    /// <summary>Built-in types cannot be deactivated or deleted.</summary>
    public bool IsSystem { get; set; }

    public bool IsActive { get; set; }

    /// <summary>Users currently holding this type, shown before deactivating.</summary>
    public int UsersCount { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class UserTypeFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Arabic name is required.")]
    [StringLength(100, ErrorMessage = "Arabic name cannot exceed 100 characters.")]
    [Display(Name = "Name (Arabic)")]
    public string NameAr { get; set; } = string.Empty;

    [Required(ErrorMessage = "English name is required.")]
    [StringLength(100, ErrorMessage = "English name cannot exceed 100 characters.")]
    [Display(Name = "Name (English)")]
    public string NameEn { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    /// <summary>Built-in types render the Active checkbox disabled — they cannot be deactivated.</summary>
    public bool IsSystem { get; set; }
}
