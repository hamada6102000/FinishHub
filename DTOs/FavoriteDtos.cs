namespace test.DTOs;

// ---------- Favorite ----------

public class FavoriteEngineerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string? City { get; set; }
    public string ProfileImage { get; set; } = string.Empty;
    public int? TotalExperience { get; set; }
}
