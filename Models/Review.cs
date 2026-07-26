namespace test.Models;

public class Review
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ReviewerId { get; set; }
    public string? Description { get; set; }
    public int Rate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public User Reviewer { get; set; } = null!;
}
