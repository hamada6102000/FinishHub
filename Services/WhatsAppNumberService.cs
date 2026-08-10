using Microsoft.EntityFrameworkCore;
using test.Data;
using test.DTOs;
using test.Helpers;
using test.Models;

namespace test.Services;

public enum WhatsAppNumberResult
{
    Success,
    NotFound,
    AlreadyExists,
    DuplicateNumber,
    InvalidNumber,
}

/// <summary>
/// Manages the single, solution-wide WhatsApp number.
/// Supported operations are Get / Add / Update only — the number is never deleted.
/// The "at most one number" rule is enforced here and, independently, by the
/// <c>CK_WhatsAppNumbers_SingleRow</c> check constraint in the database.
/// </summary>
public class WhatsAppNumberService
{
    private readonly AppDbContext _db;
    private readonly ILogger<WhatsAppNumberService> _logger;

    public WhatsAppNumberService(AppDbContext db, ILogger<WhatsAppNumberService> logger)
    {
        _db     = db;
        _logger = logger;
    }

    /// <summary>Returns the configured WhatsApp number, or null when none has been added yet.</summary>
    public async Task<WhatsAppNumberDto?> GetAsync()
    {
        var entity = await _db.WhatsAppNumbers.AsNoTracking().FirstOrDefaultAsync();
        return entity == null ? null : Map(entity);
    }

    /// <summary>Adds the WhatsApp number. Fails when one already exists.</summary>
    public async Task<(WhatsAppNumberResult result, string message, WhatsAppNumberDto? data)> CreateAsync(
        CreateWhatsAppNumberRequest req)
    {
        var phoneNumber = PhoneNumberHelper.Normalize(req.PhoneNumber);
        if (!PhoneNumberHelper.IsValid(phoneNumber))
            return (WhatsAppNumberResult.InvalidNumber, PhoneNumberHelper.Message, null);

        var existing = await _db.WhatsAppNumbers.AsNoTracking().FirstOrDefaultAsync();
        if (existing != null)
        {
            _logger.LogWarning(
                "Rejected an attempt to add a second WhatsApp number ({Attempted}); {Existing} is already configured.",
                phoneNumber, existing.PhoneNumber);

            return (WhatsAppNumberResult.AlreadyExists,
                "Only one WhatsApp number is allowed for the whole solution. A number is already configured — update it instead.",
                null);
        }

        var entity = new WhatsAppNumber
        {
            Id          = WhatsAppNumber.SingletonId,
            PhoneNumber = phoneNumber,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = null,
        };

        _db.WhatsAppNumbers.Add(entity);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // The primary key / check constraint is the last line of defence when two
            // requests race past the existence check above.
            _db.Entry(entity).State = EntityState.Detached;
            _logger.LogWarning(ex, "Database rejected adding WhatsApp number {PhoneNumber}.", phoneNumber);

            return (WhatsAppNumberResult.AlreadyExists,
                "Only one WhatsApp number is allowed for the whole solution. A number is already configured — update it instead.",
                null);
        }

        _logger.LogInformation("WhatsApp number {PhoneNumber} added.", phoneNumber);
        return (WhatsAppNumberResult.Success, "WhatsApp number added.", Map(entity));
    }

    /// <summary>Updates the existing WhatsApp number. Fails when none has been added yet.</summary>
    public async Task<(WhatsAppNumberResult result, string message, WhatsAppNumberDto? data)> UpdateAsync(
        UpdateWhatsAppNumberRequest req)
    {
        var phoneNumber = PhoneNumberHelper.Normalize(req.PhoneNumber);
        if (!PhoneNumberHelper.IsValid(phoneNumber))
            return (WhatsAppNumberResult.InvalidNumber, PhoneNumberHelper.Message, null);

        var entity = await _db.WhatsAppNumbers.FirstOrDefaultAsync();
        if (entity == null)
            return (WhatsAppNumberResult.NotFound,
                "No WhatsApp number has been configured yet. Add one first.", null);

        if (entity.PhoneNumber == phoneNumber)
            return (WhatsAppNumberResult.Success, "WhatsApp number is already up to date.", Map(entity));

        var previous = entity.PhoneNumber;
        entity.PhoneNumber = phoneNumber;
        entity.UpdatedAt   = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Database rejected updating the WhatsApp number to {PhoneNumber}.", phoneNumber);
            return (WhatsAppNumberResult.DuplicateNumber, "This WhatsApp number is already in use.", null);
        }

        _logger.LogInformation("WhatsApp number updated from {Previous} to {PhoneNumber}.", previous, phoneNumber);
        return (WhatsAppNumberResult.Success, "WhatsApp number updated.", Map(entity));
    }

    private static WhatsAppNumberDto Map(WhatsAppNumber entity) => new()
    {
        Id          = entity.Id,
        PhoneNumber = entity.PhoneNumber,
        CreatedAt   = entity.CreatedAt,
        UpdatedAt   = entity.UpdatedAt,
    };
}
