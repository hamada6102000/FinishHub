using System.Net.Http.Json;
using System.Text.Json;
using FinishHub.Admin.Models;

namespace FinishHub.Admin.Services;

public class UserTypeApiClient : IUserTypeApiClient
{
    /// <summary>Matches PaginationQuery.MaxPageSize on the API — enough to hold every type in one page.</summary>
    private const int AllTypesPageSize = 100;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;

    public UserTypeApiClient(HttpClient http) => _http = http;

    public async Task<PagedResult<UserTypeViewModel>> GetAllAsync(int page, int pageSize, bool includeInactive = true)
    {
        var response = await _http.GetAsync(
            $"api/usertypes?page={page}&pageSize={pageSize}&includeInactive={includeInactive.ToString().ToLowerInvariant()}");
        var envelope = await ReadEnvelopeAsync<PagedResult<UserTypeViewModel>>(response);
        return envelope.data ?? PagedResult<UserTypeViewModel>.Empty(page, pageSize);
    }

    public async Task<List<UserTypeViewModel>> GetActiveAsync()
    {
        var page = await GetAllAsync(1, AllTypesPageSize, includeInactive: false);
        return page.Items;
    }

    public async Task<(bool success, string message, UserTypeViewModel? data)> GetByIdAsync(int id)
    {
        var response = await _http.GetAsync($"api/usertypes/{id}");
        return await ReadEnvelopeAsync<UserTypeViewModel>(response);
    }

    public async Task<(bool success, string message, UserTypeViewModel? data)> CreateAsync(UserTypeFormViewModel form)
    {
        var response = await _http.PostAsJsonAsync("api/usertypes", new { form.NameAr, form.NameEn, form.IsActive });
        return await ReadEnvelopeAsync<UserTypeViewModel>(response);
    }

    public async Task<(bool success, string message, UserTypeViewModel? data)> UpdateAsync(int id, UserTypeFormViewModel form)
    {
        var response = await _http.PutAsJsonAsync($"api/usertypes/{id}", new { form.NameAr, form.NameEn, form.IsActive });
        return await ReadEnvelopeAsync<UserTypeViewModel>(response);
    }

    public async Task<(bool success, string message)> SetActiveAsync(int id, bool isActive)
    {
        var response = await _http.PatchAsJsonAsync($"api/usertypes/{id}/active", new { IsActive = isActive });
        var (success, message, _) = await ReadEnvelopeAsync<object>(response);
        return (success, message);
    }

    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/usertypes/{id}");
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
