using System.Net.Http.Json;
using System.Text.Json;
using FinishHub.Admin.Models;

namespace FinishHub.Admin.Services;

public class UserApiClient : IUserApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;
    private readonly ILogger<UserApiClient> _logger;

    public UserApiClient(HttpClient http, ILogger<UserApiClient> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<(bool success, string message, List<UserViewModel> data)> GetAllAsync()
    {
        var response = await _http.GetAsync("api/users");
        var (success, message, data) = await ReadEnvelopeAsync<List<UserViewModel>>(response);
        return (success, message, data ?? new());
    }

    public async Task<(bool success, string message)> SetTrustedAsync(int id, bool isTrusted)
    {
        var response = await _http.PatchAsJsonAsync($"api/users/{id}/trusted", new { IsTrusted = isTrusted });
        var (success, message, _) = await ReadEnvelopeAsync<object>(response);
        return (success, message);
    }

    private async Task<(bool success, string message, T? data)> ReadEnvelopeAsync<T>(HttpResponseMessage response)
    {
        try
        {
            var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<T>>(JsonOptions);
            if (envelope == null)
                return (false, "Unexpected response from the API.", default);

            return (envelope.Success, envelope.Message, envelope.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read users API response ({StatusCode}).", (int)response.StatusCode);
            return (false, $"Could not read the API response ({(int)response.StatusCode} {response.ReasonPhrase}): {ex.Message}", default);
        }
    }
}
