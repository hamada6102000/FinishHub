using test.Models;

namespace test.DTOs;

// ---------- Project ----------

public class CreateProjectRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public PropertyType PropertyType { get; set; }
    public string? Description { get; set; }
    public List<IFormFile>? Images { get; set; }
    public List<IFormFile>? Videos { get; set; }
}

public class UpdateProjectRequest
{
    public string? Title { get; set; }
    public string? Location { get; set; }
    public PropertyType? PropertyType { get; set; }
    public string? Description { get; set; }
}

public class ProjectDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserNameAr { get; set; } = string.Empty;
    public string UserNameEn { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public PropertyType PropertyType { get; set; }
    public string? Description { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProjectMediaDto> Media { get; set; } = new();
}

public class SetFeaturedRequest
{
    public bool IsFeatured { get; set; }
}

public class ProjectMediaDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
}
