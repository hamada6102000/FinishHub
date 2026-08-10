using test.Models;

namespace test.DTOs;

// ---------- Design Conversation ----------

public class BookDesignConversationRequest
{
    public string FullName { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;
    public int CityId { get; set; }
    public DesignService Service { get; set; }
    public DateTime PreferredSlotDate { get; set; }
    public string? ProjectBrief { get; set; }
}

public class DesignConversationUserInfo
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
}

public class DesignConversationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EngineerId { get; set; }
    public DesignConversationUserInfo? User { get; set; }
    public DesignConversationUserInfo? Engineer { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;
    public int CityId { get; set; }
    public string? City { get; set; }
    public DesignService Service { get; set; }
    public DateTime PreferredSlotDate { get; set; }
    public string? ProjectBrief { get; set; }
    public DateTime CreatedAt { get; set; }
}
