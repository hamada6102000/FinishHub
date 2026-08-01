using FinishHub.Admin.Models;

namespace FinishHub.Admin.Services;

public interface IUserApiClient
{
    Task<(bool success, string message, PagedResult<UserViewModel> data)> GetAllAsync(int page, int pageSize);
    Task<(bool success, string message)> SetTrustedAsync(int id, bool isTrusted);
}
