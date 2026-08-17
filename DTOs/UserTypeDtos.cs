namespace test.DTOs;

// ---------- UserType ----------

public class UserTypeDto
{
    public int Id { get; set; }

    /// <summary>Localised name, picked from Accept-Language exactly like CityDto.Name.</summary>
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;

    /// <summary>Stable key for the built-in types ("USER", "ENGINEER"); null for custom types.</summary>
    public string? Code { get; set; }

    /// <summary>True for the built-in types, which cannot be renamed, deactivated or deleted.</summary>
    public bool IsSystem { get; set; }

    public bool IsActive { get; set; }

    /// <summary>How many users currently hold this type, so the Dashboard can warn before deactivating.</summary>
    public int UsersCount { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateUserTypeRequest
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class UpdateUserTypeRequest
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class SetUserTypeActiveRequest
{
    public bool IsActive { get; set; }
}
