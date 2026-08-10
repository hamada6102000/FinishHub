namespace test.Models;

/// <summary>
/// Global, solution-level WhatsApp contact number.
/// The table is constrained to a single row: <see cref="Id"/> is always
/// <see cref="SingletonId"/> and a database check constraint rejects any other value,
/// so a second number can never be inserted — not even by calling the API directly
/// or writing to the database outside the Dashboard.
/// The row can be updated but is never deleted.
/// </summary>
public class WhatsAppNumber
{
    /// <summary>The only id this table ever holds.</summary>
    public const int SingletonId = 1;

    public int Id { get; set; } = SingletonId;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Null until the number is updated for the first time.</summary>
    public DateTime? UpdatedAt { get; set; }
}
