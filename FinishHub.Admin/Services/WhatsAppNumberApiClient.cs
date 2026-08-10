using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinishHub.Admin.Models;

namespace FinishHub.Admin.Services;

public class WhatsAppNumberApiClient : IWhatsAppNumberApiClient
{
    private const string Endpoint = "api/whatsappnumber";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;

    public WhatsAppNumberApiClient(HttpClient http) => _http = http;

    public async Task<(bool success, bool notConfigured, string message, WhatsAppNumberViewModel? data)> GetAsync()
    {
        var response = await _http.GetAsync(Endpoint);
        var (success, message, data) = await ReadEnvelopeAsync<WhatsAppNumberViewModel>(response);

        // 404 means "nothing configured yet" — the Dashboard shows its empty state for that.
        var notConfigured = !success && response.StatusCode == HttpStatusCode.NotFound;
        return (success, notConfigured, message, data);
    }

    public async Task<(bool success, string message, WhatsAppNumberViewModel? data)> CreateAsync(WhatsAppNumberFormViewModel form)
    {
        var response = await _http.PostAsJsonAsync(Endpoint, new { form.PhoneNumber });
        return await ReadEnvelopeAsync<WhatsAppNumberViewModel>(response);
    }

    public async Task<(bool success, string message, WhatsAppNumberViewModel? data)> UpdateAsync(WhatsAppNumberFormViewModel form)
    {
        var response = await _http.PutAsJsonAsync(Endpoint, new { form.PhoneNumber });
        return await ReadEnvelopeAsync<WhatsAppNumberViewModel>(response);
    }

    private static async Task<(bool success, string message, T? data)> ReadEnvelopeAsync<T>(HttpResponseMessage response)
    {
        try
        {
            var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<T>>(JsonOptions);
            if (envelope == null)
                return (false, "Unexpected response from the API.", default);

            return (envelope.Success, envelope.Message, envelope.Data);
        }
        catch (Exception)
        {
            return (false, $"Could not reach the API ({(int)response.StatusCode} {response.ReasonPhrase}).", default);
        }
    }
}
