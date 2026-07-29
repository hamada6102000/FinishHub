using FinishHub.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinishHub.Admin.Controllers;

public class ProjectsController : Controller
{
    private readonly IProjectApiClient _projects;

    public ProjectsController(IProjectApiClient projects) => _projects = projects;

    public async Task<IActionResult> Index()
    {
        var projects = await _projects.GetAllAsync();
        return View(projects);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetFeatured(int id, bool isFeatured)
    {
        var (success, message) = await _projects.SetFeaturedAsync(id, isFeatured);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }
}
