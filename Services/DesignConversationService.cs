using Microsoft.EntityFrameworkCore;
using test.Data;
using test.DTOs;
using test.Models;

namespace test.Services;

public enum DesignConversationResult
{
    Success,
    NotFound,
    EngineerNotFound,
    TargetIsNotEngineer,
    CityNotFound,
    InvalidSlotDate,
}

public class DesignConversationService
{
    private readonly AppDbContext _db;

    public DesignConversationService(AppDbContext db) => _db = db;

    public async Task<(DesignConversationResult result, string message, DesignConversationDto? dto)> BookAsync(
        int userId, int engineerId, BookDesignConversationRequest req)
    {
        var engineer = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == engineerId);
        if (engineer == null)
            return (DesignConversationResult.EngineerNotFound, "Engineer not found.", null);

        if (engineer.UserType != UserType.Engineer)
            return (DesignConversationResult.TargetIsNotEngineer, "A conversation can only be booked with an engineer.", null);

        if (!await _db.Cities.AnyAsync(c => c.Id == req.CityId))
            return (DesignConversationResult.CityNotFound, "City not found.", null);

        if (req.PreferredSlotDate <= DateTime.UtcNow)
            return (DesignConversationResult.InvalidSlotDate, "Preferred slot date must be in the future.", null);

        var entity = new DesignConversationRequest
        {
            UserId            = userId,
            EngineerId        = engineerId,
            FullName          = req.FullName.Trim(),
            WhatsAppNumber    = req.WhatsAppNumber.Trim(),
            CityId            = req.CityId,
            Service           = req.Service,
            PreferredSlotDate = req.PreferredSlotDate,
            ProjectBrief      = string.IsNullOrWhiteSpace(req.ProjectBrief) ? null : req.ProjectBrief.Trim(),
        };

        _db.DesignConversationRequests.Add(entity);
        await _db.SaveChangesAsync();

        var dto = await GetByIdAsync(entity.Id);
        return (DesignConversationResult.Success, "Design conversation booked.", dto);
    }

    public async Task<DesignConversationDto?> GetByIdAsync(int id) =>
        await BaseQuery().Where(r => r.Id == id).Select(Projection).FirstOrDefaultAsync();

    /// <summary>Requests the given user created as a client.</summary>
    public async Task<List<DesignConversationDto>> GetMyRequestsAsync(int userId) =>
        await BaseQuery()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(Projection)
            .ToListAsync();

    /// <summary>Requests received by the given user as the engineer.</summary>
    public async Task<List<DesignConversationDto>> GetReceivedRequestsAsync(int engineerId) =>
        await BaseQuery()
            .Where(r => r.EngineerId == engineerId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(Projection)
            .ToListAsync();

    private IQueryable<DesignConversationRequest> BaseQuery() =>
        _db.DesignConversationRequests
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Engineer)
            .Include(r => r.City);

    private static readonly System.Linq.Expressions.Expression<Func<DesignConversationRequest, DesignConversationDto>> Projection =
        r => new DesignConversationDto
        {
            Id                = r.Id,
            UserId            = r.UserId,
            EngineerId        = r.EngineerId,
            User              = new DesignConversationUserInfo
            {
                Id              = r.User.Id,
                NameAr          = r.User.NameAr,
                NameEn          = r.User.NameEn,
                Position        = r.User.Position,
                ProfileImageUrl = r.User.ProfileImageUrl,
            },
            Engineer          = new DesignConversationUserInfo
            {
                Id              = r.Engineer.Id,
                NameAr          = r.Engineer.NameAr,
                NameEn          = r.Engineer.NameEn,
                Position        = r.Engineer.Position,
                ProfileImageUrl = r.Engineer.ProfileImageUrl,
            },
            FullName          = r.FullName,
            WhatsAppNumber    = r.WhatsAppNumber,
            CityId            = r.CityId,
            City              = r.City.NameEn,
            Service           = r.Service,
            PreferredSlotDate = r.PreferredSlotDate,
            ProjectBrief      = r.ProjectBrief,
            CreatedAt         = r.CreatedAt,
        };
}
