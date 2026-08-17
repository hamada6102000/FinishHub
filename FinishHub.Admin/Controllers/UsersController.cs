using FinishHub.Admin.Models;
using FinishHub.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinishHub.Admin.Controllers;

public class UsersController : Controller
{
    private readonly IUserApiClient _users;
    private readonly IUserTypeApiClient _userTypes;

    public UsersController(IUserApiClient users, IUserTypeApiClient userTypes)
    {
        _users     = users;
        _userTypes = userTypes;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PagedResult<UserViewModel>.DefaultPageSize)
    {
        // includeInactive: true — the management screen must show active and inactive users.
        var (success, message, users) = await _users.GetAllAsync(page, pageSize, includeInactive: true);
        if (!success) TempData["Error"] = message;

        // Active types only: a deactivated type cannot be assigned, so it must not be offered.
        ViewData["UserTypes"] = await _userTypes.GetActiveAsync();
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetType(int id, int userTypeId, int page = 1, int pageSize = PagedResult<UserViewModel>.DefaultPageSize)
    {
        // The placeholder option posts 0 — treat it as "nothing chosen" rather than a failed save.
        if (userTypeId <= 0)
        {
            TempData["Error"] = "Please select a user type.";
            return RedirectToAction(nameof(Index), new { page, pageSize });
        }

        var (success, message) = await _users.SetUserTypeAsync(id, userTypeId);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index), new { page, pageSize });
    }
}
