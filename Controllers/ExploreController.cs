using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test.DTOs;
using test.Helpers;
using test.Services;

namespace test.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExploreController : ControllerBase
{
    private readonly ExploreService _explore;

    public ExploreController(ExploreService explore) => _explore = explore;

    private string GetLang() =>
        Request.Headers.AcceptLanguage.ToString().StartsWith("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";

    /// <summary>Search Engineers and/or Projects for the Explore screen (tab=all|engineers|projects).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] ExploreQuery query) =>
        Ok(ApiResponse.Success(await _explore.SearchAsync(query, GetLang())));
}
