using Microsoft.EntityFrameworkCore;
using test.Data;
using test.DTOs;
using test.Helpers;
using test.Models;

namespace test.Services;

public class PortfolioService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public PortfolioService(AppDbContext db, IWebHostEnvironment env)
    {
        _db  = db;
        _env = env;
    }

    public async Task<PortfolioDto?> GetByUserAsync(int userId)
    {
        var p = await _db.Portfolios.Include(p => p.Media).FirstOrDefaultAsync(p => p.UserId == userId);
        return p == null ? null : Map(p);
    }

    public async Task<PortfolioDto> CreateAsync(int userId)
    {
        var existing = await _db.Portfolios.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existing != null) return Map(existing);

        var portfolio = new Portfolio { UserId = userId };
        _db.Portfolios.Add(portfolio);
        await _db.SaveChangesAsync();
        return Map(portfolio);
    }

    public async Task<bool> DeleteAsync(int userId)
    {
        var portfolio = await _db.Portfolios.FirstOrDefaultAsync(p => p.UserId == userId);
        if (portfolio == null) return false;

        _db.Portfolios.Remove(portfolio);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<(bool success, PortfolioDto? dto)> AddMediaAsync(int userId, AddPortfolioMediaRequest req)
    {
        var portfolio = await _db.Portfolios.Include(p => p.Media).FirstOrDefaultAsync(p => p.UserId == userId);
        if (portfolio == null) return (false, null);

        var imageUrls = await FileUploadHelper.SaveFilesAsync(req.Images, "portfolio/images", _env);
        var videoUrls = await FileUploadHelper.SaveFilesAsync(req.Videos, "portfolio/videos", _env);

        foreach (var url in imageUrls)
            _db.PortfolioMedia.Add(new PortfolioMedia { PortfolioId = portfolio.Id, Url = url, MediaType = MediaType.Image });
        foreach (var url in videoUrls)
            _db.PortfolioMedia.Add(new PortfolioMedia { PortfolioId = portfolio.Id, Url = url, MediaType = MediaType.Video });

        await _db.SaveChangesAsync();

        await _db.Entry(portfolio).Collection(p => p.Media).LoadAsync();
        return (true, Map(portfolio));
    }

    public async Task<bool> RemoveMediaAsync(int mediaId, int userId)
    {
        var portfolio = await _db.Portfolios.FirstOrDefaultAsync(p => p.UserId == userId);
        if (portfolio == null) return false;

        var media = await _db.PortfolioMedia.FirstOrDefaultAsync(m => m.Id == mediaId && m.PortfolioId == portfolio.Id);
        if (media == null) return false;

        _db.PortfolioMedia.Remove(media);
        await _db.SaveChangesAsync();
        return true;
    }

    private static PortfolioDto Map(Portfolio p) => new()
    {
        Id     = p.Id,
        UserId = p.UserId,
        Media  = p.Media.Select(m => new PortfolioMediaDto { Id = m.Id, Url = m.Url, MediaType = m.MediaType }).ToList(),
    };
}
