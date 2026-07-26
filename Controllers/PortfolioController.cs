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
public class PortfolioController : ControllerBase
{
    private readonly PortfolioService _portfolio;

    public PortfolioController(PortfolioService portfolio) => _portfolio = portfolio;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);

    /// <summary>Get portfolio for a user.</summary>
    [HttpGet("{userId}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int userId)
    {
        var dto = await _portfolio.GetByUserAsync(userId);
        if (dto == null) return NotFound(ApiResponse.Fail("Portfolio not found."));
        return Ok(ApiResponse.Success(dto));
    }

    /// <summary>Delete portfolio for authenticated user.</summary>
    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        var success = await _portfolio.DeleteAsync(CurrentUserId);
        if (!success) return NotFound(ApiResponse.Fail("Portfolio not found."));
        return Ok(ApiResponse.Success(null, "Portfolio deleted."));
    }

    /// <summary>Add media to portfolio (creates portfolio automatically if it doesn't exist).</summary>
    [HttpPost("media")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddPortfolioMedia([FromForm] AddPortfolioMediaRequest req)
    {
        var (_, dto) = await _portfolio.AddMediaAsync(CurrentUserId, req);
        return Ok(ApiResponse.Success(dto, "Media added."));
    }

    /// <summary>Remove a media item from authenticated user's portfolio.</summary>
    [HttpDelete("media/{mediaId}")]
    public async Task<IActionResult> RemoveMedia(int mediaId)
    {
        var success = await _portfolio.RemoveMediaAsync(mediaId, CurrentUserId);
        if (!success) return NotFound(ApiResponse.Fail("Media not found."));
        return Ok(ApiResponse.Success(null, "Media removed."));
    }
}
