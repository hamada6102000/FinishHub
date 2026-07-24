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
    public string? City { get; set; }
    public string? Country { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public IFormFile? CoverImage { get; set; }
    public int? TotalExperience { get; set; }
    public UserType UserType { get; set; }
    public string? Bio { get; set; }
    public string? Position { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
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

public class UpdateProfileRequest
{
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
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
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public int? TotalExperience { get; set; }
    public UserType UserType { get; set; }
    public string? Bio { get; set; }
    public string? Position { get; set; }
    public bool IsActive { get; set; }
    public bool IsFavourite { get; set; }
    public DateTime CreatedAt { get; set; }
}
