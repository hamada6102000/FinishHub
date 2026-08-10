using System.ComponentModel.DataAnnotations;

namespace FinishHub.Admin.Models;

public class WhatsAppNumberViewModel
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class WhatsAppNumberFormViewModel
{
    /// <summary>
    /// Client-side mirror of the API rule: digits with an optional leading "+", and the
    /// usual formatting characters tolerated. Deliberately a little looser than the
    /// backend rule so it never rejects a number the API would accept — the backend
    /// re-validates and is the final authority.
    /// </summary>
    public const string PhonePattern = @"^\+?[\d\s\-\(\)\.]{8,29}$";

    public const string PhoneMessage =
        "Enter a valid international phone number, for example +971501234567.";

    [Required(ErrorMessage = "WhatsApp number is required.")]
    [RegularExpression(PhonePattern, ErrorMessage = PhoneMessage)]
    [Display(Name = "WhatsApp number")]
    public string PhoneNumber { get; set; } = string.Empty;
}
