using FinishHub.Admin.Models;

namespace FinishHub.Admin.Services;

public interface IProjectApiClient
{
    Task<PagedResult<ProjectViewModel>> GetAllAsync(int page, int pageSize);
    Task<(bool success, string message)> SetFeaturedAsync(int id, bool isFeatured);
}
