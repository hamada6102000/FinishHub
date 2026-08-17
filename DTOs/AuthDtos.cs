using test.Models;

namespace test.DTOs;

// ---------- Auth ----------

public class RegisterRequest
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int? CityId { get; set; }
    public string? Country { get; set; }
    public IFormFile ProfileImage { get; set; } = null!;
    public IFormFile CoverImage { get; set; } = null!;
    public int? TotalExperience { get; set; }

    /// <summary>
    /// Required. Id of the type the user picked, from GET /api/usertypes?isActive=true.
    /// Must reference an existing, active type.
    /// </summary>
    public int UserTypeId { get; set; }

    public string? Bio { get; set; }
    public string Position { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;

    /// <summary>Type chosen on the signup screen. Optional — falls back to <see cref="UserType"/>.</summary>
    public int? UserTypeId { get; set; }

    /// <summary>
    /// Legacy field kept so existing mobile builds keep working: when UserTypeId is not sent,
    /// this enum is resolved to the matching built-in type. New clients should send UserTypeId.
    /// </summary>
    public UserTypeKind UserType { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
   // public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

// ---------- Forgot Password ----------

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class VerifyOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

// ---------- User ----------

public class SetTrustedRequest
{
    public bool IsTrusted { get; set; }
}

public class SetUserActiveRequest
{
    public bool IsActive { get; set; }
}

/// <summary>Admin changes which user type a user belongs to.</summary>
public class SetUserTypeRequest
{
    public int UserTypeId { get; set; }
}

public class UpdateProfileRequest
{
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? PhoneNumber { get; set; }
    public int? CityId { get; set; }
    public string? Country { get; set; }
    public int? TotalExperience { get; set; }
    public string? Bio { get; set; }
    public string? Position { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public IFormFile? CoverImage { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? CityId { get; set; }
    public string? CityName { get; set; }
    public string? Country { get; set; }
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public int? TotalExperience { get; set; }

    /// <summary>
    /// Unchanged contract: still the behaviour bucket name ("User" / "Engineer"), so existing
    /// clients that branch on it keep working. The configurable label is UserTypeName.
    /// </summary>
    public UserTypeKind UserType { get; set; }

    public int UserTypeId { get; set; }

    /// <summary>The configurable type name, e.g. "Carpenter".</summary>
    public string? UserTypeName { get; set; }

    public string? Bio { get; set; }
    public string Position { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsFavourite { get; set; }
    public bool IsTrusted { get; set; }
    public DateTime CreatedAt { get; set; }
    public double Rating { get; set; }
    public int ReviewsCount { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();
    public List<ProjectDto> Projects { get; set; } = new();
    public List<PortfolioDto> Portfolio { get; set; } = new();
}
