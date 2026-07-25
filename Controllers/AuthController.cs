using Microsoft.AspNetCore.Mvc;
using test.DTOs;
using test.Helpers;
using test.Services;

namespace test.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly OtpService _otp;

    public AuthController(AuthService auth, OtpService otp)
    {
        _auth = auth;
        _otp  = otp;
    }

    /// <summary>Register a new user.</summary>
    [HttpPost("register")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Register([FromForm] RegisterRequest req)
    {
        var (success, message, response) = await _auth.RegisterAsync(req);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Success(response, message));
    }

    /// <summary>Login with email and password.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var (success, message, response) = await _auth.LoginAsync(req);
        if (!success) return Unauthorized(ApiResponse.Fail(message));
        return Ok(ApiResponse.Success(response, message));
    }

    /// <summary>Login with a Google ID token.</summary>
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest req)
    {
        var (success, message, response) = await _auth.GoogleLoginAsync(req.IdToken, req.UserType);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Success(response, message));
    }

    /// <summary>Send OTP to email for password reset.</summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        var (success, message) = await _otp.SendOtpAsync(req.Email);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Success(null, message));
    }

    /// <summary>Verify OTP code.</summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest req)
    {
        var (success, message) = await _otp.VerifyOtpAsync(req.Email, req.Otp);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Success(null, message));
    }

    /// <summary>Reset password using OTP.</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        var (success, message) = await _otp.ResetPasswordAsync(req.Email, req.Otp, req.NewPassword);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Success(null, message));
    }
}
