namespace test.Models;

public class Review
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Rate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
