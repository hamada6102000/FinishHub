namespace test.DTOs;

// ---------- Review ----------

public class AddReviewRequest
{
    public string ReviewerName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Rate { get; set; }
}

public class UpdateReviewRequest
{
    public string? ReviewerName { get; set; }
    public string? Description { get; set; }
    public int? Rate { get; set; }
}

public class ReviewDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Rate { get; set; }
    public DateTime CreatedAt { get; set; }
}
