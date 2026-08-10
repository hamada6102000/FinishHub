using FinishHub.Admin.Models;
using FinishHub.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinishHub.Admin.Controllers;

public class UsersController : Controller
{
    private readonly IUserApiClient _users;

    public UsersController(IUserApiClient users) => _users = users;

    public async Task<IActionResult> Index(int page = 1, int pageSize = PagedResult<UserViewModel>.DefaultPageSize)
    {
        // includeInactive: true — the management screen must show active and inactive users.
        var (success, message, users) = await _users.GetAllAsync(page, pageSize, includeInactive: true);
        if (!success) TempData["Error"] = message;
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetTrusted(int id, bool isTrusted, int page = 1, int pageSize = PagedResult<UserViewModel>.DefaultPageSize)
    {
        var (success, message) = await _users.SetTrustedAsync(id, isTrusted);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index), new { page, pageSize });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(int id, bool isActive, int page = 1, int pageSize = PagedResult<UserViewModel>.DefaultPageSize)
    {
        var (success, message) = await _users.SetActiveAsync(id, isActive);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index), new { page, pageSize });
    }
}
