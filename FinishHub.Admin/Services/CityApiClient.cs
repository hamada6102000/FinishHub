using System.Net.Http.Json;
using System.Text.Json;
using FinishHub.Admin.Models;

namespace FinishHub.Admin.Services;

public class CityApiClient : ICityApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;

    public CityApiClient(HttpClient http) => _http = http;

    public async Task<List<CityViewModel>> GetAllAsync()
    {
        var response = await _http.GetAsync("api/cities");
        var envelope = await ReadEnvelopeAsync<List<CityViewModel>>(response);
        return envelope.data ?? new();
    }

    public async Task<(bool success, string message, CityViewModel? data)> GetByIdAsync(int id)
    {
        var response = await _http.GetAsync($"api/cities/{id}");
        return await ReadEnvelopeAsync<CityViewModel>(response);
    }

    public async Task<(bool success, string message, CityViewModel? data)> CreateAsync(CityFormViewModel form)
    {
        var response = await _http.PostAsJsonAsync("api/cities", new { form.NameAr, form.NameEn });
        return await ReadEnvelopeAsync<CityViewModel>(response);
    }

    public async Task<(bool success, string message, CityViewModel? data)> UpdateAsync(int id, CityFormViewModel form)
    {
        var response = await _http.PutAsJsonAsync($"api/cities/{id}", new { form.NameAr, form.NameEn });
        return await ReadEnvelopeAsync<CityViewModel>(response);
    }

    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/cities/{id}");
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
