using FinishHub.Admin.Models;
using FinishHub.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinishHub.Admin.Controllers;

public class UserTypesController : Controller
{
    private const int DefaultPageSize = PagedResult<UserTypeViewModel>.DefaultPageSize;

    private readonly IUserTypeApiClient _userTypes;

    public UserTypesController(IUserTypeApiClient userTypes) => _userTypes = userTypes;

    public async Task<IActionResult> Index(int page = 1, int pageSize = DefaultPageSize)
    {
        // includeInactive: true — the management screen must show active and inactive types.
        var types = await _userTypes.GetAllAsync(page, pageSize, includeInactive: true);
        return View(types);
    }

    public IActionResult Create(int pageSize = DefaultPageSize)
    {
        ViewData["PageSize"] = pageSize;
        return View(new UserTypeFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserTypeFormViewModel form, int pageSize = DefaultPageSize)
    {
        ViewData["PageSize"] = pageSize;
        if (!ModelState.IsValid) return View(form);

        var (success, message, _) = await _userTypes.CreateAsync(form);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(form);
        }

        TempData["Success"] = message;
        return RedirectToAction(nameof(Index), new { page = 1, pageSize });
    }

    public async Task<IActionResult> Edit(int id, int page = 1, int pageSize = DefaultPageSize)
    {
        var (success, message, type) = await _userTypes.GetByIdAsync(id);
        if (!success || type == null)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Index), new { page, pageSize });
        }

        ViewData["Page"]     = page;
        ViewData["PageSize"] = pageSize;
        return View(new UserTypeFormViewModel
        {
            Id       = type.Id,
            NameAr   = type.NameAr,
            NameEn   = type.NameEn,
            IsActive = type.IsActive,
            IsSystem = type.IsSystem,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserTypeFormViewModel form, int page = 1, int pageSize = DefaultPageSize)
    {
        ViewData["Page"]     = page;
        ViewData["PageSize"] = pageSize;

        // The Active checkbox is disabled for built-in types, so the browser posts nothing for
        // it — force it back on rather than letting the API reject the update.
        if (form.IsSystem) form.IsActive = true;

        if (!ModelState.IsValid) return View(form);

        var (success, message, _) = await _userTypes.UpdateAsync(id, form);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(form);
        }

        TempData["Success"] = message;
        return RedirectToAction(nameof(Index), new { page, pageSize });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(int id, bool isActive, int page = 1, int pageSize = DefaultPageSize)
    {
        var (success, message) = await _userTypes.SetActiveAsync(id, isActive);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index), new { page, pageSize });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int page = 1, int pageSize = DefaultPageSize)
    {
        var (success, message) = await _userTypes.DeleteAsync(id);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index), new { page, pageSize });
    }
}
