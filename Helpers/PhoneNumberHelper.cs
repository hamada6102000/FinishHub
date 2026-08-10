using System.Text.RegularExpressions;

namespace test.Helpers;

/// <summary>
/// Shared phone-number normalisation and validation used by the WhatsApp number
/// validators and service so the frontend, the validator and the stored value all
/// agree on what a valid number looks like.
/// </summary>
public static class PhoneNumberHelper
{
    /// <summary>Matches the <c>PhoneNumber</c> column length.</summary>
    public const int MaxLength = 30;

    /// <summary>
    /// E.164-style: optional leading "+", a non-zero country digit, then 7-14 more digits.
    /// Applied to the normalised value, so spaces, dashes and brackets are tolerated in input.
    /// </summary>
    public const string Pattern = @"^\+?[1-9]\d{7,14}$";

    public const string Message =
        "WhatsApp number must be a valid international phone number, for example +971501234567.";

    private static readonly Regex E164 = new(Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly char[] Separators = { ' ', '\t', '-', '(', ')', '.', '/', ' ' };

    /// <summary>
    /// Strips formatting characters and converts a leading international "00" prefix to "+".
    /// Returns an empty string for null/blank input.
    /// </summary>
    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        var cleaned = new string(raw.Where(c => !Separators.Contains(c)).ToArray());

        if (cleaned.StartsWith("00", StringComparison.Ordinal))
            cleaned = "+" + cleaned[2..];

        return cleaned;
    }

    /// <summary>True when the normalised value is a valid international phone number.</summary>
    public static bool IsValid(string? raw)
    {
        var normalized = Normalize(raw);
        return normalized.Length <= MaxLength && E164.IsMatch(normalized);
    }
}
