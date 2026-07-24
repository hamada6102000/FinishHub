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

    /// <summary>Get all projects.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() =>
        Ok(ApiResponse.Success(await _projects.GetAllAsync()));

    /// <summary>Get a project by id.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _projects.GetByIdAsync(id);
        if (dto == null) return NotFound(ApiResponse.Fail("Project not found."));
        return Ok(ApiResponse.Success(dto));
    }

    /// <summary>Get all projects for a specific user.</summary>
    [HttpGet("user/{userId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserProjects(int userId) =>
        Ok(ApiResponse.Success(await _projects.GetUserProjectsAsync(userId)));

    /// <summary>Create a new project (authenticated user).</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateProjectRequest req)
    {
        var dto = await _projects.CreateAsync(CurrentUserId, req);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, ApiResponse.Success(dto, "Project created."));
    }

    /// <summary>Update a project.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectRequest req)
    {
        var (success, dto) = await _projects.UpdateAsync(id, CurrentUserId, req);
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
}
