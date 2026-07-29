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

    public async Task<List<ProjectViewModel>> GetAllAsync()
    {
        var response = await _http.GetAsync("api/projects");
        var (_, _, data) = await ReadEnvelopeAsync<List<ProjectViewModel>>(response);
        return data ?? new();
    }

    public async Task<(bool success, string message)> SetFeaturedAsync(int id, bool isFeatured)
    {
        var response = await _http.PatchAsJsonAsync($"api/projects/{id}/featured", new { IsFeatured = isFeatured });
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
