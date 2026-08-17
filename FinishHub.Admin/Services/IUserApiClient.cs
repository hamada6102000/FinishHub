using FinishHub.Admin.Models;

namespace FinishHub.Admin.Services;

public interface IUserApiClient
{
    /// <summary>
    /// Loads a page of users. The Dashboard passes includeInactive: true so administrators
    /// see deactivated users too — normal application clients get active users only.
    /// </summary>
    Task<(bool success, string message, PagedResult<UserViewModel> data)> GetAllAsync(int page, int pageSize, bool includeInactive = true);

    Task<(bool success, string message)> SetTrustedAsync(int id, bool isTrusted);

    Task<(bool success, string message)> SetActiveAsync(int id, bool isActive);

    /// <summary>Moves a user onto another user type. The target type must exist and be active.</summary>
    Task<(bool success, string message)> SetUserTypeAsync(int id, int userTypeId);
}
