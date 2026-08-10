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
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _projects;

    public ProjectsController(ProjectService projects) => _projects = projects;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);

    private string GetLang() =>
        Request.Headers.AcceptLanguage.ToString().StartsWith("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";

    /// <summary>
    /// Get a page of projects. Returns active projects owned by active users only; the
    /// Dashboard passes includeInactive=true to also get deactivated projects for management.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery pagination) =>
        Ok(ApiResponse.Success(await _projects.GetAllAsync(pagination, GetLang())));

    /// <summary>Get a project by id.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _projects.GetByIdAsync(id, GetLang());
        if (dto == null) return NotFound(ApiResponse.Fail("Project not found."));
        return Ok(ApiResponse.Success(dto));
    }

    /// <summary>Get a page of projects for a specific user.</summary>
    [HttpGet("user/{userId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserProjects(int userId, [FromQuery] PaginationQuery pagination) =>
        Ok(ApiResponse.Success(await _projects.GetUserProjectsAsync(userId, pagination, GetLang())));

    /// <summary>Create a new project (authenticated user).</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateProjectRequest req)
    {
        var dto = await _projects.CreateAsync(CurrentUserId, req, GetLang());
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, ApiResponse.Success(dto, "Project created."));
    }

    /// <summary>Update a project.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectRequest req)
    {
        var (success, dto) = await _projects.UpdateAsync(id, CurrentUserId, req, GetLang());
        if (!success) return NotFound(ApiResponse.Fail("Project not found or access denied."));
        return Ok(ApiResponse.Success(dto, "Project updated."));
    }

    /// <summary>Delete a project.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _projects.DeleteAsync(id, CurrentUserId);
        if (!success) return NotFound(ApiResponse.Fail("Project not found or access denied."));
        return Ok(ApiResponse.Success(null, "Project deleted."));
    }

    /// <summary>Rate a project (authenticated user).</summary>
    [HttpPost("{id}/rate")]
    public async Task<IActionResult> Rate(int id, [FromBody] RateProjectRequest req)
    {
        var (success, message, dto) = await _projects.RateAsync(id, CurrentUserId, req.Value, GetLang());
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Success(dto, message));
    }

    /// <summary>Mark or unmark a project as featured (admin use).</summary>
    [HttpPatch("{id}/featured")]
    [AllowAnonymous]
    public async Task<IActionResult> SetFeatured(int id, [FromBody] SetFeaturedRequest req)
    {
        var dto = await _projects.SetFeaturedAsync(id, req.IsFeatured, GetLang());
        if (dto == null) return NotFound(ApiResponse.Fail("Project not found."));
        return Ok(ApiResponse.Success(dto, "Project updated."));
    }

    /// <summary>
    /// Activate or deactivate a project (admin use). Deactivating hides the project from every
    /// normal application response but never deletes the record; the call is idempotent.
    /// </summary>
    [HttpPatch("{id}/active")]
    [AllowAnonymous]
    public async Task<IActionResult> SetActive(int id, [FromBody] SetProjectActiveRequest req)
    {
        var dto = await _projects.SetActiveAsync(id, req.IsActive, GetLang());
        if (dto == null) return NotFound(ApiResponse.Fail("Project not found."));
        return Ok(ApiResponse.Success(dto, req.IsActive ? "Project activated." : "Project deactivated."));
    }
}
