namespace test.Models;

public class DesignConversationRequest
{
    public int Id { get; set; }

    /// <summary>The client who booked the conversation (taken from the token).</summary>
    public int UserId { get; set; }

    /// <summary>The engineer the conversation was requested with.</summary>
    public int EngineerId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;
    public int CityId { get; set; }
    public DesignService Service { get; set; }
    public DateTime PreferredSlotDate { get; set; }
    public string? ProjectBrief { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public User Engineer { get; set; } = null!;
    public City City { get; set; } = null!;
}
