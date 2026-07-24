using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Google.Apis.Auth;
using test.Data;
using test.DTOs;
using test.Helpers;
using test.Models;

namespace test.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthService(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
    {
        _db = db;
        _config = config;
        _env = env;
    }

    public async Task<(bool success, string message, AuthResponse? response)> RegisterAsync(RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return (false, "Email already exists.", null);

        var profileUrl = await FileUploadHelper.SaveFileAsync(req.ProfileImage, "profiles", _env);
        var coverUrl   = await FileUploadHelper.SaveFileAsync(req.CoverImage, "covers", _env);

        var user = new User
        {
            NameAr          = req.NameAr,
            NameEn          = req.NameEn,
            PhoneNumber     = req.PhoneNumber,
            Email           = req.Email,
            City            = req.City,
            Country         = req.Country,
            ProfileImageUrl = profileUrl,
            CoverImageUrl   = coverUrl,
            TotalExperience = req.TotalExperience,
            UserType        = req.UserType,
            Bio             = req.Bio,
        };

        user.PasswordHash = _hasher.HashPassword(user, req.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var auth = BuildAuthResponse(user);
        return (true, "Registration successful.", auth);
    }

    public async Task<(bool success, string message, AuthResponse? response)> LoginAsync(LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user == null)
            return (false, "Invalid credentials.", null);

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed)
            return (false, "Invalid credentials.", null);

        return (true, "Login successful.", BuildAuthResponse(user));
    }

    public async Task<(bool success, string message, AuthResponse? response)> GoogleLoginAsync(string idToken)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
        }
        catch
        {
            return (false, "Invalid Google token.", null);
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

        if (user == null)
        {
            user = new User
            {
                Email       = payload.Email,
                NameEn      = payload.Name ?? payload.Email,
                NameAr      = payload.Name ?? payload.Email,
                PhoneNumber = string.Empty,
                GoogleId    = payload.Subject,
                PasswordHash = string.Empty,
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        else if (string.IsNullOrEmpty(user.GoogleId))
        {
            user.GoogleId = payload.Subject;
            await _db.SaveChangesAsync();
        }

        return (true, "Google login successful.", BuildAuthResponse(user));
    }

    // ---------- helpers ----------

    private AuthResponse BuildAuthResponse(User user)
    {
        var (token, expires) = GenerateJwt(user);
        return new AuthResponse
        {
            Token     = token,
            ExpiresAt = expires,
            User      = MapUser(user)
        };
    }

    private (string token, DateTime expires) GenerateJwt(User user)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(double.Parse(_config["Jwt:ExpireDays"] ?? "30"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("userType", user.UserType.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:   _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims:   claims,
            expires:  expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public static UserDto MapUser(User user) => new()
    {
        Id              = user.Id,
        NameAr          = user.NameAr,
        NameEn          = user.NameEn,
        PhoneNumber     = user.PhoneNumber,
        Email           = user.Email,
        City            = user.City,
        Country         = user.Country,
        ProfileImageUrl = user.ProfileImageUrl,
        CoverImageUrl   = user.CoverImageUrl,
        TotalExperience = user.TotalExperience,
        UserType        = user.UserType,
        Bio             = user.Bio,
        IsActive        = user.IsActive,
        IsFavourite     = user.IsFavourite,
        CreatedAt       = user.CreatedAt,
    };
}
