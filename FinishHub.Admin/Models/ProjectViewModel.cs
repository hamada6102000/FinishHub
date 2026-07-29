namespace FinishHub.Admin.Models;

public class ProjectViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserNameAr { get; set; } = string.Empty;
    public string UserNameEn { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string PropertyType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProjectMediaViewModel> Media { get; set; } = new();
}

public class ProjectMediaViewModel
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
}
