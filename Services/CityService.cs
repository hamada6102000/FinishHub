using Microsoft.EntityFrameworkCore;
using test.Data;
using test.DTOs;
using test.Helpers;
using test.Models;

namespace test.Services;

public enum CityResult
{
    Success,
    NotFound,
    DuplicateName,
    InUse,
}

public class CityService
{
    private readonly AppDbContext _db;

    public CityService(AppDbContext db) => _db = db;

    public async Task<(CityResult result, string message, CityDto? data)> CreateAsync(CreateCityRequest req, string lang = "en")
    {
        var duplicate = await _db.Cities.AnyAsync(c => c.NameEn == req.NameEn || c.NameAr == req.NameAr);
        if (duplicate)
            return (CityResult.DuplicateName, "A city with this name already exists.", null);

        var city = new City { NameAr = req.NameAr, NameEn = req.NameEn, IsPinned = req.IsPinned };
        _db.Cities.Add(city);
        await _db.SaveChangesAsync();

        return (CityResult.Success, "City created.", Map(city, lang));
    }

    public async Task<PagedResult<CityDto>> GetAllAsync(PaginationQuery pagination, string lang = "en")
    {
        var page = await _db.Cities.AsNoTracking()
            .OrderByDescending(c => c.IsPinned)
            .ThenBy(c => c.NameEn)
            .ToPagedResultAsync(pagination);
        return page.Map(c => Map(c, lang));
    }

    public async Task<CityDto?> GetByIdAsync(int id, string lang = "en")
    {
        var city = await _db.Cities.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return city == null ? null : Map(city, lang);
    }

    public async Task<(CityResult result, string message, CityDto? data)> UpdateAsync(int id, UpdateCityRequest req, string lang = "en")
    {
        var city = await _db.Cities.FirstOrDefaultAsync(c => c.Id == id);
        if (city == null)
            return (CityResult.NotFound, "City not found.", null);

        var duplicate = await _db.Cities.AnyAsync(c => c.Id != id && (c.NameEn == req.NameEn || c.NameAr == req.NameAr));
        if (duplicate)
            return (CityResult.DuplicateName, "A city with this name already exists.", null);

        city.NameAr   = req.NameAr;
        city.NameEn   = req.NameEn;
        city.IsPinned = req.IsPinned;
        await _db.SaveChangesAsync();

        return (CityResult.Success, "City updated.", Map(city, lang));
    }

    public async Task<(CityResult result, string message, CityDto? data)> SetPinnedAsync(int id, bool isPinned, string lang = "en")
    {
        var city = await _db.Cities.FirstOrDefaultAsync(c => c.Id == id);
        if (city == null)
            return (CityResult.NotFound, "City not found.", null);

        city.IsPinned = isPinned;
        await _db.SaveChangesAsync();

        return (CityResult.Success, isPinned ? "City pinned." : "City unpinned.", Map(city, lang));
    }

    public async Task<(CityResult result, string message)> DeleteAsync(int id)
    {
        var city = await _db.Cities.FirstOrDefaultAsync(c => c.Id == id);
        if (city == null)
            return (CityResult.NotFound, "City not found.");

        var inUse = await _db.Users.AnyAsync(u => u.CityId == id);
        if (inUse)
            return (CityResult.InUse, "Cannot delete a city that is assigned to existing users.");

        _db.Cities.Remove(city);
        await _db.SaveChangesAsync();

        return (CityResult.Success, "City deleted.");
    }

    private static CityDto Map(City city, string lang) => new()
    {
        Id        = city.Id,
        Name      = lang == "ar" ? city.NameAr : city.NameEn,
        NameEn    = city.NameEn,
        NameAr    = city.NameAr,
        IsPinned  = city.IsPinned,
        CreatedAt = city.CreatedAt,
    };
}
