namespace test.Models;

public class Project
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int? CityId { get; set; }
    public PropertyType PropertyType { get; set; }
    public string? Description { get; set; }
    public string? Area { get; set; }
    public string? Timeline { get; set; }
    public string? Budget { get; set; }
    public string? Category { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public City? City { get; set; }
    public List<ProjectMedia> Media { get; set; } = new();
    public List<ProjectMaterial> Materials { get; set; } = new();
}

public class ProjectMaterial
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Project Project { get; set; } = null!;
}

public class ProjectMedia
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Url { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }

    public Project Project { get; set; } = null!;
}


