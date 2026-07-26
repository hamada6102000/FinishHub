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

    /// <summary>Get all active engineers.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() =>
        Ok(ApiResponse.Success(await _engineers.GetAllAsync()));

    /// <summary>Get the complete profile for an engineer.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _engineers.GetProfileAsync(id);
        if (dto == null) return NotFound(ApiResponse.Fail("Engineer not found."));
        return Ok(ApiResponse.Success(dto));
    }
}
