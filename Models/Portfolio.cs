namespace test.Models;

public class Portfolio
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public User User { get; set; } = null!;
    public List<PortfolioMedia> Media { get; set; } = new();
}

public class PortfolioMedia
{
    public int Id { get; set; }
    public int PortfolioId { get; set; }
    public string Url { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }

    public Portfolio Portfolio { get; set; } = null!;
}
