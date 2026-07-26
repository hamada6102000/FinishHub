using Microsoft.EntityFrameworkCore;
using test.Data;
using test.DTOs;
using test.Models;

namespace test.Services;

public enum FavoriteResult
{
    Success,
    UserIsNotClient,
    EngineerNotFound,
    TargetIsNotEngineer,
    AlreadyFavorited,
    NotFavorited,
}

public class FavoriteService
{
    private readonly AppDbContext _db;

    public FavoriteService(AppDbContext db) => _db = db;

    public async Task<(FavoriteResult result, string message)> AddAsync(int userId, int engineerId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || user.UserType != UserType.User)
            return (FavoriteResult.UserIsNotClient, "Only users can add favorites.");

        var engineer = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == engineerId);
        if (engineer == null)
            return (FavoriteResult.EngineerNotFound, "Engineer not found.");

        if (engineer.UserType != UserType.Engineer)
            return (FavoriteResult.TargetIsNotEngineer, "Only engineers can be added to favorites.");

        var alreadyFavorited = await _db.Favorites.AnyAsync(f => f.UserId == userId && f.EngineerId == engineerId);
        if (alreadyFavorited)
            return (FavoriteResult.AlreadyFavorited, "Engineer is already in your favorites.");

        _db.Favorites.Add(new Favorite { UserId = userId, EngineerId = engineerId });

        var engineerTracked = await _db.Users.FirstOrDefaultAsync(u => u.Id == engineerId);
        if (engineerTracked != null)
            engineerTracked.IsFavourite = true;

        await _db.SaveChangesAsync();
        return (FavoriteResult.Success, "Engineer added to favorites.");
    }

    public async Task<(FavoriteResult result, string message)> RemoveAsync(int userId, int engineerId)
    {
        var favorite = await _db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.EngineerId == engineerId);
        if (favorite == null)
            return (FavoriteResult.NotFavorited, "Engineer is not in your favorites.");

        _db.Favorites.Remove(favorite);

        var stillFavorited = await _db.Favorites.AnyAsync(f => f.EngineerId == engineerId && f.UserId != userId);
        if (!stillFavorited)
        {
            var engineerTracked = await _db.Users.FirstOrDefaultAsync(u => u.Id == engineerId);
            if (engineerTracked != null)
                engineerTracked.IsFavourite = false;
        }

        await _db.SaveChangesAsync();
        return (FavoriteResult.Success, "Engineer removed from favorites.");
    }

    public async Task<List<FavoriteEngineerDto>> GetFavoritesAsync(int userId)
    {
        var favorites = await _db.Favorites
            .AsNoTracking()
            .Include(f => f.Engineer).ThenInclude(e => e.City)
            .Where(f => f.UserId == userId)
            .ToListAsync();

        if (favorites.Count == 0) return new();

        var engineerIds = favorites.Select(f => f.EngineerId).ToList();
        var ratings = await _db.Reviews
            .AsNoTracking()
            .Where(r => engineerIds.Contains(r.UserId))
            .GroupBy(r => r.UserId)
            .Select(g => new { UserId = g.Key, Average = g.Average(r => (double)r.Rate) })
            .ToDictionaryAsync(x => x.UserId, x => x.Average);

        return favorites.Select(f => Map(f.Engineer, ratings)).ToList();
    }

    private static FavoriteEngineerDto Map(User engineer, Dictionary<int, double> ratings) => new()
    {
        Id           = engineer.Id,
        Name         = string.IsNullOrWhiteSpace(engineer.NameEn) ? engineer.NameAr : engineer.NameEn,
        Position     = engineer.Position,
        Rating       = ratings.TryGetValue(engineer.Id, out var avg) ? Math.Round(avg, 1) : 0,
        City         = engineer.City?.NameEn,
        ProfileImage = engineer.ProfileImageUrl,
    };
}
