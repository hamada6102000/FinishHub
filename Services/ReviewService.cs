using Microsoft.EntityFrameworkCore;
using test.Data;
using test.DTOs;
using test.Models;

namespace test.Services;

public class ReviewService
{
    private readonly AppDbContext _db;

    public ReviewService(AppDbContext db) => _db = db;

    public async Task<List<ReviewDto>> GetUserReviewsAsync(int userId)
    {
        var reviews = await _db.Reviews.Where(r => r.UserId == userId).ToListAsync();
        return reviews.Select(Map).ToList();
    }

    public async Task<ReviewDto?> GetByIdAsync(int id)
    {
        var r = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        return r == null ? null : Map(r);
    }

    public async Task<(bool success, string message, ReviewDto? dto)> AddAsync(int userId, AddReviewRequest req)
    {
        if (req.Rate < 1 || req.Rate > 5)
            return (false, "Rate must be between 1 and 5.", null);

        var review = new Review
        {
            UserId       = userId,
            ReviewerName = req.ReviewerName,
            Description  = req.Description,
            Rate         = req.Rate,
        };
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();
        return (true, "Review added.", Map(review));
    }

    public async Task<(bool success, ReviewDto? dto)> UpdateAsync(int id, UpdateReviewRequest req)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review == null) return (false, null);

        if (req.ReviewerName != null) review.ReviewerName = req.ReviewerName;
        if (req.Description  != null) review.Description  = req.Description;
        if (req.Rate.HasValue)
        {
            if (req.Rate < 1 || req.Rate > 5) return (false, null);
            review.Rate = req.Rate.Value;
        }

        await _db.SaveChangesAsync();
        return (true, Map(review));
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review == null) return false;

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
        return true;
    }

    private static ReviewDto Map(Review r) => new()
    {
        Id           = r.Id,
        UserId       = r.UserId,
        ReviewerName = r.ReviewerName,
        Description  = r.Description,
        Rate         = r.Rate,
        CreatedAt    = r.CreatedAt,
    };
}
