namespace FinishHub.Admin.Models;

public class ProjectViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string PropertyType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Area { get; set; }
    public string? Timeline { get; set; }
    public string? Budget { get; set; }
    public string? Category { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProjectMediaViewModel> Media { get; set; } = new();
    public List<ProjectMaterialViewModel> Materials { get; set; } = new();
}

public class ProjectMediaViewModel
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
}

public class ProjectMaterialViewModel
{
    public int Id { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
