namespace test.DTOs;

// ---------- WhatsApp number (global, single-record configuration) ----------

public class WhatsAppNumberDto
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateWhatsAppNumberRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public class UpdateWhatsAppNumberRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}
