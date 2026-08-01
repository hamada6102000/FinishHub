using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test.Helpers;
using test.Services;

namespace test.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly FavoriteService _favorites;

    public FavoritesController(FavoriteService favorites) => _favorites = favorites;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);

    /// <summary>Add an engineer to the logged-in user's favorites.</summary>
    [HttpPost("{engineerId}")]
    public async Task<IActionResult> Add(int engineerId)
    {
        var (result, message) = await _favorites.AddAsync(CurrentUserId, engineerId);
        return result switch
        {
            FavoriteResult.Success => Ok(ApiResponse.Success(null, message)),
            FavoriteResult.UserIsNotClient => StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Fail(message)),
            FavoriteResult.EngineerNotFound => NotFound(ApiResponse.Fail(message)),
            FavoriteResult.TargetIsNotEngineer => BadRequest(ApiResponse.Fail(message)),
            FavoriteResult.AlreadyFavorited => Conflict(ApiResponse.Fail(message)),
            _ => BadRequest(ApiResponse.Fail(message)),
        };
    }

    /// <summary>Remove an engineer from the logged-in user's favorites.</summary>
    [HttpDelete("{engineerId}")]
    public async Task<IActionResult> Remove(int engineerId)
    {
        var (result, message) = await _favorites.RemoveAsync(CurrentUserId, engineerId);
        return result switch
        {
            FavoriteResult.Success => Ok(ApiResponse.Success(null, message)),
            FavoriteResult.NotFavorited => NotFound(ApiResponse.Fail(message)),
            _ => BadRequest(ApiResponse.Fail(message)),
        };
    }

    /// <summary>Get a page of favorite engineers for the logged-in user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery pagination) =>
        Ok(ApiResponse.Success(await _favorites.GetFavoritesAsync(CurrentUserId, pagination)));
}
