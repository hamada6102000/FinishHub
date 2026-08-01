using FinishHub.Admin.Models;
using FinishHub.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinishHub.Admin.Controllers;

public class CitiesController : Controller
{
    private const int DefaultPageSize = PagedResult<CityViewModel>.DefaultPageSize;

    private readonly ICityApiClient _cities;

    public CitiesController(ICityApiClient cities) => _cities = cities;

    public async Task<IActionResult> Index(int page = 1, int pageSize = DefaultPageSize)
    {
        var cities = await _cities.GetAllAsync(page, pageSize);
        return View(cities);
    }

    public IActionResult Create(int pageSize = DefaultPageSize)
    {
        ViewData["PageSize"] = pageSize;
        return View(new CityFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CityFormViewModel form, int pageSize = DefaultPageSize)
    {
        ViewData["PageSize"] = pageSize;
        if (!ModelState.IsValid) return View(form);

        var (success, message, _) = await _cities.CreateAsync(form);
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
        var (success, message, city) = await _cities.GetByIdAsync(id);
        if (!success || city == null)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Index), new { page, pageSize });
        }

        ViewData["Page"]     = page;
        ViewData["PageSize"] = pageSize;
        return View(new CityFormViewModel { Id = city.Id, NameAr = city.NameAr, NameEn = city.NameEn, IsPinned = city.IsPinned });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CityFormViewModel form, int page = 1, int pageSize = DefaultPageSize)
    {
        ViewData["Page"]     = page;
        ViewData["PageSize"] = pageSize;
        if (!ModelState.IsValid) return View(form);

        var (success, message, _) = await _cities.UpdateAsync(id, form);
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
    public async Task<IActionResult> SetPinned(int id, bool isPinned, int page = 1, int pageSize = DefaultPageSize)
    {
        var (success, message) = await _cities.SetPinnedAsync(id, isPinned);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index), new { page, pageSize });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int page = 1, int pageSize = DefaultPageSize)
    {
        var (success, message) = await _cities.DeleteAsync(id);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index), new { page, pageSize });
    }
}
