using Microsoft.AspNetCore.Mvc;
using test.Helpers;
using test.Models;

namespace test.DTOs;

// ---------- Project ----------

public class CreateProjectRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int? CityId { get; set; }
    public PropertyType PropertyType { get; set; }
    public string? Description { get; set; }
    public string? Area { get; set; }
    public string? Timeline { get; set; }
    public string? Budget { get; set; }
    public string? Category { get; set; }
    [ModelBinder(BinderType = typeof(JsonFormDataModelBinder))]
    public List<ProjectMaterialRequest>? Materials { get; set; }
    public List<IFormFile>? Images { get; set; }
}

public class UpdateProjectRequest
{
    public string? Title { get; set; }
    public string? Location { get; set; }
    public int? CityId { get; set; }
    public PropertyType? PropertyType { get; set; }
    public string? Description { get; set; }
    public string? Area { get; set; }
    public string? Timeline { get; set; }
    public string? Budget { get; set; }
    public string? Category { get; set; }
    public List<ProjectMaterialRequest>? Materials { get; set; }
}

public class ProjectMaterialRequest
{
    public string MaterialName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ProjectDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? City { get; set; }
    public PropertyType PropertyType { get; set; }
    public string? Description { get; set; }
    public string? Area { get; set; }
    public string? Timeline { get; set; }
    public string? Budget { get; set; }
    public string? Category { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public double Rate { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProjectMediaDto> Media { get; set; } = new();
    public List<ProjectMaterialDto> Materials { get; set; } = new();
}

public class SetFeaturedRequest
{
    public bool IsFeatured { get; set; }
}

public class SetProjectActiveRequest
{
    public bool IsActive { get; set; }
}

public class RateProjectRequest
{
    public double Value { get; set; }
}

public class ProjectMediaDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
}

public class ProjectMaterialDto
{
    public int Id { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
