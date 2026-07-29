using FinishHub.Admin.Models;

namespace FinishHub.Admin.Services;

public interface IProjectApiClient
{
    Task<List<ProjectViewModel>> GetAllAsync();
    Task<(bool success, string message)> SetFeaturedAsync(int id, bool isFeatured);
}
