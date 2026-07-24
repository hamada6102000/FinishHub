namespace test.Models;

public class Favorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EngineerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public User Engineer { get; set; } = null!;
}
