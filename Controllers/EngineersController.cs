using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test.Helpers;
using test.Services;

namespace test.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EngineersController : ControllerBase
{
    private readonly EngineerService _engineers;

    public EngineersController(EngineerService engineers) => _engineers = engineers;

    private string GetLang() =>
        Request.Headers.AcceptLanguage.ToString().StartsWith("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);

    /// <summary>Get a page of active engineers.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery pagination) =>
        Ok(ApiResponse.Success(await _engineers.GetAllAsync(pagination)));

    /// <summary>Get the complete profile for an engineer. IsFavourite reflects whether the logged-in user has favorited this engineer.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _engineers.GetProfileAsync(id, GetLang(), CurrentUserId);
        if (dto == null) return NotFound(ApiResponse.Fail("Engineer not found."));
        return Ok(ApiResponse.Success(dto));
    }
}
