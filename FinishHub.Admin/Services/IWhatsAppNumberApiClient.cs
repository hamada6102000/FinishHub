using FinishHub.Admin.Models;

namespace FinishHub.Admin.Services;

/// <summary>
/// The solution holds exactly zero or one WhatsApp number, so there is no list call
/// and — deliberately — no delete call.
/// </summary>
public interface IWhatsAppNumberApiClient
{
    /// <summary>
    /// Gets the configured number. <c>notConfigured</c> is true when the API reports that
    /// no number exists yet, which is a normal empty state rather than an error.
    /// </summary>
    Task<(bool success, bool notConfigured, string message, WhatsAppNumberViewModel? data)> GetAsync();

    Task<(bool success, string message, WhatsAppNumberViewModel? data)> CreateAsync(WhatsAppNumberFormViewModel form);

    Task<(bool success, string message, WhatsAppNumberViewModel? data)> UpdateAsync(WhatsAppNumberFormViewModel form);
}
