using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using test.Data;
using test.Models;

namespace test.Services;

public class OtpService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public OtpService(AppDbContext db, IConfiguration config)
    {
        _db    = db;
        _config = config;
    }

    public async Task<(bool success, string message)> SendOtpAsync(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return (false, "Email not found.");

        var code = GenerateOtp();
        var otp  = new OtpCode
        {
            UserId    = user.Id,
            Code      = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
        };
        _db.OtpCodes.Add(otp);
        await _db.SaveChangesAsync();

        await SendEmailAsync(email, "Password Reset OTP",
            $"<p>Your OTP code is: <strong>{code}</strong>. It expires in 10 minutes.</p>");

        return (true, "OTP sent successfully.");
    }

    public async Task<(bool success, string message)> VerifyOtpAsync(string email, string code)
    {
        var valid = await GetValidOtpAsync(email, code);
        if (valid == null)
            return (false, "Invalid or expired OTP.");

        return (true, "OTP is valid.");
    }

    public async Task<(bool success, string message)> ResetPasswordAsync(string email, string code, string newPassword)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return (false, "Email not found.");

        var otp = await GetValidOtpAsync(email, code);
        if (otp == null)
            return (false, "Invalid or expired OTP.");

        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, newPassword);
        user.UpdatedAt    = DateTime.UtcNow;

        otp.IsUsed = true;

        await _db.SaveChangesAsync();
        return (true, "Password reset successfully.");
    }

    // ---------- helpers ----------

    private async Task<OtpCode?> GetValidOtpAsync(string email, string code)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;

        return await _db.OtpCodes
            .Where(o => o.UserId == user.Id && o.Code == code && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    private static string GenerateOtp() => new Random().Next(100000, 999999).ToString();

    private async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var host     = _config["Email:Host"]!;
        var port     = int.Parse(_config["Email:Port"] ?? "587");
        var user     = _config["Email:Username"]!;
        var pass     = _config["Email:Password"]!;
        var fromName = _config["Email:FromName"] ?? "App";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, user));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body    = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(user, pass);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
