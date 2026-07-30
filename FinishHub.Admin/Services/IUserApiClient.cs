using FinishHub.Admin.Models;

namespace FinishHub.Admin.Services;

public interface IUserApiClient
{
    Task<(bool success, string message, List<UserViewModel> data)> GetAllAsync();
    Task<(bool success, string message)> SetTrustedAsync(int id, bool isTrusted);
}
