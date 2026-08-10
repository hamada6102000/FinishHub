using FinishHub.Admin.Models;

namespace FinishHub.Admin.Services;

public interface IProjectApiClient
{
    /// <summary>
    /// Loads a page of projects. The Dashboard passes includeInactive: true so administrators
    /// see deactivated projects too — normal application clients get active projects only.
    /// </summary>
    Task<PagedResult<ProjectViewModel>> GetAllAsync(int page, int pageSize, bool includeInactive = true);

    Task<(bool success, string message)> SetFeaturedAsync(int id, bool isFeatured);

    Task<(bool success, string message)> SetActiveAsync(int id, bool isActive);
}
