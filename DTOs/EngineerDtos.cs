namespace test.DTOs;

// ---------- Engineer Summary (list) ----------

public class EngineerSummaryDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Bio { get; set; }
    public int? TotalExperience { get; set; }
    public double Rating { get; set; }
    public int ReviewsCount { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsFavourite { get; set; }
}

// ---------- Engineer Profile ----------

public class EngineerProfileDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Bio { get; set; }
    public int? TotalExperience { get; set; }
    public double Rating { get; set; }
    public int ReviewsCount { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsFavourite { get; set; }
    public List<ProjectDto> Projects { get; set; } = new();
    public PortfolioDto? Portfolio { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();
}
