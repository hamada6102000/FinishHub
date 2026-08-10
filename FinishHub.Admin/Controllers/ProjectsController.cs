using FinishHub.Admin.Models;
using FinishHub.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinishHub.Admin.Controllers;

public class ProjectsController : Controller
{
    private readonly IProjectApiClient _projects;

    public ProjectsController(IProjectApiClient projects) => _projects = projects;

    public async Task<IActionResult> Index(int page = 1, int pageSize = PagedResult<ProjectViewModel>.DefaultPageSize)
    {
        // includeInactive: true — the management screen must show active and inactive projects.
        var projects = await _projects.GetAllAsync(page, pageSize, includeInactive: true);
        return View(projects);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetFeatured(int id, bool isFeatured, int page = 1, int pageSize = PagedResult<ProjectViewModel>.DefaultPageSize)
    {
        var (success, message) = await _projects.SetFeaturedAsync(id, isFeatured);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index), new { page, pageSize });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(int id, bool isActive, int page = 1, int pageSize = PagedResult<ProjectViewModel>.DefaultPageSize)
    {
        var (success, message) = await _projects.SetActiveAsync(id, isActive);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index), new { page, pageSize });
    }
}
