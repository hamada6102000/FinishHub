using System.Net.Http.Json;
using System.Text.Json;
using FinishHub.Admin.Models;

namespace FinishHub.Admin.Services;

public class ProjectApiClient : IProjectApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;

    public ProjectApiClient(HttpClient http) => _http = http;

    public async Task<PagedResult<ProjectViewModel>> GetAllAsync(int page, int pageSize, bool includeInactive = true)
    {
        var response = await _http.GetAsync($"api/projects?page={page}&pageSize={pageSize}&includeInactive={includeInactive.ToString().ToLowerInvariant()}");
        var (_, _, data) = await ReadEnvelopeAsync<PagedResult<ProjectViewModel>>(response);
        return data ?? PagedResult<ProjectViewModel>.Empty(page, pageSize);
    }

    public async Task<(bool success, string message)> SetFeaturedAsync(int id, bool isFeatured)
    {
        var response = await _http.PatchAsJsonAsync($"api/projects/{id}/featured", new { IsFeatured = isFeatured });
        var (success, message, _) = await ReadEnvelopeAsync<object>(response);
        return (success, message);
    }

    public async Task<(bool success, string message)> SetActiveAsync(int id, bool isActive)
    {
        var response = await _http.PatchAsJsonAsync($"api/projects/{id}/active", new { IsActive = isActive });
        var (success, message, _) = await ReadEnvelopeAsync<object>(response);
        return (success, message);
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
