using FinishHub.Admin.Models;
using FinishHub.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinishHub.Admin.Controllers;

public class CitiesController : Controller
{
    private readonly ICityApiClient _cities;

    public CitiesController(ICityApiClient cities) => _cities = cities;

    public async Task<IActionResult> Index()
    {
        var cities = await _cities.GetAllAsync();
        return View(cities);
    }

    public IActionResult Create() => View(new CityFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CityFormViewModel form)
    {
        if (!ModelState.IsValid) return View(form);

        var (success, message, _) = await _cities.CreateAsync(form);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(form);
        }

        TempData["Success"] = message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var (success, message, city) = await _cities.GetByIdAsync(id);
        if (!success || city == null)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        return View(new CityFormViewModel { Id = city.Id, NameAr = city.NameAr, NameEn = city.NameEn, IsPinned = city.IsPinned });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CityFormViewModel form)
    {
        if (!ModelState.IsValid) return View(form);

        var (success, message, _) = await _cities.UpdateAsync(id, form);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(form);
        }

        TempData["Success"] = message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPinned(int id, bool isPinned)
    {
        var (success, message) = await _cities.SetPinnedAsync(id, isPinned);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _cities.DeleteAsync(id);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }
}
