using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test.DTOs;
using test.Helpers;
using test.Services;

namespace test.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserService _users;

    public UsersController(UserService users) => _users = users;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);

    /// <summary>Get the logged-in user's profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var dto = await _users.GetProfileAsync(CurrentUserId);
        if (dto == null) return NotFound(ApiResponse.Fail("User not found."));
        return Ok(ApiResponse.Success(dto));
    }

    /// <summary>
    /// Get a page of users. Returns active users only; the Dashboard passes
    /// includeInactive=true to also get deactivated users for management.
    /// Pass userTypeId to return only the users belonging to that user type.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery pagination) =>
        Ok(ApiResponse.Success(await _users.GetAllAsync(pagination)));

    /// <summary>Update the logged-in user's profile.</summary>
    [HttpPut("me")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateMe([FromForm] UpdateProfileRequest req)
    {
        var dto = await _users.UpdateProfileAsync(CurrentUserId, req);
        if (dto == null) return NotFound(ApiResponse.Fail("User not found."));
        return Ok(ApiResponse.Success(dto, "Profile updated."));
    }

    /// <summary>Mark or unmark a user as trusted (admin use).</summary>
    [HttpPatch("{id}/trusted")]
    [AllowAnonymous]
    public async Task<IActionResult> SetTrusted(int id, [FromBody] SetTrustedRequest req)
    {
        var dto = await _users.SetTrustedAsync(id, req.IsTrusted);
        if (dto == null) return NotFound(ApiResponse.Fail("User not found."));
        return Ok(ApiResponse.Success(dto, "User updated."));
    }

    /// <summary>
    /// Activate or deactivate a user (admin use). Deactivating hides the user from every
    /// normal application response but never deletes the record; the call is idempotent.
    /// </summary>
    [HttpPatch("{id}/active")]
    [AllowAnonymous]
    public async Task<IActionResult> SetActive(int id, [FromBody] SetUserActiveRequest req)
    {
        var dto = await _users.SetActiveAsync(id, req.IsActive);
        if (dto == null) return NotFound(ApiResponse.Fail("User not found."));
        return Ok(ApiResponse.Success(dto, req.IsActive ? "User activated." : "User deactivated."));
    }

    /// <summary>
    /// Change which user type a user belongs to (admin use). The target type must exist and be
    /// active; deactivated types cannot be assigned.
    /// </summary>
    [HttpPatch("{id}/type")]
    [AllowAnonymous]
    public async Task<IActionResult> SetUserType(int id, [FromBody] SetUserTypeRequest req)
    {
        var (result, message, data) = await _users.SetUserTypeAsync(id, req.UserTypeId);
        return result switch
        {
            SetUserTypeResult.Success => Ok(ApiResponse.Success(data, message)),
            SetUserTypeResult.UserNotFound => NotFound(ApiResponse.Fail(message)),
            _ => BadRequest(ApiResponse.Fail(message)),
        };
    }
}
