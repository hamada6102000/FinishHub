namespace test.Models;

public class Project
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public PropertyType PropertyType { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public List<ProjectMedia> Media { get; set; } = new();
}

public class ProjectMedia
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Url { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }

    public Project Project { get; set; } = null!;
}
