namespace test.DTOs;

// ---------- Review ----------

public class AddReviewRequest
{
    public string? Description { get; set; }
    public int Rate { get; set; }
}

public class UpdateReviewRequest
{
    public string? Description { get; set; }
    public int? Rate { get; set; }
}

public class ReviewerInfo
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
}

public class ReviewDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ReviewerInfo? Reviewer { get; set; }
    public string? Description { get; set; }
    public int Rate { get; set; }
    public DateTime CreatedAt { get; set; }
}
