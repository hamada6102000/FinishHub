namespace test.DTOs;

// ---------- City ----------

public class CityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCityRequest
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
}

public class UpdateCityRequest
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
}

public class SetCityPinnedRequest
{
    public bool IsPinned { get; set; }
}
