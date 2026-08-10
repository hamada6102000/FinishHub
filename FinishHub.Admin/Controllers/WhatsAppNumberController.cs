using FinishHub.Admin.Models;
using FinishHub.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinishHub.Admin.Controllers;

/// <summary>
/// Manages the single, solution-wide WhatsApp number.
/// Add is only offered when no number exists; afterwards only Edit is available.
/// There is intentionally no Delete action.
/// </summary>
public class WhatsAppNumberController : Controller
{
    private readonly IWhatsAppNumberApiClient _whatsAppNumbers;

    public WhatsAppNumberController(IWhatsAppNumberApiClient whatsAppNumbers) => _whatsAppNumbers = whatsAppNumbers;

    public async Task<IActionResult> Index()
    {
        var (success, notConfigured, message, number) = await _whatsAppNumbers.GetAsync();

        if (!success && !notConfigured)
            TempData["Error"] = message;

        // null model => the view renders the empty state with the "Add" action.
        return View(success ? number : null);
    }

    public async Task<IActionResult> Create()
    {
        // Guard the empty state: if a number already exists, adding a second one is not allowed.
        var (success, _, _, number) = await _whatsAppNumbers.GetAsync();
        if (success && number != null)
        {
            TempData["Error"] = "A WhatsApp number is already configured. Only one is allowed — update it instead.";
            return RedirectToAction(nameof(Index));
        }

        return View(new WhatsAppNumberFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WhatsAppNumberFormViewModel form)
    {
        if (!ModelState.IsValid) return View(form);

        var (success, message, _) = await _whatsAppNumbers.CreateAsync(form);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(form);
        }

        TempData["Success"] = message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit()
    {
        var (success, notConfigured, message, number) = await _whatsAppNumbers.GetAsync();
        if (!success || number == null)
        {
            TempData["Error"] = notConfigured
                ? "No WhatsApp number has been configured yet. Add one first."
                : message;
            return RedirectToAction(nameof(Index));
        }

        return View(new WhatsAppNumberFormViewModel { PhoneNumber = number.PhoneNumber });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(WhatsAppNumberFormViewModel form)
    {
        if (!ModelState.IsValid) return View(form);

        var (success, message, _) = await _whatsAppNumbers.UpdateAsync(form);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(form);
        }

        TempData["Success"] = message;
        return RedirectToAction(nameof(Index));
    }
}
