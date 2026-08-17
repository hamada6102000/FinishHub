using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinishHub.Admin.Models;

public class UserViewModel
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// The behaviour bucket ("User" / "Engineer"). The API writes this as an enum name, but
    /// falls back to the raw number for values that are not defined in the enum (e.g. a user
    /// stored with 0). The configurable type shown to administrators is UserTypeName.
    /// </summary>
    [JsonConverter(typeof(LenientStringConverter))]
    public string UserType { get; set; } = string.Empty;

    /// <summary>Id of the configurable user type, used to preselect the change-type dropdown.</summary>
    public int UserTypeId { get; set; }

    /// <summary>Configurable type name, e.g. "Carpenter". Falls back to UserType when absent.</summary>
    public string? UserTypeName { get; set; }

    /// <summary>What the users table shows in the Type column.</summary>
    public string DisplayUserType =>
        string.IsNullOrWhiteSpace(UserTypeName) ? UserType : UserTypeName;

    public bool IsActive { get; set; }
    public bool IsTrusted { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Reads a JSON string, number or null into a string without throwing.</summary>
public class LenientStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            JsonTokenType.Null   => string.Empty,
            JsonTokenType.Number => reader.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
            JsonTokenType.True   => "true",
            JsonTokenType.False  => "false",
            _                    => string.Empty,
        };

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}
