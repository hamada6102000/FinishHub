using FinishHub.Admin.Models;

namespace FinishHub.Admin.Services;

public interface ICityApiClient
{
    Task<List<CityViewModel>> GetAllAsync();
    Task<(bool success, string message, CityViewModel? data)> GetByIdAsync(int id);
    Task<(bool success, string message, CityViewModel? data)> CreateAsync(CityFormViewModel form);
    Task<(bool success, string message, CityViewModel? data)> UpdateAsync(int id, CityFormViewModel form);
    Task<(bool success, string message)> DeleteAsync(int id);
}
