using Microsoft.EntityFrameworkCore;
using test.Data;
using test.DTOs;
using test.Helpers;
using test.Models;

namespace test.Services;

public enum UserTypeResult
{
    Success,
    NotFound,
    DuplicateName,
    InUse,
    SystemType,
}

/// <summary>Outcome of resolving a UserTypeId that is about to be assigned to a user.</summary>
public enum UserTypeAssignment
{
    Ok,
    NotFound,
    Inactive,
}

public class UserTypeService
{
    private readonly AppDbContext _db;

    public UserTypeService(AppDbContext db) => _db = db;

    /// <summary>
    /// The one place a UserTypeId coming from a client is validated. Registration, Google login
    /// and the admin "change type" endpoint all go through this, so the rules cannot drift apart:
    /// the type must exist and must be active.
    /// </summary>
    internal static async Task<(UserTypeAssignment result, string message, UserType? type)> ResolveAssignableAsync(
        AppDbContext db, int userTypeId)
    {
        if (userTypeId <= 0)
            return (UserTypeAssignment.NotFound, "Invalid user type selected.", null);

        var type = await db.UserTypes.FirstOrDefaultAsync(t => t.Id == userTypeId);
        if (type == null)
            return (UserTypeAssignment.NotFound, "Invalid user type selected.", null);

        if (!type.IsActive)
            return (UserTypeAssignment.Inactive, "The selected user type is not active.", null);

        return (UserTypeAssignment.Ok, "Success", type);
    }

    /// <summary>
    /// Assigns a type to a user. Always sets the behaviour bucket alongside the foreign key so
    /// User.UserType can never drift away from the type the user actually holds.
    /// </summary>
    internal static void Assign(User user, UserType type)
    {
        user.UserTypeId = type.Id;
        user.UserType   = type.Kind;
    }

    public async Task<(UserTypeResult result, string message, UserTypeDto? data)> CreateAsync(CreateUserTypeRequest req, string lang = "en")
    {
        var nameEn = req.NameEn.Trim();
        var nameAr = req.NameAr.Trim();

        var duplicate = await _db.UserTypes.AnyAsync(t => t.NameEn == nameEn || t.NameAr == nameAr);
        if (duplicate)
            return (UserTypeResult.DuplicateName, "A user type with this name already exists.", null);

        var type = new UserType
        {
            NameAr   = nameAr,
            NameEn   = nameEn,
            IsActive = req.IsActive,
            // Administrator-created types are ordinary users behaviourally: only the built-in
            // Engineer type is served by /api/engineers, favourites and design conversations.
            Kind     = UserTypeKind.User,
            IsSystem = false,
            Code     = null,
        };

        _db.UserTypes.Add(type);
        await _db.SaveChangesAsync();

        return (UserTypeResult.Success, "User type created.", Map(type, 0, lang));
    }

    /// <summary>
    /// A page of user types. Active only by default — that is what the signup screen calls;
    /// the Dashboard passes includeInactive=true to manage deactivated types as well.
    /// </summary>
    public async Task<PagedResult<UserTypeDto>> GetAllAsync(PaginationQuery pagination, string lang = "en")
    {
        var query = _db.UserTypes.AsNoTracking().AsQueryable();
        if (!pagination.IncludeInactive)
            query = query.Where(t => t.IsActive);

        var page = await query
            .OrderByDescending(t => t.IsSystem)
            .ThenBy(t => t.NameEn)
            .Select(t => new Row { Type = t, UsersCount = t.Users.Count })
            .ToPagedResultAsync(pagination);

        return page.Map(r => Map(r.Type, r.UsersCount, lang));
    }

    public async Task<UserTypeDto?> GetByIdAsync(int id, string lang = "en")
    {
        var row = await _db.UserTypes
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new Row { Type = t, UsersCount = t.Users.Count })
            .FirstOrDefaultAsync();

        return row == null ? null : Map(row.Type, row.UsersCount, lang);
    }

    public async Task<(UserTypeResult result, string message, UserTypeDto? data)> UpdateAsync(int id, UpdateUserTypeRequest req, string lang = "en")
    {
        var type = await _db.UserTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (type == null)
            return (UserTypeResult.NotFound, "User type not found.", null);

        var nameEn = req.NameEn.Trim();
        var nameAr = req.NameAr.Trim();

        var duplicate = await _db.UserTypes.AnyAsync(t => t.Id != id && (t.NameEn == nameEn || t.NameAr == nameAr));
        if (duplicate)
            return (UserTypeResult.DuplicateName, "A user type with this name already exists.", null);

        // Built-in types may be renamed freely — nothing looks them up by name, only by id and
        // Code — but they may never be deactivated, or the engineer/client rules lose their type.
        if (type.IsSystem && !req.IsActive)
            return (UserTypeResult.SystemType, "Built-in user types cannot be deactivated.", null);

        type.NameAr   = nameAr;
        type.NameEn   = nameEn;
        type.IsActive = req.IsActive;
        await _db.SaveChangesAsync();

        var usersCount = await _db.Users.CountAsync(u => u.UserTypeId == id);
        return (UserTypeResult.Success, "User type updated.", Map(type, usersCount, lang));
    }

    /// <summary>
    /// Activates or deactivates a type. Deactivating is allowed even while users hold the type —
    /// that is precisely when an administrator wants to retire it — and never touches those users:
    /// they keep their type, they are just the last ones who can have it. The type simply stops
    /// appearing at signup and can no longer be assigned to anyone.
    /// </summary>
    public async Task<(UserTypeResult result, string message, UserTypeDto? data)> SetActiveAsync(int id, bool isActive, string lang = "en")
    {
        var type = await _db.UserTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (type == null)
            return (UserTypeResult.NotFound, "User type not found.", null);

        if (type.IsSystem && !isActive)
            return (UserTypeResult.SystemType, "Built-in user types cannot be deactivated.", null);

        type.IsActive = isActive;
        await _db.SaveChangesAsync();

        var usersCount = await _db.Users.CountAsync(u => u.UserTypeId == id);
        return (UserTypeResult.Success, isActive ? "User type activated." : "User type deactivated.", Map(type, usersCount, lang));
    }

    public async Task<(UserTypeResult result, string message)> DeleteAsync(int id)
    {
        var type = await _db.UserTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (type == null)
            return (UserTypeResult.NotFound, "User type not found.");

        if (type.IsSystem)
            return (UserTypeResult.SystemType, "Built-in user types cannot be deleted.");

        var inUse = await _db.Users.AnyAsync(u => u.UserTypeId == id);
        if (inUse)
            return (UserTypeResult.InUse, "Cannot delete a user type that is assigned to existing users. Deactivate it instead.");

        _db.UserTypes.Remove(type);
        await _db.SaveChangesAsync();

        return (UserTypeResult.Success, "User type deleted.");
    }

    /// <summary>A type plus how many users hold it, so the page query stays a single round trip.</summary>
    private class Row
    {
        public UserType Type { get; set; } = null!;
        public int UsersCount { get; set; }
    }

    private static UserTypeDto Map(UserType type, int usersCount, string lang) => new()
    {
        Id         = type.Id,
        Name       = lang == "ar" ? type.NameAr : type.NameEn,
        NameEn     = type.NameEn,
        NameAr     = type.NameAr,
        Code       = type.Code,
        IsSystem   = type.IsSystem,
        IsActive   = type.IsActive,
        UsersCount = usersCount,
        CreatedAt  = type.CreatedAt,
    };
}
