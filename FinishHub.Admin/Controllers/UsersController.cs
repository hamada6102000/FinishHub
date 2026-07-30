using FinishHub.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinishHub.Admin.Controllers;

public class UsersController : Controller
{
    private readonly IUserApiClient _users;

    public UsersController(IUserApiClient users) => _users = users;

    public async Task<IActionResult> Index()
    {
        var (success, message, users) = await _users.GetAllAsync();
        if (!success) TempData["Error"] = message;
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetTrusted(int id, bool isTrusted)
    {
        var (success, message) = await _users.SetTrustedAsync(id, isTrusted);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }
}
