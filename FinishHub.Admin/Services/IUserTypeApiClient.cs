using FinishHub.Admin.Models;

namespace FinishHub.Admin.Services;

public interface IUserTypeApiClient
{
    /// <summary>
    /// Loads a page of user types. The Dashboard passes includeInactive: true so administrators
    /// see deactivated types too; the signup screen gets active types only.
    /// </summary>
    Task<PagedResult<UserTypeViewModel>> GetAllAsync(int page, int pageSize, bool includeInactive = true);

    /// <summary>Active types only, for the "change user type" dropdown on the users screen.</summary>
    Task<List<UserTypeViewModel>> GetActiveAsync();

    Task<(bool success, string message, UserTypeViewModel? data)> GetByIdAsync(int id);
    Task<(bool success, string message, UserTypeViewModel? data)> CreateAsync(UserTypeFormViewModel form);
    Task<(bool success, string message, UserTypeViewModel? data)> UpdateAsync(int id, UserTypeFormViewModel form);
    Task<(bool success, string message)> SetActiveAsync(int id, bool isActive);
    Task<(bool success, string message)> DeleteAsync(int id);
}
